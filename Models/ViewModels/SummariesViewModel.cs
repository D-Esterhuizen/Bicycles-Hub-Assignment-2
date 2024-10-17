using System;
using System.Collections.Generic;

namespace Assignment_2.Models.ViewModels
{
    public class SummariesViewModel
    {
        public int SummaryResult { get; set; }
        public string QueryType { get; set; }

        // List for ListingsPerBrand
        public List<ListingsPerBrandViewModel> ListingsPerBrandList { get; set; }

        // List for AvgSalesPerBrand
        public List<AvgSalesPerBrandViewModel> AvgSalesPerBrandList { get; set; }

        // List for TotalsPerBrandCategory
        public List<TotalsPerBrandCategoryViewModel> TotalsPerBrandCategoryList { get; set; }

        // List for SalesPerBrand
        public List<SalesPerBrandViewModel> SalesPerBrandList { get; set; }

        // For store stock
        public List<StoreStockViewModel> StoreStockList { get; set; }
    }

    // ViewModel for ListingsPerBrand
    public class ListingsPerBrandViewModel
    {
        public string BrandName { get; set; }
        public int Quantity { get; set; }
    }

    // ViewModel for AvgSalesPerBrand
    public class AvgSalesPerBrandViewModel
    {
        public string BrandName { get; set; }
        public decimal AverageSalePrice { get; set; }
    }

    // ViewModel for TotalsPerBrandCategory
    public class TotalsPerBrandCategoryViewModel
    {
        public string BrandName { get; set; }
        public int Mountain { get; set; }
        public int Road { get; set; }
        public int Electric { get; set; }
        public int Children { get; set; }
        public int Comfort { get; set; }
        public int Cruisers { get; set; }
        public int Cyclocross { get; set; }
    }
    // ViewModel for each brand's sales
    public class SalesPerBrandViewModel
    {
        public string BrandName { get; set; }
        public int SoldBikes { get; set; }
    }
    public class StoreStockViewModel
    {
        public string StoreName { get; set; }
        public int TotalQuantity { get; set; }
    }

}




