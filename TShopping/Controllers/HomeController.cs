using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Drawing.Printing;
using TShopping.Data;
using TShopping.Helpers;
using TShopping.Models;
using TShopping.ViewModels;

namespace TShopping.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
        private readonly TshoppingContext _context;
        private readonly int PageSize = 24;

        public HomeController(ILogger<HomeController> logger, TshoppingContext context)
        {
            _logger = logger;
            _context = context;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
            var query = _context.HangHoas.Include(hh => hh.MaLoaiNavigation).AsQueryable();
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
                generateUrl = (p) => Url.Action("Index", "Home", new { area = "", page = p }) ?? "/"
            };
            ViewBag.pagingModel = pagingModel;
            return View(result);
        }
		[Route("/404")]
		public IActionResult PageNotFound()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		[Route("/AccessDenied")]
		public IActionResult AccessDenied()
		{
			return View();
		}
        [HttpGet]
        public async Task<IActionResult> BestSale()
        {
            var result = _context.ChiTietHds.Include(ct => ct.MaHhNavigation)
                                            .GroupBy(ct => ct.MaHh)
                                            .OrderByDescending(g => g.Sum(ct => ct.SoLuong))
                                            .Select(g => new
                                            {
                                                addtocart = Url.Action("AddToCart","Cart", new {id = g.Key}),
                                                detail = Url.Action("Detail", "HangHoa", new { id = g.Key }),
                                                tenhh = g.FirstOrDefault().MaHhNavigation.TenHh,
                                                dongia = string.Format("{0:N0}", g.FirstOrDefault().MaHhNavigation.DonGia) + "đ",
                                                hinh = Url.Content($"~/Hinh/HangHoa/{g.FirstOrDefault().MaHhNavigation.Hinh}") ?? ""
                                            });  
            return Json(new { result = result.Take(12) });
        }

    }
}
