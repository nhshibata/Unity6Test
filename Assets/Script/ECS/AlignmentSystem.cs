using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(MoveSystem))]
[UpdateAfter(typeof(NeighborsDetectionSystem))]
public partial struct AlignmentSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;
    private ComponentLookup<Fish> fishLookUp;


    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        fishLookUp = state.GetComponentLookup<Fish>(isReadOnly: true);
    }

    public void OnUpdate(ref SystemState state)
    {
        paramLookUp.Update(ref state);
        fishLookUp.Update(ref state);

        foreach (var (fish, neighbors) in SystemAPI.Query<RefRW<Fish>, DynamicBuffer<NeighborsEntityBufferElement>>())
        {
            var n = neighbors.Length;
            if (n == 0) 
                continue;

            var averageV = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].entity;
                var neighborV = fishLookUp[neighborEntity].velocity;
                averageV += neighborV;
            }
            averageV /= n;

            var param = paramLookUp[fish.ValueRW.paramEntity];
            var v = fish.ValueRO.velocity;

            fish.ValueRW.acceleration += (averageV - v) * param.alignmentForce;
        }
    }
}