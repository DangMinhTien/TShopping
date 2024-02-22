using System.ComponentModel.DataAnnotations;
using TShopping.Validation;

namespace TShopping.Areas.Admin.Models
{
    public class InputHangHoa
    {
        [Required(ErrorMessage = "Tên hàng hóa phải nhập")]
        public string TenHh { get; set; } = null!;
        [Required(ErrorMessage = "Loại hàng phải chọn")]
        public int MaLoai { get; set; }
        [Required(ErrorMessage = "Mô tả đơn vị phải nhập")]
        public string MoTaDonVi { get; set; } = null!;
        [Required(ErrorMessage = "Đơn giá phải nhập")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        public double? DonGia { get; set; }
        [Required(ErrorMessage = "Ngày sản xuất phải nhập")]
        public DateTime? NgaySX { get; set; }
        [Required(ErrorMessage = "Giảm giá phải nhập")]
        [Range(0, double.MaxValue, ErrorMessage = "Giảm giá phải lớn hơn 0")]
        public float? GiamGia { get; set; }
        [Required(ErrorMessage = "Mô tả phải nhập")]
        public string MoTa { get; set; } = null!;
        [Required(ErrorMessage = "Nhà cung cấp phải chọn")]
        public string MaNCC { get; set; } = null!;
        [ChkFileExtension(Extensions = "png,jpg,jpeg,gif", ErrorMessage = "File ảnh không đúng định dạng")]
        public IFormFile? Hinh { get; set; }
    }
}
