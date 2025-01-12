using UnityEngine;

public class ConeSearcher : TargetDetector
{
    [SerializeField]
    private float coneAngle = 45.0f;

    protected override void Search()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius);
        foreach (var collider in hitColliders)
        {
            if (IsValidTarget(collider) && IsWithinCone(collider.transform.position))
            {
                currentTarget.Value = collider.gameObject;
                break;
            }
        }
    }

    private bool IsWithinCone(Vector3 position)
    {
        Vector3 directionToTarget = (position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= coneAngle / 2;
    }

    protected override void DrawGizmo()
    {
        Vector3 forward = transform.forward * searchRadius;
        Vector3 leftBoundary = Quaternion.Euler(0, -coneAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, coneAngle / 2, 0) * forward;

        Gizmos.DrawWireSphere(transform.position, searchRadius);
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
    }
}
