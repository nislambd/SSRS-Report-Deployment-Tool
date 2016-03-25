using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SSRSDeployTool.Helpers
{
    public class FileHelper
    {
        public static List<FileInfo> GetFilesInDirectory(string directoryPath, string pattern="*.*")
        {
            var files = new List<string>(Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories));

            return files.Select(filename => new FileInfo(filename)).ToList();
        }

        public static  string GetMimeType(FileInfo fileInfo)
        {
            string mimeType = "application/unknown";

            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(
                fileInfo.Extension.ToLower()
                );

            if (regKey != null)
            {
                object contentType = regKey.GetValue("Content Type");

                if (contentType != null)
                    mimeType = contentType.ToString();
            }

            return mimeType;
        }

        private static readonly Dictionary<string, string> FileTypesDictionary = new Dictionary<string, string>()
        {
            {"sql", "SQL Files"},
            {"rdl", "SSRS Report Files"},
            {"exe", "Applications"},
            {"jpg", "Images"},
            {"png", "Images"},
            {"gif", "Images"},
            {"dll", "Application Executables"}
            
        };
        public static string GetFileType(string fileName)
        {
            //get file extension
            string extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            if (!string.IsNullOrEmpty(extension) && FileTypesDictionary.ContainsKey(extension.Remove(0, 1)))
            {
                return FileTypesDictionary[extension.Remove(0, 1)];
            }
            return "Others";
        }

        public static bool IsDirectory(string path)
        {
            FileAttributes fileAttributes = File.GetAttributes(path);
            return fileAttributes.HasFlag(FileAttributes.Directory);
        }
    }
}
