using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;
using System.Web.Security;

namespace SimpleEcommerce.Controllers
{
    public class AdminAccountController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Admin Login Page
        public ActionResult AdminLogin()
        {
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        public ActionResult AdminLogin(string email, string password)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT * FROM AdminUsers WHERE Email = @Email";
                AdminUser admin = dbConnection.QueryFirstOrDefault<AdminUser>(query, new { Email = email });

                if (admin != null && admin.PasswordHash == password)
                {
                    FormsAuthentication.SetAuthCookie(admin.Email, false);
                    Session["AdminId"] = admin.Id;
                    Session["AdminName"] = admin.Name;
                    Session["AdminEmail"] = admin.Email;

                    return RedirectToAction("Dashboard", "Admin"); // Redirect to Admin Dashboard
                }

                ViewBag.Error = "Invalid email or password.";
                return View();
            }
        }

        // GET: Admin Logout
        public ActionResult AdminLogout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("AdminLogin");
        }
    }
}
