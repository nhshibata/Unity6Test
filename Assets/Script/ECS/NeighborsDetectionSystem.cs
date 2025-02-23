using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// 各インスタンスの視界前方にいる別のインスタンスを見つける
/// </summary>
public partial struct NeighborsDetectionSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;

    private BufferLookup<NeighborsEntityBufferElement> neighborsLookUp;

    // 毎回 SystemAPI.Query ではなく予めクエリをキャッシュしておく
    private EntityQuery query;

    // グリッドのセルの設定
    private float _cellsize;
    private NativeArray<int3> _cellOffsets;


    public int GetHash(int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }


    public void OnCreate(ref SystemState state)
    {
        // LookUp 生成
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        neighborsLookUp = state.GetBufferLookup<NeighborsEntityBufferElement>(isReadOnly: false);
        // クエリを生成
        query = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform>().Build();
        _cellsize = 0.5f;

        // 3 次元グリッドの周囲 27 セルのオフセット
        _cellOffsets = new NativeArray<int3>(27, Allocator.Persistent);
        {
            var i = 0;
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    for (int z = -1; z <= 1; ++z)
                    {
                        _cellOffsets[i++] = new int3(x, y, z);
                    }
                }
            }
        }
    }

    public void OnDestroy(ref SystemState state)
    {
        if (_cellOffsets.IsCreated) 
            _cellOffsets.Dispose();
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

        // ハッシュマップはキーがハッシュ値、
        // バリューがそこに含まれているインスタンスのインデックス
        int n = fishes.Length;
        using var hashMap = new NativeParallelMultiHashMap<int, int>(n, Allocator.Temp);

        // セルのサイズは設定された NeighborsDistance パラメタの中で
        // もっとも大きいものの半分サイズとする
        // 大きすぎるとエンティティの数が多くなってしまい、
        // 逆に小さすぎると十分な範囲が見れないため
        float nextCellSize = 0.1f;

        // 全てのインスタンスの位置をハッシュ値に変換してハッシュマップに格納
        // 加えて上記のグリッドサイズ決めもここで行う（次フレームで使う）
        for (int i = 0; i < n; ++i)
        {
            var pos = localTransforms[i].Position;
            var cell = (int3)(pos / _cellsize);
            var hash = GetHash(cell);
            hashMap.Add(hash, i);

            var fish = fishes[i];
            var paramEntity = fish.paramEntity;
            var param = paramLookUp[paramEntity];
            nextCellSize = math.max(nextCellSize, param.neighborDistance * 0.5f);
        }

        // 全てのインスタンスを見る
        for (int i = 0; i < n; ++i)
        {
            // 種々の自身のパラメタを計算
            var pos0 = localTransforms[i].Position;
            var cell0 = (int3)(pos0 / _cellsize);

            var fish0 = fishes[i];
            var fwd0 = math.normalizesafe(fish0.velocity);

            var param = paramLookUp[fish0.paramEntity];
            var neighborAngle = math.radians(param.neighborAngle);
            var neighborDist = param.neighborDistance;
            var prodThresh = math.cos(neighborAngle);

            var entity0 = entities[i];
            var neighbors0 = neighborsLookUp[entity0];
            neighbors0.Clear();

            // 隣り合うセルを見ていく
            for (int offsetIndex = 0; offsetIndex < _cellOffsets.Length; ++offsetIndex)
            {
                // 隣り合うセルのハッシュを計算
                var hash0 = GetHash(cell0 + _cellOffsets[offsetIndex]);

                // ハッシュ値からそのセルに含まれている個体のインデックスを取得
                // TryGetFirstValue -> TryGetNextValue でイテレーション
                // 含まれていない場合はスキップ
                if (!hashMap.TryGetFirstValue(hash0, out var j, out var it)) 
                    continue;

                // 含まれている分だけ TriyGetNextValue で見ていく
                do
                {
                    // 内部の判定は以前と全く同じ
                    var entity1 = entities[j];
                    if (entity0 == entity1) 
                        continue;

                    var lt1 = localTransforms[j];
                    var pos1 = lt1.Position;
                    var to = pos1 - pos0;
                    var dist = math.length(to);
                    if (dist > neighborDist) 
                        continue;

                    var dir = to / math.max(dist, 1e-3f);
                    var prod = math.dot(dir, fwd0);
                    if (prod < prodThresh) 
                        continue;

                    var elem = new NeighborsEntityBufferElement() { entity = entity1 };
                    neighbors0.Add(elem);
                    if (neighbors0.Length >= neighbors0.Capacity) 
                        break;
                } while (hashMap.TryGetNextValue(out j, ref it));
            }
        }

        // 次フレームでは自動計算されたセルサイズを使う
        _cellsize = nextCellSize;
    }
}