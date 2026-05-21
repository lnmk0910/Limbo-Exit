// ExitButton.cs — Thoát game
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // Thoat game (hoac dung play mode trong Editor)
    public void OnExitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
