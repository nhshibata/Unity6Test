using System;
using Unity.Burst;
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

            if(bullet.ValueRO.type != Bullet.BulletType.Enemy)
            {
                // 敵の当たり判定
                foreach (var (enemyTransform, enemy, hitbox, enemyEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyData>, RefRO<Hitbox>>().WithEntityAccess())
                {
                    if (Hitbox.CheckAABB(bulletPos, enemyTransform.ValueRO.Position, hitbox.ValueRO.Size))
                    {
                        enemy.ValueRW.Hp -= bullet.ValueRO.Damage;
                        EnemyScoreEvent.OnDamaged(bullet.ValueRO.Damage);
                        if (enemy.ValueRW.Hp <= 0)
                        {
                            // 再帰的に子を削除
                            DestroyChildrenRecursively(ecb, state.EntityManager, enemyEntity);

                            ecb.DestroyEntity(enemyEntity);
                            EnemyScoreEvent.OnDefeated(1);
                        }
                        ecb.DestroyEntity(bulletEntity);
                    }
                }
            }

            if (bullet.ValueRO.type != Bullet.BulletType.Player)
            {
                // プレイヤーの当たり判定
                foreach (var (playerTransform, player, hitbox, playerEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<PlayerData>, RefRO<Hitbox>>().WithEntityAccess())
                {
                    if (Hitbox.CheckAABB(bulletPos, playerTransform.ValueRO.Position, hitbox.ValueRO.Size))
                    {
                        player.ValueRW.Hp -= bullet.ValueRO.Damage;
                        PlayerHealthEvent.OnHealthChange(player.ValueRO.MaxHp, player.ValueRW.Hp);

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
        }

        ecb.Playback(state.EntityManager);
    }

    void DestroyChildrenRecursively(EntityCommandBuffer ecb, EntityManager entityManager, Entity parentEntity)
    {
        if (entityManager.HasComponent<Child>(parentEntity))
        {
            DynamicBuffer<Child> children = entityManager.GetBuffer<Child>(parentEntity);

            foreach (var child in children)
            {
                DestroyChildrenRecursively(ecb, entityManager, child.Value);
                ecb.DestroyEntity(child.Value);
            }
        }
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
    public static event Action<int, int> OnHealthChanged;
    public static event Action OnPlayerDead;

    public static void OnHealthChange(int maxHp, int hp) => OnHealthChanged?.Invoke(maxHp, hp);
    public static void OnDefeated() => OnPlayerDead?.Invoke();
}