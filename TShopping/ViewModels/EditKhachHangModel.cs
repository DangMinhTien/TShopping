using System.ComponentModel.DataAnnotations;
using TShopping.Validation;

namespace TShopping.ViewModels
{
    public class EditKhachHangModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string MaKh { get; set; } = null!;
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(50, ErrorMessage = "Họ tên tối đa 20 kí tự")]
        public string HoTen { get; set; } = null!;
        [Required(ErrorMessage = "giới tính không được để trống")]
        public bool? GioiTinh { get; set; }
        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime? NgaySinh { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
        public string? DiaChi { get; set; }
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? DienThoai { get; set; }
    }
}
