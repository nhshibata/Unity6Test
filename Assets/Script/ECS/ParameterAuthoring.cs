using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Parameter : IComponentData
{
    public float minSpeed;
    public float maxSpeed;
    public float3 areaScale;
    public float areaDistance;
    public float areaForce;
    public float neighborDistance;
    public float neighborAngle;
    public float separationForce;
    public float alignmentForce;
    public float cohesionForce;
}

public class ParameterAuthoring : MonoBehaviour
{
    [Header("Move")]
    public float minSpeed = 2.0f;
    public float maxSpeed = 5.0f;

    [Header("Area")]
    public float3 areaScale = 5.0f;
    public float areaDistance = 3.0f;
    public float areaForce = 1.0f;

    [Header("Neighbors")]
    public float neighborDistance = 1.0f;
    public float neighborFov = 90.0f;
    public float neighborAngle = 0.0f;

    [Header("Separation")]
    public float separationForce = 5.0f;

    [Header("Alignment")]
    public float alignmentForce = 2.0f;

    [Header("Cohesion")]
    public float cohesionForce = 2.0f;
}

public class ParameterBaker : Baker<ParameterAuthoring>
{
    public override void Bake(ParameterAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new Parameter()
        {
            minSpeed = src.minSpeed,
            maxSpeed = src.maxSpeed,
            areaScale = src.areaScale,
            areaDistance = src.areaDistance,
            areaForce = src.areaForce,
            neighborDistance = src.neighborDistance,
            neighborAngle = src.neighborAngle,
            separationForce = src.separationForce,
            alignmentForce = src.alignmentForce,
            cohesionForce = src.cohesionForce,
        });
    }
}