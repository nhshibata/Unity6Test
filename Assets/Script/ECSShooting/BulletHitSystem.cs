using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct BulletHitSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (bulletTransform, bullet, bulletEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<Bullet>>().WithEntityAccess())
        {
            float3 bulletPos = bulletTransform.ValueRO.Position;

            // 敵の当たり判定
            foreach (var (enemyTransform, enemy, hitbox, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyData>, RefRO<Hitbox>>().WithEntityAccess())
            {
                if (CheckAABB(bulletPos, enemyTransform.ValueRO.Position, hitbox.ValueRO.Size))
                {
                    enemy.ValueRW.Hp -= bullet.ValueRO.Damage;
                    EnemyScoreEvent.OnDamaged(bullet.ValueRO.Damage);
                    if (enemy.ValueRW.Hp <= 0)
                    {
                        ecb.DestroyEntity(enemyEntity);
                        EnemyScoreEvent.OnDefeated(1);
                    }
                    ecb.DestroyEntity(bulletEntity);
                }
            }

            // プレイヤーの当たり判定
            foreach (var (playerTransform, player, hitbox, playerEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<PlayerData>, RefRO<Hitbox>>().WithEntityAccess())
            {
                if (CheckAABB(bulletPos, playerTransform.ValueRO.Position, hitbox.ValueRO.Size))
                {
                    player.ValueRW.Hp -= bullet.ValueRO.Damage;
                    PlayerHealthEvent.OnHealthChange(player.ValueRW.Hp);

                    if (player.ValueRW.Hp <= 0)
                    {
                        player.ValueRW.IsDead = true;
                        PlayerHealthEvent.OnDefeated();
                        ecb.DestroyEntity(playerEntity);
                    }
                    ecb.DestroyEntity(bulletEntity);
                }
            }
        }

        ecb.Playback(state.EntityManager);
    }

    /// <summary>
    /// AABB 判定を行うヘルパー関数
    /// </summary>
    private static bool CheckAABB(float3 pos1, float3 pos2, float3 size)
    {
        return math.abs(pos1.x - pos2.x) < size.x * 0.5f &&
               math.abs(pos1.y - pos2.y) < size.y * 0.5f &&
               math.abs(pos1.z - pos2.z) < size.z * 0.5f;
    }
}

public static class EnemyScoreEvent
{
    public static event Action<int> OnEnemyDamaged;
    public static event Action<int> OnEnemyDefeated;

    public static void OnDamaged(int damage) => OnEnemyDamaged?.Invoke(damage);
    public static void OnDefeated(int count) => OnEnemyDefeated?.Invoke(count);
}

public static class PlayerHealthEvent
{
    public static event Action<int> OnHealthChanged;
    public static event Action OnPlayerDead;

    public static void OnHealthChange(int hp) => OnHealthChanged?.Invoke(hp);
    public static void OnDefeated() => OnPlayerDead?.Invoke();
}