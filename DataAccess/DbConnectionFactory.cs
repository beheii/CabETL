using Microsoft.Data.SqlClient;

namespace CabETL.DataAccess
{
    public interface IDbConnectionFactory
    {
        SqlConnection Create();
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection Create() => new SqlConnection(_connectionString);
    }
}

