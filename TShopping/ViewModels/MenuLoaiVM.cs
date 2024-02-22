namespace TShopping.ViewModels
{
    public class MenuLoaiVM
    {
        public int MaLoai { get; set; }
        public string TenLoai { get; set; } = null!;
        public int SoLuong { get; set; }
        public bool IsActive { get; set; }
    }
}
