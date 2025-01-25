using UnityEngine;

public class LineSearcher : TargetDetector
{
    [SerializeField]
    private float lineLength = 10.0f;
    [SerializeField]
    private float lineWidth = 1.0f;

    protected override void Search()
    {
        RaycastHit[] hits = Physics.BoxCastAll(transform.position, new Vector3(lineWidth / 2, lineWidth / 2, lineLength / 2), transform.forward);
        foreach (var hit in hits)
        {
            if (IsValidTarget(hit.collider))
            {
                FoundSetting(hit.collider.gameObject);
                return;
            }
        }
    }

    protected override void DrawGizmo()
    {
        Gizmos.DrawWireCube(transform.position + transform.forward * lineLength / 2, new Vector3(lineWidth, lineWidth, lineLength));
    }
}
