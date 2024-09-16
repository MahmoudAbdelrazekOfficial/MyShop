using Microsoft.EntityFrameworkCore;
using myshop1.Entities.Models;


namespace myshop.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options):base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }

    }
}
