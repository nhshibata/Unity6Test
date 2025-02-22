using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Fish : IComponentData
{
    public float3 acceleration;
    public float3 velocity;
    public Entity paramEntity;
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
        // 移動を行うため動的
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new Fish()
        {
            velocity = UnityEngine.Random.insideUnitSphere,
            acceleration = 0.0f,
            paramEntity = Entity.Null,
        });
    }

}

public partial struct MoveSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;

    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(true);
    }

    public void OnUpdate(ref SystemState state)
    {
        // チャンクが再構築された時のために更新
        paramLookUp.Update(ref state);

        var dt = SystemAPI.Time.DeltaTime;

        // 指定したコンポーネントを所持するEntityを取得
        foreach (var (fish, lt) in
            SystemAPI.Query<
                RefRW<Fish>,
                RefRW<LocalTransform>>())
        {
            var param = paramLookUp[fish.ValueRO.paramEntity];

            fish.ValueRW.velocity += fish.ValueRO.acceleration * dt;
            var speed = math.length(fish.ValueRO.velocity);
            speed = math.clamp(speed, param.minSpeed, param.maxSpeed);
            var dir = math.normalize(fish.ValueRO.velocity);
            var up = math.up();
            fish.ValueRW.velocity = dir * speed;
            lt.ValueRW.Rotation = quaternion.LookRotationSafe(dir, up);
            lt.ValueRW.Position += fish.ValueRO.velocity * dt;
        }
    }
}