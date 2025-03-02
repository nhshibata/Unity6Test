using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct PlayerShootSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (shooter, transform) in SystemAPI.Query<RefRW<PlayerShooter>, RefRO<LocalTransform>>())
        {
            shooter.ValueRW.TimeUntilNextShot -= deltaTime;

            if (Input.GetKey(KeyCode.Space) && shooter.ValueRW.TimeUntilNextShot <= 0)
            {
                shooter.ValueRW.TimeUntilNextShot = shooter.ValueRO.FireRate;

                // Prefab から弾を生成
                Entity bullet = ecb.Instantiate(shooter.ValueRO.Prefab);
                var pos = transform.ValueRO.Position + transform.ValueRO.Forward() * transform.ValueRO.Scale * 3;
                ecb.SetComponent(bullet, new LocalTransform
                {
                    Position = pos,
                    Rotation = transform.ValueRO.Rotation,
                    Scale = 1.0f,
                });
            }
        }

        ecb.Playback(state.EntityManager);
    }
}
