using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SSRSDeployTool.Helpers;

namespace SSRSDeployTool.Services
{
    public class SqlService : ISqlService
    {
        public async Task<bool> TestConnection(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                connection.Close();
            }

            return true;
        }

        public async Task<bool> ExecuteSqlScript(string fileName, SqlConnection connection)
        {
            
            var sqlScript = System.IO.File.ReadAllText(fileName);

            //Using SQL Server Management Objects (SMO) 
            if (RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully)
            {
                Server server = new Server(new ServerConnection(connection));
                await Task.Run(() => server.ConnectionContext.ExecuteNonQuery(sqlScript));
            }
            else
            {
                //Using simple command object. 
                sqlScript = sqlScript.Replace("GO", "");
                sqlScript = Regex.Replace(sqlScript, "([/*][*]).*([*][/])", "");
                sqlScript = Regex.Replace(sqlScript, @"\s{2,}", " ");
                using (SqlCommand sqlCommand = new SqlCommand(sqlScript, connection))
                {
                    await Task.Run(()=>sqlCommand.ExecuteNonQuery());
                }
            }

            return true;
        }

        public async Task<SqlConnection> GetConnection(string connectionString)
        {
            var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}
