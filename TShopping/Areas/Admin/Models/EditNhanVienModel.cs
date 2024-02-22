using System.ComponentModel.DataAnnotations;

namespace TShopping.Areas.Admin.Models
{
    public class EditNhanVienModel
    {
        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        public string MaNv { get; set; } = null!;
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; } = null!;
        [Required(ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
    }
}
