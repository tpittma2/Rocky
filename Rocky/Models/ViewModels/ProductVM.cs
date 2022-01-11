using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }// = new Product();

        public IEnumerable<SelectListItem>  CategorySelectList { get; set; }
    }
}
