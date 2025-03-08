using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[Serializable]
public struct SpawnArea : IComponentData
{
    public float3 Center;
    public float3 Extents;
    public bool HasSpawned;
    public Entity EnemyPrefab; 

    public int Rows;
    public int Columns;
    public float Spacing;
    public float OffsetZ;
}

public class SpawnAreaAuthoring : MonoBehaviour
{
    [SerializeField]
    private SpawnArea spawnArea;
    [SerializeField]
    private GameObject enemyPrefab;

    class Baker : Baker<SpawnAreaAuthoring>
    {
        public override void Bake(SpawnAreaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            if (authoring.spawnArea.EnemyPrefab == null)
            {
                UnityEngine.Debug.LogError("Enemy prefab is NULL!");
                return;
            }

            Entity prefabEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic);

            if (prefabEntity == Entity.Null)
            {
                UnityEngine.Debug.LogError("Enemy prefab failed to convert to Entity!");
            }
            else
            {
                UnityEngine.Debug.Log($"Converted enemy prefab to Entity {prefabEntity.Index}");
            }

            authoring.spawnArea.EnemyPrefab = prefabEntity;
            AddComponent(entity, authoring.spawnArea);
        }
    }
}

