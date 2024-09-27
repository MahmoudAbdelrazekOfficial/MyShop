using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using myshop.Utilities;
using Stripe;

namespace myshop.web.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.AdminRole)]
	public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        //data Bind
        [BindProperty]
        public OrderViewModel OrderViewModel { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = _unitOfWork.OrderHeader.GetAll(IncludeWord: "ApplicationUser");
            return Json(new {data= orderHeaders});
        }
        [HttpGet]
        public IActionResult Details(int orderId)
        {
            OrderViewModel orderViewModel = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id==orderId , IncludeWord:"ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x=>x.OrderId==orderId , IncludeWord:"Product")
            };
            return View(orderViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details()
        {
            var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id);
            orderFromDb.Name = OrderViewModel.OrderHeader.Name;
            orderFromDb.Phone = OrderViewModel.OrderHeader.Phone;
            orderFromDb.Address = OrderViewModel.OrderHeader.Address;
            orderFromDb.City = OrderViewModel.OrderHeader.City;

            if(OrderViewModel.OrderHeader.Carrier != null)
            {
                orderFromDb.Carrier= OrderViewModel.OrderHeader.Carrier;
            }
            if (OrderViewModel.OrderHeader.TrackingNo != null)
            {
                orderFromDb.TrackingNo= OrderViewModel.OrderHeader.TrackingNo;
            }
            _unitOfWork.OrderHeader.Update(orderFromDb);
            _unitOfWork.Complete();

            TempData["Update"] = "Item Has Updated Successfully ";
            return RedirectToAction("Details", "Order", new {orderid = orderFromDb.Id});
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProccess()
        {
            _unitOfWork.OrderHeader.UpdateOrderStatus(OrderViewModel.OrderHeader.Id ,SD.Proccessing,null) ; 
            _unitOfWork.Complete() ;
			TempData["Update"] = "Order Status Has Updated Successfully ";
			return RedirectToAction("Details", "Order", new { orderid = OrderViewModel.OrderHeader.Id });

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StartShip()
		{
			var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id);
            orderFromDb.TrackingNo = OrderViewModel.OrderHeader.TrackingNo;
            orderFromDb.Carrier = OrderViewModel.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Shipped;
            orderFromDb.ShippingDate = DateTime.Now;

            _unitOfWork.OrderHeader.Update(orderFromDb);
			_unitOfWork.Complete();

			
			
			TempData["Update"] = "Order Has Shipped Successfully ";
			return RedirectToAction("Details", "Order", new { orderid = OrderViewModel.OrderHeader.Id });

		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CancelOrder()
		{
			var orderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id);

			if (orderFromDb.PaymentStatus == SD.Approve)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Refund);
			}
            else
            {
				_unitOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Cancelled);

			}
            _unitOfWork.Complete();

			TempData["Update"] = "Order  Has Cancelled   Successfully ";
			return RedirectToAction("Details", "Order", new { orderid = OrderViewModel.OrderHeader.Id });

		}
	}
}
