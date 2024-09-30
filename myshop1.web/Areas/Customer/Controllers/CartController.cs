using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using myshop.Utilities;
using Stripe.Checkout;
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
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
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
            if (shoppingCart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                _unitOfWork.Complete();
                return RedirectToAction("Index", "Home");
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
            if (shoppingCart.Count<=1)
            {
                _unitOfWork.ShoppingCart.Remove(shoppingCart);
                var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count() -1;
                HttpContext.Session.SetInt32(SD.SessionKey, count);
            }
            else
            {
                _unitOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);
            }
            
            _unitOfWork.Complete();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartID)
        {
            var shoppingCart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartID);
            _unitOfWork.ShoppingCart.Remove(shoppingCart);
            _unitOfWork.Complete();
            var count = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count() ;
            HttpContext.Session.SetInt32(SD.SessionKey, count);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartViewModel = new ShoppingCartViewModel()
            {
                CartsList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, IncludeWord: "Product"),
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

            shoppingCartViewModel.OrderHeader.OrderStatus = SD.Pending;
            shoppingCartViewModel.OrderHeader.PaymentStatus = SD.Pending;
            shoppingCartViewModel.OrderHeader.OrderDate = DateTime.Now;
            shoppingCartViewModel.OrderHeader.ApplicationUserId = claim.Value;


            foreach (var item in shoppingCartViewModel.CartsList)
            {
                shoppingCartViewModel.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }
            _unitOfWork.OrderHeader.Add(shoppingCartViewModel.OrderHeader);
            _unitOfWork.Complete();

            foreach (var item in shoppingCartViewModel.CartsList)
            {
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderId = shoppingCartViewModel.OrderHeader.Id,

                    Price = item.Product.Price,
                    Count = item.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Complete();
            }
            var domain = "https://localhost:44387/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
               
                Mode = "payment",
                SuccessUrl = domain+$"customer/cart/orderconfirmation?id={shoppingCartViewModel.OrderHeader.Id}",
                CancelUrl = domain+$"customer/cart/index",
            };

            foreach (var item in shoppingCartViewModel.CartsList)
            {

                var sessionLineOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineOptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            shoppingCartViewModel.OrderHeader.SessionId = session.Id;
            

            _unitOfWork.Complete();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

            
        }
        public IActionResult OrderConfirmation(int id )
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve, SD.Approve);
				 orderHeader.PaymentIntentId = session.PaymentIntentId;
				_unitOfWork.Complete();
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll
                (u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Complete();
            return View(id);
        }
    }
}
