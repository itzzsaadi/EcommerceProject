using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;

namespace SimpleEcommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Home Page with Featured Products
        public ActionResult Index()
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Price, ImageUrl FROM Products WHERE Id IN (3, 8, 12, 7);";
                ViewBag.FeaturedProducts = dbConnection.Query<Product>(query).ToList();
            }
            return View();
        }


        // GET: Contact Us Page
        public ActionResult Contact()
        {
            return View();
        }

        // GET: Privacy Policy Page
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}
