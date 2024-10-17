using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment_2.Models.ViewModels
{
    public class StoreViewModel
    {
        public int store_id { get; set; }
        public string store_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
    }
}