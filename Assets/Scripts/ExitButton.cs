using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void OnExitClicked()
    {
        Debug.Log("Thoát game!");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
