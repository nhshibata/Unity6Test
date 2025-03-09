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
        var playerEntity = playerQuery.GetSingletonEntity();
        var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        float3 playerPos = playerTransform.Position;

        foreach (var (spawnArea, spawnEnemy, lt, entity) in SystemAPI.Query<RefRW<SpawnArea>, RefRW<SpawnEnemy>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (spawnArea.ValueRO.HasSpawned)
                continue;

            if (!Hitbox.CheckAABB(playerPos, spawnArea.ValueRO.Center, spawnArea.ValueRO.Extents))
                continue;

            // グリッド配置の座標を計算
            GetGridPositions(lt.ValueRO.Position + spawnArea.ValueRO.Center, spawnArea.ValueRO, out var positions);

            for (int i = 0; i < spawnArea.ValueRO.createNum; i++)
            {
                Entity newEnemy = ecb.Instantiate(spawnEnemy.ValueRO.EnemyPrefab);
                ecb.SetComponent(newEnemy, new LocalTransform
                {
                    Position = positions[i],
                    Rotation = lt.ValueRO.Rotation,
                    Scale = 1.0f
                });
            }

            positions.Dispose(); // NativeArray のメモリ解放
            spawnArea.ValueRW.HasSpawned = true;
            Debug.Log("敵生成");
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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

                positions[index++] = center + new float3(
                    -width / 2 + j * spawnArea.Spacing + offsetX,
                    -height / 2 + i * spawnArea.Spacing + offsetY,
                    spawnArea.OffsetZ + offsetZ // Z軸方向にもランダムなオフセットを加える
                );
            }
        }
    }

}
