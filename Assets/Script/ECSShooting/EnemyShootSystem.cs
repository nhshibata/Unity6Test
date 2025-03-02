using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyShootSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (shooter, transform)
                 in SystemAPI.Query<RefRW<EnemyShooter>, RefRO<LocalTransform>>())
        {
            shooter.ValueRW.TimeUntilNextShot -= deltaTime;

            if (shooter.ValueRW.TimeUntilNextShot <= 0)
            {
                shooter.ValueRW.TimeUntilNextShot = shooter.ValueRO.FireRate;

                // 🔹 Prefab から弾を生成
                Entity bullet = ecb.Instantiate(shooter.ValueRO.Prefab);
                ecb.SetComponent(bullet, new LocalTransform
                {
                    Position = transform.ValueRO.Position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
            }
        }

        ecb.Playback(state.EntityManager);
    }
}
