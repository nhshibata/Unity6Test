using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SaveDistance", story: "Check distance between [Self] and [Target] and save it to [Distance]", category: "Action", id: "8ad517ee4d7b38f1c177c5d2e661ed10")]
public partial class SaveDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Distance;

    protected override Status OnStart()
    {
        if(Target.Value == null)
            return Status.Success;

        Distance.Value = Vector3.Distance(Self.Value.transform.position, Target.Value.transform.position);
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null)
            return Status.Success;

        Distance.Value = Vector3.Distance(Target.Value.transform.position, Self.Value.transform.position);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

