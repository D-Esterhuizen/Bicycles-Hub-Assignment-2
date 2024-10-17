using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Assignment_2.ViewModels;

namespace Assignment_2.Controllers
{
    public static class Globals
    {
        
        
            public static string ConnectionString = "Data Source=DYLANS-SPACESHI\\SQLEXPRESS01;Initial Catalog=BikeStores;Integrated Security=True";

            public static List<BuyerViewModel> BuyerList = new List<BuyerViewModel>();
        
    }
}