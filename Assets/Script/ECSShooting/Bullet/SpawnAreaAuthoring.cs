using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct SpawnArea : IComponentData
{
    public float3 Center;
    public float3 Extents;
    public bool HasSpawned;
    public BlobAssetReference<SpawnEnemyList> EnemyPrefabs;

    public int Rows;
    public int Columns;
    public float Spacing;
}

public struct SpawnEnemyList
{
    public BlobArray<Entity> Prefabs;
}

public class SpawnAreaAuthoring : MonoBehaviour
{
    [SerializeField]
    private Vector3 center;
    [SerializeField]
    private Vector3 size;
    [SerializeField]
    private GameObject[] enemyPrefabs;
    [SerializeField]
    private int rows = 3;
    [SerializeField]
    private int columns = 3;
    [SerializeField]
    private float spacing = 2.0f;

    class Baker : Baker<SpawnAreaAuthoring>
    {
        public override void Bake(SpawnAreaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            var builder = new BlobBuilder(Unity.Collections.Allocator.Temp);
            ref var enemyList = ref builder.ConstructRoot<SpawnEnemyList>();
            var prefabArray = builder.Allocate(ref enemyList.Prefabs, authoring.enemyPrefabs.Length);

            for (int i = 0; i < authoring.enemyPrefabs.Length; i++)
            {
                prefabArray[i] = GetEntity(authoring.enemyPrefabs[i], TransformUsageFlags.Dynamic);
            }

            var blobAsset = builder.CreateBlobAssetReference<SpawnEnemyList>(Unity.Collections.Allocator.Persistent);
            builder.Dispose();

            AddComponent(entity, new SpawnArea
            {
                Center = authoring.center,
                Extents = authoring.size * 0.5f,
                HasSpawned = false,
                EnemyPrefabs = blobAsset,
                Rows = authoring.rows,
                Columns = authoring.columns,
                Spacing = authoring.spacing
            });
        }
    }
}
