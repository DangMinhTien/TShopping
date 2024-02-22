namespace TShopping.ViewModels
{
    public class CartItem
    {
        public int MaHh { get; set; }
        public string TenHh { get; set; } = null!;
        public string Hinh { get; set; } = null!;
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => DonGia * SoLuong;
    }
}
