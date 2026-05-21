// MouseLook.cs — Góc nhìn thứ nhất chuẩn FPS
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("=== ĐỘ NHẠY CHUỘT ===")]
    public float doNhay = 120f;

    [Header("=== GIỚI HẠN NHÌN LÊN/XUỐNG ===")]
    public float gocDocMin = -60f;
    public float gocDocMax =  60f;

    private float gocDoc = 0f;
    private Transform playerBody;

    // Khoi tao cursor lock va lay player body
    void Start()
    {
        playerBody = transform.parent;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Xu ly quay ngang/dung va toggle cursor
    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * doNhay * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * doNhay * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);

        gocDoc -= mouseY;
        gocDoc = Mathf.Clamp(gocDoc, gocDocMin, gocDocMax);
        transform.localRotation = Quaternion.Euler(gocDoc, 0f, 0f);

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
