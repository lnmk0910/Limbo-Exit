using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    public string mainMapSceneName = "Game";

    // Quay ve scene chinh sau khi ket thuc minigame
    public void ReturnToMainMap()
    {
        SceneManager.LoadScene(mainMapSceneName);
    }
}
