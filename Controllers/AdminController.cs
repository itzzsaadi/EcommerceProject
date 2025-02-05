using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;

namespace SimpleEcommerce.Controllers
{
    public class AdminController : Controller
    {
        private readonly Database _db = new Database();

        // Restrict access to admins only
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["AdminId"] == null) // If Admin is not logged in
            {
                filterContext.Result = RedirectToAction("AdminLogin", "AdminAccount"); // Redirect to Admin Login
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Admin Dashboard
        public ActionResult Dashboard()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                // Get total number of orders
                int totalOrders = dbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Orders");

                // Get total revenue
                decimal totalRevenue = dbConnection.ExecuteScalar<decimal?>("SELECT SUM(TotalAmount) FROM Orders") ?? 0;

                // Get total number of users (Excluding Admins)
                int totalUsers = dbConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM Users");

                ViewBag.TotalOrders = totalOrders;
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TotalUsers = totalUsers;

                return View();
            }
        }

        // GET: Add Product Page
        public ActionResult AddProduct()
        {
            return View();
        }

        // POST: Add New Product
        [HttpPost]
        public ActionResult AddProduct(Product product)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "INSERT INTO Products (Name, Description, Price, ImageUrl) VALUES (@Name, @Description, @Price, @ImageUrl)";
                dbConnection.Execute(query, product);
            }

            return RedirectToAction("ManageProducts");
        }

        // GET: Edit Product Page
        public ActionResult EditProduct(int id)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT * FROM Products WHERE Id = @ProductId";
                Product product = dbConnection.QueryFirstOrDefault<Product>(query, new { ProductId = id });

                if (product == null)
                {
                    return HttpNotFound();
                }
                return View(product);
            }
        }

        // POST: Update Product
        [HttpPost]
        public ActionResult EditProduct(Product product)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, ImageUrl = @ImageUrl WHERE Id = @Id";
                dbConnection.Execute(query, product);
            }

            return RedirectToAction("ManageProducts");
        }

        // POST: Delete Product
        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                // Ensure product exists before deleting
                string checkQuery = "SELECT COUNT(*) FROM Products WHERE Id = @ProductId";
                int productExists = dbConnection.ExecuteScalar<int>(checkQuery, new { ProductId = id });

                if (productExists > 0)
                {
                    string deleteQuery = "DELETE FROM Products WHERE Id = @ProductId";
                    dbConnection.Execute(deleteQuery, new { ProductId = id });
                }
            }

            return RedirectToAction("ManageProducts");
        }

        // GET: Manage Products
        public ActionResult ManageProducts()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Description, Price, ImageUrl FROM Products";
                List<Product> products = dbConnection.Query<Product>(query).ToList();
                return View(products);
            }
        }

        // GET: Manage Orders
        public ActionResult ManageOrders()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT * FROM Orders";
                List<Order> orders = dbConnection.Query<Order>(query).ToList();
                return View(orders);
            }
        }

        // GET: Order Details
        public ActionResult OrderDetails(int id)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                // Fetch the order details by OrderId
                string orderQuery = "SELECT * FROM Orders WHERE Id = @OrderId";
                Order order = dbConnection.QueryFirstOrDefault<Order>(orderQuery, new { OrderId = id });

                if (order == null)
                {
                    return HttpNotFound();
                }

                // Fetch OrderDetails including customer info and product details
                string orderDetailsQuery = @"
        SELECT 
            OrderDetails.Id, 
            OrderDetails.OrderId, 
            OrderDetails.ProductId, 
            OrderDetails.Quantity, 
            OrderDetails.Price, 
            OrderDetails.CustomerName, 
            OrderDetails.PhoneNumber, 
            OrderDetails.City, 
            OrderDetails.Address, 
            OrderDetails.ZipCode, 
            Products.Name AS ProductName 
        FROM OrderDetails
        INNER JOIN Products ON OrderDetails.ProductId = Products.Id
        WHERE OrderDetails.OrderId = @OrderId";

                List<OrderDetailsViewModel> orderDetails = dbConnection.Query<OrderDetailsViewModel>(orderDetailsQuery, new { OrderId = id }).ToList();

                ViewBag.Order = order;
                return View(orderDetails);
            }
        }


        // GET: Manage Users (Only Normal Users, No Admins)
        public ActionResult ManageUsers()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Email FROM Users";
                List<User> users = dbConnection.Query<User>(query).ToList();
                return View(users);
            }
        }
    }
}
