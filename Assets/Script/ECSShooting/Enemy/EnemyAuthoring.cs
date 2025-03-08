using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public enum EnemyMovementType
{
    Linear, 
    Random, 
    SinWave,
    Waypoint
}

[Serializable]
public struct EnemyData : IComponentData
{
    public int Hp;
    public int Damage;
}

/// <summary>
/// 敵の移動コンポーネント
/// </summary>
[Serializable]
public struct EnemyMove : IComponentData
{
    public EnemyMovementType MovementType;
    public bool IsMovingForward;
    public float Speed;       
    public float3 StartPos;   
    public float3 EndPos;     
    public bool MovingForward;

    public float TargetDistance;

    // ランダム移動用
    public float3 RandomDirection;
    [HideInInspector]
    public float TimeSinceDirectionChange;
    public float RandomChangeInterval;

    // サイン波用
    public float WaveAmplitude;
    public float WaveFrequency;

    // ウェイポイント移動用
    public FixedList512Bytes<float3> Waypoints;
    [HideInInspector]
    public int CurrentWaypointIndex;
}

public struct EnemyShooter : IComponentData
{
    public float FireRate;
    public float TimeUntilNextShot;
    public Entity Prefab;
}

public class EnemyAuthoring : MonoBehaviour
{
    [SerializeField]
    private EnemyData enemyData;
    [SerializeField]
    private EnemyMove enemyMove;
    [SerializeField]
    private float fireRate;
    [SerializeField]
    private GameObject prefabEntity;
    [SerializeField]
    private Hitbox hitbox;

    class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, authoring.enemyData);

            AddComponent(entity, authoring.enemyMove);

            AddComponent(entity, new LocalTransform
            {
                Position = authoring.enemyMove.StartPos,
                Rotation = quaternion.identity,
                Scale = 1.0f,
            });

            Entity bulletPrefab = GetEntity(authoring.prefabEntity, TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyShooter
            {
                FireRate = authoring.fireRate,
                Prefab = bulletPrefab,
            });

            AddComponent(entity, authoring.hitbox);
        }
    }
}
