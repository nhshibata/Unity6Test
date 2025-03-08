using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    private Unity.Mathematics.Random _random;

    public void OnCreate(ref SystemState state)
    {
        _random = new Unity.Mathematics.Random(12345);
    }

    public void OnUpdate(ref SystemState state)
    {
        var playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerData, LocalTransform>().Build();
        if (playerQuery.IsEmpty) return;

        var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerQuery.GetSingletonEntity());
        float3 playerPos = playerTransform.Position;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (spawnArea, entity) in SystemAPI.Query<RefRW<SpawnArea>>().WithEntityAccess())
        {
            if (spawnArea.ValueRO.HasSpawned) continue;

            if (IsInsideAABB(playerPos, spawnArea.ValueRO.Center, spawnArea.ValueRO.Extents))
            {
                ref var enemyList = ref spawnArea.ValueRO.EnemyPrefabs.Value;
                int enemyCount = spawnArea.ValueRO.Rows * spawnArea.ValueRO.Columns;

                // グリッド配置の座標を計算
                GetGridPositions(spawnArea.ValueRO.Center, spawnArea.ValueRO.Rows, spawnArea.ValueRO.Columns, spawnArea.ValueRO.Spacing, out var positions);

                for (int i = 0; i < enemyCount; i++)
                {
                    int enemyIndex = _random.NextInt(enemyList.Prefabs.Length);
                    Entity enemyPrefab = enemyList.Prefabs[enemyIndex];

                    Entity newEnemy = ecb.Instantiate(enemyPrefab);
                    ecb.SetComponent(newEnemy, new LocalTransform { Position = positions[i] });
                }

                spawnArea.ValueRW.HasSpawned = true;
                positions.Dispose();
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private bool IsInsideAABB(float3 position, float3 center, float3 extents)
    {
        return math.all(position >= (center - extents) & position <= (center + extents));
    }

    // グリッド状の配置を計算
    private void GetGridPositions(float3 center, int rows, int cols, float spacing, out NativeArray<float3> positions)
    {
        int total = rows * cols;
        positions = new NativeArray<float3>(total, Unity.Collections.Allocator.Temp);

        float width = (cols - 1) * spacing;
        float height = (rows - 1) * spacing;

        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                positions[index++] = center + new float3(
                    -width / 2 + j * spacing,
                    0,
                    -height / 2 + i * spacing
                );
            }
        }
    }

}
