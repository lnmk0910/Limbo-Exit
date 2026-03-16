using UnityEngine;

public class DiskMover : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool isMoving = false;
    public float moveSpeed = 5f;

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            // Khi đến gần đích thì dừng
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        targetPosition = destination;
        isMoving = true;
    }
}
