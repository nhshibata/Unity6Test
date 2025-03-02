using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
/// <summary>
/// 敵の移動システム
/// </summary>
[BurstCompile]
public partial struct EnemyMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (enemy, transform) in SystemAPI.Query<RefRW<EnemyMove>, RefRW<LocalTransform>>())
        {
            float3 targetPos = enemy.ValueRO.MovingForward ? enemy.ValueRO.EndPos : enemy.ValueRO.StartPos;
            float3 dir = math.normalize(targetPos - transform.ValueRO.Position);
            transform.ValueRW.Position += dir * enemy.ValueRO.Speed * deltaTime;

            // 目標地点に到達したら方向を切り替える
            if (math.distance(transform.ValueRO.Position, targetPos) < 0.1f)
            {
                enemy.ValueRW.MovingForward = !enemy.ValueRO.MovingForward;
            }
        }
    }
}
