using System.Collections.Generic;
using Microsoft.SqlServer.ReportingServices2005;

namespace SSRSDeployTool.Services
{
    public interface IReportingService
    {
        List<string> DeployReport(string fileName, string folderName, string url, string userName, string password);
        List<string> GetFolderList(string url, string userName, string password);
    }
}