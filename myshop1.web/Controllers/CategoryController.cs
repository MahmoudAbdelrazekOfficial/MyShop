using Microsoft.AspNetCore.Mvc;
using myshop.DataAccess;
using myshop.Entities.Repositories;
using myshop1.Entities.Models;


namespace myshop.web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork )
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var categories = _unitOfWork.Category.GetAll();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                //_context.Categories.Add(category);\
                _unitOfWork.Category.add(category);
                //_context.SaveChanges();
                _unitOfWork.Complete();
                TempData["Create"] = "Data has Created Succesfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if(id == null | id == 0)
            {
                NotFound();
            }
            //var categoryFromDb = _context.Categories.Find(id);
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(categoryFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                //_context.Categories.Update(category);
                _unitOfWork.Category.update(category);
                //_context.SaveChanges();
                _unitOfWork.Complete();
                TempData["Update"] = "Data has Updated Succesfully";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        [HttpGet]
        public IActionResult Delete(int? id )
        {
            if (id == null | id == 0)
            {
                NotFound();
            }
            //var categoryFromDb = _context.Categories.Find(id);
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult DeleteCategory(int? id)
        {
            //var categoryFromDb = _context.Categories.Find(id);
            var categoryFromDb = _unitOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            
            if (categoryFromDb == null )
            {
                NotFound();
            }
            //_context.Categories.Remove(categoryFromDb);
            _unitOfWork.Category.Remove(categoryFromDb);

            //_context.SaveChanges();
            _unitOfWork.Complete();
            TempData["Delete"] = "Data has deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
