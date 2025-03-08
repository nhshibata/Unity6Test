using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        if (playerQuery.IsEmpty) 
            return;

        var playerEntity = playerQuery.GetSingletonEntity();
        if (playerEntity == Entity.Null)
            return;

        var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
        float3 playerPos = playerTransform.Position;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (spawnArea, lt, entity) in SystemAPI.Query<RefRW<SpawnArea>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (spawnArea.ValueRO.HasSpawned)
                continue;

            if (CheckAABB(playerPos, spawnArea.ValueRO.Center, spawnArea.ValueRO.Extents))
            {
                int enemyCount = spawnArea.ValueRO.Rows * spawnArea.ValueRO.Columns;
                Entity enemyPrefab = spawnArea.ValueRO.EnemyPrefab;

                if (!state.EntityManager.HasComponent<Prefab>(enemyPrefab))
                {
                    UnityEngine.Debug.LogError("Enemy prefab does not have a Prefab component!");
                    continue;
                }

                // グリッド配置の座標を計算
                GetGridPositions(lt.ValueRO.Position + spawnArea.ValueRO.Center, spawnArea.ValueRO, out var positions);

                for (int i = 0; i < enemyCount; i++)
                {
                    Entity newEnemy = ecb.Instantiate(enemyPrefab);
                    ecb.SetComponent(newEnemy, new LocalTransform { Position = positions[i] });
                }

                spawnArea.ValueRW.HasSpawned = true;
                positions.Dispose(); // NativeArray のメモリ解放
                Debug.Log("敵生成!");
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private static bool CheckAABB(float3 pos1, float3 pos2, float3 size)
    {
        return math.abs(pos1.x - pos2.x) < size.x * 0.5f &&
               math.abs(pos1.y - pos2.y) < size.y * 0.5f &&
               math.abs(pos1.z - pos2.z) < size.z * 0.5f;
    }

    private void GetGridPositions(float3 center, SpawnArea spawnArea, out NativeArray<float3> positions)
    {
        int total = spawnArea.Rows * spawnArea.Columns;
        positions = new NativeArray<float3>(total, Unity.Collections.Allocator.Temp);

        float width = (spawnArea.Columns - 1) * spawnArea.Spacing;
        float height = (spawnArea.Rows - 1) * spawnArea.Spacing;

        int index = 0;
        for (int i = 0; i < spawnArea.Rows; i++)
        {
            for (int j = 0; j < spawnArea.Columns; j++)
            {
                // 少しランダムにずらす（-0.5f ~ 0.5f の範囲でランダム）
                float offsetX = _random.NextFloat(-0.5f, 0.5f);
                float offsetY = _random.NextFloat(-0.5f, 0.5f);
                float offsetZ = _random.NextFloat(-0.5f, 0.5f);

                positions[index++] = spawnArea.Center + new float3(
                    -width / 2 + j * spawnArea.Spacing + offsetX,
                    -height / 2 + i * spawnArea.Spacing + offsetY,
                    spawnArea.OffsetZ + offsetZ // Z軸方向にもランダムなオフセットを加える
                );
            }
        }
    }

}
