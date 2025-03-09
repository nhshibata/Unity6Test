using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct SpawnArea : IComponentData
{
    public float3 Center;
    public float3 Extents;
    public bool HasSpawned;

    public int Rows;
    public int Columns;
    public float Spacing;
    public float OffsetZ;
    public int createNum;
}

public struct SpawnEnemy : IComponentData
{
    public Entity EnemyPrefab;
}

public class SpawnAreaAuthoring : MonoBehaviour
{
    [SerializeField]
    private SpawnArea spawnArea;
    [SerializeField]
    private SpawnEnemy spawnEnemy;
    [SerializeField]
    private GameObject enemyPrefab;

    class Baker : Baker<SpawnAreaAuthoring>
    {
        public override void Bake(SpawnAreaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            Entity prefabEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);

            if (prefabEntity == Entity.Null)
            {
                UnityEngine.Debug.LogError("Enemy prefab failed to convert to Entity!");
            }
            else
            {
                UnityEngine.Debug.Log($"Converted enemy prefab to Entity {prefabEntity.Index}");
            }

            AddComponent(entity, authoring.spawnArea);
            AddComponent(entity, new SpawnEnemy
            {
                EnemyPrefab = prefabEntity,
            });
        }
    }
}

