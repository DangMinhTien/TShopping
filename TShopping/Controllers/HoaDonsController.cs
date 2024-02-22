using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TShopping.Data;
using TShopping.Helpers;

namespace TShopping.Controllers
{
    [Authorize(Roles = MyRole.Customer)]
    public class HoaDonsController : Controller
    {
        private readonly TshoppingContext _context;

        public HoaDonsController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }
            var userId = HttpContext.User.FindFirst(MySetting.Claim_CustomerId)?.Value;
            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == userId);
            if (khachHang == null)
            {
                TempData["error"] = "Không tìm thấy thông tin khách hàng đăng nhập";
                return Redirect("/404");
            }
            var hoaDons = await _context.HoaDons
                                .Include(hd => hd.MaTrangThaiNavigation)
                                .Include(hd => hd.MaKhNavigation)
                                .Where(hd => hd.MaKh == khachHang.MaKh)
                                .OrderByDescending(hd => hd.MaHd)
                                .ToListAsync();
            return View(hoaDons);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int MaHd)
        {
            var tongTien = _context.ChiTietHds.Where(ct => ct.MaHd == MaHd).Sum(ct => ct.SoLuong * ct.DonGia);
            var tongSoLuong = _context.ChiTietHds.Where(ct => ct.MaHd == MaHd).Sum(ct => ct.SoLuong);
            var hoadon = await _context.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Include(hd => hd.MaKhNavigation)
                .Where(hd => hd.MaHd == MaHd)
                .Select(hd => new
                {
                    mahd = hd.MaHd,
                    nguoidat = hd.MaKhNavigation.HoTen ?? "",
                    nguoinhan = hd.HoTen ?? hd.MaKhNavigation.HoTen,
                    ngaydat = hd.NgayDat.ToString("dd-MM-yyyy"),
                    cachthanhtoan = hd.CachThanhToan ?? "không có",
                    cachvanchuyen = hd.CachVanChuyen ?? "không có",
                    ghichu = hd.GhiChu,
                    diachi = hd.DiaChi,
                    dienthoai = hd.DienThoai,
                    trangthai = hd.MaTrangThaiNavigation.TenTrangThai,
                    tongsoluong = tongSoLuong,
                    tongtien = string.Format("{0:N0}", tongTien) + " đ"
                }).FirstOrDefaultAsync();
            if (hoadon == null)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần xem", errorDev = "HoaDon is not found" });
            }
            var chiTietHoaDon = _context.ChiTietHds
                .Include(ct => ct.MaHhNavigation)
                .Where(ct => ct.MaHd == MaHd)
                .Select(ct => new
                {
                    tenhang = ct.MaHhNavigation.TenHh,
                    soluong = ct.SoLuong,
                    dongia = string.Format("{0:N0}", ct.DonGia) + " đ",
                    thanhtien = string.Format("{0:N0}", ct.SoLuong * ct.DonGia) + " đ",
                    Anh = Url.Content($"~/Hinh/HangHoa/{ct.MaHhNavigation.Hinh}")
                });
            return Json(new { hoadon = hoadon, chitiethoadon = chiTietHoaDon });
        }
        [HttpPost]
        public async Task<IActionResult> Abort(int MaHd)
        {
            var hoadon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaHd == MaHd);
            if (hoadon == null)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần sửa trạng thái", errorDev = "HoaDon is not found" });
            }
            if (hoadon.MaTrangThai == 3)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không thể hủy đơn hàng đã giao thành công", errorDev = "HoaDon don't abort because it is success" });
            }
            if (hoadon.MaTrangThai == -1)
            {
                return BadRequest(new { isvalid = true, errorClient = "Đơn hàng này đã được hủy rồi", errorDev = "HoaDon was abort" });
            }
            try
            {
                hoadon.MaTrangThai = -1;
                _context.HoaDons.Update(hoadon);
                await _context.SaveChangesAsync();
                TempData["success"] = "Hủy đơn hàng thành công";
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không thể hủy đơn hàng này", errorDev = ex.Message });
            }
        }
    }
}
