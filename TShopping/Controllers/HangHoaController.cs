using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TShopping.Data;
using TShopping.Helpers;
using TShopping.ViewModels;

namespace TShopping.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly int PageSize = 12;
        private readonly TshoppingContext _context;
        public HangHoaController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? maloai = null, int page = 1)
        {
            var query = _context.HangHoas.Include(hh => hh.MaLoaiNavigation).AsQueryable();
            if(maloai.HasValue)
            {
                query = query.Where(hh => hh.MaLoai == maloai.Value);
            }
            var pageSize = PageSize;
            var total = await query.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if (page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var hangHoas = await query.ToListAsync();
            var result = hangHoas.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(hh => new HangHoaVM
            {
                MaHh = hh.MaHh,
                TenHh = hh.TenHh,
                TenLoai = hh.MaLoaiNavigation.TenLoai,
                DonGia = hh.DonGia ?? 0,
                MoTaNgan = hh.MoTaDonVi,
                Hinh = hh.Hinh ?? ""
            }).ToList();
            var pagingModel = new PagingModel
            {
                currentPage = page,
                countPage = totalPage,
                generateUrl = (p) => Url.Action("Index","HangHoa",new {area = "", page = p, maloai = maloai}) ?? "/"
            };
            ViewBag.pagingModel = pagingModel;
            ViewBag.maLoai = maloai;
            return View(result);
        }
        public async Task<IActionResult> Search(string? search, int page = 1)
        {
            var query = _context.HangHoas.Include(hh => hh.MaLoaiNavigation).AsQueryable();
            if (search != null)
            {
                query = query.Where(hh => hh.TenHh.Contains(search));
            }
            var pageSize = PageSize;
            var total = await query.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if (page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var hangHoas = await query.ToListAsync();
            var result = hangHoas.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(hh => new HangHoaVM
            {
                MaHh = hh.MaHh,
                TenHh = hh.TenHh,
                TenLoai = hh.MaLoaiNavigation.TenLoai,
                DonGia = hh.DonGia ?? 0,
                MoTaNgan = hh.MoTaDonVi,
                Hinh = hh.Hinh ?? ""
            }).ToList();
            var pagingModel = new PagingModel
            {
                currentPage = page,
                countPage = totalPage,
                generateUrl = (p) => Url.Action("Search", "HangHoa", new { area = "", page = p, search = search }) ?? "/"
            };
            ViewBag.pagingModel = pagingModel;
            ViewBag.search = search;
            ViewBag.count = total;
            return View(result);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var hangHoa = await _context.HangHoas.Include(hh => hh.MaLoaiNavigation).FirstOrDefaultAsync(hh => hh.MaHh == id);
            if (hangHoa == null)
            {
                TempData["Message"] = "Không tìm thấy sản phẩm vừa chọn";
                return Redirect("/404");
            }
            var result = new HangHoaVM
            {
                MaHh = hangHoa.MaHh,
                TenHh = hangHoa.TenHh,
                TenLoai = hangHoa.MaLoaiNavigation.TenLoai,
                DonGia = hangHoa.DonGia ?? 0,
                MoTaNgan = hangHoa.MoTaDonVi,
                Hinh = hangHoa.Hinh ?? "",
                ChiTiet = hangHoa.MoTa,
                DiemDanhGia = 5,
                SoLuongTon = 100
            };
            return View(result);
        }
    }
}
