using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Windows.Input;
using Assignment_2.Models;
using Assignment_2.Models.ViewModels;
using Assignment_2.ViewModels;

namespace Assignment_2.Controllers
{
    public class HomeController : Controller
    {
        private SqlConnection myConnection = new SqlConnection(Globals.ConnectionString);

        public ActionResult Index()
        {
            return View();
        }
        //nav bar pages
        //list and edit listing page
        public ActionResult GeneralSearchResult(string searchTerm)
        {
            // Initialize a list to hold products
            var products = new List<ProductViewModel>();

            // Open the database connection
            myConnection.Open();

            // Define your SQL query to search based on the search term
            var sqlQuery = @"
                                SELECT b.brand_name, CONCAT('R ', p.list_price) AS formatted_price, p.product_name, p.Listing_B64 
                                FROM production.products p
                                INNER JOIN production.brands b ON p.brand_id = b.brand_id
                                WHERE 1 = 1";

            // Add search term filtering if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sqlQuery += " AND (b.brand_name LIKE @SearchInput OR p.product_name LIKE @SearchInput)";
            }

            // Prepare SQL command
            using (var command = new SqlCommand(sqlQuery, myConnection))
            {
                // Add parameter for search term if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    command.Parameters.AddWithValue("@SearchInput", "%" + searchTerm.Trim() + "%");
                }

