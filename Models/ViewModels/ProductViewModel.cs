using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment_2.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string FormattedPrice { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        // New properties for adding a listing
        public int BrandId { get; set; }  // Selected brand from dropdown
        public int CategoryId { get; set; }  // Selected category from dropdown
        public int ModelYear { get; set; }  // Model year
        public decimal ListPrice { get; set; }  // Listing price
        public string Base64Image { get; set; }
        public int Staff_ID { get; set; }
        public DateTime ListedDate { get; set; }

        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        public IEnumerable<ProductViewModel> Products { get; set; }  // For listing products
        
    }
    public class ProductFilterViewModel
    {
        // Filtering Properties
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public IEnumerable<SelectListItem> Brands { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        // List of Products
        public IEnumerable<ProductViewModel> Products { get; set; }
    }

}