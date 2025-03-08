using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Bullet : IComponentData
{
    public enum BulletType
    {
        Player,
        Enemy
    }

    public float Speed;
    public float Lifetime;
    public int Damage;
    public BulletType type;
}

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField]
    private Bullet bullet;

    public class BulletBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.bullet);
        }
    }
}

