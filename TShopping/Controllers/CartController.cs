using Microsoft.AspNetCore.Mvc;
using TShopping.Data;
using TShopping.ViewModels;
using TShopping.Helpers;
using Microsoft.AspNetCore.Authorization;
using TShopping.Services;

namespace TShopping.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly TshoppingContext _context;
        private readonly IVnPayService _vnPayService;

        public CartController(TshoppingContext context, PaypalClient paypalClient, IVnPayService vnPayService)
        {
            _paypalClient = paypalClient;
            _context = context;
            _vnPayService = vnPayService;
        }
        const string CART_KEY = "TSCART";
        public List<CartItem> Cart {
            get
            {
                return HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            }
        }
        public IActionResult Index()
        {
            return View(Cart);
        }
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var myCart = Cart;
            var cartItem = myCart.FirstOrDefault(c => c.MaHh == id);
            if (cartItem == null)
            {
                var hangHoa = _context.HangHoas.FirstOrDefault(h => h.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = "Không tìm thấy sản phẩm vừa chọn";
                    return Redirect("/404");
                }
                myCart.Add(new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    SoLuong = quantity,
                    Hinh = hangHoa.Hinh ?? "",
                    DonGia = hangHoa.DonGia ?? 0
                });
            }
            else
            {
                cartItem.SoLuong += quantity;
                if(cartItem.SoLuong <= 0)
                    cartItem.SoLuong = 1;
            }
            HttpContext.Session.Set<List<CartItem>>(CART_KEY, myCart);
            return RedirectToAction("Index");
        }
        public IActionResult RemoveCart(int id)
        {
            var myCart = Cart;
            var cartItem = myCart.FirstOrDefault(c => c.MaHh == id);
            if (cartItem == null)
            {
                return Redirect("/404");
            }
            myCart.Remove(cartItem);
            HttpContext.Session.Set<List<CartItem>>(CART_KEY, myCart);
            return RedirectToAction("Index");
        }
        [Authorize(Roles = MyRole.Customer)]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
                return Redirect("/");
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }
        [Authorize(Roles = MyRole.Customer)]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM checkout, string payment = "COD")
        {
            ViewBag.payment = payment;
            if (Cart.Count == 0)
                return Redirect("/");
            if(ModelState.IsValid)
            {
                if(payment == TShopping.Helpers.PaymentType.VnPay)
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(c => c.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Decription = $"{checkout.HoTen}_{checkout.DienThoai}",
                        FullName = checkout.HoTen ?? "Đặng M Tiến",
                        OrderId = new Random().Next(1000, 100000)
                    };
                    TempData["HoTen"] = checkout.HoTen;
                    TempData["DiaChi"] = checkout.DiaChi;
                    TempData["DienThoai"] = checkout.DienThoai;
                    TempData["GhiChu"] = checkout.GhiChu;
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }
                var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == MySetting.Claim_CustomerId)?.Value;
                if(customerId == null)
                {
                    TempData["Message"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);
                if (khachHang == null)
                {
                    TempData["Message"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var hoaDon = new HoaDon
                {
                    MaKh = khachHang.MaKh,
                    HoTen = checkout.HoTen == null ? khachHang.HoTen : checkout.HoTen,
                    DiaChi = checkout.DiaChi,
                    DienThoai = checkout.DienThoai,
                    NgayDat = DateTime.Today,
                    CachThanhToan = "COD",
                    CachVanChuyen = checkout.CachVanChuyen,
                    MaTrangThai = 0,
                    GhiChu = checkout.GhiChu,
                };
                _context.Database.BeginTransaction();
                try
                {
                    _context.HoaDons.Add(hoaDon);
                    _context.SaveChanges();
                    var cthd = new List<ChiTietHd>();
                    foreach(var item in Cart)
                    {
                        cthd.Add(new ChiTietHd
                        {
                            MaHd = hoaDon.MaHd,
                            SoLuong = item.SoLuong,
                            MaHh = item.MaHh,
                            DonGia = item.DonGia,
                            GiamGia = 0
                        });
                    }
                    _context.ChiTietHds.AddRange(cthd);
                    _context.SaveChanges();
                    _context.Database.CommitTransaction();
                    TempData["success"] = "Đặt hàng thành công";
                    return View("Success");
                }
                catch
                {
                    _context.Database.RollbackTransaction();
                    TempData["error"] = "Đặt hàng thất bại";
                    return View("Failure");
                }
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }
        #region PayPal payment
        [Authorize(Roles = MyRole.Customer)]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // thông tin của đơn hàng gửi qua paypal
            var tongTien = (Cart.Sum(c => c.ThanhTien) / 24000f).ToString("N1");
            var donViTien = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();
            try { 
                var response = await _paypalClient.CreateOrder(tongTien, donViTien, maDonHangThamChieu);

                return Ok(response);
            
            }catch(Exception ex)
            {
                var error = new {ex.GetBaseException().Message};
                return BadRequest(error);
            }
        }
        public decimal ConvertToUSD(decimal vndAmount, decimal exchangeRate)
        {
            // Thực hiện chuyển đổi
            decimal usdAmount = vndAmount * exchangeRate;
            return usdAmount;
        }
        [Authorize(Roles = MyRole.Customer)]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder([FromBody] CheckoutVM checkout,string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);
                // Lưu database đơn hàng của mình
                if(response.status != "COMPLETED")
                {
                    TempData["error"] = "Đặt hàng thất bại do thanh toán Paypal không thành công";
                    return BadRequest(new {error = "Đặt hàng thất bại do thanh toán không thành công"});
                }
                var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == MySetting.Claim_CustomerId)?.Value;
                if (customerId == null)
                {
                    TempData["error"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);
                if (khachHang == null)
                {
                    TempData["error"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var hoaDon = new HoaDon
                {
                    MaKh = khachHang.MaKh,
                    HoTen = string.IsNullOrEmpty(checkout.HoTen) ? khachHang.HoTen : checkout.HoTen,
                    DiaChi = checkout.DiaChi,
                    DienThoai = checkout.DienThoai,
                    NgayDat = DateTime.Today,
                    CachThanhToan = checkout.CachThanhToan,
                    CachVanChuyen = checkout.CachVanChuyen,
                    MaTrangThai = 1,
                    GhiChu = string.IsNullOrEmpty(checkout.GhiChu) ? null : checkout.GhiChu
                };
                _context.Database.BeginTransaction();
                _context.HoaDons.Add(hoaDon);
                _context.SaveChanges();
                var cthd = new List<ChiTietHd>();
                foreach (var item in Cart)
                {
                    cthd.Add(new ChiTietHd
                    {
                        MaHd = hoaDon.MaHd,
                        SoLuong = item.SoLuong,
                        MaHh = item.MaHh,
                        DonGia = item.DonGia,
                        GiamGia = 0
                    });
                }
                _context.ChiTietHds.AddRange(cthd);
                _context.SaveChanges();
                _context.Database.CommitTransaction();
                TempData["success"] = $"Đặt hàng bằng thanh toán Paypal thành công";
                return Ok(response);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Đặt hàng thất bại";
                return BadRequest(new { error = ex.GetBaseException().Message });
            }
        }
        #endregion
        [Route("/Cart/OrderSuccess")]
        public IActionResult OrderSuccess()
        {
            return View("Success");
        }
        public IActionResult OrderFail()
        {
            return View("Failure");
        }
        [Authorize(Roles = MyRole.Customer)]
        public IActionResult PaymentCallBack()
        {
            try
            {
                var response = _vnPayService.PaymentExcute(Request.Query);
                if(response == null || response.VnPayResponseCode != "00")
                {
                    //TempData["error"] = $"Lỗi thanh toán VnPay: {response.VnPayResponseCode}";
                    TempData["error"] = "Đặt hàng thất bại do không thanh toán được VnPay";
                    return RedirectToAction("OrderFail");
                }
                // Lưu đơn hàng vào database
                CheckoutVM checkout = new CheckoutVM
                {
                    HoTen = TempData["HoTen"] as string ?? null,
                    DiaChi = TempData["DiaChi"] as string ?? "",
                    DienThoai = TempData["DienThoai"] as string ?? "",
                    GhiChu = TempData["GhiChu"] as string ?? null,
                    CachThanhToan = "Ví điện thử VnPay",
                    CachVanChuyen = "Giao hàng nhanh"
                };
                var customerId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == MySetting.Claim_CustomerId)?.Value;
                if (customerId == null)
                {
                    TempData["error"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == customerId);
                if (khachHang == null)
                {
                    TempData["error"] = "Không tìm thấy tài khoản đăng nhập";
                    return Redirect("/404");
                }
                var hoaDon = new HoaDon
                {
                    MaKh = khachHang.MaKh,
                    HoTen = string.IsNullOrEmpty(checkout.HoTen) ? khachHang.HoTen : checkout.HoTen,
                    DiaChi = checkout.DiaChi,
                    DienThoai = checkout.DienThoai,
                    NgayDat = DateTime.Today,
                    CachThanhToan = checkout.CachThanhToan,
                    CachVanChuyen = checkout.CachVanChuyen,
                    MaTrangThai = 1,
                    GhiChu = string.IsNullOrEmpty(checkout.GhiChu) ? null : checkout.GhiChu
                };
                _context.Database.BeginTransaction();
                _context.HoaDons.Add(hoaDon);
                _context.SaveChanges();
                var cthd = new List<ChiTietHd>();
                foreach (var item in Cart)
                {
                    cthd.Add(new ChiTietHd
                    {
                        MaHd = hoaDon.MaHd,
                        SoLuong = item.SoLuong,
                        MaHh = item.MaHh,
                        DonGia = item.DonGia,
                        GiamGia = 0
                    });
                }
                _context.ChiTietHds.AddRange(cthd);
                _context.SaveChanges();
                _context.Database.CommitTransaction();
                TempData["success"] = $"Thanh toán VnPay thành công";
                return RedirectToAction("OrderSuccess");
            }
            catch(Exception ex)
            {
                _context.Database.RollbackTransaction();
                TempData["error"] = $"Đặt hàng thất bại";
                return RedirectToAction("OrderFail");
            }
        }
    }
}
