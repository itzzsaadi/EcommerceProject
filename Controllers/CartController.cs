using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;

namespace SimpleEcommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Cart - View Cart Items
        public ActionResult Cart()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Require login to access cart
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

                return View(cartItems);
            }
        }

        // POST: Cart/Add (Adds a product to the cart)
        [HttpPost]
        public ActionResult Add(int productId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Require login before adding to cart
            }

            int userId = (int)Session["UserId"];

            using (IDbConnection dbConnection = _db.GetConnection())
            {
                // Check if product is already in the cart
                string checkQuery = "SELECT * FROM Cart WHERE UserId = @UserId AND ProductId = @ProductId";
                var existingItem = dbConnection.QueryFirstOrDefault<Cart>(checkQuery, new { UserId = userId, ProductId = productId });

                if (existingItem != null)
                {
                    // Update quantity if product already exists
                    string updateQuery = "UPDATE Cart SET Quantity = Quantity + 1 WHERE Id = @Id";
                    dbConnection.Execute(updateQuery, new { Id = existingItem.Id });
                }
                else
                {
                    // Insert new product into cart
                    string insertQuery = "INSERT INTO Cart (UserId, ProductId, Quantity) VALUES (@UserId, @ProductId, 1)";
                    dbConnection.Execute(insertQuery, new { UserId = userId, ProductId = productId });
                }
            }

            return RedirectToAction("Cart");
        }

        // POST: Cart/Update (Updates quantity of a product in the cart)
        [HttpPost]
        public ActionResult Update(int cartId, int quantity)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Require login
            }

            if (quantity <= 0)
            {
                return Remove(cartId);
            }

            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "UPDATE Cart SET Quantity = @Quantity WHERE Id = @CartId";
                dbConnection.Execute(query, new { CartId = cartId, Quantity = quantity });
            }

            return RedirectToAction("Cart");
        }

        // POST: Cart/Remove (Removes a product from the cart)
        [HttpPost]
        public ActionResult Remove(int cartId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Account"); // Require login
            }

            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "DELETE FROM Cart WHERE Id = @CartId";
                dbConnection.Execute(query, new { CartId = cartId });
            }

            return RedirectToAction("Cart");
        }
    }
}
