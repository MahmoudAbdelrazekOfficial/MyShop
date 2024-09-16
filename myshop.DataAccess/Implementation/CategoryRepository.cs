using myshop.Entities.Repositories;
using myshop1.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
    public class CategoryRepository : GenericRepository<Category> , ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void update(Category category)
        {
            var categoryInDb = _context.Categories.FirstOrDefault(x=> x.Id == category.Id);
            if (categoryInDb != null)
            {
                categoryInDb.Name = category.Name;
                categoryInDb.Description = category.Description;
                categoryInDb.CreatedTime = DateTime.Now;
            }

        }
    }
}
