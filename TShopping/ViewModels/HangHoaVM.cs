namespace TShopping.ViewModels
{
    public class HangHoaVM
    {
        public int MaHh { get; set; }
        public string TenHh { get; set; } = null!;
        public double? DonGia { get; set; }
        public string? MoTaNgan { get; set; }
        public string TenLoai { get; set; } = null!;
        public string Hinh { get; set; } = null!;
        public string? ChiTiet { get; set; }
        public int? DiemDanhGia { get; set; }
        public int? SoLuongTon { get; set; }

    }
}
