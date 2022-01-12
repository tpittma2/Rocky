using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rocky.Utility;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM
            {
                Categories = _db.Category,
                Products = _db.Product.Include(x=> x.Category).Include(x=> x.ApplicationType),
            };
            return View(homeVM);
        }

        public async Task<IActionResult> Details(int id) //retrieveing from _IndividualProductCard asp-route-Id
        {
            bool inCart = false;
            var cart = GetShoppingCartList();
            if (cart != null)
            {
                inCart = cart.Any(x => x.ProductId == id);
            }
            var detailsVM = new DetailsVM()
            {
                Product = await _db.Product.Include(x => x.Category).Include(x => x.ApplicationType).FirstOrDefaultAsync(x => x.Id == id),
                InCart = inCart
            };

            return View(detailsVM);
        }

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            var cart = GetShoppingCartList();
            if (!cart.Any(x=> x.ProductId == id))
            {
                cart.Add(new ShoppingCart { ProductId = id });
            }
            HttpContext.Session.Set(WC.SessionCart, cart);

            //return RedirectToAction(nameof(Index));
            return RedirectToAction(nameof(Details), new { Id = id });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetShoppingCartList();
            var cartObj = cart.FirstOrDefault(x => x.ProductId == id);
            if (cartObj != null)
            {
                cart.Remove(cartObj);
            }
            
            HttpContext.Session.Set(WC.SessionCart, cart);

            return RedirectToAction("Details", new { Id = id });
        }

        private List<ShoppingCart> GetShoppingCartList()
        {
            var cart = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            if (cart == null)
            {
                cart = new List<ShoppingCart>();
            }
            return cart;
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
    }
}
