// PlayerController.cs (cập nhật)
// Đọc cấp nâng cấp tốc độ từ PlayerData
// GẮN vào: Player GameObject

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== TỐC ĐỘ CƠ BẢN ===")]
    public float tocDo     = 5f;
    public float tocDoChay = 9f;

    [Header("=== NÂNG CẤP ===")]
    public float bonusTocDoMoiCap = 0.5f;   // +0.5 mỗi cấp nâng

    private Rigidbody rb;
    private float heSoBiome  = 1f;
    private float bonusTocDo = 0f;
    private Animator anim;   // Animator trên Model Mixamo


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Tìm Animator trong các con cháu (Model Mixamo nằm bên trong Player)
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        heSoBiome = BiomeManager.LayHeSoTocDo();

        // Đọc nâng cấp tốc độ
        PlayerData data = SaveSystem.LoadGame();
        bonusTocDo = data.capTocDo * bonusTocDoMoiCap;

        if (bonusTocDo > 0)
            Debug.Log($"⚡ Tốc độ nâng cấp: +{bonusTocDo} (Cấp {data.capTocDo})");
    }

    void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 huong = (transform.right * inputX + transform.forward * inputZ).normalized;
        bool dangChay = Input.GetKey(KeyCode.LeftShift);
        float tocDoCuThe = (dangChay ? tocDoChay : tocDo) + bonusTocDo;
        tocDoCuThe *= heSoBiome;

        rb.linearVelocity = new Vector3(
            huong.x * tocDoCuThe,
            rb.linearVelocity.y,
            huong.z * tocDoCuThe
        );

        // Đồng bộ animation: speed=0 khi đứng, speed=0.5 khi đi, speed=1 khi chạy
        if (anim != null)
        {
            float tocDoThucTe = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            float animSpeed = tocDoThucTe / tocDoChay;  // 0.0 → 1.0
            anim.SetFloat("Speed", animSpeed);
        }
    }
}
