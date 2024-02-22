using System.ComponentModel.DataAnnotations;

namespace TShopping.ViewModels
{
    public class LoginNV
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string MaNv { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string MatKhau { get; set; } = null!;
    }
}
