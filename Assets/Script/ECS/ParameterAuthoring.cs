using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct Parameter : IComponentData
{
    public int type;
    [Header("Move")]
    public float minSpeed;
    public float maxSpeed;
    [Header("Area")]
    public float3 areaScale;
    public float areaDistance;
    public float areaForce;
    [Header("Neighbors")]
    public float neighborDistance;
    public float neighborFov;
    public float neighborAngle;
    [Header("Separation")]
    public float separationForce;
    [Header("Alignment")]
    public float alignmentForce;
    [Header("Cohesion")]
    public float cohesionForce;

    // デフォルトパラメタ
    public static Parameter Default
    {
        get => new Parameter()
        {
            type = 0,
            minSpeed = 2f,
            maxSpeed = 5f,
            areaScale = 5f,
            areaDistance = 3f,
            areaForce = 1f,
            neighborDistance = 1f,
            neighborFov = 90.0f,
            neighborAngle = 90f,
            separationForce = 5f,
            alignmentForce = 5f,
            cohesionForce = 5f,
        };
    }

    public static bool Set(in Parameter newParam)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadWrite<Parameter>());
        var entities = query.ToEntityArray(Allocator.Temp);
        if (entities.Length == 0) 
            return false;

        bool set = false;
        foreach (var entity in entities)
        {
            var param = manager.GetComponentData<Parameter>(entity);
            if (param.type != newParam.type) 
                continue;
            manager.SetComponentData(entity, newParam);
            set = true;
        }
        return set;
    }

    public static bool Get(ref Parameter outParam)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadOnly<Parameter>());
        var parameters = query.ToComponentDataArray<Parameter>(Allocator.Temp);
        if (parameters.Length == 0) 
            return false;

        foreach (var param in parameters)
        {
            if (param.type != outParam.type) 
                continue;
            outParam = param;
            return true;
        }
        return false;
    }
}

public class ParameterAuthoring : MonoBehaviour
{
    public Parameter param = Parameter.Default;
}

public class ParameterBaker : Baker<ParameterAuthoring>
{
    public override void Bake(ParameterAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        // scaleをxyz同一以外で扱う為にPostTransformMatrix を付与する
        AddTransformUsageFlags(entity, TransformUsageFlags.NonUniformScale);
        // 直接セット
        AddComponent(entity, src.param);
    }
}