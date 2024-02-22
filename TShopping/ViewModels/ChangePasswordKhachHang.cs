using System.ComponentModel.DataAnnotations;

namespace TShopping.ViewModels
{
    public class ChangePasswordKhachHang
    {
        [Required(ErrorMessage = "Mã khách hàng không được để trống")]
        public string MaKh { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string MatKhauCu { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        public string MatKhauMoi { get; set; } = null!;
    }
}
