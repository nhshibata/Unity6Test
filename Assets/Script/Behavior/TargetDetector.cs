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


    protected virtual void Update()
    {
        if (Time.timeScale >= 1.0f)
        {
            Search();
        }
    }

    protected abstract void Search(); // 派生クラスで探索ロジックを実装

    protected bool IsValidTarget(Collider collider)
    {
        return collider.CompareTag(targetTag);
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = currentTarget.Value ? Color.red : Color.green;
        DrawGizmo();
    }

    protected abstract void DrawGizmo(); // 派生クラスでGizmoの描画を実装
}
