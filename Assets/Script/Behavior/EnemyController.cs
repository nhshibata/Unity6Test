using Unity.Behavior;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private BehaviorGraphAgent agent;

    void Awake()
    {
        // 初期化の確認
        EnemyMode mode;
        float speed;
        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.CurrentMode, out mode);
        agent.BlackboardReference.GetVariableValue(EnemyBlackboardConstants.Speed, out speed);

        Debug.Log($"speed:{speed} mode:{mode}");
    }

}
