using UnityEngine;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    public string mainMapSceneName = "Game";

    public void ReturnToMainMap()
    {
        SceneManager.LoadScene(mainMapSceneName);
    }
}