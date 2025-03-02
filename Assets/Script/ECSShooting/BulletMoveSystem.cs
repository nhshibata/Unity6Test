using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// 弾の動きを制御するシステム
/// </summary>
[BurstCompile]
public partial struct BulletMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // 削除蓄積
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (bullet, transform, entity) in SystemAPI.Query<RefRW<Bullet>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            // 位置を更新
            transform.ValueRW.Position += bullet.ValueRO.Direction * bullet.ValueRO.Speed * deltaTime;

            // 生存時間を減らす
            bullet.ValueRW.Lifetime -= deltaTime;
            if (bullet.ValueRW.Lifetime <= 0)
            {
                Debug.Log("削除");
                ecb.DestroyEntity(entity);
            }
        }
        // ここで削除を適用
        ecb.Playback(state.EntityManager);
    }
}