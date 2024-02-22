using System.ComponentModel.DataAnnotations;
using TShopping.Validation;

namespace TShopping.Areas.Admin.Models
{
    public class InputNhaCungCapModel
    {
        [Required(ErrorMessage = "Tên công ty không được để trống")]
        public string TenCongTy { get; set; } = null!;
        [Required(ErrorMessage = "Người liên lạc không được để trống")]
        public string NguoiLienLac { get; set; } = null!;
        [Required(ErrorMessage = "Điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Điện thoại không đúng định dạng")]
        public string DienThoai { get; set; } = null!;
        [Required(ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DiaChi { get; set; } = null!;
        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string MoTa { get; set; } = null!;
        [ChkFileExtension(Extensions = "png,jpg,jpeg,gif", ErrorMessage = "File ảnh không đúng định dạng")]
        public IFormFile? Logo { get; set; }
    }
}
