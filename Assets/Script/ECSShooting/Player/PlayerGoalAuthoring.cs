using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerGoalData : IComponentData
{
    public float3 Position;
    public bool IsAchieved;
}

public class PlayerGoalAuthoring : MonoBehaviour
{
    public Vector3 goalPosition;
    public Hitbox hitbox;

    public class GoalAuthoringBaker : Baker<PlayerGoalAuthoring>
    {
        public override void Bake(PlayerGoalAuthoring authoring)
        {
            var goalEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(goalEntity, new PlayerGoalData
            {
                Position = authoring.goalPosition,
                IsAchieved = false
            });

            AddComponent(goalEntity, authoring.hitbox);
        }
    }
}
