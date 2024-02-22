using System.ComponentModel.DataAnnotations;
using TShopping.Validation;

namespace TShopping.ViewModels
{
    public class RegisterCustomerVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [MaxLength(20, ErrorMessage = "{0} tối đa 20 kí tự")]
        public string MaKh { get; set; } = null!;
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [DataType(DataType.Password, ErrorMessage = "{0} không đúng định dạng")]
        public string MatKhau { get; set; } = null!;
        [Required(ErrorMessage = "{0} không được để trống")]
        [MaxLength(50, ErrorMessage = "{0} tối đa 20 kí tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = null!;
        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public bool GioiTinh { get; set; }
        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public DateTime NgaySinh { get; set; }
        [MaxLength(60, ErrorMessage = "{0} tối đa 20 kí tự")]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
        [MaxLength(24, ErrorMessage = "{0} tối đa 20 kí tự")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "{0} không đúng định dạng Việt Nam")]
        [Display(Name = "Số điện thoại")]
        public string? DienThoai { get; set; }
        [Required(ErrorMessage = "{0} không được để trống")]
        [MaxLength(50, ErrorMessage = "{0} tối đa 20 kí tự")]
        [EmailAddress(ErrorMessage = "{0} phải là địa chỉ email")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = null!;
        [Display(Name = "Hình ảnh đại diện")]
        [ChkFileExtension(Extensions = "png,jpg,jpeg,gif", ErrorMessage = "File {0} không đúng định dạng")]
        public IFormFile? Anh { get; set; }
    }
}
