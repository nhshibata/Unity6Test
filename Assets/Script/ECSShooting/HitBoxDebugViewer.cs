using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitBoxDebugViewer : MonoBehaviour
{
    [SerializeField]
    private bool isHitbox = false;
    [SerializeField]
    private bool isArea = false;

    void OnDrawGizmos()
    {
        if (isArea)
            DrawAreas();
        if(isHitbox)
            DrawHitbox();
    }

    private void DrawHitbox()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<Hitbox>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var hitbox = manager.GetComponentData<Hitbox>(entity);
            var lt = manager.GetComponentData<LocalTransform>(entity);

            Gizmos.color = new Color(1, 0, 1, 1.0f);
            Gizmos.matrix = Matrix4x4.TRS(lt.Position, Quaternion.identity, hitbox.Size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;

#if UNITY_EDITOR
            Handles.Label(lt.Position, "Hitbox");
#endif
        }
    }

    private void DrawAreas()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<SpawnArea>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var area = manager.GetComponentData<SpawnArea>(entity);
            var lt = manager.GetComponentData<LocalTransform>(entity);
            Gizmos.color = (Vector4)(new float4(0, 0, 1, 1.0f));
            Gizmos.matrix = Matrix4x4.TRS(lt.Position, lt.Rotation, area.Extents);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;

#if UNITY_EDITOR
            Handles.Label(lt.Position, "SpawnArea");
#endif
        }
    }
}
