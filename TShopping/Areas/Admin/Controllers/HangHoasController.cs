using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TShopping.Data;
using TShopping.Areas.Admin.Models;
using TShopping.Helpers;
using TShopping.ViewModels;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Authorization;

namespace TShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = MyRole.Employee)]
    public class HangHoasController : Controller
    {
        private readonly TshoppingContext _context;

        public HangHoasController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var hangHoas = _context.HangHoas.Include(hh => hh.MaLoaiNavigation)
                .Include(hh => hh.MaNccNavigation).AsQueryable();
            if(!string.IsNullOrEmpty(search))
            {
                hangHoas = hangHoas.Where(hh => hh.TenHh.Contains(search));
            }
            var pageSize = TShopping.Helpers.MySetting.PageSize;
            var total = await hangHoas.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if(page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var result = hangHoas.ToList().Skip((page - 1) * pageSize).Take(pageSize);
            var pagingmodel = new PagingModel()
            {
                countPage = totalPage,
                currentPage = (int)page,
                generateUrl = (p) => Url.Action("Index", "HangHoas", new { area = "Admin", page = p , search = search}) ?? "/"
            };
            ViewBag.Paging = pagingmodel;
            ViewBag.Loai = _context.Loais.ToList();
            ViewBag.NhaCungCap = _context.NhaCungCaps.ToList();
            ViewBag.search = search;
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(InputHangHoa hanghoa)
        {
            if(hanghoa.Hinh == null)
            {
                ModelState.AddModelError("FileHinh", "Phải chọn hình ảnh");
            }
            if (!string.IsNullOrEmpty(hanghoa.TenHh))
            {
                if (_context.HangHoas.Any(hh => hh.TenHh == hanghoa.TenHh))
                    ModelState.AddModelError("Slug", "Tên hàng hóa bị trùng");
            }
            if(ModelState.IsValid)
            {
                HangHoa hh = new HangHoa();
                if (hanghoa.Hinh != null)
                    hh.Hinh = TShopppingUtil.UploadPhoto(hanghoa.Hinh, "HangHoa");
                hh.TenHh = hanghoa.TenHh;
                hh.TenAlias = Generation.GenerateSlug(hanghoa.TenHh);
                hh.MaLoai = hanghoa.MaLoai;
                hh.MoTaDonVi = hanghoa.MoTaDonVi;
                hh.DonGia = hanghoa.DonGia;
                hh.NgaySx = hanghoa.NgaySX ?? DateTime.Now;
                hh.GiamGia = hanghoa.GiamGia ?? 0;
                hh.SoLanXem = 0;
                hh.MoTa = hanghoa.MoTa;
                hh.MaNcc = hanghoa.MaNCC;
                try
                {
                    _context.HangHoas.Add(hh);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch(Exception ex)
                {
                    return BadRequest(new {isvalid = true, errorCLient = "Lỗi không thêm được hàng hóa", errorDev = ex.Message});
                }
            }
            List<string> error = new List<string>();
            foreach(var value in ModelState.Values)
            {
                foreach(var err in  value.Errors) 
                {
                    error.Add(err.ErrorMessage);
                }
            }
            return BadRequest(new {error = error, isvalid = false });
        }
        [HttpPost]
        public async Task<IActionResult> Edit(InputHangHoa hanghoa, int MaHh)
        {
            if (!string.IsNullOrEmpty(hanghoa.TenHh))
            {
                if (_context.HangHoas.Any(hh => hh.TenHh == hanghoa.TenHh && hh.MaHh != MaHh))
                    ModelState.AddModelError(string.Empty, "Tên hàng hóa bị trùng");
            }
            if (ModelState.IsValid)
            {
                var hh = _context.HangHoas.FirstOrDefault(h => h.MaHh == MaHh);
                if (hh == null)
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không tìm thấy hàng hóa", errorDev = "Hàng hóa not exist" });
                if (hanghoa.Hinh != null)
                    hh.Hinh = TShopppingUtil.UploadPhoto(hanghoa.Hinh, "HangHoa");
                hh.TenHh = hanghoa.TenHh;
                hh.TenAlias = Generation.GenerateSlug(hanghoa.TenHh);
                hh.MaLoai = hanghoa.MaLoai;
                hh.MoTaDonVi = hanghoa.MoTaDonVi;
                hh.DonGia = hanghoa.DonGia;
                hh.NgaySx = hanghoa.NgaySX ?? DateTime.Now;
                hh.GiamGia = hanghoa.GiamGia ?? 0;
                hh.MoTa = hanghoa.MoTa;
                hh.MaNcc = hanghoa.MaNCC;
                try
                {
                    _context.HangHoas.Update(hh);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không sửa được hàng hóa", errorDev = ex.Message });
                }
            }
            List<string> error = new List<string>();
            foreach (var value in ModelState.Values)
            {
                foreach (var err in value.Errors)
                {
                    error.Add(err.ErrorMessage);
                }
            }
            return BadRequest(new { error = error, isvalid = false });
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int MaHh)
        {
            var hh = _context.HangHoas.FirstOrDefault(h => h.MaHh == MaHh);
            if (hh == null)
                return BadRequest(new { errorCLient = "Lỗi không tìm thấy hàng hóa", errorDev = "Hàng hóa not exist" });
            try
            {
                _context.HangHoas.Remove(hh);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(new {errorClient = "Lỗi không xóa được mặt hàng này", errorDev =ex.Message});
            }
        }
    }
}
