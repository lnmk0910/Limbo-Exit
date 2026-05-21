// MazeCell.cs — Đại diện cho 1 ô trong lưới mê cung
public class MazeCell
{
    public bool tuongTren;
    public bool tuongDuoi;
    public bool tuongTrai;
    public bool tuongPhai;
    public bool daThăm;
    public int col;
    public int row;

    // Khoi tao o me cung voi day du tuong
    public MazeCell(int col, int row)
    {
        this.col = col;
        this.row = row;
        tuongTren = true;
        tuongDuoi = true;
        tuongTrai = true;
        tuongPhai = true;
        daThăm = false;
    }
}
