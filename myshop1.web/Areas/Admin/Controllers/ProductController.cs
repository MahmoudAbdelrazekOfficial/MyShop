using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using myshop.DataAccess;
using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop.Entities.ViewModels;
using myshop1.Entities.Models;


namespace myshop.web.Areas.Area.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork , IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
           
            return View();
        }
        [HttpGet]
        public IActionResult GetData()
        {
            var products = _unitOfWork.Product.GetAll(IncludeWord:"Category");
            return Json(new {data = products});
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductViewModel productVM = new ProductViewModel()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(x=> new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductViewModel productVM ,IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(rootPath,@"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    using (var filestream = new FileStream(Path.Combine(upload,fileName+ext),FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productVM.Product.Img = @"Images\Products\" + fileName + ext;

                }

            

                _unitOfWork.Product.add(productVM.Product);
            
                _unitOfWork.Complete();
                TempData["Create"] = "Item has Created Succesfully";
                return RedirectToAction("Index");
            }
            return View(productVM.Product);
        }



        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                NotFound();
            }

            ProductViewModel productVM = new ProductViewModel()
            {
                Product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id),
                CategoryList = _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            return View(productVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductViewModel productViewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var upload = Path.Combine(rootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if(productViewModel.Product.Img != null)
                    {
                        var oldImg = Path.Combine(rootPath, productViewModel.Product.Img.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImg))
                        {
                            System.IO.File.Delete(oldImg);
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(upload, fileName + ext), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productViewModel.Product.Img = @"Images\Products" + fileName + ext;

                }
                //_context.Products.Update(product);
                _unitOfWork.Product.update(productViewModel.Product);
                //_context.SaveChanges();
                _unitOfWork.Complete();
                TempData["Update"] = "Data has Updated Succesfully";
                return RedirectToAction("Index");
            }
            return View(productViewModel.Product);
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            //var productFromDb = _context.Products.Find(id);
            var productFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (productFromDb == null)
            {
               return Json(new { success = false, message = "Error while you delete intem " });
               
            }
            //_context.Products.Remove(categoryFromDb);
            _unitOfWork.Product.Remove(productFromDb);
            var oldImg = Path.Combine(_webHostEnvironment.WebRootPath,productFromDb.Img.TrimStart('\\'));
            if (System.IO.File.Exists(oldImg))
            {
                System.IO.File.Delete(oldImg);
            }
            //_context.SaveChanges();
            _unitOfWork.Complete();
             return Json(new { success = true, message = " Item has been deleted " });
            
        }
    }
}
