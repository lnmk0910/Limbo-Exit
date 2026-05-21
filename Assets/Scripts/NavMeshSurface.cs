// NavMeshBaker.cs — Tự động bake NavMesh sau khi maze render xong
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface surface;

    // Delay bake de dam bao maze da render xong
    void Start()
    {
        Invoke(nameof(Bake), 0.5f);
    }

    // Thuc hien bake NavMesh
    void Bake() => surface?.BuildNavMesh();
}
