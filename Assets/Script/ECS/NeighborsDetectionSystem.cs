using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// 2 つの Job から使うので別クラスへ分離
[BurstCompile]
internal static class NeighborsDetectionUtil
{
    [BurstCompile]
    public static int GetHash(in int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }
}

[BurstCompile]
public struct NeighborsCleanUpJob : IJob
{
    [DeallocateOnJobCompletion][ReadOnly] 
    public NativeArray<Entity> entities;
    [DeallocateOnJobCompletion][ReadOnly] 
    public NativeArray<Fish> fishes;
    [DeallocateOnJobCompletion][ReadOnly] 
    public NativeArray<LocalTransform> localTransforms;
    public void Execute() { }
}

[BurstCompile]
public struct NeighborsDetectionJob : IJobParallelFor
{
    [WriteOnly] public NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement>.ParallelWriter Neighbors;
    [ReadOnly] public NativeArray<Entity> Entities;
    [ReadOnly] public NativeArray<Fish> Fishes;
    [ReadOnly] public NativeArray<LocalTransform> LocalTransforms;
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    [ReadOnly] public NativeParallelMultiHashMap<int, int> HashMap;
    [ReadOnly] public float CellSize;
    [ReadOnly] public NativeArray<int3> CellOffsets;

    public void Execute(int index)
    {
        var posSelf = LocalTransforms[index].Position;
        var cellSelf = (int3)(posSelf / CellSize);

        var fishSelf = Fishes[index];
        var forwardSelf = math.normalizesafe(fishSelf.velocity);

        var param = ParamLookUp[fishSelf.paramEntity];
        var neighborAngle = math.radians(param.neighborAngle);
        var neighborDist = param.neighborDistance;
        var prodThresh = math.cos(neighborAngle);

        var entitySelf = Entities[index];
        int neighborsCount = 0;
        bool isNeighborCountFull = false;
        int maxNeighborsCount = FishConfig.NeighborsEntityBufferMaxSize;

        for (int offsetIndex = 0; offsetIndex < CellOffsets.Length; ++offsetIndex)
        {
            var cell = cellSelf + CellOffsets[offsetIndex];
            var hashSelf = NeighborsDetectionUtil.GetHash(cell);
            if (!HashMap.TryGetFirstValue(hashSelf, out var j, out var it)) continue;

            do
            {
                var entityTarget = Entities[j];
                if (entitySelf == entityTarget) continue;

                var ltTarget = LocalTransforms[j];
                var posTarget = ltTarget.Position;
                var to = posTarget - posSelf;
                var dist = math.length(to);
                if (dist > neighborDist) continue;

                var dir = to / math.max(dist, 1e-3f);
                var prod = math.dot(dir, forwardSelf);
                if (prod < prodThresh) continue;

                var elem = new NeighborsEntityBufferElement() { entity = entityTarget };
                Neighbors.Add(entitySelf, elem);

                ++neighborsCount;
                isNeighborCountFull = neighborsCount >= maxNeighborsCount;
                if (isNeighborCountFull) break;
            } while (HashMap.TryGetNextValue(out j, ref it));

            if (isNeighborCountFull) break;
        }
    }
}

[BurstCompile]
public partial struct NeighborsWriteJob : IJobEntity
{
    [ReadOnly]
    public NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement> Neighbors;

    // IJobEntity 経由で DynamicBuffer を受け取り
    public void Execute(Entity entity, ref DynamicBuffer<NeighborsEntityBufferElement> buffer)
    {
        // この DynamicBuffer へ DetectionJob の結果を移す
        buffer.Clear();
        if (!Neighbors.TryGetFirstValue(entity, out var elem, out var it)) 
            return;
        do
        {
            buffer.Add(elem);
        } while (Neighbors.TryGetNextValue(out elem, ref it));
    }
}

[BurstCompile]
public struct NeighborsHashMapBuildJob : IJobParallelFor
{
    // ハッシュマップは ParallelWriter で受け取る
    [WriteOnly] public NativeParallelMultiHashMap<int, int>.ParallelWriter HashMap;
    [ReadOnly] public float CellSize;
    [ReadOnly] public NativeArray<LocalTransform> LocalTransforms;

    public void Execute(int index)
    {
        var pos = LocalTransforms[index].Position;
        var cell = (int3)(pos / CellSize);
        var hash = NeighborsDetectionUtil.GetHash(cell);
        HashMap.Add(hash, index);
    }
}

