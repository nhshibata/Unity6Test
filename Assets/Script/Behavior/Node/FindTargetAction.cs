using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindTarget", story: "Check if [targetDetector] has a [Target]", category: "Action", id: "63389d85260a8851e8e0b22b5279e9ce")]
public partial class FindTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<TargetDetector> targetDetector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        Target.Value = targetDetector.Value.CurrentTarget.Value;

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        Target.Value = targetDetector.Value.CurrentTarget.Value;

        return Target.Value != null ?
            Status.Success :
            Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

