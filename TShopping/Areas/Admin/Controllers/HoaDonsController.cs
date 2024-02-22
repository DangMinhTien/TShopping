using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TShopping.Data;
using TShopping.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = MyRole.Employee)]
    public class HoaDonsController : Controller
    {
        private readonly TshoppingContext _context;

        public HoaDonsController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var query = _context.HoaDons
                .Include(hd => hd.MaTrangThaiNavigation)
                .Include(hd => hd.MaKhNavigation)
                .AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(hd => hd.MaHd.ToString().Contains(search));
            }
            var pageSize = TShopping.Helpers.MySetting.PageSize;
            var total = await query.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if (page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var hoadons = await query.ToListAsync();
            var result = hoadons.Skip((page - 1) * pageSize).Take(pageSize);
            var pagingmodel = new PagingModel()
            {
                countPage = totalPage,
                currentPage = (int)page,
                generateUrl = (p) => Url.Action("Index", "HoaDons", new { area = "Admin", page = p, search = search }) ?? "/"
            };
            ViewBag.Paging = pagingmodel;
            ViewBag.search = search;
            var trangThais = await _context.TrangThais.Where(tt => tt.MaTrangThai != -1).ToListAsync();
            ViewBag.TrangThai = trangThais;
            return View(result);
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
            if(hoadon == null)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần xem", errorDev = "HoaDon is not found" });
            }
            var chiTietHoaDon = _context.ChiTietHds
                .Include(ct => ct.MaHhNavigation)
                .Where(ct => ct.MaHd == MaHd)
                .Select(ct => new
                {
                    mahang = ct.MaHhNavigation.MaHh,
                    tenhang = ct.MaHhNavigation.TenHh,
                    soluong = ct.SoLuong,
                    dongia = string.Format("{0:N0}", ct.DonGia) + " đ",
                    thanhtien = string.Format("{0:N0}", ct.SoLuong * ct.DonGia) + " đ",
                    Anh = Url.Content($"~/Hinh/HangHoa/{ct.MaHhNavigation.Hinh}")
                });
            return Json(new {hoadon = hoadon, chitiethoadon = chiTietHoaDon});
        }
        [HttpPost]
        public async Task<IActionResult> Update(int MaHd, int MaTrangThai)
        {
            var hoadon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaHd == MaHd);
            if (hoadon == null)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần sửa trạng thái", errorDev = "HoaDon is not found" });
            }
            try
            {
                hoadon.MaTrangThai = MaTrangThai;
                _context.HoaDons.Update(hoadon);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { isvalid = true, errorClient = "Cập nhật trạng thái hóa đơn không thành công", errorDev = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Abort(int MaHd)
        {
            var hoadon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaHd == MaHd);
            if (hoadon == null)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần sửa trạng thái", errorDev = "HoaDon is not found" });
            }
            if(hoadon.MaTrangThai == 3)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không thể hủy đơn hàng đã giao thành công", errorDev = "HoaDon don't abort because it is success" });
            }
            try
            {
                hoadon.MaTrangThai = -1;
                _context.HoaDons.Update(hoadon);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { isvalid = true, errorClient = "Không tìm thấy hóa đơn cần sửa trạng thái", errorDev = ex.Message });
            }
        }
    }
}
