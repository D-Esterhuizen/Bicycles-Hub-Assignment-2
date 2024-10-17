using Assignment_2.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment_2.ViewModels
{
	public class BuyerViewModel
	{

        //stuff for pages
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public IEnumerable<BuyerViewModel> Buyers { get; set; }


		// actual stuff
        public int cusotmer_id { get; set; }
		public string first_Name { get; set; }
		public string last_Name { get; set; }
		public string phone { get; set; }
		public string email { get; set; }
		public string street { get; set; }
		public string city { get; set; }
		public string state { get; set; }
		public string zip_code { get; set; }
		public string CPassword { get; set; }
		public string S_Country { get; set; }

        public bool RegisteredAsSeller { get; set; }

		public StaffViewModel staff_id { get; set; }
        public StaffViewModel first_name { get; set; }
		public StaffViewModel last_name { get; set; }
		public OrderViewModel customer_id { get; set; }
		public OrderViewModel  order_date { get; set; }
        public StoreViewModel store_name { get; set; }


        public BuyerViewModel() 
		{
			staff_id = new StaffViewModel();
			first_name = new StaffViewModel();
			last_name = new StaffViewModel();
			customer_id = new OrderViewModel();
			order_date = new OrderViewModel();
			store_name = new StoreViewModel();
		}
	}
}