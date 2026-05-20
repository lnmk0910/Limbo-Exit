// AncientCompassItem.cs
// La Ban Co: Nhan 3 → hien mui ten chi huong Cong Thoat trong 3s
// Tu dong tao UI neu chua co — KHONG can thiet lap trong Inspector
// GAN vao: Player GameObject

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AncientCompassItem : MonoBehaviour
{
    [Header("=== THAM CHIEU (tu dong tim neu de trong) ===")]
    public Transform    exitGate;       // ExitGate — tu tim neu null
    public GameObject   panelLaBan;     // Panel UI — tu tao neu null
    public TMP_Text     txtHuong;       // Text hien huong
    public Image        imgMuiTen;      // Image mui ten xoay

    [Header("=== THOI GIAN ===")]
    public float thoiGianHien = 3f;

    [Header("=== NANG CAP ===")]
    public float bonusThoiGianMoiCap = 1f; // +1s moi cap nang cap La Ban

    private bool dangDung = false;
    private float thoiGianThucTe; // = thoiGianHien + bonus tu nang cap

    void Start()
    {
        // Doc nang cap tu Save
        PlayerData data = SaveSystem.LoadGame();
        thoiGianThucTe = thoiGianHien + (data.capLaBan * bonusThoiGianMoiCap);

        if (panelLaBan != null) panelLaBan.SetActive(false);
    }

    void Update()
    {
        if (!UIManager.DangTrongGame()) return;

        if (Input.GetKeyDown(KeyCode.Alpha3))
            DungLaBan();

        // Cap nhat mui ten xoay huong ve ExitGate
        if (dangDung && exitGate != null && imgMuiTen != null)
        {
            Vector3 huong = exitGate.position - transform.position;
            huong.y = 0;
            float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;

            // Tinh goc tuong doi so voi huong nhin cua Player
            float gocPlayer = transform.eulerAngles.y;
            float gocCuoi = goc - gocPlayer;

            imgMuiTen.transform.rotation = Quaternion.Euler(0f, 0f, -gocCuoi);
        }
    }

    void DungLaBan()
    {
        if (dangDung) return; // Khong dung chong khi dang hien

        if (!ItemSystem.DungLaBan()) return;

        // Tu dong tim ExitGate
        TimExitGate();

        if (exitGate == null)
        {
            Debug.LogWarning("[LB] Khong tim thay ExitGate! La Ban khong hien duoc.");
            return;
        }

        // Tu tao panel UI neu chua co
        if (panelLaBan == null)
            TaoPanelLaBan();

        StartCoroutine(HienLaBan());
    }

    void TimExitGate()
    {
        if (exitGate != null) return;

        // Tim bang ten (do GameManager Instantiate)
        GameObject gate = GameObject.Find("ExitGate(Clone)");

        // Fallback: tim bang component
        if (gate == null)
        {
            ExitGate eg = Object.FindFirstObjectByType<ExitGate>();
            if (eg != null) gate = eg.gameObject;
        }

        if (gate != null)
        {
            exitGate = gate.transform;
            Debug.Log($"[LB] Tim thay ExitGate tai {exitGate.position}");
        }
    }

    IEnumerator HienLaBan()
    {
        dangDung = true;
        if (panelLaBan != null) panelLaBan.SetActive(true);

        Debug.Log($"[LB] La Ban kich hoat! Chi huong Cong Thoat trong {thoiGianThucTe}s (Cap {SaveSystem.LoadGame().capLaBan})");

        // Tinh khoang cach va huong text
        float khoangCach = Vector3.Distance(transform.position, exitGate.position);
        string huongText = LayHuongText(exitGate.position - transform.position);

        // Dem nguoc
        float conLai = thoiGianThucTe;
        while (conLai > 0)
        {
            // Cap nhat thong tin moi frame
            if (exitGate != null)
            {
                khoangCach = Vector3.Distance(transform.position, exitGate.position);
                huongText = LayHuongText(exitGate.position - transform.position);
            }

            if (txtHuong != null)
                txtHuong.text = $"Cổng Thoát: {huongText}\n" +
                                $"Khoảng cách: {khoangCach:F0}m\n" +
                                $"({conLai:F1}s)";

            conLai -= Time.deltaTime;
            yield return null;
        }

        if (panelLaBan != null) panelLaBan.SetActive(false);
        dangDung = false;
        Debug.Log("[LB] La Ban het hieu luc!");
    }

    // Tra ve huong dang chu (Bac/Nam/Dong/Tay)
    string LayHuongText(Vector3 huong)
    {
        huong.y = 0;
        float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;
        if (goc < 0) goc += 360f;

        if (goc >= 337.5f || goc < 22.5f)   return "Bắc";
        if (goc >= 22.5f  && goc < 67.5f)   return "Đông Bắc";
        if (goc >= 67.5f  && goc < 112.5f)  return "Đông";
        if (goc >= 112.5f && goc < 157.5f)  return "Đông Nam";
        if (goc >= 157.5f && goc < 202.5f)  return "Nam";
        if (goc >= 202.5f && goc < 247.5f)  return "Tây Nam";
        if (goc >= 247.5f && goc < 292.5f)  return "Tây";
        return "Tây Bắc";
    }

    // -----------------------------------------------
    // TU TAO PANEL UI (neu chua thiet ke trong Unity)
    // -----------------------------------------------
    void TaoPanelLaBan()
    {
        // Tim Canvas trong scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[LB] Khong tim thay Canvas de tao panel La Ban!");
            return;
        }

        // Tao Panel nen
        panelLaBan = new GameObject("Panel_LaBan_Auto");
        panelLaBan.transform.SetParent(canvas.transform, false);

        RectTransform rt = panelLaBan.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 150f); // Hoi tren giua man hinh
        rt.sizeDelta = new Vector2(300f, 200f);

        Image bg = panelLaBan.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f); // Nen den mo

        // Tao Text huong dan
        GameObject txtObj = new GameObject("Txt_Huong");
        txtObj.transform.SetParent(panelLaBan.transform, false);

        RectTransform rtText = txtObj.AddComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero;
        rtText.anchorMax = Vector2.one;
        rtText.offsetMin = new Vector2(10f, 10f);
        rtText.offsetMax = new Vector2(-10f, -60f);

        txtHuong = txtObj.AddComponent<TextMeshProUGUI>();
        txtHuong.text = "La Ban Co...";
        txtHuong.fontSize = 22;
        txtHuong.color = Color.white;
        txtHuong.alignment = TextAlignmentOptions.Center;

        // Tao Image mui ten
        GameObject arrowObj = new GameObject("Img_MuiTen");
        arrowObj.transform.SetParent(panelLaBan.transform, false);

        RectTransform rtArrow = arrowObj.AddComponent<RectTransform>();
        rtArrow.anchorMin = new Vector2(0.5f, 1f);
        rtArrow.anchorMax = new Vector2(0.5f, 1f);
        rtArrow.pivot     = new Vector2(0.5f, 0.5f);
        rtArrow.anchoredPosition = new Vector2(0f, -30f);
        rtArrow.sizeDelta = new Vector2(50f, 50f);

        imgMuiTen = arrowObj.AddComponent<Image>();
        imgMuiTen.color = Color.yellow;
        // Tao hinh tam giac don gian bang sprite mac dinh
        // (trong Unity se la hinh vuong vang xoay — van hieu duoc huong)

        panelLaBan.SetActive(false);

        Debug.Log("[LB] Da tu dong tao Panel La Ban UI!");
    }
}
