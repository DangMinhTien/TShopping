using System.ComponentModel.DataAnnotations;

namespace TShopping.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "{0} không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "{0} không được để trống")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;
    }
}
