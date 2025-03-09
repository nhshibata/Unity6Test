using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Hitbox : IComponentData
{
    public float3 Size;

    /// <summary>
    /// AABB 判定を行うヘルパー関数
    /// </summary>
    public static bool CheckAABB(float3 pos1, float3 pos2, float3 size)
    {
        return math.abs(pos1.x - pos2.x) < size.x * 0.5f &&
               math.abs(pos1.y - pos2.y) < size.y * 0.5f &&
               math.abs(pos1.z - pos2.z) < size.z * 0.5f;
    }
}
