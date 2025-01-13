using UnityEngine;

public class PlayerController : ActionCharacter
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float moveSpeed = 5.0f; // 移動速度
    [SerializeField]
    private float rotationSpeed = 10.0f; // 回転速度


    void Update()
    {
        // カメラの前方向を取得（XZ 平面のみ）
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // 上下成分を無視
        cameraForward.Normalize();

        // カメラの右方向を取得（XZ 平面のみ）
        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // 入力による移動方向の計算
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) 
            moveDirection += cameraForward; 
        if (Input.GetKey(KeyCode.S)) 
            moveDirection -= cameraForward; 
        if (Input.GetKey(KeyCode.A)) 
            moveDirection -= cameraRight;   
        if (Input.GetKey(KeyCode.D)) 
            moveDirection += cameraRight;   

        // 移動処理
        if (moveDirection != Vector3.zero)
        {
            // 移動方向を正規化して速度を掛ける
            transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

            // プレイヤーの向きを移動方向に合わせる
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            animator.SetInteger("mode", 1);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            animator.SetInteger("mode", 0);
        }
    }
}