using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public partial struct PlayerInputSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float2 move = new float2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        bool shoot = Input.GetKeyDown(KeyCode.Space);

        foreach (var input in SystemAPI.Query<RefRW<PlayerInput>>())
        {
            input.ValueRW.Move = move;
            input.ValueRW.Shoot = shoot;
        }
    }
}
