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
    public class NhaCungCapsController : Controller
    {
        private readonly TshoppingContext _context;

        public NhaCungCapsController(TshoppingContext context)
        {
            _context = context;   
        }
        public async Task<IActionResult> Index()
        {
            var nhaCC = await _context.NhaCungCaps.ToListAsync();
            return View(nhaCC);
        }
        [HttpPost]
        public async Task<IActionResult> Create(InputNhaCungCapModel nhaCCModel)
        {
            if (nhaCCModel.Logo == null)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn hình ảnh logo");
            }
            if (!string.IsNullOrEmpty(nhaCCModel.TenCongTy))
            {
                if (_context.NhaCungCaps.Any(hh => hh.TenCongTy == nhaCCModel.TenCongTy))
                    ModelState.AddModelError("Slug", "Tên công ty bị trùng");
            }
            if (ModelState.IsValid)
            {
                var nhaCungCap = new NhaCungCap();
                if (nhaCCModel.Logo != null)
                    nhaCungCap.Logo = TShopppingUtil.UploadPhoto(nhaCCModel.Logo, "NhaCC");
                nhaCungCap.MaNcc = Guid.NewGuid().ToString();
                nhaCungCap.TenCongTy = nhaCCModel.TenCongTy;
                nhaCungCap.NguoiLienLac = nhaCCModel.NguoiLienLac;
                nhaCungCap.Email = nhaCCModel.Email;
                nhaCungCap.DienThoai = nhaCCModel.DienThoai;
                nhaCungCap.MoTa = nhaCCModel.MoTa;
                nhaCungCap.DiaChi = nhaCCModel.DiaChi;
                try
                {
                    _context.NhaCungCaps.Add(nhaCungCap);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không thêm được nhà cung cấp", errorDev = ex.Message });
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
        public async Task<IActionResult> Edit(InputNhaCungCapModel nhaCCModel, string MaNCC)
        {
            if (!string.IsNullOrEmpty(nhaCCModel.TenCongTy))
            {
                if (_context.NhaCungCaps.Any(ncc => ncc.TenCongTy == nhaCCModel.TenCongTy && ncc.MaNcc != MaNCC))
                    ModelState.AddModelError("Slug", "Tên công ty bị trùng");
            }
            if (ModelState.IsValid)
            {
                var nhaCungCap = _context.NhaCungCaps.FirstOrDefault(h => h.MaNcc == MaNCC);
                if (nhaCungCap == null)
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không tìm thấy nhà cung cấp cần sửa", errorDev = "NhaCungCap not exist" });
                if (nhaCCModel.Logo != null)
                    nhaCungCap.Logo = TShopppingUtil.UploadPhoto(nhaCCModel.Logo, "NhaCC");
                nhaCungCap.TenCongTy = nhaCCModel.TenCongTy;
                nhaCungCap.Email = nhaCCModel.Email;
                nhaCungCap.DienThoai = nhaCCModel.DienThoai;
                nhaCungCap.DiaChi = nhaCCModel.DiaChi;
                nhaCungCap.MoTa = nhaCCModel.MoTa;
                nhaCungCap.NguoiLienLac = nhaCCModel.NguoiLienLac;
                try
                {
                    _context.NhaCungCaps.Update(nhaCungCap);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không sửa được nhà cung cấp", errorDev = ex.Message });
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
        public async Task<IActionResult> Delete(string MaNCC)
        {
            var nhaCungCap = _context.NhaCungCaps.FirstOrDefault(ncc => ncc.MaNcc == MaNCC);
            if (nhaCungCap == null)
                return BadRequest(new { errorCLient = "Lỗi không tìm thấy nhà cung cấp", errorDev = "NhaCungCap not exist" });
            try
            {
                _context.NhaCungCaps.Remove(nhaCungCap);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorClient = "Lỗi không xóa được nhà cung cấp này", errorDev = ex.Message });
            }
        }
    }
}
