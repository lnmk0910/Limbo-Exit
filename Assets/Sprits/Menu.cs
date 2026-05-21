using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Vao scene game
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }
    // Thoat ung dung
    public void QuitGame()
    {
        Application.Quit();
    }
}
