using Unity.Entities;
using UnityEngine;

public struct Bullet : IComponentData
{
    public float Speed;
    public float Lifetime;
    public int Damage;
}

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    private float lifetime = 5f;

    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet
            {
                Speed = authoring.speed,
                Lifetime = authoring.lifetime,
                Damage = 1,
            });
        }
    }
}

