using System.ComponentModel.DataAnnotations;

namespace TShopping.Areas.Admin.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string MaNv { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string MatKhauCu { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        public string MatKhauMoi { get; set; } = null!;
    }
}
