// MazeCell.cs
// Đại diện cho MỘT Ô (Cell) trong lưới mê cung
// Mỗi ô biết mình còn tường ở hướng nào và đã được thăm chưa

public class MazeCell
{
    // -----------------------------------------------
    // TƯỜNG: true = còn tường, false = đã phá (lối đi)
    // -----------------------------------------------
    public bool tuongTren;    // Tường phía Trên  (North)
    public bool tuongDuoi;    // Tường phía Dưới  (South)
    public bool tuongTrai;    // Tường phía Trái  (West)
    public bool tuongPhai;    // Tường phía Phải  (East)

    // -----------------------------------------------
    // TRẠNG THÁI
    // -----------------------------------------------
    public bool daThăm;       // Đã được thuật toán đi qua chưa?

    // Vị trí ô trong lưới (dùng để debug)
    public int col;           // Cột (trục X)
    public int row;           // Hàng (trục Z)

    // -----------------------------------------------
    // CONSTRUCTOR: mặc định 4 tường, chưa thăm
    // -----------------------------------------------
    public MazeCell(int col, int row)
    {
        this.col = col;
        this.row = row;

        // Khởi tạo: ô nào cũng có đủ 4 tường
        tuongTren  = true;
        tuongDuoi  = true;
        tuongTrai  = true;
        tuongPhai  = true;

        daThăm = false;
    }
}
