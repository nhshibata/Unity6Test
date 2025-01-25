using UnityEngine;

public class CircleSearcher : TargetDetector
{
    protected override void Search()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius);
        foreach (var collider in hitColliders)
        {
            if (IsValidTarget(collider))
            {
                FoundSetting(collider.gameObject);
                return;
            }
        }
    }

    protected override void DrawGizmo()
    {
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
