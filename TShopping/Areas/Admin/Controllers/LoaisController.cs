using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TShopping.Areas.Admin.Models;
using TShopping.Data;
using TShopping.Helpers;

namespace TShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = MyRole.Employee)]
    public class LoaisController : Controller
    {
        private readonly TshoppingContext _context;

        public LoaisController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var Loais = await _context.Loais.ToListAsync();
            return View(Loais);
        }
        [HttpPost]
        public async Task<IActionResult> Create(InputLoaiModel loaiModel)
        {
            if (!string.IsNullOrEmpty(loaiModel.TenLoai))
            {
                var Slug = Generation.GenerateSlug(loaiModel.TenLoai);
                if(_context.Loais.Any(lo => lo.TenLoaiAlias == Slug))
                {
                    ModelState.AddModelError(string.Empty, "Tên loại bị trùng");
                }
            }
            if (ModelState.IsValid)
            {
                var loai = new Loai
                {
                    TenLoai = loaiModel.TenLoai,
                    MoTa = loaiModel.MoTa,
                    TenLoaiAlias = Generation.GenerateSlug(loaiModel.TenLoai)
                };
                try
                {
                    _context.Loais.Add(loai);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true,  errorClient = "Lỗi không thêm được loại hàng", errorDev = ex.Message});
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
        public async Task<IActionResult> Edit(InputLoaiModel loaiModel, int MaLoai)
        {
            if (!string.IsNullOrEmpty(loaiModel.TenLoai))
            {
                var Slug = Generation.GenerateSlug(loaiModel.TenLoai);
                if (_context.Loais.Any(lo => lo.TenLoaiAlias == Slug && lo.MaLoai != MaLoai))
                {
                    ModelState.AddModelError(string.Empty, "Tên loại bị trùng");
                }
            }
            if (ModelState.IsValid)
            {
                var loai = _context.Loais.FirstOrDefault(lo => lo.MaLoai == MaLoai);
                if(loai == null)
                {
                    return BadRequest(new { isvalid = true, errorClient = "Lỗi không tìm thấy loại hàng cần sửa" });
                }
                try
                {
                    loai.TenLoai = loaiModel.TenLoai;
                    loai.MoTa = loaiModel.MoTa;
                    loai.TenLoaiAlias = Generation.GenerateSlug(loaiModel.TenLoai);
                    _context.Loais.Update(loai);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorClient = "Lỗi không sửa được loại hàng", errorDev = ex.Message });
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
        public async Task<IActionResult> Delete(int MaLoai)
        {
            var loai = _context.Loais.FirstOrDefault(h => h.MaLoai == MaLoai);
            if (loai == null)
                return BadRequest(new { errorCLient = "Lỗi không tìm thấy loại hàng hóa", errorDev = "Loai not exist" });
            try
            {
                _context.Loais.Remove(loai);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorClient = "Lỗi không xóa được loại hàng này", errorDev = ex.Message });
            }
        }
    }
}
