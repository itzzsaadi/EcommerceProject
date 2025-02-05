using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;

namespace SimpleEcommerce.Controllers
{
    public class OrderController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Checkout (Only for Logged-in Users)
        public ActionResult Checkout()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect guests to login page
            }

            int userId = (int)Session["UserId"];

            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = @"
                    SELECT Cart.Id, Cart.UserId, Cart.ProductId, Cart.Quantity, 
                           Products.Name, Products.Price, Products.ImageUrl 
                    FROM Cart
                    INNER JOIN Products ON Cart.ProductId = Products.Id
                    WHERE Cart.UserId = @UserId";

                List<CartItemViewModel> cartItems = dbConnection.Query<CartItemViewModel>(query, new { UserId = userId }).ToList();

                if (!cartItems.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                ViewBag.TotalAmount = cartItems.Sum(item => item.Price * item.Quantity);
                return View(cartItems);
            }
        }

        // POST: Place Order
        [HttpPost]
        public ActionResult PlaceOrder(string CustomerName, string PhoneNumber, string City, string Address, string ZipCode, string PaymentMethod)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Require login before placing order
            }

            int userId = (int)Session["UserId"];

            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string cartQuery = "SELECT * FROM Cart WHERE UserId = @UserId";
                List<Cart> cartItems = dbConnection.Query<Cart>(cartQuery, new { UserId = userId }).ToList();

                if (!cartItems.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                // Get all product details in one query
                string productQuery = "SELECT * FROM Products WHERE Id IN @ProductIds";
                var productIds = cartItems.Select(c => c.ProductId).ToList();
                List<Product> products = dbConnection.Query<Product>(productQuery, new { ProductIds = productIds }).ToList();

                decimal totalAmount = cartItems.Sum(item =>
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    return product != null ? product.Price * item.Quantity : 0;
                });

                // Insert Order
                string orderQuery = "INSERT INTO Orders (UserId, TotalAmount, OrderDate) VALUES (@UserId, @TotalAmount, GETDATE()); SELECT SCOPE_IDENTITY();";
                int? orderId = dbConnection.QuerySingleOrDefault<int?>(orderQuery, new { UserId = userId, TotalAmount = totalAmount });

                if (orderId == null)
                {
                    throw new Exception("Failed to insert order.");
                }

                // Insert Order Details
                foreach (var item in cartItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product == null) continue;

                    string orderDetailsQuery = @"
                    INSERT INTO OrderDetails (OrderId, ProductId, Quantity, Price, CustomerName, PhoneNumber, City, Address, ZipCode, PaymentMethod)
                    VALUES (@OrderId, @ProductId, @Quantity, @TotalPrice, @CustomerName, @PhoneNumber, @City, @Address, @ZipCode, @PaymentMethod)";

                    dbConnection.Execute(orderDetailsQuery, new
                    {
                        OrderId = orderId.Value,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        TotalPrice = product.Price * item.Quantity, // Corrected Price Calculation
                        CustomerName,
                        PhoneNumber,
                        City,
                        Address,
                        ZipCode,
                        PaymentMethod
                    });
                }

                // Clear Cart
                string clearCartQuery = "DELETE FROM Cart WHERE UserId = @UserId";
                dbConnection.Execute(clearCartQuery, new { UserId = userId });

                return RedirectToAction("OrderConfirmation", new { orderId = orderId.Value });
            }
        }

        // GET: Order Confirmation
        public ActionResult OrderConfirmation(int orderId)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT * FROM Orders WHERE Id = @OrderId";
                Order order = dbConnection.QueryFirstOrDefault<Order>(query, new { OrderId = orderId });

                if (order == null)
                {
                    return HttpNotFound();
                }

                return View(order);
            }
        }
    }
}
