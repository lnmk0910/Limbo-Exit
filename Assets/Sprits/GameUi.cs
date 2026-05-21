using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUi : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    // Cap nhat UI thoi gian moi frame
    private void Update()
    {
        HienThiThoiGianGame();
    }

    // Hien thi thoi gian con lai
    public void HienThiThoiGianGame()
    {
        timeText.SetText(Mathf.FloorToInt(GameManager1.Instance.thoiGianChoPhepVeDich).ToString());
    }

    // Choi lai scene
    public void ChoiLai()
    {
        SceneManager.LoadScene("Game");
    } 
    // Ve menu chinh
    public void VeMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
