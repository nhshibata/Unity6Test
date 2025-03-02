using Unity.Entities;
using UnityEngine;

public struct School : IComponentData
{
    public Entity prefab;
    public int spawnCount;
    public uint randomSeed;
    public bool initialized;
}

public class SchoolAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public int spawnCount = 100;
    public uint randomSeed = 100;
    public bool initialized;
}

public class SchoolBaker : Baker<SchoolAuthoring>
{
    public override void Bake(SchoolAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var prefab = GetEntity(src.prefab, TransformUsageFlags.Dynamic);

        AddComponent(entity, new School()
        {
            prefab = prefab,
            spawnCount = src.spawnCount,
            randomSeed = src.randomSeed,
            initialized = src.initialized
        });
    }
}