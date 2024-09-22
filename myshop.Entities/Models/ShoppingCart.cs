using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public class ShoppingCart
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProductId { get; set; }


        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }


        [Range(1, 10, ErrorMessage = "You must Enter Value 1 to 10")]
        public int Count { get; set; }
        public string ApplicationUserId { get; set; }


        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
    }
}
