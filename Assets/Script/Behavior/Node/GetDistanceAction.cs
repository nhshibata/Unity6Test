using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using static UnityEngine.GraphicsBuffer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetDistance", story: "[Self] [Character] [saveDistance]", category: "Action", id: "c1d4d8c43827750b8daef2193cae5247")]
public partial class GetDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Character;
    [SerializeReference] public BlackboardVariable<float> SaveDistance;

    protected override Status OnStart()
    {
        float distance = Vector3.Distance(Self.Value.transform.position, Character.Value.transform.position);
        Debug.Log($"Target {distance}");
        SaveDistance.Value = distance;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

