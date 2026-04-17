using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUi : MonoBehaviour
{
    public TextMeshProUGUI timeText;

    private void Update()
    {
        HienThiThoiGianGame();
    }

    public void HienThiThoiGianGame()
    {
        timeText.SetText(Mathf.FloorToInt(GameManager1.Instance.thoiGianChoPhepVeDich).ToString());
    }

    public void ChoiLai()
    {
        SceneManager.LoadScene("Game");
    } 
    public void VeMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
