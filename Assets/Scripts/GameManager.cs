using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Rod rodA;
    public Rod rodB;
    public Rod rodC;

    private Disk selectedDisk = null;
    private Rod selectedRod = null;

    void Start()
    {
        // Đăng ký đĩa ban đầu vào Rod_A (từ lớn đến nhỏ)
        rodA.disks.Clear();
        rodA.disks.Add(GameObject.Find("Disk_5").GetComponent<Disk>());
        rodA.disks.Add(GameObject.Find("Disk_4").GetComponent<Disk>());
        rodA.disks.Add(GameObject.Find("Disk_3").GetComponent<Disk>());
        rodA.disks.Add(GameObject.Find("Disk_2").GetComponent<Disk>());
        rodA.disks.Add(GameObject.Find("Disk_1").GetComponent<Disk>());
    }

    // Gọi khi người chơi click vào Rod
    public void OnRodClicked(Rod clickedRod)
    {
        // Chưa chọn đĩa nào → chọn đĩa trên cùng của cột này
        if (selectedDisk == null)
        {
            Disk top = clickedRod.GetTopDisk();
            if (top == null) return; // cột rỗng, bỏ qua

            selectedDisk = clickedRod.RemoveTopDisk();
            selectedRod = clickedRod;

            // Nâng đĩa lên để thấy được chọn
            Vector3 pos = selectedDisk.transform.position;
            selectedDisk.GetComponent<DiskMover>().MoveTo(new Vector3(pos.x, 3.5f, pos.z));        }
        else
        {
            // Đã có đĩa đang chọn → thử đặt vào cột này
            Disk topOfTarget = clickedRod.GetTopDisk();

            // Kiểm tra luật: không đặt đĩa lớn lên đĩa nhỏ
            if (topOfTarget != null && selectedDisk.size > topOfTarget.size)
            {
                Debug.Log("Không hợp lệ! Đĩa lớn không thể đặt lên đĩa nhỏ.");

                // Trả đĩa về chỗ cũ
                selectedRod.AddDisk(selectedDisk);
                float returnY = selectedRod.GetNextDiskY() - 0.3f; // vừa add xong
                float fixedY = 0.2f + ((selectedRod.disks.Count - 1) * 0.3f);
                selectedDisk.GetComponent<DiskMover>().MoveTo(new Vector3(
                selectedRod.transform.position.x, fixedY, 0));
                selectedDisk = null;
                selectedRod = null;
                return;
            }

            // Hợp lệ → đặt đĩa vào cột đích
            clickedRod.AddDisk(selectedDisk);
            float newY = 0.2f + ((clickedRod.disks.Count - 1) * 0.3f);
            selectedDisk.GetComponent<DiskMover>().MoveTo(new Vector3(
            clickedRod.transform.position.x, newY, 0));

            selectedDisk = null;
            selectedRod = null;

            // Kiểm tra thắng
            CheckWin();
        }
    }

    void CheckWin()
    {
        if (rodC.disks.Count == 5)
        {
            Debug.Log("Bạn đã thắng!");
            WinManager.Instance.ShowWin();
        }
    }
}
