using Azure;
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
    public class NhanViensController : Controller
    {
        private readonly TshoppingContext _context;

        public NhanViensController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var nhanViens = _context.NhanViens.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                nhanViens = nhanViens.Where(nv => nv.HoTen.Contains(search));
            }
            var pageSize = TShopping.Helpers.MySetting.PageSize;
            var total = await nhanViens.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if (page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var result = nhanViens.ToList().Skip((page - 1) * pageSize).Take(pageSize);
            var pagingmodel = new PagingModel()
            {
                countPage = totalPage,
                currentPage = (int)page,
                generateUrl = (p) => Url.Action("Index", "NhanViens", new { area = "Admin", page = p, search = search }) ?? "/"
            };
            ViewBag.Paging = pagingmodel;
            ViewBag.search = search;
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(InputNhanVienModel nhanVienModel)
        {
            if (!string.IsNullOrEmpty(nhanVienModel.Email))
            {
                if(_context.NhanViens.Any(nv => nv.Email == nhanVienModel.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email này đã tồn tại bởi một nhân viên khác");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var nhanVien = new NhanVien
                    {
                        MaNv = nhanVienModel.MaNv,
                        HoTen = nhanVienModel.HoTen,
                        Email = nhanVienModel.Email,
                        MatKhau = nhanVienModel.MatKhau
                    };
                    _context.NhanViens.Add(nhanVien);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không thêm được nhân viên", errorDev = ex.Message });
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
        public async Task<IActionResult> Delete(string MaNv)
        {
            var nhanVien = _context.NhanViens.FirstOrDefault(nv => nv.MaNv == MaNv);
            if (nhanVien == null)
            {
                return BadRequest(new { errorClient = "Không tìm thấy nhân viên cần xóa", errorDev = "KhachHang not found" });
            }
            try
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorClient = "Lỗi không xóa được nhân viên này", errorDev = ex.Message });
            }
        }
        public async Task<IActionResult> Profile()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("DangNhap", "KhachHang");
            }
            var userId = HttpContext.User.FindFirst(MySetting.Claim_EmployeeId)?.Value;
            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaNv == userId);
            if (nhanVien == null)
            {
                TempData["error"] = "Không tìm thấy thông tin nhân viên đăng nhập";
                return Redirect("/404");
            }
            return View(nhanVien);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditNhanVienModel nhanVienModel)
        {
            if (!string.IsNullOrEmpty(nhanVienModel.Email) && !string.IsNullOrEmpty(nhanVienModel.MaNv))
            {
                if (_context.NhanViens.Any(nv => nv.Email == nhanVienModel.Email && nv.MaNv != nhanVienModel.MaNv))
                {
                    ModelState.AddModelError(string.Empty, "Email này đã tồn tại bởi một nhân viên khác");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaNv == nhanVienModel.MaNv);
                    if(nhanVien == null)
                    {
                        return BadRequest(new { isvalid = true, errorCLient = "Lỗi không tìm được nhân viên cần sửa", errorDev = "NhanVien not found" });
                    }
                    nhanVien.Email = nhanVienModel.Email;
                    nhanVien.HoTen = nhanVienModel.HoTen;
                    _context.NhanViens.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không sửa được thông tin nhân viên", errorDev = ex.Message });
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
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!string.IsNullOrEmpty(model.MatKhauCu) && !string.IsNullOrEmpty(model.MaNv))
            {
                if (!_context.NhanViens.Any(nv => nv.MatKhau == model.MatKhauCu && nv.MaNv == model.MaNv))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu cũ bị sai");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.MaNv == model.MaNv);
                    if (nhanVien == null)
                    {
                        return BadRequest(new { isvalid = true, errorCLient = "Lỗi không tìm được nhân viên cần sửa", errorDev = "NhanVien not found" });
                    }
                    nhanVien.MatKhau = model.MatKhauMoi;
                    _context.NhanViens.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không sửa được thông tin nhân viên", errorDev = ex.Message });
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
    }
}
