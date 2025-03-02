using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Player : IComponentData
{
    public float Speed;
    public float2 Bounds;
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
    private float speed;   
    [SerializeField]
    private float2 bounds; 
    [SerializeField]
    private float2 move;
    [SerializeField]
    private bool shoot;
    [SerializeField]
    private float fireRate;
    [SerializeField]
    private GameObject prefabEntity;

    public class PlayerBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Player
            {
                Speed = authoring.speed,
                Bounds = authoring.bounds,
            });

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
        }
    }
}