                // Execute the query and read the results
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new ProductViewModel
                        {
                            BrandName = reader["brand_name"].ToString(),
                            FormattedPrice = reader["formatted_price"].ToString(),
                            ProductName = reader["product_name"].ToString(),
                            Base64Image = reader["Listing_B64"].ToString(),
                        };
                        products.Add(product);
                    }
                }
            }

            // Return the view with the list of products
            return View(products);
        }

        public ActionResult Sell()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Seller")
            {
                TempData["Message"] = "You need to log in as a seller to perform this action.";
                return RedirectToAction("Login", "Home");
            }
            try
            {
                // Open the database connection
                myConnection.Open();

                // Get the logged-in seller's staff_id from the session
                int staffId = Convert.ToInt32(Session["UserId"]);

                // Query to fetch listings for the logged-in seller
                string query = @"
            SELECT  p.product_id,
                   p.product_name, 
                   CONCAT('R ', p.list_price) AS formatted_price, 
                   b.brand_name, 
                   c.category_name, 
                   p.Listing_B64
            FROM production.products p
            INNER JOIN production.brands b ON p.brand_id = b.brand_id
            INNER JOIN production.categories c ON p.category_id = c.category_id
            WHERE p.staff_id = @StaffId";  // Filter by the logged-in user's staff_id

                // Prepare the SQL command
                SqlCommand command = new SqlCommand(query, myConnection);
                command.Parameters.AddWithValue("@StaffId", staffId);  // Add staffId parameter to the query

                // Execute the query and retrieve the results
                SqlDataReader reader = command.ExecuteReader();
                var productList = new List<ProductViewModel>();

                while (reader.Read())
                {
                    var product = new ProductViewModel
                    {
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        ProductName = reader["product_name"].ToString(),
                        FormattedPrice = reader["formatted_price"].ToString(),
                        BrandName = reader["brand_name"].ToString(),
                        CategoryName = reader["category_name"].ToString(),
                        Base64Image = reader["Listing_B64"].ToString(),
                    };
                    productList.Add(product);
                }
                reader.Close();

                // Return view with the product listings
                return View(productList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(new List<ProductViewModel>());
            }
            finally
            {
                myConnection.Close();
            }

        }
        public ActionResult ViewSimilarBikes(string productName, string brandName, decimal price)
        {
            //Initialize list to hold similar products
            var similarProducts = new List<ProductViewModel>();

            
            myConnection.Open();

            // SQL query to find bikes with the same brand or within the price range of R500
            var sqlQuery = @"
                        SELECT b.brand_name, CONCAT('R ', p.list_price) AS formatted_price, p.product_name, p.Listing_B64 
                        FROM production.products p
                        INNER JOIN production.brands b ON p.brand_id = b.brand_id
                        WHERE b.brand_name = @BrandName OR (ABS(p.list_price - @Price) <= 500)";

            // Prepare the SQL command
            using (var command = new SqlCommand(sqlQuery, myConnection))
            {
                command.Parameters.AddWithValue("@BrandName", brandName);
                command.Parameters.AddWithValue("@Price", price);

                // Execute the query and read the results
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new ProductViewModel
                        {
                            BrandName = reader["brand_name"].ToString(),
                            FormattedPrice = reader["formatted_price"].ToString(),
                            ProductName = reader["product_name"].ToString(),
                            Base64Image = reader["Listing_B64"].ToString(),
                        };
                        similarProducts.Add(product);
                    }
                }
            }

            // Close the connection
            myConnection.Close();

            // Return the view with the list of similar products
            return View("SimilarBikes", similarProducts);
        }

        public ActionResult ViewBikesByPriceRange(decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice == null || maxPrice == null)
            {
               
                return View(new List<ProductViewModel>());
            }

            var products = new List<ProductViewModel>();

            var sqlQuery = @"
      SELECT b.brand_name, CONCAT('R ', p.list_price) AS formatted_price, p.product_name, p.Listing_B64 
      FROM production.products p
      INNER JOIN production.brands b ON p.brand_id = b.brand_id
      WHERE p.list_price BETWEEN @MinPrice AND @MaxPrice";

            using (var command = new SqlCommand(sqlQuery, myConnection))
            {
                command.Parameters.AddWithValue("@MinPrice", minPrice.Value);
                command.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);

                myConnection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new ProductViewModel
                        {
                            BrandName = reader["brand_name"].ToString(),
                            FormattedPrice = reader["formatted_price"].ToString(),
                            ProductName = reader["product_name"].ToString(),
                            Base64Image = reader["Listing_B64"].ToString(),
                        };
                        products.Add(product);
                    }
                }
            }

            return View(products);
        }

        [HttpPost]
        public ActionResult DoDeleteListing(int productId)
        {


            try
            {
                myConnection.Open();

                // Delete command
                SqlCommand deleteCommand = new SqlCommand("DELETE FROM production.products WHERE product_id = @ProductId", myConnection);
                deleteCommand.Parameters.AddWithValue("@ProductId", productId);

                deleteCommand.ExecuteNonQuery();
                TempData["Message"] = "Listing deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
            }
            finally
            {
                myConnection.Close();
            }

            return RedirectToAction("Sell");
        }
        public ActionResult AddListing()
        {
            var model = new ProductViewModel
            {
                Brands = GetBrands(),  // Fetch brands
                Categories = GetCategories()  // Fetch categories
            };
            return View(model);
        }
        //methods to get the brands and categories 
        private IEnumerable<SelectListItem> GetBrands()
        {
            var brands = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(Globals.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT brand_id, brand_name FROM production.brands", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    brands.Add(new SelectListItem
                    {
                        Value = reader["brand_id"].ToString(),
                        Text = reader["brand_name"].ToString()
                    });
                }
            }
            return brands;
        }
        private IEnumerable<SelectListItem> GetCategories()
        {
            var categories = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(Globals.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("SELECT category_id, category_name FROM production.categories", connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    categories.Add(new SelectListItem
                    {
                        Value = reader["category_id"].ToString(),
                        Text = reader["category_name"].ToString()
                    });
                }
            }
            return categories;
        }
        [HttpPost]
        public ActionResult DoAddListing(ProductViewModel model, string Base64Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current seller's staff_id
                    int staffId = Convert.ToInt32(Session["UserId"]);



                    myConnection.Open();

                    // Insert the new product listing into the database
                    SqlCommand myCommand = new SqlCommand(
                        "INSERT INTO production.products (product_name, brand_ID, category_ID, model_year, list_price, staff_id, Listing_B64, ListedDate) " +
                        "VALUES (@ProductName, @BrandId, @CategoryId, @ModelYear, @ListPrice, @StaffId, @ImageData, GETDATE())",
                        myConnection);

                    myCommand.Parameters.AddWithValue("@ProductName", model.ProductName);
                    myCommand.Parameters.AddWithValue("@BrandId", model.BrandId);
                    myCommand.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                    myCommand.Parameters.AddWithValue("@ModelYear", model.ModelYear);
                    myCommand.Parameters.AddWithValue("@ListPrice", model.ListPrice);
                    myCommand.Parameters.AddWithValue("@StaffId", staffId);
                    myCommand.Parameters.AddWithValue("@ImageData", Base64Image ?? (object)DBNull.Value);

                    myCommand.ExecuteNonQuery();

                    ViewBag.Message = "Listing added successfully!";
                }
                catch (Exception err)
                {
                    ViewBag.Message = "Error: " + err.Message;
                }
                finally
                {
                    myConnection.Close();
                }

                return RedirectToAction("Sell");  // Redirect to some index or listing page
            }

            // If the model state is not valid, return the same view with the existing model

            return View("AddListing", model);
        }
        public ActionResult EditListing(int id)
        { // Open connection to database
            myConnection.Open();

            // Query to fetch the product by ID
            SqlCommand myCommand = new SqlCommand(
                "SELECT product_name, brand_ID, category_ID, model_year, list_price, Listing_B64 " +
                "FROM production.products WHERE product_id = @ProductId", myConnection);
            myCommand.Parameters.AddWithValue("@ProductId", id);

            SqlDataReader reader = myCommand.ExecuteReader();

            ProductViewModel model = new ProductViewModel();

            if (reader.Read())
            {
                // Fill the ViewModel with data from the database
                model.ProductId = id;
                model.ProductName = reader["product_name"].ToString();
                model.BrandId = Convert.ToInt32(reader["brand_ID"]);
                model.CategoryId = Convert.ToInt32(reader["category_ID"]);
                model.ModelYear = Convert.ToInt32(reader["model_year"]);
                model.ListPrice = Convert.ToDecimal(reader["list_price"]);
                model.Base64Image = reader["Listing_B64"] != DBNull.Value ? reader["Listing_B64"].ToString() : null;
            }

            // Close the connection
            reader.Close();
            myConnection.Close();

            // Populate dropdowns (Brands and Categories)
            model.Brands = GetBrands();
            model.Categories = GetCategories();

            return View(model);
        }
        [HttpPost]
        public ActionResult DoEditListing(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Open connection to database
                    myConnection.Open();

                    // Update the product listing in the database
                    SqlCommand myCommand = new SqlCommand(
                        "UPDATE production.products " +
                        "SET product_name = @ProductName, brand_ID = @BrandId, category_ID = @CategoryId, " +
                        "model_year = @ModelYear, list_price = @ListPrice, Listing_B64 = @ImageData " +
                        "WHERE product_id = @ProductId", myConnection);

                    myCommand.Parameters.AddWithValue("@ProductName", model.ProductName);
                    myCommand.Parameters.AddWithValue("@BrandId", model.BrandId);
                    myCommand.Parameters.AddWithValue("@CategoryId", model.CategoryId);
                    myCommand.Parameters.AddWithValue("@ModelYear", model.ModelYear);
                    myCommand.Parameters.AddWithValue("@ListPrice", model.ListPrice);
                    myCommand.Parameters.AddWithValue("@ImageData", model.Base64Image);
                    myCommand.Parameters.AddWithValue("@ProductId", model.ProductId);

                    myCommand.ExecuteNonQuery();

                    ViewBag.Message = "Listing updated successfully!";
                }
                catch (Exception err)
                {
                    ViewBag.Message = "Error: " + err.Message;
                }
                finally
                {
                    myConnection.Close();
                }

                return RedirectToAction("Sell");  // Redirect to the main listing page after saving
            }

            // If the model state is not valid, return the same view with the existing model
            model.Brands = GetBrands();
            model.Categories = GetCategories();
            return View("EditListing", model);
        }
        //View all bikes available for sale, liste dby other users
        public ActionResult Buy()
        {
            //Check if the user is logged in as a Customer
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Customer")
            {
                TempData["Message"] = "You need to log in as a Customer to perform this action.";
                return RedirectToAction("Login", "Home");
            }

            try
            {
                myConnection.Open();

                var productList = new List<ProductViewModel>();

                string query = @"SELECT p.product_name, CONCAT('R ', p.list_price) AS formatted_price, p.product_id, b.brand_name, c.category_name, p.Listing_B64  
                 FROM production.products p   
                 INNER JOIN production.brands b ON p.brand_id = b.brand_id      
                 INNER JOIN production.categories c ON p.category_id = c.category_id
                 WHERE p.staff_id IS NOT NULL";

                SqlCommand command = new SqlCommand(query, myConnection);
                SqlDataReader reader = command.ExecuteReader();

                // Populate productList with data from the database
                while (reader.Read())
                {
                    var product = new ProductViewModel
                    {
                        ProductName = reader["product_name"].ToString(),
                        FormattedPrice = reader["formatted_price"].ToString(),
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        BrandName = reader["brand_name"].ToString(),
                        CategoryName = reader["category_name"].ToString(),
                        Base64Image = reader["Listing_B64"].ToString(),
                    };
                    productList.Add(product);
                }
                reader.Close();

                // Retrieve brands and categories
                var brands = GetBrands();
                var categories = GetCategories();

                // Populate ProductFilterViewModel
                var model = new ProductFilterViewModel
                {
                    Products = productList,   // List of products
                    Brands = brands,          // List of brands
                    Categories = categories    // List of categories
                };

                return View(model); // Pass the correct model type to the view
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(new ProductFilterViewModel { Products = new List<ProductViewModel>() }); // Return an empty ProductFilterViewModel in case of error
            }
            finally
            {
                myConnection.Close();
            }
        }
        public ActionResult SearchBuy(string searchTerm, int? brandId, int? categoryId)
        {
            // Initialize a list to hold your products
            var products = new List<ProductViewModel>();

            myConnection.Open();

            // Define your SQL command with parameters
            var sqlQuery = @"
    SELECT b.brand_name, CONCAT('R ', p.list_price) AS formatted_price, p.product_name, p.Listing_B64, c.category_name 
    FROM production.products p
    INNER JOIN production.brands b ON p.brand_id = b.brand_id  
    INNER JOIN production.categories c ON p.category_id = c.category_id 
    WHERE 1 = 1"; 

            //search term filtering
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sqlQuery += " AND (b.brand_name LIKE @SearchInput OR p.product_name LIKE @SearchInput)";
            }

            // brand filtering if selected
            if (brandId.HasValue && brandId.Value > 0)
            {
                sqlQuery += " AND p.brand_id = @BrandId";
            }

            //category filtering if selected
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                sqlQuery += " AND p.category_id = @CategoryId";
            }

            decimal priceInput;
            bool applyPriceFilter = false;

            // Check if the search term is a valid decimal for price filtering
            if (decimal.TryParse(searchTerm, out priceInput))
            {
                sqlQuery += " OR p.list_price BETWEEN @PriceLower AND @PriceUpper";
                applyPriceFilter = true;
            }

            using (var command = new SqlCommand(sqlQuery, myConnection))
            {
                // Add parameters to the SQL command
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    command.Parameters.AddWithValue("@SearchInput", "%" + searchTerm.Trim() + "%");
                }

                if (brandId.HasValue && brandId.Value > 0)
                {
                    command.Parameters.AddWithValue("@BrandId", brandId.Value);
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    command.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                }

                if (applyPriceFilter)
                {
                    command.Parameters.AddWithValue("@PriceLower", priceInput - 500);
                    command.Parameters.AddWithValue("@PriceUpper", priceInput + 500);
                }

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var product = new ProductViewModel
                        {
                            BrandName = reader["brand_name"].ToString(),
                            FormattedPrice = reader["formatted_price"].ToString(),
                            ProductName = reader["product_name"].ToString(),
                            Base64Image = reader["Listing_B64"].ToString(), 
                        };
                        products.Add(product);
                    }
                }
            }

            // Get the list of brands and categories for the filter
            var brands = GetBrands();
            var categories = GetCategories();

            // Populate the ProductFilterViewModel
            var model = new ProductFilterViewModel
            {
                Products = products, // List of filtered products
                Brands = brands,     // List of brands
                Categories = categories, // List of categories
                BrandId = brandId,  
                CategoryId = categoryId 
            };

            // Return the view with the filtered products
            return View("Buy", model); // Return the ProductFilterViewModel to the Buy view
        }
        //login page with its logic 
        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                myConnection.Open();

                // Query for Customer
                string customerQuery = "SELECT customer_id, first_name, last_name FROM sales.customers WHERE email = @Email AND CPassword = @Password";
                SqlCommand customerCommand = new SqlCommand(customerQuery, myConnection);
                customerCommand.Parameters.AddWithValue("@Email", model.Email);
                customerCommand.Parameters.AddWithValue("@Password", model.Password);

                SqlDataReader customerReader = customerCommand.ExecuteReader();
                if (customerReader.Read())
                {
                    // Customer login successful,stores the user as a customer for this sassion
                    Session["UserType"] = "Customer";
                    Session["UserId"] = customerReader["customer_id"];
                    Session["UserName"] = customerReader["first_name"].ToString() + " " + customerReader["last_name"].ToString();

                    return RedirectToAction("Index", "Home");
                }
                customerReader.Close();

                // Query for Seller (Staff)
                string sellerQuery = "SELECT staff_id, first_name, last_name FROM sales.staffs WHERE email = @Email AND S_Password = @Password";
                SqlCommand sellerCommand = new SqlCommand(sellerQuery, myConnection);
                sellerCommand.Parameters.AddWithValue("@Email", model.Email);
                sellerCommand.Parameters.AddWithValue("@Password", model.Password);

                SqlDataReader sellerReader = sellerCommand.ExecuteReader();
                if (sellerReader.Read())
                {
                    // Seller login successful, stores the user as a seller for this sassion
                    Session["UserType"] = "Seller";
                    Session["UserId"] = sellerReader["staff_id"];
                    Session["UserName"] = sellerReader["first_name"].ToString() + " " + sellerReader["last_name"].ToString();

                    return RedirectToAction("Index", "Home");
                }
                sellerReader.Close();

                // If no match is found in either table
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
                return View(model);
            }
            finally
            {
                myConnection.Close();
            }
        }
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear(); // Clear all session data
            return RedirectToAction("Index", "Home");
        }
        //pages nested under register button from nav bar ↓
        public ActionResult AddCustomer()
        {
            return View();
        }
        public ActionResult DoAddCustomer(string first_name, string last_name, string phone, string email, string street, string city, string state, string zip_code, string CPassword, bool? RegisteredAsSeller)
        {
            try
            {
                myConnection.Open();

                // Step 1: Insert into the customers table
                SqlCommand myCommand = new SqlCommand(
                    "INSERT INTO sales.customers (first_name, last_name, phone, email, street, city, [state], zip_code, CPassword)" +
                    " VALUES (@first_name, @last_name, @phone, @email, @street, @city, @state, @zip_code, @CPassword)",
                    myConnection);

                myCommand.Parameters.AddWithValue("@first_name", first_name);
                myCommand.Parameters.AddWithValue("@last_name", last_name);
                myCommand.Parameters.AddWithValue("@phone", phone);
                myCommand.Parameters.AddWithValue("@email", email);
                myCommand.Parameters.AddWithValue("@street", street);
                myCommand.Parameters.AddWithValue("@city", city);
                myCommand.Parameters.AddWithValue("@state", state);
                myCommand.Parameters.AddWithValue("@zip_code", zip_code);
                myCommand.Parameters.AddWithValue("@CPassword", CPassword);

                myCommand.ExecuteNonQuery();

                // Variable to store the seller's ID
                int sellerID = 0;

                // Step 2: Check if the user is registering as a seller
                if (RegisteredAsSeller == true)
                {
                    SqlCommand sellerCommand = new SqlCommand(
                        "INSERT INTO sales.staffs (first_name, last_name, phone, email, store_id, active, manager_id, S_Password)" +
                        " OUTPUT INSERTED.staff_id" + 
                        " VALUES (@first_name, @last_name, @phone, @email, 4, 1, 13, @S_Password)",
                        myConnection);

                    sellerCommand.Parameters.AddWithValue("@first_name", first_name);
                    sellerCommand.Parameters.AddWithValue("@last_name", last_name);
                    sellerCommand.Parameters.AddWithValue("@phone", phone);
                    sellerCommand.Parameters.AddWithValue("@email", email);
                    sellerCommand.Parameters.AddWithValue("@S_Password", CPassword); // Reusing customer password

                    // Execute query and get the inserted seller's ID
                    sellerID = (int)sellerCommand.ExecuteScalar();
                }

                // Step 3: Update the customer record with the seller ID, if seller ID exists
                if (sellerID > 0)
                {
                    SqlCommand updateCustomerCommand = new SqlCommand(
                        "UPDATE sales.customers SET seller_id = @seller_id WHERE email = @email",
                        myConnection);

                    updateCustomerCommand.Parameters.AddWithValue("@seller_id", sellerID);
                    updateCustomerCommand.Parameters.AddWithValue("@email", email);

                    updateCustomerCommand.ExecuteNonQuery();
                }

                // Display success message
                ViewBag.Message = "Customer registration successful.";
                if (RegisteredAsSeller == true)
                {
                    ViewBag.Message += " You have also been registered as a seller.";
                }
            }
            catch (Exception err)
            {
                ViewBag.Message = "Error: " + err.Message;
            }
            finally
            {
                myConnection.Close();
            }

            return View("Index");
        }
        public ActionResult AddSeller()
        {
            ViewBag.CountryList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "United States", Value = "US" },
                new SelectListItem { Text = "South Africa", Value = "RSA" },
                new SelectListItem { Text = "United Kingdom", Value = "UK" }
            }, "Value", "Text");

            return View();
        }
        public ActionResult DoAddSeller(string first_name, string last_name, string phone, string email, string S_Password)
        {


            try
            {
                SqlCommand myCommand = new SqlCommand(
                    "INSERT INTO sales.staffs (first_name, last_name, phone, email, store_id, active, manager_id, S_Password)" +
                    " VALUES (@first_name, @last_name, @phone, @email, 4, 1, 13, @S_Password)",
                    myConnection);

                myCommand.Parameters.AddWithValue("@first_name", first_name);
                myCommand.Parameters.AddWithValue("@last_name", last_name);
                myCommand.Parameters.AddWithValue("@phone", phone);
                myCommand.Parameters.AddWithValue("@email", email);
                myCommand.Parameters.AddWithValue("@S_Password", S_Password);
                

                myConnection.Open();
                myCommand.ExecuteNonQuery();
            }
            catch (Exception err)
            {
                ViewBag.Message = "Error: " + err.Message;
            }
            finally
            {
                myConnection.Close();
            }

            return View("Index");
        }

        public ActionResult MyBikes()
        {
            // Check if the user is logged in as a Seller
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Seller")
            {
                TempData["Message"] = "You need to log in as a seller to perform this action.";
                return RedirectToAction("Login", "Home");
            }

            try
            {
                // Open the database connection
                myConnection.Open();

                // Get the logged-in seller's staff_id from the session
                int staffId = Convert.ToInt32(Session["UserId"]);

                // Query to fetch bikes that are either for sale or have been bought by the logged-in seller
                string query = @"
            SELECT p.product_id,
                   p.product_name, 
                   CONCAT('R ', p.list_price) AS formatted_price, 
                   b.brand_name, 
                   c.category_name, 
                   p.Listing_B64
            FROM production.products p
            INNER JOIN production.brands b ON p.brand_id = b.brand_id
            INNER JOIN production.categories c ON p.category_id = c.category_id
            LEFT JOIN sales.orders o ON p.product_id = o.product_id
            WHERE p.staff_id = @StaffId OR (o.customer_id = @StaffId)";

                // Prepare the SQL command
                SqlCommand command = new SqlCommand(query, myConnection);
                command.Parameters.AddWithValue("@StaffId", staffId); // Add staffId parameter to the query

                // Execute the query and retrieve the results
                SqlDataReader reader = command.ExecuteReader();
                var productList = new List<ProductViewModel>();

                while (reader.Read())
                {
                    var product = new ProductViewModel
                    {
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        ProductName = reader["product_name"].ToString(),
                        FormattedPrice = reader["formatted_price"].ToString(),
                        BrandName = reader["brand_name"].ToString(),
                        CategoryName = reader["category_name"].ToString(),
                        Base64Image = reader["Listing_B64"].ToString(),
                    };
                    productList.Add(product);
                }
                reader.Close();

                // Return view with the bike listings
                return View(productList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(new List<ProductViewModel>());
            }
            finally
            {
                myConnection.Close();
            }
        }

        // Lists containg all info from top of index page
        public ActionResult Buyers(int page = 1, int pageSize = 100)
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Seller")
            {
                TempData["Message"] = "You need to log in as a seller to perform this action.";
                return RedirectToAction("Login", "Home");
            }
            try
            {//query
                myConnection.Open();

                string query = "SELECT DISTINCT sales.customers.first_name, sales.customers.last_name, sales.customers.email, " +
                    "COUNT(sales.orders.customer_id) AS NumberOfOrders, " +
                    "CONCAT(sales.staffs.first_name, ' ', sales.staffs.last_name) AS SalesStaff, " +
                    "sales.stores.store_name, sales.orders.order_date " +
                    "FROM sales.customers " +
                    "INNER JOIN sales.orders ON sales.orders.customer_id = sales.customers.customer_id " +
                    "INNER JOIN sales.staffs ON sales.orders.staff_id = sales.staffs.staff_id " +
                    "INNER JOIN sales.stores ON sales.stores.store_id = sales.orders.store_id " +
                    "GROUP BY sales.customers.first_name, sales.customers.last_name, sales.customers.email, " +
                    "sales.staffs.first_name, sales.staffs.last_name, sales.stores.store_name, sales.orders.order_date ";

                // Apply sorting based on user choice
                if (HomeController.SortOrderFilter == "store_name")
                {
                    query += "ORDER BY sales.stores.store_name";
                }
                else // Default to sorting by order date
                {
                    query += "ORDER BY sales.orders.order_date";
                }


                SqlCommand myCommand = new SqlCommand(query,myConnection);
                SqlDataReader myReader = myCommand.ExecuteReader();
                var buyerList = new List<BuyerViewModel>();
                while (myReader.Read())
                {
                    BuyerViewModel buyer = new BuyerViewModel
                    {
                        first_Name = myReader["first_name"].ToString(),
                        last_Name = myReader["last_name"].ToString(),
                        email = myReader["email"].ToString(),
                        order_date = new OrderViewModel { order_date = Convert.ToDateTime(myReader["order_date"]) },
                        store_name = new StoreViewModel { store_name = myReader["store_name"].ToString() },
                        first_name = new StaffViewModel { first_name = myReader["SalesStaff"].ToString() }
                    };
                    buyerList.Add(buyer);
                }

                // Pagination
                var totalRecords = buyerList.Count;
                var pagedBuyers = buyerList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModel = new BuyerViewModel
                {
                    Buyers = pagedBuyers,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
                };

                return View(viewModel);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                ViewBag.Status = 0;
                return View(new BuyerViewModel());
            }
            finally
            {
                myConnection.Close();
            }

        }
        //filtering the buyers List
        public static string SortOrderFilter = "order_date"; // Default sorting by Order Date
        public ActionResult SetSortOrder(string sortOrder)
        {
            // Update the static field to store the user's choice
            HomeController.SortOrderFilter = sortOrder;
            return RedirectToAction("Buyers"); // Redirect to the Buyers action
        }
        public ActionResult Sellers()
        {
            if (Session["UserType"] == null || Session["UserType"].ToString() != "Seller")
            {
                TempData["Message"] = "You need to log in as a seller to perform this action.";
                return RedirectToAction("Login", "Home");
            }
            try
            {
                myConnection.Open();

                string query = "SELECT DISTINCT sales.staffs.first_name, sales.staffs.last_name, sales.staffs.email, sales.staffs.phone " +
                               "FROM sales.staffs " +
                               "WHERE manager_id = 13";

                SqlCommand myCommand = new SqlCommand(query, myConnection);
                SqlDataReader myReader = myCommand.ExecuteReader();
                var staffList = new List<StaffViewModel>();

                while (myReader.Read())
                {
                    StaffViewModel staff = new StaffViewModel
                    {
                        first_name = myReader["first_name"].ToString(),
                        last_name = myReader["last_name"].ToString(),
                        email = myReader["email"].ToString(),
                    };
                    staffList.Add(staff);
                }

                // Create a view model and assign the staffList to Staffs
                var viewModel = new StaffViewModel
                {
                    Staffs = staffList
                };

                return View(viewModel);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                ViewBag.Status = 0;
                return View(new StaffViewModel());  // Return an empty view model on error
            }
            finally
            {
                myConnection.Close();
            }
        }
        public ActionResult Bikes()
        {
            return View();
        }
        public ActionResult BikeStores()
        {
            return View();
        }
        public ActionResult Prices()
        {
            return View();
        }
        public ActionResult AddToOrder(int productId)
        {
            try
            {
                // Open the database connection
                myConnection.Open();

                // Get the logged-in user's customer ID from the session
                int customerId = Convert.ToInt32(Session["UserId"]);

                // First, fetch product details from the Product table using the productId
                string fetchProductQuery = @"
            SELECT product_id, product_name, brand_id, category_id, model_year, list_price, Listing_B64, staff_id, ListedDate    
            FROM production.products
            WHERE product_id = @ProductID AND staff_id IS NOT NULL";

                SqlCommand fetchCommand = new SqlCommand(fetchProductQuery, myConnection);
                fetchCommand.Parameters.AddWithValue("@ProductID", productId);

                SqlDataReader reader = fetchCommand.ExecuteReader();

                // Assuming only one product is fetched, read the details
                if (reader.Read())
                {
                    var productName = reader["product_name"].ToString();
                    var brandId = Convert.ToInt32(reader["brand_id"]);
                    var categoryId = Convert.ToInt32(reader["category_id"]);
                    var modelYear = Convert.ToInt32(reader["model_year"]);
                    var listPrice = Convert.ToDecimal(reader["list_price"]);
                    var base64Image = reader["Listing_B64"].ToString();
                    var StaffID = Convert.ToInt32(reader["staff_id"]);
                    var LDate = Convert.ToDateTime(reader["ListedDate"]);

                    reader.Close();

                    // Now, insert the fetched product details into the PurchaseOrder table
                    string insertQuery = @"
                INSERT INTO PurchaseOrder
                (product_id, product_name, brand_id, category_id, model_year, list_price, staff_id, Listing_B64, ListedDate, customer_id)
                VALUES
                (@ProductID, @ProductName, @BrandID, @CategoryID, @ModelYear, @ListPrice, @StaffID, @ListingB64, @ListedDate, @CustomerId)";

                    SqlCommand insertCommand = new SqlCommand(insertQuery, myConnection);

                    // Set the parameters for the INSERT statement
                    insertCommand.Parameters.AddWithValue("@ProductID", productId);
                    insertCommand.Parameters.AddWithValue("@ProductName", productName);
                    insertCommand.Parameters.AddWithValue("@BrandID", brandId);
                    insertCommand.Parameters.AddWithValue("@CategoryID", categoryId);
                    insertCommand.Parameters.AddWithValue("@ModelYear", modelYear);
                    insertCommand.Parameters.AddWithValue("@ListPrice", listPrice);
                    insertCommand.Parameters.AddWithValue("@StaffID", StaffID);
                    insertCommand.Parameters.AddWithValue("@ListingB64", base64Image);
                    insertCommand.Parameters.AddWithValue("@ListedDate", LDate); 
                    insertCommand.Parameters.AddWithValue("@CustomerId", customerId);

                    // Execute the insert command
                    insertCommand.ExecuteNonQuery();
                }

                // Redirect to the Buy action or the cart overview page
                return RedirectToAction("Buy");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Index");
            }
            finally
            {
                myConnection.Close();
            }
        }
        public ActionResult PurchaseOrder()
        {

            try
            {

                myConnection.Open();


                int CustomerID = Convert.ToInt32(Session["UserId"]);


                string query = @"   SELECT p.product_name, p.list_price ,  p.product_id, b.brand_name, c.category_name, p.Listing_B64   
                                    FROM PurchaseOrder p    
                                    INNER JOIN production.brands b ON p.brand_id = b.brand_id       
                                    INNER JOIN production.categories c ON p.category_id = c.category_id       
                                    WHERE customer_id = @CustomerID";

                // Prepare the SQL command
                SqlCommand command = new SqlCommand(query, myConnection);
                command.Parameters.AddWithValue("@CustomerID", CustomerID);

                // Execute the query and retrieve the results
                SqlDataReader reader = command.ExecuteReader();
                var PurchaseOrderList = new List<PurchaseOrderViewModel>();

                while (reader.Read())
                {
                    var PurchaseOrder = new PurchaseOrderViewModel
                    {

                        ProductName = reader["product_name"].ToString(),
                        ListPrice = Convert.ToDecimal(reader["list_price"]),
                        BrandName = reader["brand_name"].ToString(),
                        CategoryName = reader["category_name"].ToString(),
                        Base64Image = reader["Listing_B64"].ToString(),

                    };
                    PurchaseOrderList.Add(PurchaseOrder);
                }
                reader.Close();

                // Return the view with the product listings
                return View(PurchaseOrderList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(new List<PurchaseOrderViewModel>());
            }
            finally
            {
                myConnection.Close();
            }


        }
        [HttpPost]
        public ActionResult RemoveFromPurchaseOrder(int productID)
        {
            try
            {
                myConnection.Open();

                // Define the SQL query to remove the item from the customer's purchase order
                SqlCommand command = new SqlCommand("DELETE FROM PurchaseOrder WHERE product_id = @productID AND customer_id = @CustomerId", myConnection);
                command.Parameters.AddWithValue("@productID", productID);
                command.Parameters.AddWithValue("@CustomerId", Convert.ToInt32(Session["UserId"]));

                command.ExecuteNonQuery();
                TempData["Message"] = "Item removed from the purchase order!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
            }
            finally
            {
                myConnection.Close();
            }

            return RedirectToAction("PurchaseOrder");
        }
        public ActionResult CompleteOrder(List<PurchaseOrderViewModel> model, int? staff_id)
            {
                    if (staff_id == null)
                    {
                        ModelState.AddModelError("", "Staff ID is missing.");
                        return View(model);
                    }
            if (ModelState.IsValid)
                {
                    
                        myConnection.Open();
                        var transaction = myConnection.BeginTransaction();

                        try
                        {
                            int customerId = Convert.ToInt32(Session["UserId"]);

                            // Create a new order
                            string orderQuery = @"INSERT INTO sales.orders (customer_id, order_status, order_date, required_date, store_id, staff_id) 
                                      VALUES (@CustomerID, 4, @OrderDate, @OrderDate, 4, @staff_id);
                                      SELECT SCOPE_IDENTITY();"; // Get the new OrderID
                            var orderCommand = new SqlCommand(orderQuery, myConnection, transaction);

                            orderCommand.Parameters.AddWithValue("@CustomerID", customerId);
                            orderCommand.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                            orderCommand.Parameters.AddWithValue("@staff_id", staff_id);
                            int newOrderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                            // Loop through the cart items and add them to OrderItems table
                            foreach (var purchaseOrder in model) // Now using 'model' which is a list of cart items
                            {
                                string orderItemQuery = @"INSERT INTO sales.order_items (order_id, product_id, quantity, list_price, discount) 
                                                  VALUES (@OrderID, @ProductID, @Quantity, @Price, 0.00)";
                                var orderItemCommand = new SqlCommand(orderItemQuery, myConnection, transaction);
                                orderItemCommand.Parameters.AddWithValue("@OrderID", newOrderId);
                                orderItemCommand.Parameters.AddWithValue("@ProductID", purchaseOrder.productID);
                                orderItemCommand.Parameters.AddWithValue("@Quantity", 1);
                                orderItemCommand.Parameters.AddWithValue("@Price", purchaseOrder.ListPrice);
                                orderItemCommand.ExecuteNonQuery();

                                // Optionally, remove the item from the Products table if needed
                                string removeProductQuery = @"DELETE FROM production.products WHERE product_id = @ProductID";
                                var removeProductCommand = new SqlCommand(removeProductQuery, myConnection, transaction);
                                removeProductCommand.Parameters.AddWithValue("@ProductID", purchaseOrder.productID);
                                removeProductCommand.ExecuteNonQuery();
                            }

                            // Commit the transaction
                            transaction.Commit();
                            return RedirectToAction("Index"); // Redirect to an order confirmation page
                        }
                        catch (Exception ex)
                        {
                            // Handle error and rollback
                            transaction.Rollback();
                            Console.WriteLine(ex.Message);
                            ModelState.AddModelError("", "An error occurred while processing your order.");
                        }
                    
                }

                return View("index"); // Return the same view if there was an error
            }
        //(right) bikes detail lists
        [HttpGet]
        public ActionResult BikeDetails(string category)
        {
            string query = @"
    SELECT p.product_id, p.product_name, p.list_price, p.model_year, p.Listing_B64, b.brand_name AS BrandName, c.category_name AS CategoryName
    FROM production.products p 
    INNER JOIN production.brands b ON p.brand_id = b.brand_id
    INNER JOIN production.categories c ON p.category_id = c.category_id
    WHERE b.brand_name = @category AND p.Listing_B64 IS NOT NULL";

            List<ProductViewModel> products = new List<ProductViewModel>();

            
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConnection))
                {
                    cmd.Parameters.AddWithValue("@category", category);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new ProductViewModel
                            {
                                ProductId = Convert.ToInt32(reader["product_id"]),
                                ProductName = reader["product_name"].ToString(),
                                ListPrice = Convert.ToDecimal(reader["list_price"]),
                                ModelYear = Convert.ToInt32(reader["model_year"]),
                                Base64Image = reader["Listing_B64"].ToString(),
                                BrandName = reader["BrandName"].ToString(),
                                CategoryName = reader["CategoryName"].ToString()
                            });
                        }
                    }
                }
            

            // Pass the products list to the view
            return View(products);
        }
        [HttpGet]
        public ActionResult ViewProduct(int id)
        {
            string query = @"
    SELECT p.product_id, p.product_name, p.list_price, p.model_year, p.Listing_B64, b.brand_name AS BrandName, c.category_name AS CategoryName
    FROM production.products p 
    INNER JOIN production.brands b ON p.brand_id = b.brand_id
    INNER JOIN production.categories c ON p.category_id = c.category_id
    WHERE p.product_id = @id AND p.Listing_B64 IS NOT NULL";

            ProductViewModel product = null;

            
                myConnection.Open();
                using (SqlCommand cmd = new SqlCommand(query, myConnection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new ProductViewModel
                            {
                                ProductId = Convert.ToInt32(reader["product_id"]),
                                ProductName = reader["product_name"].ToString(),
                                ListPrice = Convert.ToDecimal(reader["list_price"]),
                                ModelYear = Convert.ToInt32(reader["model_year"]),
                                Base64Image = reader["Listing_B64"].ToString(),
                                BrandName = reader["BrandName"].ToString(),
                                CategoryName = reader["CategoryName"].ToString()
                            };
                        }
                    }
                }
            

            if (product == null)
            {
                return HttpNotFound(); // Handle not found case
            }

            return View(product); // Return the single product to the view
        }
        //(left) Summaries 
        private int ExecuteScalarQuery(string query)
        {
            int result = 0;

            // Open the database connection
            
                myConnection.Open();

                // Create the SQL command to execute
                using (var command = new SqlCommand(query, myConnection))
                {
                    // Execute the query and get the result
                    result = (int)command.ExecuteScalar();
                }

                myConnection.Close();
            

            return result;
        }
        [HttpGet]
        public ActionResult NewStocks()
        {
            string query = "SELECT COUNT(product_id) FROM production.products WHERE MONTH(ListedDate) = MONTH(GETDATE()) AND YEAR(ListedDate) = YEAR(GETDATE());";
            int result = ExecuteScalarQuery(query);

            var viewModel = new SummariesViewModel
            {
                SummaryResult = result,
                QueryType = "NewStocks" 
            };
            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult ListedForSale()
        {
            string query = "SELECT SUM(s.quantity) FROM production.stocks s";
            int result = ExecuteScalarQuery(query);
            var viewModel = new SummariesViewModel
            {
                SummaryResult = result,
                QueryType = "ListedForSale" // You can use this in the view to conditionally display content
            };
            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult TotalSold()
        {
            string query = "SELECT SUM(sales.order_items.quantity) AS 'Quantity Sold' FROM sales.order_items";
            int result = ExecuteScalarQuery(query);
            var viewModel = new SummariesViewModel
            {
                SummaryResult = result,
                QueryType = "TotalSold" // You can use this in the view to conditionally display content
            };
            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult SalesPerBrand()
        {
            string query = @"
        SELECT b.brand_name, SUM(o.quantity) AS 'Sold Bikes' 
        FROM production.brands b 
        INNER JOIN production.products p ON b.brand_id = p.brand_id 
        INNER JOIN sales.order_items o ON p.product_id = o.product_id 
        GROUP BY b.brand_name";

            List<SalesPerBrandViewModel> salesPerBrandList = new List<SalesPerBrandViewModel>();

            
                myConnection.Open();

                using (var command = new SqlCommand(query, myConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SalesPerBrandViewModel brandSales = new SalesPerBrandViewModel
                            {
                                BrandName = reader["brand_name"].ToString(),
                                SoldBikes = Convert.ToInt32(reader["Sold Bikes"])
                            };
                            salesPerBrandList.Add(brandSales);
                        }
                    }
                }
            

            // Create the main view model and set the list of sales per brand
            var viewModel = new SummariesViewModel
            {
                SalesPerBrandList = salesPerBrandList,
                QueryType = "SalesPerBrand"  // To identify this query type in the view
            };

            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult ListingsPBrand()
        {
            string query = @"
        SELECT b.brand_name, COUNT(s.quantity) AS 'Quantity' 
        FROM production.brands b 
        INNER JOIN production.products p ON b.brand_id=p.brand_id 
        INNER JOIN production.stocks s ON s.product_id= p.product_id 
        GROUP BY b.brand_name";

            List<ListingsPerBrandViewModel> listingsPerBrandList = new List<ListingsPerBrandViewModel>();

            
                myConnection.Open();

                using (var command = new SqlCommand(query, myConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ListingsPerBrandViewModel listing = new ListingsPerBrandViewModel
                            {
                                BrandName = reader["brand_name"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"])
                            };
                            listingsPerBrandList.Add(listing);
                        }
                    }
                }
            

            var viewModel = new SummariesViewModel
            {
                ListingsPerBrandList = listingsPerBrandList,
                QueryType = "ListingsPBrand"
            };

            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult AvgSalesPBrand()
        {
            string query = @"
        SELECT b.brand_name, AVG(p.list_price) AS 'Average Sale Price' 
        FROM production.products p 
        INNER JOIN production.brands b ON b.brand_id = p.brand_id 
        GROUP BY b.brand_name";

            List<AvgSalesPerBrandViewModel> avgSalesPerBrandList = new List<AvgSalesPerBrandViewModel>();

            
                myConnection.Open();

                using (var command = new SqlCommand(query, myConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AvgSalesPerBrandViewModel avgSale = new AvgSalesPerBrandViewModel
                            {
                                BrandName = reader["brand_name"].ToString(),
                                AverageSalePrice = Convert.ToDecimal(reader["Average Sale Price"])
                            };
                            avgSalesPerBrandList.Add(avgSale);
                        }
                    }
                }
            

            var viewModel = new SummariesViewModel
            {
                AvgSalesPerBrandList = avgSalesPerBrandList,
                QueryType = "AvgSalesPBrand"
            };

            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult TotalsPBrandCategory()
        {
            string query = @"
        SELECT b.brand_name,  
        SUM(CASE WHEN c.category_name = 'Mountain Bikes' THEN s.quantity ELSE 0 END) AS Mountain,
        SUM(CASE WHEN c.category_name = 'Road Bikes' THEN s.quantity ELSE 0 END) AS Road,
        SUM(CASE WHEN c.category_name = 'Electric Bikes' THEN s.quantity ELSE 0 END) AS Electric,
        SUM(CASE WHEN c.category_name = 'Children Bicycles' THEN s.quantity ELSE 0 END) AS Children,
        SUM(CASE WHEN c.category_name = 'Comfort Bicycles' THEN s.quantity ELSE 0 END) AS Comfort,
        SUM(CASE WHEN c.category_name = 'Cruisers Bicycles' THEN s.quantity ELSE 0 END) AS Cruisers,
        SUM(CASE WHEN c.category_name = 'Cyclocross Bicycles' THEN s.quantity ELSE 0 END) AS Cyclocross
        FROM production.stocks s 
        INNER JOIN production.products p ON s.product_id = p.product_id
        INNER JOIN production.categories c ON p.category_id = c.category_id
        INNER JOIN production.brands b ON p.brand_id = b.brand_id 
        GROUP BY b.brand_name 
        ORDER BY b.brand_name";

            List<TotalsPerBrandCategoryViewModel> totalsPerBrandCategoryList = new List<TotalsPerBrandCategoryViewModel>();

            
                myConnection.Open();

                using (var command = new SqlCommand(query, myConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TotalsPerBrandCategoryViewModel total = new TotalsPerBrandCategoryViewModel
                            {
                                BrandName = reader["brand_name"].ToString(),
                                Mountain = Convert.ToInt32(reader["Mountain"]),
                                Road = Convert.ToInt32(reader["Road"]),
                                Electric = Convert.ToInt32(reader["Electric"]),
                                Children = Convert.ToInt32(reader["Children"]),
                                Comfort = Convert.ToInt32(reader["Comfort"]),
                                Cruisers = Convert.ToInt32(reader["Cruisers"]),
                                Cyclocross = Convert.ToInt32(reader["Cyclocross"])
                            };
                            totalsPerBrandCategoryList.Add(total);
                        }
                    }
                }
            

            var viewModel = new SummariesViewModel
            {
                TotalsPerBrandCategoryList = totalsPerBrandCategoryList,
                QueryType = "TotalsPBrandCategory"
            };

            return View("Summaries", viewModel);
        }
        [HttpGet]
        public ActionResult StockAtStores()
        {
            string query = @"
    SELECT sa.store_name, SUM(s.quantity) AS 'TotalQuantity' 
    FROM production.stocks s 
    INNER JOIN sales.stores sa ON sa.store_id = s.store_id 
    GROUP BY sa.store_name";

            // Create a list to store results
            List<StoreStockViewModel> storeStockList = new List<StoreStockViewModel>();

            myConnection.Open();

            using (var command = new SqlCommand(query, myConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Populate the ViewModel with data from the query
                        StoreStockViewModel storeStock = new StoreStockViewModel
                        {
                            StoreName = reader["store_name"].ToString(),
                            TotalQuantity = Convert.ToInt32(reader["TotalQuantity"])
                        };
                        storeStockList.Add(storeStock);
                    }
                }
            }

            // Create the main view model and set the list of store stock data
            var viewModel = new SummariesViewModel
            {
                StoreStockList = storeStockList,
                QueryType = "StockAtStores" // To identify this query type in the view
            };

            return View("Summaries", viewModel);
        }
       








    }
}