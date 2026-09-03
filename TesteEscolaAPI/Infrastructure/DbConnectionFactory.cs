using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace TesteEscolaAPI.Infrastructure
{
    public class DbConnectionFactory:IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory()
        {
            
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ConfigurationErrorsException("Error obtaining connection string");
            }
        }

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
