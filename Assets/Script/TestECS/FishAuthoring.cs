using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(8)] // 指定分のバッファをインライン展開
public struct NeighborsEntityBufferElement : IBufferElementData
{
    public Entity entity;
}

public static class FishConfig
{
    public const int NeighborsEntityBufferMaxSize = 8;
}

public struct Fish : IComponentData
{
    public float3 acceleration;
    public float3 velocity;
    public Entity paramEntity;
}

public struct FishJobData : IComponentData
{
    public float3 Position;
    public float3 Velocity;
}

public class FishAuthoring : MonoBehaviour
{
   
}

/// <summary>
/// 変換処理
/// </summary>
public class FishBaker : Baker<FishAuthoring>
{
    public override void Bake(FishAuthoring fishAuthoring)
    {
        // 移動を行うため動的（LocalTransform が付与される）
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new Fish()
        {
            velocity = UnityEngine.Random.insideUnitSphere,
            acceleration = 0.0f,
            paramEntity = Entity.Null,
        });

        AddComponent(entity, new FishJobData()
        {
            Position = fishAuthoring.transform.position,
            Velocity = UnityEngine.Random.insideUnitSphere,
        });

        // エンティティにDynamicBufferを付与
        AddBuffer<NeighborsEntityBufferElement>(entity);
    }
}
