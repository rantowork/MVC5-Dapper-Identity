using System.Data.SqlClient;
using DapperIdentity.Core.Interfaces;

namespace DapperIdentity.Data.Connections
{
    /// <summary>
    /// Connection factory responsible for creating a new SqlConnection.  Should be injected into the repository.  I prefer injecting it in a manner in which the 
    /// connection string can be passed in (in Ninject it is Bind.To.WithConstructorArgument)
    /// </summary>
    public class SqlConnectionFactory : IConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            return connection;
        }
    }
}
