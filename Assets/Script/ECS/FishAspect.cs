using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct FishAspect : IAspect
{
    readonly RefRW<Fish> fish;
    readonly RefRW<LocalTransform> localTransform;
    readonly DynamicBuffer<NeighborsEntityBufferElement> neighborsEntityBuffer;

    public Fish Fish
    {
        get => fish.ValueRO;
        set => fish.ValueRW = value;
    }

    public float3 Velocity
    {
        get => fish.ValueRO.velocity;
        set => fish.ValueRW.velocity = value;
    }

    public float3 Acceleration
    {
        get => fish.ValueRO.acceleration;
        set => fish.ValueRW.acceleration = value;
    }

    public Entity ParamEntity
    {
        get => fish.ValueRO.paramEntity;
    }

    public LocalTransform LocalTransform
    {
        get => localTransform.ValueRO;
        set => localTransform.ValueRW = value;
    }

    public DynamicBuffer<NeighborsEntityBufferElement> Neighbors => neighborsEntityBuffer;
}