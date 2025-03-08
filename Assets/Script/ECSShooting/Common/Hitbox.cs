using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Hitbox : IComponentData
{
    public float3 Size;
}