/// <summary>
/// 各インスタンスの視界前方にいる別のインスタンスを見つける
/// </summary>
public partial struct NeighborsDetectionSystem : ISystem
{
    private ComponentLookup<Parameter> paramLookUp;
    private BufferLookup<NeighborsEntityBufferElement> neighborsLookUp;
    private NativeParallelMultiHashMap<int, int> hashMap;
    private EntityQuery fishQuery;
    private EntityQuery paramQuery;
    private NativeArray<int3> cellOffsets;
    // 毎回 SystemAPI.Query ではなく予めクエリをキャッシュしておく
    private EntityQuery query;
    // DetectionJob の結果を一時的に格納するハッシュマップ
    private NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement> neighborMap;

    public int GetHash(int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }

    public void OnCreate(ref SystemState state)
    {
        paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        neighborsLookUp = state.GetBufferLookup<NeighborsEntityBufferElement>(isReadOnly: false);
        hashMap = new NativeParallelMultiHashMap<int, int>(100, Allocator.Persistent);
        fishQuery = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform, NeighborsEntityBufferElement>().Build();
        paramQuery = SystemAPI.QueryBuilder().WithAll<Parameter>().Build();
        neighborMap = new NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement>(100, Allocator.Persistent);

        cellOffsets = new NativeArray<int3>(27, Allocator.Persistent);
        {
            var i = 0;
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    for (int z = -1; z <= 1; ++z)
                    {
                        cellOffsets[i++] = new int3(x, y, z);
                    }
                }
            }
        }
    }

    public void OnDestroy(ref SystemState state)
    {
        if (cellOffsets.IsCreated) 
            cellOffsets.Dispose();
        if (hashMap.IsCreated) 
            hashMap.Dispose();
        if (neighborMap.IsCreated) 
            neighborMap.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // LookUp 更新
        paramLookUp.Update(ref state);
        neighborsLookUp.Update(ref state);

        // バッファは Temp だとすぐ解放されてしまうので TempJob でアロケーション
        // 解放は後で見ますが、スコープ外で解放を行うので using はつけない
        var entities = fishQuery.ToEntityArray(Allocator.TempJob);
        var fishes = fishQuery.ToComponentDataArray<Fish>(Allocator.TempJob);
        var localTransforms = fishQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

        // 最大値を見たいセルサイズ決定は並列にしづらい & 
        // パラメタ数が少なければ旨味がないので、ここで計算してしまう
        using var parameters = paramQuery.ToComponentDataArray<Parameter>(Allocator.Temp);
        float cellSize = 0.1f;
        for (int i = 0; i < parameters.Length; ++i)
        {
            var param = parameters[i];
            cellSize = math.max(cellSize, param.neighborDistance * 0.5f);
        }

        // ハッシュマップのクリアとサイズ変更
        hashMap.Clear();
        int n = fishes.Length;
        if (hashMap.Capacity < n) hashMap.Capacity = n;

        // ハッシュマップ構築ジョブの実行
        var hashMapBuildJob = new NeighborsHashMapBuildJob()
        {
            HashMap = hashMap.AsParallelWriter(), // これが大事
            CellSize = cellSize,
            LocalTransforms = localTransforms,
        };
        state.Dependency = hashMapBuildJob.Schedule(n, 32, state.Dependency);

        // 一時格納バッファのサイズは各個体が見る最大個体数 * Entity の数
        neighborMap.Clear();
        var maxBufferSize = n * FishConfig.NeighborsEntityBufferMaxSize;
        if (neighborMap.Capacity < maxBufferSize) 
            neighborMap.Capacity = maxBufferSize;

        // 近傍探索ジョブの予約
        var detectionJob = new NeighborsDetectionJob()
        {
            Entities = entities,
            Fishes = fishes,
            LocalTransforms = localTransforms,
            ParamLookUp = paramLookUp,
            HashMap = hashMap,
            Neighbors = neighborMap.AsParallelWriter(),
            CellSize = cellSize,
            CellOffsets = cellOffsets,
        };
        state.Dependency = detectionJob.Schedule(n, 16, state.Dependency);

        // 書き込みジョブの予約
        var writeJob = new NeighborsWriteJob()
        {
            Neighbors = neighborMap,
        };
        state.Dependency = writeJob.ScheduleParallel(fishQuery, state.Dependency);

        var cleanUpJob = new NeighborsCleanUpJob()
        {
            entities = entities,
            fishes = fishes,
            localTransforms = localTransforms,
        };
        state.Dependency = cleanUpJob.Schedule(state.Dependency);
    }
}

