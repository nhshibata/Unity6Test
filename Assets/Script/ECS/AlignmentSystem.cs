using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct AlignmentJob : IJobEntity
{
    [ReadOnly] 
    public ComponentLookup<Parameter> paramLookUp;
    [ReadOnly] 
    public ComponentLookup<FishJobData> fishJobDataLookUp;


    void Execute(ref Fish fish, in DynamicBuffer<NeighborsEntityBufferElement> neighbors)
    {
        var n = neighbors.Length;
        if (n == 0) 
            return;

        var averageV = float3.zero;
        for (int i = 0; i < n; ++i)
        {
            var neighborEntity = neighbors[i].entity;
            // 他の個体へのアクセスは FishJobData コンポーネントを通じて行う
            var neighborV = fishJobDataLookUp[neighborEntity].Velocity;
            averageV += neighborV;
        }
        averageV /= n;

        var param = paramLookUp[fish.paramEntity];
        var v = fish.velocity;

        fish.acceleration += (averageV - v) * param.alignmentForce;
    }
}

[UpdateBefore(typeof(MoveSystem))]
[UpdateAfter(typeof(NeighborsDetectionSystem))]
public partial struct AlignmentSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;
    private ComponentLookup<FishJobData> fishJobDataLookUp;

    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(true);
        fishJobDataLookUp = state.GetComponentLookup<FishJobData>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        paramLookUp.Update(ref state);
        fishJobDataLookUp.Update(ref state);

        var job = new AlignmentJob()
        {
            fishJobDataLookUp = fishJobDataLookUp,
            paramLookUp = paramLookUp,
        };

        var query = SystemAPI.QueryBuilder().WithAll<Fish, NeighborsEntityBufferElement>().Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}