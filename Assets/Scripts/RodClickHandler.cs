using UnityEngine;

public class RodClickHandler : MonoBehaviour
{
    private GameManager gameManager;
    private Rod rod;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        rod = GetComponent<Rod>();
    }

    void OnMouseDown()
    {
        gameManager.OnRodClicked(rod);
    }
}
