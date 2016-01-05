using System.Data.SqlClient;

namespace DapperIdentity.Core.Interfaces
{
    public interface IConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
