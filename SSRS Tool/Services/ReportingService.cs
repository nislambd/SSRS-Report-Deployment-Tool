using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.SqlServer.ReportingServices2005;
using SSRSDeployTool.Helpers;

//using CatalogItem = Microsoft.SqlServer.ReportingServices2010.CatalogItem;
//using Warning = Microsoft.SqlServer.ReportingServices2010.Warning;

namespace SSRSDeployTool.Services
{
    public class ReportingService: IReportingService
    {
        public List<string> DeployReport(string fileName, string folderName, string url, string userName, string password)
        {
            ReportingService2005 reportingService = GetReportingServiceInstance(url, userName, password);

            //read rdl file  

            var fileStream = File.OpenRead(fileName);

            byte[] reportDefinition = new Byte[fileStream.Length];

            fileStream.Read(reportDefinition, 0, (int)fileStream.Length);

            fileStream.Close();

            //upload the file

            Warning[] warnings = null;

            //CatalogItem c = reportingService.CreateCatalogItem("Report",Path.GetFileName(fileName) , $"/{folderName}", true, reportDefinition, null, out warnings);

            var fileinfo = new FileInfo(fileName);
            
            if (fileinfo.Extension == ".rdl")
            {
                warnings = reportingService.CreateReport(Path.GetFileNameWithoutExtension(fileName), $"/{folderName}", true,
                    reportDefinition, null);
            }
            else
            {
                reportingService.CreateResource(Path.GetFileName(fileName), $"/{folderName}", true, reportDefinition,
                    FileHelper.GetMimeType(fileinfo), null);
            }

            return warnings?.Select(warning => warning.Message).ToList();
        }

        private ReportingService2005 GetReportingServiceInstance(string url, string userName, string password)
        {
            return new ReportingService2005
            {
                Credentials =
                                string.IsNullOrEmpty(userName)
                                    ? CredentialCache.DefaultCredentials
                                    : new NetworkCredential(userName, password),
                Url = url + "/reportservice2005.asmx"
            };
        }

        public List<string> GetFolderList(string url, string userName, string password)
        {
            var folders = new List<string>();

            var reportingService = GetReportingServiceInstance(url, userName, password);

            var catalogItems = reportingService.ListChildren("/", false);

            foreach (var item in catalogItems)
            {
                if (item.Type == ItemTypeEnum.Folder)
                {
                    folders.Add(item.Name);
                }
            }

            return folders;
        }
    }
}
