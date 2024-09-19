using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.ViewModels
{
    public  class ShoppingCartViewModel
    {
        public Product Product { get; set; }
        [Range(1,10,ErrorMessage = "You must Enter Value 1 to 10")]
        public int Count { get; set; }
    }
}
