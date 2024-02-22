using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TShopping.Helpers;
using TShopping.ViewModels;

namespace TShopping.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        const string CART_ITEM = "TSCART";
        public IViewComponentResult Invoke()
        {
            var myCart = HttpContext.Session.Get<List<CartItem>>(CART_ITEM) ?? new List<CartItem>();
            return View(myCart);
        }

    }
}
