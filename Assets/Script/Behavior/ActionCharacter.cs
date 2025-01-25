using UnityEngine;

public class ActionCharacter : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody rb;
    [SerializeField]
    protected string targetName = "target";
    [SerializeField]
    protected float knockBackPower = 1.0f;


    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(targetName))
            return;
        StartKnockBack(other.transform);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(targetName))
            return;
        StartKnockBack(collision.transform);
    }

    protected void StartKnockBack(Transform other)
    {
        Debug.Log($"this:{this.tag} tag:{other.gameObject.tag}");

        rb.angularVelocity = Vector3.zero;

        // 自分の位置と接触してきたオブジェクトの位置とを計算して、距離と方向を出して正規化(速度ベクトルを算出)
        Vector3 distination = (transform.position - other.position).normalized;
        rb.AddForce(distination * knockBackPower, ForceMode.VelocityChange);
    }

}
