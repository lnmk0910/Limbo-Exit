using System.Collections.Generic;
using UnityEngine;

public class Rod : MonoBehaviour
{
    public List<Disk> disks = new List<Disk>(); // danh sách đĩa trên cột này

    // Lấy đĩa trên cùng
    public Disk GetTopDisk()
    {
        if (disks.Count == 0) return null;
        return disks[disks.Count - 1];
    }

    // Thêm đĩa vào cột
    public void AddDisk(Disk disk)
    {
        disks.Add(disk);
    }

    // Lấy và xóa đĩa trên cùng khỏi cột
    public Disk RemoveTopDisk()
    {
        if (disks.Count == 0) return null;
        Disk top = disks[disks.Count - 1];
        disks.RemoveAt(disks.Count - 1);
        return top;
    }

    // Tính vị trí Y để đặt đĩa tiếp theo
    public float GetNextDiskY()
    {
        return -0.1f + (disks.Count * 0.3f);
    }
}
