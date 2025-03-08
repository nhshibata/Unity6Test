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

        // ElapsedTimeを使うが、ゼロにならないようにオフセットを追加
        uint seed = (uint)(state.WorldUnmanaged.Time.ElapsedTime * 1000);
        seed = seed == 0 ? 1 : seed; // シードが0だった場合は1に変更
        var random = new Unity.Mathematics.Random(seed);
        double elapsedTime = state.WorldUnmanaged.Time.ElapsedTime;

        foreach (var (enemy, transform) in SystemAPI.Query<RefRW<EnemyMove>, RefRW<LocalTransform>>())
        {
            float3 newPos = transform.ValueRO.Position;

            switch (enemy.ValueRO.MovementType)
            {
                case EnemyMovementType.Linear:
                    newPos = MoveLinear(ref enemy.ValueRW, ref transform.ValueRW, newPos, deltaTime, elapsedTime);
                    break;
                case EnemyMovementType.Random:
                    newPos = MoveRandom(ref enemy.ValueRW, ref transform.ValueRW, newPos, deltaTime, ref random);
                    break;
                case EnemyMovementType.SinWave:
                    newPos = MoveLinear(ref enemy.ValueRW, ref transform.ValueRW, newPos, deltaTime, elapsedTime);
                    break;
                case EnemyMovementType.Waypoint:
                    newPos = MoveWaypoint(ref enemy.ValueRW, ref transform.ValueRW, newPos, deltaTime);
                    break;
            }

            transform.ValueRW.Position = newPos;
        }
    }

    private float3 MoveLinear(ref EnemyMove enemy, ref LocalTransform transform, float3 pos, float deltaTime, double elapsedTime)
    {
        // 進行方向に基づいて新しい位置を計算
        float3 targetPos = enemy.StartPos + (transform.Forward() * enemy.TargetDistance);
        float3 dir = math.normalize(targetPos - pos);
        pos += dir * enemy.Speed * deltaTime;

        // サイン波による動きを加える
        pos = MoveSinWave(ref enemy, ref transform,  pos, elapsedTime);

        return pos;
    }

    private float3 MoveRandom(ref EnemyMove enemy, ref LocalTransform transform, float3 pos, float deltaTime, ref Random random)
    {
        // ランダムな移動方向の生成
        if (enemy.TimeSinceDirectionChange > enemy.RandomChangeInterval)
        {
            enemy.RandomDirection = random.NextFloat3Direction();
            enemy.TimeSinceDirectionChange = 0;
            enemy.RandomChangeInterval = random.NextFloat(1.0f, 3.0f);
        }

        // 進行方向にランダムの方向を加える
        pos += (enemy.RandomDirection + transform.Forward()) * enemy.Speed * deltaTime;
        enemy.TimeSinceDirectionChange += deltaTime;

        return pos;
    }

    private float3 MoveSinWave(ref EnemyMove enemy, ref LocalTransform transform, float3 startPos, double elapsedTime)
    {
        // sin波による動き
        float offsetX = math.sin((float)elapsedTime * enemy.WaveFrequency) * enemy.WaveAmplitude;
        float offsetY = math.sin((float)elapsedTime * enemy.WaveFrequency * 2f) * enemy.WaveAmplitude * 0.5f;

        // サイン波の影響を加える
        float3 newPos = startPos + new float3(offsetX, offsetY, 0);
        newPos += transform.Forward() * 0.1f;

        return newPos;
    }

    private float3 MoveWaypoint(ref EnemyMove enemy, ref LocalTransform transform, float3 pos, float deltaTime)
    {
        if (enemy.Waypoints.Length == 0) return pos;

        // 現在のウェイポイントの方向
        float3 targetPos = enemy.Waypoints[enemy.CurrentWaypointIndex];
        float3 dir = math.normalize(targetPos - pos);

        // 進行方向に沿って追加
        pos += (dir + transform.Forward()) * enemy.Speed * deltaTime;

        // 目標地点に到達したか判定
        if (math.distance(pos, targetPos) < 0.1f)
        {
            enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.Waypoints.Length;
        }
        return pos;
    }

}
