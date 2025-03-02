using Unity.Burst;
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        paramLookUp.Update(ref state);
        transformLookUp.Update(ref state);

        // DynamicBuffer をクエリに指定してアクセス可能
        foreach (var fish in SystemAPI.Query<FishAspect>())
        {
            var n = fish.Neighbors.Length;
            if (n == 0) continue;

            var pos = fish.LocalTransform.Position;

            var forceDir = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = fish.Neighbors[i].entity;
                var neighborPos = transformLookUp[neighborEntity].Position;
                var to = neighborPos - pos;
                forceDir += -math.normalizesafe(to);
            }
            forceDir /= n;

            var param = paramLookUp[fish.ParamEntity];
            fish.Acceleration += forceDir * param.separationForce;
        }
    }
}