using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess.Implementation;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;

namespace myshop.web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var products = _unitOfWork.Product.GetAll();
            return View(products);
        }
        public IActionResult Details(int? id)
        {
            ShoppingCartViewModel obj = new ShoppingCartViewModel()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(v => v.Id == id, IncludeWord: "Category"),
                Count = 1
            };
            return View(obj);
        }
    }
}
