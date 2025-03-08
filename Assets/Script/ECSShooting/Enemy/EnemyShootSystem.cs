using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyShootSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (shooter, enemyData, transform) in SystemAPI.Query<RefRW<EnemyShooter>, RefRO<EnemyData>, RefRO<LocalTransform>>())
        {
            shooter.ValueRW.TimeUntilNextShot -= deltaTime;

            if (shooter.ValueRW.TimeUntilNextShot > 0)
                continue;

            shooter.ValueRW.TimeUntilNextShot = shooter.ValueRO.FireRate;

            // Prefab から弾を生成
            Entity bullet = ecb.Instantiate(shooter.ValueRO.Prefab);
            var pos = transform.ValueRO.Position + transform.ValueRO.Forward() * transform.ValueRO.Scale * 3;

            // Transform を設定（位置は敵の位置、回転はなし）
            ecb.SetComponent(bullet, new LocalTransform
            {
                Position = pos,
                Rotation = transform.ValueRO.Rotation,
                Scale = 1.0f
            });
        }

        ecb.Playback(state.EntityManager);
    }
}
