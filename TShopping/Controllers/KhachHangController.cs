using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using TShopping.Areas.Admin.Models;
using TShopping.Data;
using TShopping.Helpers;
using TShopping.ViewModels;

namespace TShopping.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly TshoppingContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;

        public KhachHangController(TshoppingContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
        }
        #region Register
        public IActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangKy(RegisterCustomerVM registerCustomer)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    var khachHang = _mapper.Map<KhachHang>(registerCustomer);
                    khachHang.RandomKey = TShopppingUtil.GenerateRandomKey();
                    khachHang.MatKhau = registerCustomer.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.HieuLuc = true;
                    khachHang.VaiTro = 0;
                    if (registerCustomer.Anh != null)
                    {
                        khachHang.Hinh = TShopppingUtil.UploadPhoto(registerCustomer.Anh, "KhachHang");
                    }
                    _context.KhachHangs.Add(khachHang);
                    _context.SaveChanges();
                    return RedirectToAction("Index","Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View();
        }
        #endregion
        #region Login
        [HttpGet]
        [Route("/DangNhap")]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }
        [HttpPost]
        [Route("/DangNhap")]
        public async Task<IActionResult> DangNhap(string? ReturnUrl, LoginVM login)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                ReturnUrl = ReturnUrl ?? "/";
                var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == login.UserName);
                if (khachHang == null)
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập không tồn tại");
                    return View();
                }
                var password = login.Password.ToMd5Hash(khachHang.RandomKey);
                if(khachHang.MatKhau != password)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu khách hàng bị sai");
                    return View();
                }
                if (!khachHang.HieuLuc)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản này bị hết hiệu lực");
                    return View();
                }
                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, khachHang.HoTen),
                    new Claim(ClaimTypes.Email, khachHang.Email),
                    new Claim(MySetting.Claim_CustomerId, khachHang.MaKh),
                    new Claim(ClaimTypes.Role, "Customer")
                };
                var claimIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimPrincipal = new ClaimsPrincipal(claimIdentity);
                await HttpContext.SignInAsync(/*"LoginCustomer",*/ claimPrincipal);
                TempData["success"] = "Đăng nhập thành công";
                return Redirect(ReturnUrl);
            }
            return View();
        }
        #endregion
        [HttpGet]
        [Authorize(Roles = MyRole.Customer)]
        public IActionResult Profile()
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
            return View(khachHang);
        }
        [HttpGet]
        [Route("/DangXuat")]
        public async Task<IActionResult> DangXuat(string? returnUrl = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
        [HttpPost]
        [Authorize(Roles = MyRole.Customer)]
        public async Task<IActionResult> EditProfile(EditKhachHangModel model)
        {
            if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.MaKh))
            {
                if (_context.KhachHangs.Any(kh => kh.Email == model.Email && kh.MaKh != model.MaKh))
                {
                    ModelState.AddModelError(string.Empty, "Email này đã tồn tại bởi một tài khoản khác");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKh == model.MaKh);
                    if (khachHang == null)
                    {
                        TempData["error"] = "Không thay đổi được thông tin do không tìm thấy tài khoản của bạn";
                        return BadRequest(new { isvalid = true, errorClient = "Lỗi không tìm thấy tài khoản của bạn", errorDev = "TaiKhoan not found" });
                    }
                    khachHang.HoTen = model.HoTen;
                    khachHang.GioiTinh = model.GioiTinh ?? true;
                    khachHang.DienThoai = model.DienThoai;
                    khachHang.Email = model.Email;
                    khachHang.DiaChi = model.DiaChi;
                    khachHang.NgaySinh = model.NgaySinh ?? DateTime.Now;
                    _context.KhachHangs.Update(khachHang);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Không sửa được thông tin tài khoản";
                    return BadRequest(new { isvalid = true, errorClient = "Lỗi không sửa được thông tin tài khoản", errorDev = ex.Message });
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
        [Authorize(Roles = MyRole.Customer)]
        public async Task<IActionResult> ChangePassword(ChangePasswordKhachHang model)
        {
            if (!string.IsNullOrEmpty(model.MatKhauCu) && !string.IsNullOrEmpty(model.MaKh))
            {
                var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKh == model.MaKh);
                if (khachHang == null)
                {
                    return BadRequest(new { isvalid = true, errorClient = "Lỗi không sửa được mật khẩu do không tìm thấy tài khoản của bạn", errorDev = "TaiKhoan not found" });
                }
                var password = model.MatKhauCu.ToMd5Hash(khachHang.RandomKey);
                if (password != khachHang.MatKhau)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu cũ bị sai");
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKh == model.MaKh);
                    if (khachHang == null)
                    {
                        return BadRequest(new { isvalid = true, errorClient = "Lỗi không tìm thấy tài khoản của bạn", errorDev = "TaiKhoan not found" });
                    }
                    var password = model.MatKhauMoi.ToMd5Hash(khachHang.RandomKey);
                    khachHang.MatKhau = password;
                    _context.KhachHangs.Update(khachHang);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Bạn vừa thay đổi mật khẩu thành công";
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { isvalid = true, errorCLient = "Lỗi không sửa được mật khẩu tài khoản", errorDev = ex.Message });
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
