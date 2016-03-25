using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SSRSDeployTool.Services
{
    public interface ISqlService
    {
        Task<bool> TestConnection(string connectionString);
        Task<bool> ExecuteSqlScript(string fileName, SqlConnection connection);
        Task<SqlConnection> GetConnection(string connectionString);
    }
}