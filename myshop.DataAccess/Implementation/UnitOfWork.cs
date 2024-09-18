﻿using myshop.Entities.Repositories;


namespace myshop.DataAccess.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        
        public UnitOfWork(ApplicationDbContext context) 
        {
            _context = context;
            Category = new CategoryRepository(context);
            Product = new ProductRepository(context);
        }
        

        public int Complete()
        {
           return _context.SaveChanges();
        }

        public void Dispose()
        {
           _context.Dispose();
        }
    }
}
