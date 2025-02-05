using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace SimpleEcommerce.Models
{
    public class Database
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["EcommerceDB"].ConnectionString;

        public IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
