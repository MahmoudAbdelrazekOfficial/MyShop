using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using myshop.Utilities;
using System.Security.Claims;

namespace myshop.web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value , IncludeWord:"Product"),
                OrderHeader = new()
            };

            foreach (var item in ShoppingCartViewModel.CartsList)
            {
                ShoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartViewModel);
        }
        public IActionResult Plus(int cartID)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartID);
            if(shoppingCart.Count <= 1 )
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                _unitOfWork.Complete();
                return RedirectToAction("Index","Home");
            }
            else
            {
                _unitOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            }
            
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartID)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartID);
            _unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartID)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartID);
            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                CartsList =_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == claim.Value, IncludeWord:"Product"),
                OrderHeader = new()
            };
            ShoppingCartViewModel.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

            ShoppingCartViewModel.OrderHeader.Name = ShoppingCartViewModel.OrderHeader.ApplicationUser.Name;
            ShoppingCartViewModel.OrderHeader.Address = ShoppingCartViewModel.OrderHeader.ApplicationUser.Address;
            ShoppingCartViewModel.OrderHeader.City = ShoppingCartViewModel.OrderHeader.ApplicationUser.City;
            ShoppingCartViewModel.OrderHeader.Phone = ShoppingCartViewModel.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in ShoppingCartViewModel.CartsList)
            {
                ShoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }
            return View(ShoppingCartViewModel);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult PostSummary(ShoppingCartViewModel shoppingCartViewModel)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCartViewModel.CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product");

            ShoppingCartViewModel.OrderHeader.OrderStatus = SD.Pending;
            ShoppingCartViewModel.OrderHeader.PaymentStatus = SD.Pending;
            ShoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var item in ShoppingCartViewModel.CartsList)
            {
                ShoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }
            _unitOfWork.OrderHeader.Add(shoppingCartViewModel.OrderHeader);
            _unitOfWork.Complete();

            foreach (var item in ShoppingCartViewModel.CartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderId = ShoppingCartViewModel.OrderHeader.Id,
                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Complete();
            }
            return View();
        }
    }
}
