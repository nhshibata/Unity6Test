using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct PlayerGoalSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var playerQuery = SystemAPI.QueryBuilder().WithAll<PlayerData, LocalTransform>().Build();
        if (playerQuery.IsEmpty)
            return;

        var playerEntity = playerQuery.GetSingletonEntity();
        if (playerEntity == Entity.Null)
            return;

        var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
        float3 playerPos = playerTransform.Position;

        // Goalの位置をチェック
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach (var (goalData, hitbox, goalEntity) in SystemAPI.Query<RefRW<PlayerGoalData>, RefRW<Hitbox>>().WithEntityAccess())
        {
            if (goalData.ValueRO.IsAchieved)
                continue;

            if (Hitbox.CheckAABB(playerPos, goalData.ValueRO.Position, hitbox.ValueRO.Size))
            {
                // 目標達成
                goalData.ValueRW.IsAchieved = true;
                GoalEvent.OnGoalAchieved(); // 目標達成のイベント発行
                ecb.DestroyEntity(goalEntity); // Goal を削除
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

public static class GoalEvent
{
    public static event Action OnGoalEvent;
    public static void OnGoalAchieved() => OnGoalEvent?.Invoke();

}

