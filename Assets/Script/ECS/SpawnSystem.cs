using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))] // 他のシステムより優先的に更新
public partial struct SpawnSystem : ISystem
{
    public void OnCreste(ref SystemState systemState)
    {
        systemState.RequireForUpdate<School>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (school, param, entity) in
            SystemAPI.Query<
                RefRW<School>,
                RefRO<Parameter>>().WithEntityAccess()) // Entity も取得
        {
            if (school.ValueRO.initialized)
                continue;
            Create(ref state, school.ValueRO, param.ValueRO, entity);
            school.ValueRW.initialized = true;
        }
    }

    void Create(
        ref SystemState state,
        in School school,
        in Parameter param,
        Entity groupEntity)
    {
        var entities = state.EntityManager.Instantiate(
            school.prefab,
            school.spawnCount,
            Allocator.Temp);

        var random = new Random(school.randomSeed);

        foreach (var entity in entities)
        {
            var fish = SystemAPI.GetComponentRW<Fish>(entity);
            fish.ValueRW.paramEntity = groupEntity;

            var lt = SystemAPI.GetComponentRW<LocalTransform>(entity);

            var pos = random.NextFloat3() - 0.5f;
            pos *= 5.0f; // 適当な範囲
            lt.ValueRW.Position = pos;

            var dir = random.NextFloat3Direction();
            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotation(dir, up);

            fish.ValueRW.velocity = dir * 2.0f;
            fish.ValueRW.acceleration = 0f;

        }
    }

}
