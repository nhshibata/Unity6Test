using R3;
using UnityEngine;

public abstract class TargetDetector : MonoBehaviour
{
    [SerializeField]
    protected string targetTag = "Target";
    [SerializeField]
    protected float searchRadius = 5.0f;

    protected ReactiveProperty<GameObject> currentTarget = new ReactiveProperty<GameObject>(null);
    public ReactiveProperty<GameObject> CurrentTarget { get => currentTarget; set => currentTarget = value; }

    protected ReactiveProperty<bool> isFound = new ReactiveProperty<bool>(false);
    protected ReactiveProperty<bool> IsFound { get => isFound; set => isFound = value; }


    protected virtual void Update()
    {
        if (Time.timeScale >= 1.0f)
        {
            isFound.Value = false;
            Search();
        }
    }

    /// <summary>
    /// 派生クラスで探索ロジックを実装
    /// </summary>
    protected abstract void Search(); 

    protected bool IsValidTarget(Collider collider)
    {
        return collider.CompareTag(targetTag);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = isFound.Value ? Color.red : Color.green;
        DrawGizmo();
    }

    /// <summary>
    /// 派生クラスでGizmoの描画を実装
    /// </summary>
    protected abstract void DrawGizmo(); 

    protected virtual void FoundSetting(GameObject obj)
    {
        isFound.Value = true;
        currentTarget.Value = obj;
    }
}
