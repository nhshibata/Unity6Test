using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
        foreach (var (fish, lt) in SystemAPI.Query<RefRW<Fish>, RefRW<LocalTransform>>())
        {
            var param = paramLookUp[fish.ValueRO.paramEntity];

            fish.ValueRW.velocity += fish.ValueRO.acceleration * dt;
            var speed = math.length(fish.ValueRO.velocity);
            var dir = math.normalize(fish.ValueRO.velocity);
            var up = math.up();

            speed = math.clamp(speed, param.minSpeed, param.maxSpeed);
            fish.ValueRW.velocity = dir * speed;
            lt.ValueRW.Rotation = quaternion.LookRotationSafe(dir, up);
            lt.ValueRW.Position += fish.ValueRO.velocity * dt;
            Debug.DrawRay(lt.ValueRW.Position, fish.ValueRO.velocity * 0.3f, Color.green, 0f, true);
        }
    }
}