// AncientCompassItem.cs — La Bàn Cổ: nhấn 3 để hiện mũi tên chỉ hướng Cổng Thoát
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AncientCompassItem : MonoBehaviour
{
    [Header("=== THAM CHIẾU ===")]
    public Transform  exitGate;
    public GameObject panelLaBan;
    public TMP_Text   txtHuong;
    public Image      imgMuiTen;

    [Header("=== THỜI GIAN ===")]
    public float thoiGianHien = 3f;

    [Header("=== NÂNG CẤP ===")]
    public float bonusThoiGianMoiCap = 1f;

    private bool dangDung = false;
    private float thoiGianThucTe;

    // Tinh thoi gian hien theo cap la ban
    void Start()
    {
        PlayerData data = SaveSystem.LoadGame();
        thoiGianThucTe = thoiGianHien + (data.capLaBan * bonusThoiGianMoiCap);
        if (panelLaBan != null) panelLaBan.SetActive(false);
    }

    // Lang nghe phim tat va xoay mui ten khi dang dung
    void Update()
    {
        if (!UIManager.DangTrongGame()) return;
        if (Input.GetKeyDown(KeyCode.Alpha3)) DungLaBan();

        if (dangDung && exitGate != null && imgMuiTen != null)
        {
            Vector3 huong = exitGate.position - transform.position;
            huong.y = 0;
            float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;
            float gocPlayer = transform.eulerAngles.y;
            imgMuiTen.transform.rotation = Quaternion.Euler(0f, 0f, -(goc - gocPlayer));
        }
    }

    // Dung la ban va mo UI
    void DungLaBan()
    {
        if (dangDung) return;
        if (!ItemSystem.DungLaBan()) return;

        TimExitGate();
        if (exitGate == null) return;
        if (panelLaBan == null) TaoPanelLaBan();

        StartCoroutine(HienLaBan());
    }

    // Tim ExitGate trong scene
    void TimExitGate()
    {
        if (exitGate != null) return;
        GameObject gate = GameObject.Find("ExitGate(Clone)");
        if (gate == null)
        {
            ExitGate eg = Object.FindFirstObjectByType<ExitGate>();
            if (eg != null) gate = eg.gameObject;
        }
        if (gate != null) exitGate = gate.transform;
    }

    // Hien UI la ban trong mot khoang thoi gian
    IEnumerator HienLaBan()
    {
        dangDung = true;
        if (panelLaBan != null) panelLaBan.SetActive(true);

        float conLai = thoiGianThucTe;
        while (conLai > 0)
        {
            if (exitGate != null)
            {
                float khoangCach = Vector3.Distance(transform.position, exitGate.position);
                string huongText = LayHuongText(exitGate.position - transform.position);
                if (txtHuong != null)
                    txtHuong.text = $"Cổng Thoát: {huongText}\nKhoảng cách: {khoangCach:F0}m\n({conLai:F1}s)";
            }
            conLai -= Time.deltaTime;
            yield return null;
        }

        if (panelLaBan != null) panelLaBan.SetActive(false);
        dangDung = false;
    }

    // Doi vector huong thanh text huong (Bac/Dong/Nam/Tay)
    string LayHuongText(Vector3 huong)
    {
        huong.y = 0;
        float goc = Mathf.Atan2(huong.x, huong.z) * Mathf.Rad2Deg;
        if (goc < 0) goc += 360f;

        if (goc >= 337.5f || goc < 22.5f)  return "Bắc";
        if (goc >= 22.5f  && goc < 67.5f)  return "Đông Bắc";
        if (goc >= 67.5f  && goc < 112.5f) return "Đông";
        if (goc >= 112.5f && goc < 157.5f) return "Đông Nam";
        if (goc >= 157.5f && goc < 202.5f) return "Nam";
        if (goc >= 202.5f && goc < 247.5f) return "Tây Nam";
        if (goc >= 247.5f && goc < 292.5f) return "Tây";
        return "Tây Bắc";
    }

    // Tao panel la ban neu chua co tren canvas
    void TaoPanelLaBan()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        panelLaBan = new GameObject("Panel_LaBan_Auto");
        panelLaBan.transform.SetParent(canvas.transform, false);
        RectTransform rt = panelLaBan.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 150f); rt.sizeDelta = new Vector2(300f, 200f);
        Image bg = panelLaBan.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject txtObj = new GameObject("Txt_Huong");
        txtObj.transform.SetParent(panelLaBan.transform, false);
        RectTransform rtText = txtObj.AddComponent<RectTransform>();
        rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one;
        rtText.offsetMin = new Vector2(10f, 10f); rtText.offsetMax = new Vector2(-10f, -60f);
        txtHuong = txtObj.AddComponent<TextMeshProUGUI>();
        txtHuong.text = "La Bàn Cổ..."; txtHuong.fontSize = 22;
        txtHuong.color = Color.white; txtHuong.alignment = TextAlignmentOptions.Center;

        GameObject arrowObj = new GameObject("Img_MuiTen");
        arrowObj.transform.SetParent(panelLaBan.transform, false);
        RectTransform rtArrow = arrowObj.AddComponent<RectTransform>();
        rtArrow.anchorMin = new Vector2(0.5f, 1f); rtArrow.anchorMax = new Vector2(0.5f, 1f);
        rtArrow.pivot = new Vector2(0.5f, 0.5f);
        rtArrow.anchoredPosition = new Vector2(0f, -30f); rtArrow.sizeDelta = new Vector2(50f, 50f);
        imgMuiTen = arrowObj.AddComponent<Image>();
        imgMuiTen.color = Color.yellow;

        panelLaBan.SetActive(false);
    }
}
