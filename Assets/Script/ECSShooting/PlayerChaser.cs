using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerChaser : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<PlayerData>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            var lt = manager.GetComponentData<LocalTransform>(entity);
            this.transform.position = lt.Position;
            break;
        }

    }
}
