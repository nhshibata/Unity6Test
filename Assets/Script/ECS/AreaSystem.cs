using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct AreaSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;
    private ComponentLookup<LocalTransform> transformLookUp;
    private ComponentLookup<PostTransformMatrix> postTransformMatrixLookUp;

    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        transformLookUp = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
        postTransformMatrixLookUp = state.GetComponentLookup<PostTransformMatrix>(isReadOnly: true);
    }

    public void OnUpdate(ref SystemState state)
    {
        paramLookUp.Update(ref state);
        transformLookUp.Update(ref state);
        postTransformMatrixLookUp.Update(ref state);

        foreach (var (fish, lt) in SystemAPI.Query<RefRW<Fish>, RefRO<LocalTransform>>())
        {
            var paramEntity = fish.ValueRW.paramEntity;
            var param = paramLookUp[paramEntity];
            var areaLt = transformLookUp[paramEntity];
            var areaPtm = postTransformMatrixLookUp[paramEntity];

            var scale = areaPtm.Value.Scale() * 0.5f;
            var thresh = param.areaDistance;
            var weight = param.areaForce;

            // 箱のローカル座標系に変換する
            // ただし、スケール成分だけはそのまま = ワールド座標系での大きさを使う
            var pos = lt.ValueRO.Position;
            var transformRt = float4x4.TRS(areaLt.Position, areaLt.Rotation, 1.0f);
            pos = math.transform(math.inverse(transformRt), pos);

            // 座標系としてはローカル座標系（大きさはそのままに位置は原点、回転なし）
            var addAccel =
                GetAccelAgainstWall(pos.x - -scale.x, math.right(), thresh, weight) +
                GetAccelAgainstWall(pos.y - -scale.y, math.up(), thresh, weight) +
                GetAccelAgainstWall(pos.z - -scale.z, math.forward(), thresh, weight) +
                GetAccelAgainstWall(+scale.x - pos.x, math.left(), thresh, weight) +
                GetAccelAgainstWall(+scale.y - pos.y, math.down(), thresh, weight) +
                GetAccelAgainstWall(+scale.z - pos.z, math.back(), thresh, weight);

            // 力のかかる方向をワールド座標系
            addAccel = math.rotate(areaLt.Rotation, addAccel);
            fish.ValueRW.acceleration += addAccel;

            Debug.Log($"{paramEntity}:{scale}:");
        }
    }

    float3 GetAccelAgainstWall(float dist, float3 dir, float thresh, float weight)
    {
        if (dist < thresh)
        {
            dist = math.max(dist, 0.01f);
            var a = dist / math.max(thresh, 0.01f);
            return dir * (weight / a);
        }
        return float3.zero;
    }
}
