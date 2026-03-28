// NavMeshBaker.cs - GẮN VÀO: NavMeshSurface
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    public NavMeshSurface surface;
    void Start()
    {
        // Tự động bake sau khi maze render xong (delay nhỏ)
        Invoke(nameof(Bake), 0.5f);
    }
    void Bake() => surface?.BuildNavMesh();
}
