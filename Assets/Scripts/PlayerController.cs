// PlayerController.cs — Di chuyển FPS: đi, chạy, animation, bước chân
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== TỐC ĐỘ CƠ BẢN ===")]
    public float tocDo     = 5f;
    public float tocDoChay = 9f;

    [Header("=== NÂNG CẤP ===")]
    public float bonusTocDoMoiCap = 0.5f;

    private Rigidbody rb;
    private float heSoBiome  = 1f;
    private float bonusTocDo = 0f;
    private Animator anim;
    private int biomeHienTai = 0;

    // Khoi tao Rigidbody va Animator
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        anim = GetComponentInChildren<Animator>();
    }

    // Nap he so biome va bonus toc do tu save
    void Start()
    {
        heSoBiome = BiomeManager.LayHeSoTocDo();

        PlayerData data = SaveSystem.LoadGame();
        bonusTocDo = data.capTocDo * bonusTocDoMoiCap;

        biomeHienTai = 0;
        if (data.biomeSequence != null && data.biomeSequence.Length > 0)
        {
            int idx = Mathf.Clamp((data.mapHienTai - 1) % data.biomeSequence.Length, 0, data.biomeSequence.Length - 1);
            biomeHienTai = data.biomeSequence[idx];
        }
    }

    // Doc input va cap nhat van toc + animation
    void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 huong = (transform.right * inputX + transform.forward * inputZ).normalized;
        bool dangChay = Input.GetKey(KeyCode.LeftShift);
        float tocDoCuThe = (dangChay ? tocDoChay : tocDo) + bonusTocDo;
        tocDoCuThe *= heSoBiome;

        rb.linearVelocity = new Vector3(huong.x * tocDoCuThe, rb.linearVelocity.y, huong.z * tocDoCuThe);

        if (anim != null)
        {
            float tocDoThucTe = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
            anim.SetFloat("Speed", tocDoThucTe / tocDoChay);

            if (tocDoThucTe > 0.5f) AudioManager.BatBuocChan(biomeHienTai);
            else AudioManager.TatBuocChan();
        }
    }
}
