using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;

namespace SimpleEcommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Shop Page (Product Listing)
        public ActionResult Shop()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Description, Price, ImageUrl FROM Products";
                List<Product> products = dbConnection.Query<Product>(query).ToList();
                return View(products);
            }
        }

        // GET: Product Details Page
        public ActionResult Details(int id)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Description, Price, ImageUrl FROM Products WHERE Id = @ProductId";
                Product product = dbConnection.QueryFirstOrDefault<Product>(query, new { ProductId = id });

                if (product == null)
                {
                    return HttpNotFound();
                }
                return View(product);
            }
        }
    }
}
