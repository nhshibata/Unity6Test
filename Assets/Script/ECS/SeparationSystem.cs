using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct SeparationSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;

    // Neighbors のバッファのエンティティから LocalTransform 引きするためのルックアップ
    private ComponentLookup<LocalTransform> transformLookUp;


    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        transformLookUp = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
    }

    public void OnUpdate(ref SystemState state)
    {
        paramLookUp.Update(ref state);
        transformLookUp.Update(ref state);

        // DynamicBuffer をクエリに指定してアクセス可能
        foreach (var (fish, lt, neighbors) in SystemAPI.Query<RefRW<Fish>, RefRO<LocalTransform>, DynamicBuffer<NeighborsEntityBufferElement>>())
        {
            var n = neighbors.Length;
            if (n == 0) 
                continue;

            var param = paramLookUp[fish.ValueRW.paramEntity];
            var pos = lt.ValueRO.Position;

            // 平均の離れる方向ベクトルを計算
            var forceDir = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].entity;
                var neighborPos = transformLookUp[neighborEntity].Position;
                var to = neighborPos - pos;
                forceDir += -math.normalizesafe(to);
            }
            forceDir /= n;
            forceDir = math.normalizesafe(forceDir);

            // 加速度に足す
            fish.ValueRW.acceleration += forceDir * param.separationForce;
        }
    }
}