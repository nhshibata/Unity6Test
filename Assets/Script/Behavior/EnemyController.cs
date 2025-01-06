using Unity.Behavior;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private BehaviorGraphAgent agent;
    [SerializeField]
    private bool isMove = true;
    [SerializeField]
    private float moveSpeed = 2.0f;

    void Awake()
    {
        // 初期化の確認
        bool move;
        float speed;
        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Move, out move);
        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Speed, out speed);

        Debug.Log($"speed:{speed} move:{move}");

        agent.BlackboardReference.SetVariableValue(EnemyBlackboardConstants.Move, isMove);
        agent.BlackboardReference.SetVariableValue(EnemyBlackboardConstants.Speed, moveSpeed);

        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Move, out move);
        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Speed, out speed);
        Debug.Log($"speed:{speed} move:{move}");
    }

    // Update is called once per frame
    void Update()
    {
        //bool move;
        //float speed;

        //agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Move, out move);
        //agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Speed, out speed);
        //Debug.Log($"speed:{speed} move:{move}");

        agent.BlackboardReference.SetVariableValue(EnemyBlackboardConstants.Move, isMove);
        agent.BlackboardReference.SetVariableValue(EnemyBlackboardConstants.Speed, moveSpeed);
    }
}
