using myshop.Entities.Models;
using myshop1.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Repositories
{
    public interface IShoppingCartRepository : IGenericRepository<ShoppingCart>
    {
        int IncreaseCount (ShoppingCart shoppingCart, int count );
        int DecreaseCount (ShoppingCart shoppingCart, int count );
    }
}
