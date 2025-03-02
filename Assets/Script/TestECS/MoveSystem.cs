using Unity.Burst;
using Unity.Entities;
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // チャンクが再構築された時のために更新
        paramLookUp.Update(ref state);

        var dt = SystemAPI.Time.DeltaTime;

        // 指定したコンポーネントを所持するEntityを取得
        foreach (var (fish, jobData, lt) in SystemAPI.Query<RefRW<Fish>, RefRW<FishJobData>, RefRW<LocalTransform>>())
        {
            var param = paramLookUp[fish.ValueRO.paramEntity];

            var v = fish.ValueRO.velocity;
            v += fish.ValueRO.acceleration * dt;
            var speed = math.length(v);
            speed = math.clamp(speed, param.minSpeed, param.maxSpeed);
            var dir = math.normalize(v);
            v = dir * speed;
            fish.ValueRW.velocity = v;

            fish.ValueRW.acceleration = 0f;

            var pos = lt.ValueRO.Position;
            pos += fish.ValueRO.velocity * dt;
            lt.ValueRW.Position = pos;

            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotationSafe(dir, up);
            Debug.DrawRay(lt.ValueRW.Position, fish.ValueRO.velocity * 0.3f, Color.green, 0f, true);

            jobData.ValueRW.Position = pos;
            jobData.ValueRW.Velocity = v;
        }
    }
}