using Assignment_2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment_2.Models.ViewModels
{
    public class StaffViewModel
    {

        public IEnumerable<StaffViewModel> Staffs { get; set; }
    
        public int staff_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public byte active { get; set; }
        public int store_id { get; set; }
        public string S_Password { get; set; }

        public Nullable<int> manager_id { get; set; }
    }
}