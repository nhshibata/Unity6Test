using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "[Anim] play animation state [StateName]", category: "Action", id: "d2c0418586fb1b735e9d61831eed3de8")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<Animator> Anim;
    [SerializeReference] public BlackboardVariable<string> StateName;

    protected override Status OnStart()
    {
        // アニメーションを再生
        Debug.Log($"アニメーションを再生{StateName.Value}");
        Anim.Value.Play(StateName.Value);
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

