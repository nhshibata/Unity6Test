using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemyMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var random = new Unity.Mathematics.Random((uint)SystemAPI.Time.ElapsedTime);

        foreach (var (enemy, transform) in SystemAPI.Query<RefRW<EnemyMove>, RefRW<LocalTransform>>())
        {
            float3 newPos = transform.ValueRO.Position;

            switch (enemy.ValueRO.MovementType)
            {
                case EnemyMovementType.Linear:
                    newPos = MoveLinear(ref enemy.ValueRW, newPos, deltaTime);
                    break;
                case EnemyMovementType.Random:
                    newPos = MoveRandom(ref enemy.ValueRW, newPos, deltaTime, ref random);
                    break;
                case EnemyMovementType.SinWave:
                    double elapsedTime = state.WorldUnmanaged.Time.ElapsedTime;
                    newPos = MoveSinWave(ref enemy.ValueRW, enemy.ValueRO.StartPos, elapsedTime);
                    break;
                case EnemyMovementType.Waypoint:
                    newPos = MoveWaypoint(ref enemy.ValueRW, newPos, deltaTime);
                    break;
            }

            transform.ValueRW.Position = newPos;
        }
    }

    // 直線移動
    private float3 MoveLinear(ref EnemyMove enemy, float3 pos, float deltaTime)
    {
        float3 targetPos = enemy.MovingForward ? enemy.EndPos : enemy.StartPos;
        float3 dir = math.normalize(targetPos - pos);
        pos += dir * enemy.Speed * deltaTime;

        if (math.distance(pos, targetPos) < 0.1f)
        {
            enemy.MovingForward = !enemy.MovingForward;
        }
        return pos;
    }

    // ランダム移動
    private float3 MoveRandom(ref EnemyMove enemy, float3 pos, float deltaTime, ref Random random)
    {
        if (enemy.TimeSinceDirectionChange > enemy.RandomChangeInterval)
        {
            enemy.RandomDirection = random.NextFloat3Direction();
            enemy.TimeSinceDirectionChange = 0;
            enemy.RandomChangeInterval = random.NextFloat(1.0f, 3.0f);
        }

        pos += enemy.RandomDirection * enemy.Speed * deltaTime;
        enemy.TimeSinceDirectionChange += deltaTime;
        return pos;
    }

    // sin移動
    private float3 MoveSinWave(ref EnemyMove enemy, float3 startPos, double elapsedTime)
    {
        float offsetX = math.sin((float)elapsedTime * enemy.WaveFrequency) * enemy.WaveAmplitude;
        float offsetY = math.sin((float)elapsedTime * enemy.WaveFrequency * 2f) * enemy.WaveAmplitude * 0.5f;

        return new float3(startPos.x + offsetX, startPos.y + offsetY, startPos.z);
    }

    // ウェイポイント移動
    private float3 MoveWaypoint(ref EnemyMove enemy, float3 pos, float deltaTime)
    {
        if (enemy.Waypoints.Length == 0) return pos;

        float3 targetPos = enemy.Waypoints[enemy.CurrentWaypointIndex];
        float3 dir = math.normalize(targetPos - pos);
        pos += dir * enemy.Speed * deltaTime;

        if (math.distance(pos, targetPos) < 0.1f)
        {
            enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.Waypoints.Length;
        }
        return pos;
    }
}
