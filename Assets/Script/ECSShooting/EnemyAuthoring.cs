using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct EnemyData : IComponentData
{
    public int Hp;
    public int Damage;
}

/// <summary>
/// 敵の移動コンポーネント
/// </summary>
public struct EnemyMove : IComponentData
{
    public float Speed;       
    public float3 StartPos;   
    public float3 EndPos;     
    public bool MovingForward;
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
    private float speed = 2.0f;
    [SerializeField]
    private Vector3 startPos = new Vector3(-3, 0, 0);
    [SerializeField]
    private Vector3 endPos = new Vector3(3, 0, 0);
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

            AddComponent(entity, new EnemyMove
            {
                Speed = authoring.speed,
                StartPos = authoring.startPos,
                EndPos = authoring.endPos,
                MovingForward = true,
            });

            AddComponent(entity, new LocalTransform
            {
                Position = authoring.startPos,
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
