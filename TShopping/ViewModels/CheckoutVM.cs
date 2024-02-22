using System.ComponentModel.DataAnnotations;

namespace TShopping.ViewModels
{
    public class CheckoutVM
    {
        [Display(Name = "Họ tên người đặt")]
        public string? HoTen {  get; set; }
        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string DiaChi { get; set; } = null!;
        [Display(Name = "Số điện thoại người nhận hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [MaxLength(24, ErrorMessage = "{0} tối đa 20 kí tự")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "{0} không đúng định dạng Việt Nam")]
        public string DienThoai { get; set; } = null!;
        public string? GhiChu { get; set; }
        public string CachThanhToan { get; set; } = "Ví điện tử VN Pay";
        public string CachVanChuyen { get; set; } = "Chuyển phát nhanh";
    }
}
