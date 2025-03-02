using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugViewer : MonoBehaviour
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
            ComponentType.ReadOnly<Parameter>(),
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<PostTransformMatrix>());
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var param = manager.GetComponentData<Parameter>(entity);
            var lt = manager.GetComponentData<LocalTransform>(entity);
            var ptm = manager.GetComponentData<PostTransformMatrix>(entity);
            Gizmos.color = (Vector4)(new float4(param.debugAreaColor, 1.0f));
            Gizmos.matrix = Matrix4x4.TRS(lt.Position, lt.Rotation, ptm.Value.Scale());
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;

#if UNITY_EDITOR
            Handles.Label(lt.Position, "Boids");
#endif
        }
    }
}
