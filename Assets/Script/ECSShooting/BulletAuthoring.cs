using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Bullet : IComponentData
{
    public float Speed;
    public float3 Direction;
    public float Lifetime;
}

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float lifetime = 5f;
    [SerializeField]
    private Vector3 direction = Vector3.forward;

    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet
            {
                Speed = authoring.speed,
                Direction = authoring.direction,
                Lifetime = authoring.lifetime
            });
        }
    }
}

