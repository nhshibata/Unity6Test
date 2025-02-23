using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 各インスタンスの視界前方にいる別のインスタンスを見つける
/// </summary>
public partial struct NeighborsDetectionSystem : ISystem
{
    // パラメタ用の LookUp
    private ComponentLookup<Parameter> paramLookUp;

    // DynamicBuffer をエンティティ引きするための LookUp
    private BufferLookup<NeighborsEntityBufferElement> neighborsLookUp;

    // 毎回 SystemAPI.Query ではなく予めクエリをキャッシュしておく
    private EntityQuery query;


    public void OnCreate(ref SystemState state)
    {
        // LookUp 生成
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        neighborsLookUp = state.GetBufferLookup<NeighborsEntityBufferElement>(isReadOnly: false);

        // クエリを生成
        query = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform>().Build();
    }

    public void OnUpdate(ref SystemState state)
    {
        // LookUp 更新
        paramLookUp.Update(ref state);
        neighborsLookUp.Update(ref state);

        // すべて Dispose をスコープから抜けたら行うために using をつけている
        // 二重ループを回すために、クエリからエンティティやコンポーネントの配列を取得
        using var entities = query.ToEntityArray(Allocator.Temp);
        using var localTransforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        using var fishes = query.ToComponentDataArray<Fish>(Allocator.Temp);

        // ループは for 文で回してインデックスでアクセス
        for (int i = 0; i < entities.Length; ++i)
        {
            var entity0 = entities[i];

            // DynamicBuffer はエンティティ添字アクセスで取得可能
            // バッファはクリアしておく
            var neighbors0 = neighborsLookUp[entity0];
            neighbors0.Clear();

            var fish0 = fishes[i];
            var param = paramLookUp[fish0.paramEntity];

            var neighborAngle = math.radians(param.neighborAngle);
            var neighborDist = param.neighborDistance;
            var prodThresh = math.cos(neighborAngle);

            var lt0 = localTransforms[i];
            var pos0 = lt0.Position;
            var fwd0 = math.normalizesafe(fish0.velocity);

            // 二重ループで総当り
            for (int j = 0; j < entities.Length; ++j)
            {
                if (i == j) 
                    continue;

                // 相手との距離を見る
                var lt1 = localTransforms[j];
                var pos1 = lt1.Position;
                var to = pos1 - pos0;
                var dist = math.length(to);
                if (dist > neighborDist) 
                    continue;

                // 視界に入っているかをチェック
                var dir = to / math.max(dist, 1e-3f);
                var prod = math.dot(dir, fwd0);
                if (prod < prodThresh) 
                    continue;

                // バッファに追加
                var entity1 = entities[j];
                var elem = new NeighborsEntityBufferElement() { entity = entity1 };
                neighbors0.Add(elem);

                // インライン展開から外れないようにキャパに到達したら終わり
                if (neighbors0.Length == neighbors0.Capacity) 
                    break;
            }
        }
    }
}