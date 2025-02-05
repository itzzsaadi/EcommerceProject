using System.Linq;
using System.Web.Mvc;
using SimpleEcommerce.Models;
using System.Data;
using Dapper;
using System.Web.Security;

namespace SimpleEcommerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly Database _db = new Database();

        // GET: Login Page
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login User 
        [HttpPost]
        public ActionResult Login(string email, string password, bool rememberMe = false)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string query = "SELECT Id, Name, Email, PasswordHash FROM Users WHERE Email = @Email";
                User user = dbConnection.QueryFirstOrDefault<User>(query, new { Email = email });

                if (user != null)
                {
                    if (string.Equals(user.PasswordHash, password)) // Simple string comparison
                    {
                        // Set authentication cookie with Remember Me option
                        FormsAuthentication.SetAuthCookie(user.Email, rememberMe);

                        // Store user data in Session (for use across pages)
                        Session["UserId"] = user.Id;
                        Session["UserName"] = user.Name;
                        Session["UserEmail"] = user.Email;

                        return RedirectToAction("Index", "Home");
                    }
                }

                ViewBag.Error = "Invalid email or password.";
                return View();
            }
        }

        // GET: Signup Page
        public ActionResult Signup()
        {
            return View();
        }

        // POST: Register New User
        [HttpPost]
        public ActionResult Signup(User model)
        {
            using (IDbConnection dbConnection = _db.GetConnection())
            {
                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                int existingUserCount = dbConnection.ExecuteScalar<int>(checkUserQuery, new { Email = model.Email });

                if (existingUserCount > 0)
                {
                    ViewBag.Error = "Email already exists. Try another one.";
                    return View();
                }

                string insertQuery = "INSERT INTO Users (Name, Email, PasswordHash) VALUES (@Name, @Email, @Password)";
                dbConnection.Execute(insertQuery, new { model.Name, model.Email, Password = model.PasswordHash });

                return RedirectToAction("Login");
            }
        }

        // GET: Logout User
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear(); // Clears all session data
            return RedirectToAction("Index", "Home");
        }
    }
}
