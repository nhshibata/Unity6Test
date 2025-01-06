using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Modifier = Unity.Behavior.Modifier;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindTarget", story: "[Self] find target : [isFound]", category: "Flow", id: "11831d4e40f8b8fc97cb15d1ef704670")]
public partial class FindTargetModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Angle = new(60f);
    [SerializeReference] public BlackboardVariable<float> Distance = new(10f);

    // TrueならTargetを見つけたらAbort。Falseなら見失ったらAbort
    [SerializeReference] public BlackboardVariable<bool> isFound = new(true);

    // 対象が見つかった時にセットする
    [SerializeReference] public BlackboardVariable<GameObject> foundTarget;

    private const string TargetTag = "Player";

    protected override Status OnStart()
    {
        if (Distance.Value < 0 || Angle.Value < 0)
            return Status.Failure;

        return !FindTarget() ?
            StartNode(Child) : // 子ノードの処理を開始する。StartNodeはOnStartのみで実行
            Status.Failure;
    }

    protected override Status OnUpdate()
    {
        Debug.Log("Update");
        // 子ノードが有効な間は毎フレーム実行
        return FindTarget() ?
            Status.Success : // 子ノードの結果を取得する
            Status.Failure;
    }

    protected override void OnEnd()
    {
        // 子ノードも含めてツリーから外れた時に呼ばれる
    }

    private bool FindTarget()
    {
        // 自身の参照を取得
        var selfObject = Self.Value;

        // 自身のTransform
        var selfTransform = selfObject.transform;

        // タグを持つオブジェクトを検索
        var targets = GameObject.FindGameObjectsWithTag(TargetTag);
        foreach (var target in targets)
        {
            // ターゲットの位置が無効なら他を探す
            if (IsValidTarget(selfTransform, target.transform) == false)
                continue;

            // 一つでも見つけたら完了
            SetFoundTarget(target);
            return true;
        }

        // 条件に合致するオブジェクトが見つからなかった
        SetFoundTarget(null);
        return false;
    }

    private void SetFoundTarget(GameObject target)
    {
        if (foundTarget != null)
            foundTarget.Value = target;
    }

    private bool IsValidTarget(Transform selfTransform, Transform playerTransform)
    {
        var direction = playerTransform.position - selfTransform.position;
        if (direction.sqrMagnitude > Distance.Value * Distance.Value)
            return false;

        if (Vector3.Angle(selfTransform.forward, direction) > Angle.Value / 2f)
            return false;

        return !NavMesh.Raycast(selfTransform.position, playerTransform.position, out _, NavMesh.AllAreas);
    }
}

