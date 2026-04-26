// MouseLook.cs
// Góc nhìn thứ nhất CHUẨN FPS (như mắt người):
// - Camera chỉ xoay LÊN/XUỐNG (trục X) → không thấy trên đầu tường
// - Player Body xoay TRÁI/PHẢI (trục Y) → full 360° như người thật
// - Camera đặt ngang tầm mắt → không nhìn từ trên xuống
// GẮN vào: PlayerCamera (con của Player)

using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("=== ĐỘ NHẠY CHUỘT ===")]
    public float doNhay = 120f;

    [Header("=== GIỚI HẠN NHÌN LÊN/XUỐNG ===")]
    public float gocDocMin = -60f;   // Nhìn xuống tối đa
    public float gocDocMax =  60f;   // Nhìn lên tối đa

    private float gocDoc = 0f;       // Pitch tích lũy (chỉ cho trục X)
    private Transform playerBody;    // Thân Player → xoay trái/phải

    void Start()
    {
        // Lấy Player body (cha của Camera)
        playerBody = transform.parent;

        // Khóa chuột
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // LateUpdate → chạy sau physics, camera mượt không giật
    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * doNhay * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * doNhay * Time.deltaTime;

        // --- Xoay TRÁI/PHẢI: áp lên Player Body (full 360°) ---
        playerBody.Rotate(Vector3.up * mouseX);

        // --- Xoay LÊN/XUỐNG: áp lên Camera, giới hạn góc ---
        gocDoc -= mouseY;
        gocDoc = Mathf.Clamp(gocDoc, gocDocMin, gocDocMax);

        // Camera chỉ xoay trục X (pitch), không ảnh hưởng trục Y
        transform.localRotation = Quaternion.Euler(gocDoc, 0f, 0f);

        // --- Toggle chuột ---
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
