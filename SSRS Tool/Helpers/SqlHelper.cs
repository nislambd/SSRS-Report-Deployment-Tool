using System;
using GalaSoft.MvvmLight;

namespace SSRSDeployTool.Helpers
{
    
    public static class SqlHelper
    {
        public enum SqlAuthenticationType
        {
            WindowsAuthenticated,
            SqlCredentials
        }
        
        public static string PrepareConnectionString(string sqlServerName, string sqlUserName, string sqlPassword, SqlAuthenticationType sqlAuthenticationType)
        {
            if (string.IsNullOrEmpty(sqlServerName)) throw new ArgumentNullException("Sql Server name is missing");

            var connectionString = $"Server={sqlServerName}; Trusted_Connection=True;";

            if (sqlAuthenticationType == SqlAuthenticationType.SqlCredentials)
            {
                if (string.IsNullOrEmpty(sqlUserName)) throw new ArgumentNullException("Sql User name is missing");

                if (string.IsNullOrEmpty(sqlPassword)) throw new ArgumentNullException("Sql Password is missing");

                connectionString = $"Server={sqlServerName}; User Id={sqlUserName}; Password={sqlPassword}";
            }

            return connectionString;
        }
    }

   
}