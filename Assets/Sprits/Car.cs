using UnityEngine;

public class Car : MonoBehaviour
{
    [SerializeField] private float tocDoXe = 100f;
    [SerializeField] private float lucReXe = 100f;
    [SerializeField] private float lucPhanh = 5f;
    [SerializeField] private GameObject hieuUngPhanh;

    private float dauVaoDiChuyen;
    private float dauVaoRe;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        dauVaoDiChuyen = Input.GetAxis("Vertical");
        dauVaoRe = Input.GetAxis("Horizontal");

        DiChuyenXe();
        ReXe();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            PhanhXe();
        }
    }

    public void DiChuyenXe()
    {
        rb.AddRelativeForce(Vector3.forward * dauVaoDiChuyen * tocDoXe);

        if (hieuUngPhanh != null)
            hieuUngPhanh.SetActive(false);
    }

    public void ReXe()
    {
        Quaternion re = Quaternion.Euler(Vector3.up * dauVaoRe * lucReXe * Time.deltaTime);
        rb.MoveRotation(rb.rotation * re);
    }

    public void PhanhXe()
    {
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.deltaTime * lucPhanh);

        if (hieuUngPhanh != null)
            hieuUngPhanh.SetActive(true);
    }
}