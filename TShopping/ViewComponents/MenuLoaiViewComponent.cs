using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TShopping.ViewModels;
using TShopping.Data;

namespace TShopping.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent
    {
        private readonly TshoppingContext _context;
        public MenuLoaiViewComponent(TshoppingContext context)
        {
                _context = context;
        }
        public IViewComponentResult Invoke(int? maLoai)
        {
            var data = _context.Loais.Include(lo => lo.HangHoas).Select(lo => new MenuLoaiVM
            {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count(),
                IsActive = lo.MaLoai == maLoai ? true : false,
            });
            return View(data);
        }
    }
}
