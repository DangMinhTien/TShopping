using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TShopping.Data;
using TShopping.Helpers;
using TShopping.ViewModels;

namespace TShopping.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly TshoppingContext _context;

        public NhanVienController(TshoppingContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginNV login, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            var returnUrl = ReturnUrl ?? "/Admin";
            if(ModelState.IsValid)
            {
                var nhanVien = _context.NhanViens.FirstOrDefault(nv => nv.MaNv == login.MaNv);
                if (nhanVien == null)
                {
                    ModelState.AddModelError(string.Empty, "Nhân viên không tồn tại");
                    return View("/Views/KhachHang/DangNhap.cshtml");
                }
                if(nhanVien.MatKhau !=  login.MatKhau)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu nhân viên bị sai");
                    return View("/Views/KhachHang/DangNhap.cshtml");
                }
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, nhanVien.HoTen),
                    new Claim(ClaimTypes.Email, nhanVien.Email),
                    new Claim(MySetting.Claim_EmployeeId, nhanVien.MaNv),
                    new Claim(ClaimTypes.Role, MyRole.Employee)
                };
                var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimPrincipal = new ClaimsPrincipal(claimIdentity);
                await HttpContext.SignInAsync(/*"LoginCustomer",*/ claimPrincipal);
                TempData["success"] = "Đăng nhập tài khoản nhân viên thành công";
                return Redirect(returnUrl);
            }
            return View("/Views/KhachHang/DangNhap.cshtml");
        }
        [HttpGet]
        public async Task<IActionResult> DangXuat(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}
