// LoginUI.cs — Màn hình Đăng nhập / Đăng ký (tự tạo UI bằng code)
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("=== SCENE ===")]
    public string tenSceneMenu = "MainMenu";

    private Canvas canvas;
    private GameObject panelLogin;
    private GameObject panelDangKy;
    private TMP_InputField inputTenDN;
    private TMP_InputField inputMatKhauDN;
    private TMP_InputField inputTenDK;
    private TMP_InputField inputMatKhauDK;
    private TMP_InputField inputXacNhanMK;
    private TMP_Text txtLoiDN;
    private TMP_Text txtLoiDK;
    private TMP_Text txtTieuDe;

    // Khoi tao UI va focus input dau tien
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 1f;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        TaoGiaoDien();
        HienDangNhap();
        StartCoroutine(FocusSauMotFrame());
    }

    // Delay 1 frame de EventSystem co san
    System.Collections.IEnumerator FocusSauMotFrame()
    {
        yield return null;
        if (inputTenDN != null)
        {
            EventSystem.current.SetSelectedGameObject(inputTenDN.gameObject);
            inputTenDN.ActivateInputField();
        }
    }

    // Xu ly Tab/Enter cho login va dang ky
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (panelLogin.activeSelf)
            {
                ChonInput(inputTenDN.isFocused ? inputMatKhauDN : inputTenDN);
            }
            else if (panelDangKy.activeSelf)
            {
                if (shift)
                {
                    if (inputXacNhanMK.isFocused) ChonInput(inputMatKhauDK);
                    else if (inputMatKhauDK.isFocused) ChonInput(inputTenDK);
                    else ChonInput(inputXacNhanMK);
                }
                else
                {
                    if (inputTenDK.isFocused) ChonInput(inputMatKhauDK);
                    else if (inputMatKhauDK.isFocused) ChonInput(inputXacNhanMK);
                    else ChonInput(inputTenDK);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (panelLogin.activeSelf) OnClick_DangNhap();
            else if (panelDangKy.activeSelf) OnClick_DangKy();
        }
    }

    // Chuyen focus sang input duoc chon
    void ChonInput(TMP_InputField input)
    {
        EventSystem.current.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
    }

    // Tao toan bo giao dien login/dang ky bang code
    void TaoGiaoDien()
    {
        GameObject canvasObj = new GameObject("Canvas_Login");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bg = TaoPanel(canvasObj.transform, "BG", new Color(0.05f, 0.05f, 0.08f, 1f));
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;

        // Panel Đăng nhập
        panelLogin = TaoPanel(canvasObj.transform, "Panel_Login", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        DatKichThuocPanel(panelLogin, 500, 480);

        txtTieuDe = TaoText(panelLogin.transform, "LIMBO EXIT", 40, new Vector2(0, 180), Color.white);
        TaoText(panelLogin.transform, "LỐI THOÁT HƯ VÔ", 16, new Vector2(0, 148), new Color(0.5f, 0.5f, 0.6f));

        TaoText(panelLogin.transform, "Tên đăng nhập:", 18, new Vector2(0, 100), new Color(0.7f, 0.7f, 0.8f));
        inputTenDN = TaoInputField(panelLogin.transform, "Nhập tên...", new Vector2(0, 65), false);

        TaoText(panelLogin.transform, "Mật khẩu:", 18, new Vector2(0, 20), new Color(0.7f, 0.7f, 0.8f));
        inputMatKhauDN = TaoInputField(panelLogin.transform, "Nhập mật khẩu...", new Vector2(0, -15), true);

        TaoButton(panelLogin.transform, "ĐĂNG NHẬP", new Vector2(0, -80), new Color(0.2f, 0.6f, 0.3f), OnClick_DangNhap);
        TaoButton(panelLogin.transform, "TẠO TÀI KHOẢN MỚI", new Vector2(0, -135), new Color(0.3f, 0.3f, 0.5f), OnClick_HienDangKy);

        txtLoiDN = TaoText(panelLogin.transform, "", 16, new Vector2(0, -185), new Color(1f, 0.3f, 0.3f));
        TaoText(panelLogin.transform, "[Tab] Chuyển ô  |  [Enter] Xác nhận", 14, new Vector2(0, -215), new Color(0.4f, 0.4f, 0.5f));

        // Panel Đăng ký
        panelDangKy = TaoPanel(canvasObj.transform, "Panel_DangKy", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        DatKichThuocPanel(panelDangKy, 500, 560);

        TaoText(panelDangKy.transform, "TẠO TÀI KHOẢN", 36, new Vector2(0, 220), Color.white);

        TaoText(panelDangKy.transform, "Tên đăng nhập (3-20 ký tự):", 18, new Vector2(0, 165), new Color(0.7f, 0.7f, 0.8f));
        inputTenDK = TaoInputField(panelDangKy.transform, "Nhập tên...", new Vector2(0, 130), false);

        TaoText(panelDangKy.transform, "Mật khẩu (tối thiểu 4 ký tự):", 18, new Vector2(0, 80), new Color(0.7f, 0.7f, 0.8f));
        inputMatKhauDK = TaoInputField(panelDangKy.transform, "Nhập mật khẩu...", new Vector2(0, 45), true);

        TaoText(panelDangKy.transform, "Xác nhận mật khẩu:", 18, new Vector2(0, -5), new Color(0.7f, 0.7f, 0.8f));
        inputXacNhanMK = TaoInputField(panelDangKy.transform, "Nhập lại mật khẩu...", new Vector2(0, -40), true);

        TaoButton(panelDangKy.transform, "ĐĂNG KÝ", new Vector2(0, -110), new Color(0.2f, 0.6f, 0.3f), OnClick_DangKy);
        TaoButton(panelDangKy.transform, "QUAY LẠI", new Vector2(0, -165), new Color(0.4f, 0.3f, 0.3f), OnClick_HienDangNhap);

        txtLoiDK = TaoText(panelDangKy.transform, "", 16, new Vector2(0, -215), new Color(1f, 0.3f, 0.3f));
        TaoText(panelDangKy.transform, "[Tab] Chuyển ô  |  [Shift+Tab] Lùi  |  [Enter] Xác nhận", 14, new Vector2(0, -250), new Color(0.4f, 0.4f, 0.5f));
    }

    // Xu ly dang nhap va vao menu
    void OnClick_DangNhap()
    {
        string ten = inputTenDN.text.Trim();
        string mk  = inputMatKhauDN.text;
        string loi = AccountManager.DangNhap(ten, mk);
        if (string.IsNullOrEmpty(loi)) SceneManager.LoadScene(tenSceneMenu);
        else txtLoiDN.text = loi;
    }

    // Xu ly dang ky tai khoan moi
    void OnClick_DangKy()
    {
        string ten = inputTenDK.text.Trim();
        string mk  = inputMatKhauDK.text;
        string xn  = inputXacNhanMK.text;

        if (mk != xn) { txtLoiDK.text = "Mật khẩu xác nhận không khớp!"; return; }

        string loi = AccountManager.DangKy(ten, mk);
        if (string.IsNullOrEmpty(loi)) SceneManager.LoadScene(tenSceneMenu);
        else txtLoiDK.text = loi;
    }

    // Hien panel dang nhap va clear loi
    void HienDangNhap()
    {
        panelLogin.SetActive(true);
        panelDangKy.SetActive(false);
        txtLoiDN.text = "";
        StartCoroutine(FocusInput(inputTenDN));
    }

    void OnClick_HienDangNhap() => HienDangNhap();

    // Hien panel dang ky va clear loi
    void OnClick_HienDangKy()
    {
        panelLogin.SetActive(false);
        panelDangKy.SetActive(true);
        txtLoiDK.text = "";
        StartCoroutine(FocusInput(inputTenDK));
    }

    // Focus input sau 1 frame de UI cap nhat
    System.Collections.IEnumerator FocusInput(TMP_InputField input)
    {
        yield return null;
        if (input != null)
        {
            EventSystem.current.SetSelectedGameObject(input.gameObject);
            input.ActivateInputField();
        }
    }

    // === Helper tạo UI ===

    // Tao panel co mau nen
    GameObject TaoPanel(Transform parent, string ten, Color mau)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = mau; img.raycastTarget = true;
        return go;
    }

    // Dat kich thuoc va can giua panel
    void DatKichThuocPanel(GameObject panel, float w, float h)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
    }

    // Tao text trung tam voi kich thuoc co dinh
    TMP_Text TaoText(Transform parent, string noidung, int size, Vector2 pos, Color mau)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = noidung; txt.fontSize = size; txt.color = mau;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableWordWrapping = false; txt.raycastTarget = false;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(450, 35); rt.anchoredPosition = pos;
        return txt;
    }

    // Tao InputField (co the la mat khau)
    TMP_InputField TaoInputField(Transform parent, string placeholder, Vector2 pos, bool laMatKhau)
    {
        GameObject go = new GameObject("InputField");
        go.transform.SetParent(parent, false);
        Image bgImg = go.AddComponent<Image>();
        bgImg.color = new Color(0.18f, 0.18f, 0.25f, 1f); bgImg.raycastTarget = true;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380, 42); rt.anchoredPosition = pos;

        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(go.transform, false);
        RectTransform taRect = textArea.AddComponent<RectTransform>();
        taRect.anchorMin = Vector2.zero; taRect.anchorMax = Vector2.one;
        taRect.offsetMin = new Vector2(10, 2); taRect.offsetMax = new Vector2(-10, -2);
        textArea.AddComponent<RectMask2D>();

        GameObject phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI phTxt = phGo.AddComponent<TextMeshProUGUI>();
        phTxt.text = placeholder; phTxt.fontSize = 18;
        phTxt.fontStyle = FontStyles.Italic; phTxt.color = new Color(0.4f, 0.4f, 0.5f);
        phTxt.enableWordWrapping = false; phTxt.raycastTarget = false;
        RectTransform phRect = phGo.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero; phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero; phRect.offsetMax = Vector2.zero;

        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI inputTxt = txtGo.AddComponent<TextMeshProUGUI>();
        inputTxt.fontSize = 18; inputTxt.color = Color.white;
        inputTxt.enableWordWrapping = false; inputTxt.raycastTarget = false;
        RectTransform txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;

        GameObject caretGo = new GameObject("Caret");
        caretGo.transform.SetParent(textArea.transform, false);
        RectTransform caretRect = caretGo.AddComponent<RectTransform>();
        caretRect.anchorMin = Vector2.zero; caretRect.anchorMax = Vector2.one;
        caretRect.offsetMin = Vector2.zero; caretRect.offsetMax = Vector2.zero;

        TMP_InputField input = go.AddComponent<TMP_InputField>();
        input.textViewport = taRect; input.textComponent = inputTxt;
        input.placeholder = phTxt; input.fontAsset = inputTxt.font;
        input.caretColor = Color.white;
        input.selectionColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        input.caretWidth = 2; input.customCaretColor = true;

        if (laMatKhau) { input.contentType = TMP_InputField.ContentType.Password; input.asteriskChar = '*'; }

        Navigation nav = input.navigation;
        nav.mode = Navigation.Mode.Automatic;
        input.navigation = nav;

        return input;
    }

    // Tao Button va gan action
    void TaoButton(Transform parent, string text, Vector2 pos, Color mau, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_" + text);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>(); img.color = mau; img.raycastTarget = true;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(action);
        ColorBlock cb = btn.colors; cb.highlightedColor = mau * 1.3f; cb.pressedColor = mau * 0.7f; btn.colors = cb;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(380, 48); rt.anchoredPosition = pos;

        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
        txt.text = text; txt.fontSize = 22; txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center; txt.raycastTarget = false;
        RectTransform txtRect = txtGo.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero; txtRect.offsetMax = Vector2.zero;
    }
}
