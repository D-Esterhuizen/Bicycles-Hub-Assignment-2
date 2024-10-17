using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment_2.Models.ViewModels
{
    public class PurchaseOrderViewModel
    {
        public int OrderId { get; set; }
        public string ProductName { get; set; }
        public decimal ListPrice { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public string Base64Image { get; set; }
        public DateTime OrderDate { get; set; }
        public int productID { get; set; }
        public int staff_id { get; set; }
        public string FormattedPrice
        {
            get
            {
                return string.Format("R{0:0.00}", ListPrice);
            }
        }

        public IEnumerable <PurchaseOrderViewModel> PurchaseOrder { get; set; }
    }

}