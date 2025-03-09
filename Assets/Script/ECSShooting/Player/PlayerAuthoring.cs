using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct PlayerData : IComponentData
{
    [HideInInspector]
    public int MaxHp;
    public int Hp;
    public float Speed;
    public float2 Bounds;
    public int Damage;
    public bool IsDead;
    public bool AutoMoveForward; 
    public float ForwardSpeed;

    public PlayerData(int hp, int maxHp, float speed, float2 bounds, int damage, bool isDead, bool autoMoveForward, float forwardSpeed)
    {
        Hp = hp;
        MaxHp = Hp;
        Speed = speed;
        Bounds = bounds;
        Damage = damage;
        IsDead = false;
        AutoMoveForward = autoMoveForward;
        ForwardSpeed = forwardSpeed;
    }
}

public struct PlayerInput : IComponentData
{
    public float2 Move;
    public bool Shoot;
}

public struct PlayerShooter : IComponentData
{
    public float FireRate;
    public float TimeUntilNextShot;
    public Entity Prefab;
}

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private float2 move;
    [SerializeField]
    private bool shoot;
    [SerializeField]
    private float fireRate;
    [SerializeField]
    private GameObject prefabEntity;
    [SerializeField]
    private Hitbox hitbox;

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.playerData);

            AddComponent(entity, new PlayerInput
            {
                Move = authoring.move,
                Shoot = authoring.shoot,
            });

            Entity bulletPrefab = GetEntity(authoring.prefabEntity, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerShooter
            {
                FireRate = authoring.fireRate,
                Prefab = bulletPrefab,
            });

            AddComponent(entity, authoring.hitbox);
        }
    }
}
