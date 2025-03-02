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
    public bool area = false;

    void OnDrawGizmos()
    {
        if (area)
            DrawAreas();
    }

    void DrawAreas()
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
            Gizmos.color = (Vector4)(new float4(1, 0, 1, 1.0f));
            Gizmos.matrix = Matrix4x4.TRS(lt.Position, lt.Rotation, hitbox.Size);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;

#if UNITY_EDITOR
            Handles.Label(lt.Position, "Shooting");
#endif
        }
    }
}
