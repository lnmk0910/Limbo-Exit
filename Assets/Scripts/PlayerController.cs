// PlayerController.cs
// Di chuyển Player theo hướng body đang nhìn (chuẩn FPS)
// WASD: di chuyển | Shift: chạy nhanh
// Việc xoay body do MouseLook đảm nhiệm → script này chỉ lo di chuyển
// GẮN vào: GameObject Player (Capsule)

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== TỐC ĐỘ ===")]
    public float tocDo = 5f;
    public float tocDoChay = 9f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;   // Rigidbody không tự xoay
    }

    void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S

        // Di chuyển theo hướng body (MouseLook đã xoay body đúng hướng rồi)
        Vector3 huong = (transform.right * inputX + transform.forward * inputZ).normalized;
        float tocDoCuThe = Input.GetKey(KeyCode.LeftShift) ? tocDoChay : tocDo;

        rb.linearVelocity = new Vector3(
            huong.x * tocDoCuThe,
            rb.linearVelocity.y,
            huong.z * tocDoCuThe
        );
    }
}
