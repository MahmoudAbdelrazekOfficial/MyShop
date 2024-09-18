using myshop.Entities.Models;
using myshop.Entities.Repositories;
using myshop1.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
    public class ProductRepository : GenericRepository<Product> , IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context) 
        {
            _context = context;
        }

        public void update(Product product)
        {
            var productInDb = _context.Products.FirstOrDefault(x=> x.Id == product.Id);
            if (productInDb != null)
            {
                productInDb.Name = product.Name;
                productInDb.Description = product.Description;
                productInDb.Price = product.Price;
                productInDb.Img = product.Img;
                productInDb.CategoryId = product.CategoryId;
                
            }

        }
    }
}
