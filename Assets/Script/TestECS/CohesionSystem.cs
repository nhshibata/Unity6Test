using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct CohesionSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;
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

        foreach (var (fish, lt, neighbors) in
            SystemAPI.Query<
                RefRW<Fish>,
                RefRO<LocalTransform>,
                DynamicBuffer<NeighborsEntityBufferElement>>())
        {
            var n = neighbors.Length;
            if (n == 0) continue;

            var averagePos = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].entity;
                var neighborPos = transformLookUp[neighborEntity].Position;
                averagePos += neighborPos;
            }
            averagePos /= n;

            var pos = lt.ValueRO.Position;
            var param = paramLookUp[fish.ValueRW.paramEntity];
            fish.ValueRW.acceleration += (averagePos - pos) * param.cohesionForce;
        }
    }
}