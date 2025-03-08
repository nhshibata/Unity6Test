using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerMoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (player, input, transform) in SystemAPI.Query<RefRO<PlayerData>, RefRO<PlayerInput>, RefRW<LocalTransform>>())
        {
            float2 moveDir = input.ValueRO.Move;
            float3 newPos = transform.ValueRW.Position + new float3(moveDir.x, moveDir.y, 0) * player.ValueRO.Speed * deltaTime;

            // 自動前進
            if (player.ValueRO.AutoMoveForward)
            {
                newPos.z += player.ValueRO.ForwardSpeed * deltaTime;
            }

            // 画面の範囲制限
            newPos.x = math.clamp(newPos.x, -player.ValueRO.Bounds.x, player.ValueRO.Bounds.x);
            newPos.y = math.clamp(newPos.y, -player.ValueRO.Bounds.y, player.ValueRO.Bounds.y);

            transform.ValueRW.Position = newPos;
        }
    }
}
