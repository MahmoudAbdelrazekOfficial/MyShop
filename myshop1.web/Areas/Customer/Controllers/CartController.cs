using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
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
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value , IncludeWord:"Product")
            };
            foreach (var item in ShoppingCartViewModel.CartsList)
            {
                ShoppingCartViewModel.TotalCarts += (item.Count * item.Product.Price);
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
    }
}
