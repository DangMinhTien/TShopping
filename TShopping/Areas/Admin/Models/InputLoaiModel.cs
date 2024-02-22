using System.ComponentModel.DataAnnotations;

namespace TShopping.Areas.Admin.Models
{
    public class InputLoaiModel
    {
        [Required(ErrorMessage = "Tên loại không được để trống")]
        public string TenLoai { get; set; } = null!;
        [Required(ErrorMessage = "Mô tả loại không được để trống")]
        public string MoTa { get; set; } = null!;
    }
}
