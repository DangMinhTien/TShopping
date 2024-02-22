using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TShopping.Data;
using TShopping.Helpers;

namespace TShopping.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = MyRole.Employee)]
    public class KhachHangsController : Controller
    {
        private readonly TshoppingContext _context;

        public KhachHangsController(TshoppingContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var khachHangs = _context.KhachHangs.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                khachHangs = khachHangs.Where(hh => hh.HoTen.Contains(search));
            }
            var pageSize = TShopping.Helpers.MySetting.PageSize;
            var total = await khachHangs.CountAsync();
            var totalPage = (int)Math.Ceiling((double)total / pageSize);
            if (page < 1)
                page = 1;
            if (page > totalPage)
                page = totalPage;
            var result = khachHangs.ToList().Skip((page - 1) * pageSize).Take(pageSize);
            var pagingmodel = new PagingModel()
            {
                countPage = totalPage,
                currentPage = (int)page,
                generateUrl = (p) => Url.Action("Index", "KhachHangs", new { area = "Admin", page = p, search = search }) ?? "/"
            };
            ViewBag.Paging = pagingmodel;
            ViewBag.search = search;
            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeEffect(string MaKh)
        {
            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == MaKh);
            if(khachHang == null)
            {
                return BadRequest(new { errorClient = "Không tìm thấy khách hàng cần sửa", errorDev = "KhachHang not found" });
            }
            try
            {
                khachHang.HieuLuc = khachHang.HieuLuc == true ? false : true;
                _context.KhachHangs.Update(khachHang);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(new { errorClient = "Không sửa được hiệu lực của khách hàng", errorDev = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string MaKh)
        {
            var khachHang = _context.KhachHangs.FirstOrDefault(kh => kh.MaKh == MaKh);
            if (khachHang == null)
            {
                return BadRequest(new { errorClient = "Không tìm thấy khách hàng cần sửa", errorDev = "KhachHang not found" });
            }
            try
            {
                _context.KhachHangs.Remove(khachHang);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { errorClient = "Lỗi không xóa được khách hàng này", errorDev = ex.Message });
            }
        }
    }
}
