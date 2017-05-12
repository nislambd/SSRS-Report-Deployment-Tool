using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using SSRSDeployTool.Helpers;
using SSRSDeployTool.Model;
using SSRSDeployTool.Services;
using File = SSRSDeployTool.Model.File;

namespace SSRSDeployTool.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        #region Form Field Properties

        #region SQL Properties

        public string SqlServerName { get; set; }
        
        private int _sqlAuthenticateTypeSelectedIndex;

        public int SqlAuthenticateTypeSelectedIndex
        {
            get { return _sqlAuthenticateTypeSelectedIndex; }
            set
            {
                _sqlAuthenticateTypeSelectedIndex = value;
                RaisePropertyChanged("SqlCredentialIsEnabled");
            }
        }

        public string SqlUserName { get; set; }

        public Boolean SqlCredentialIsEnabled => SqlAuthenticateTypeSelectedIndex == 1;

        //public string SqlPassword { get; set; }

        private ObservableCollection<Model.File> _files;

        public ObservableCollection<Model.File> Files
        {
            get { return _files; }
            set { _files = value; RaisePropertyChanged(); }
        }

        private Model.File _selectedFile;

        public Model.File SelectedFile
        {
            get { return _selectedFile; }
            set { _selectedFile = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<LogMessage> _logMessages; 
        public ObservableCollection<LogMessage> LogMessages {
            get { return _logMessages;}
                set { _logMessages = value; RaisePropertyChanged(); }
        }
        #endregion



        #region SSRS Properties

        public string SsrsServerUrl{ get; set; }
        
        private int _ssrsAuthenticateTypeSelectedIndex;

        public int SsrsAuthenticateTypeSelectedIndex
        {
            get { return _ssrsAuthenticateTypeSelectedIndex; }
            set
            {
                _ssrsAuthenticateTypeSelectedIndex = value;
                RaisePropertyChanged("SsrsCredentialIsEnabled");
            }
        }

        public string SsrsUserName { get; set; }

        public Boolean SsrsCredentialIsEnabled => SsrsAuthenticateTypeSelectedIndex == 1;

        //public string SsrsPassword { get; set; }

        #endregion

        private Boolean _isBusyTesting;

        public Boolean IsBusyTesting
        {
            get { return _isBusyTesting; }
            set
            {
                _isBusyTesting = value;
                IsDeploying = value;
                RaisePropertyChanged();
            }
        }

        private Boolean _isDeploying;

        public Boolean IsDeploying
        {
            get { return _isDeploying;}
            set { _isDeploying = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<string> _folderList;

        public ObservableCollection<string> FolderList
        {
            get { return _folderList; }
            set { this._folderList = value; RaisePropertyChanged(); }
        }

        private string _reportFolder;

        public string ReportFolder
        {
            get { return _reportFolder; }
            set { this._reportFolder = value; RaisePropertyChanged(); }
        }
        #endregion


        #region Command Properties

        public ICommand SqlTestCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public ICommand DeployCommand { get; set; }

        public ICommand CloseCommand { get; set; }

        private ICommand _PreviewDropCommand;
        public ICommand PreviewDropCommand
        {
            get { return _PreviewDropCommand ?? (_PreviewDropCommand = new RelayCommand<object>(HandlePreviewDrop)); }
            set
            {
                _PreviewDropCommand = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RemoveFileCommand { get; set; }

        public ICommand ClearLogCommand { get; set; }
        #endregion

        #region Services

        private readonly ISqlService _sqlService;

        private readonly IReportingService _reportingService;

        private readonly IDialogService _diallogService;

        #endregion  

        public MainWindowViewModel()
        {
            SqlServerName = "(local)";

            SsrsServerUrl = "http://localhost/ReportServer";

            SqlAuthenticateTypeSelectedIndex = 0;

            CloseCommand = new RelayCommand(CloseWindow, () => true);

            SqlTestCommand = new RelayCommand<object>(TestSqlConnection);

            LoadCommand = new RelayCommand(LoadFiles,()=> true);

            DeployCommand = new RelayCommand<object>(DeployFiles);

            RemoveFileCommand = new RelayCommand(RemoveFile,() => true);

            ClearLogCommand = new RelayCommand(()=> LogMessages.Clear());

            Files = new ObservableCollection<File>();

            LogMessages = new ObservableCollection<LogMessage>();

            if (IsInDesignMode)
            {
                Files = new ObservableCollection<File>
                {
                    new Model.File() {Name = "File1", Type = "SQL Files", Status = "Status", BusyIcon = "ClockOutlined"},
                    new Model.File() {Name = "File2", Type = "SQL Files", Status = "Status", BusyIcon = "ClockOutlined"},
                    new Model.File() {Name = "File3", Type = "Report Files", Status = "Status",  BusyIcon = "ClockOutlined"}
                };

                LogMessages.Add(new LogMessage() {Message = "Log message 1", EventDate = DateTime.Now} );
            }
            else{
                
                LoadFiles();
            }

            _folderList = new ObservableCollection<string> ();

        }

        [PreferredConstructor]
        public MainWindowViewModel(ISqlService sqlService, IReportingService reportingService, IDialogService dialogService):this()
        {
            _sqlService = sqlService;
            _reportingService = reportingService;
            _diallogService = dialogService;
        }

        private async void TestSqlConnection(object parameter)
        {
            try
            {
                var sqlPasswordBox = parameter as PasswordBox;
                string connectionString = GetConnectionString(sqlPasswordBox);

                var sqlService = _sqlService;

                IsBusyTesting = true;

                try
                {
                    LogEvent($"Connecting to {SqlServerName}");

                    var result = await sqlService.TestConnection(connectionString);

                    if (result)
                    {
                        LogEvent($"Test connecting with {SqlServerName} successful!");
                        await _diallogService.ShowMessage("Test Successful!!!", "Sucess");
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"Test connecting with {SqlServerName} Failed!", "Error");
                    await _diallogService.ShowMessage($"Test Failed, Error:{ex.Message}", "Error");
                }


            }
            catch (Exception ex)
            {
                LogEvent( $"Error in testing connectivity! Message : {ex.Message}", "Error");
            }

            IsBusyTesting = false;
        }
        
        private void LoadFiles()
        {
            try
            {
                Files.Clear();

                var currentDirectory = Environment.CurrentDirectory;

                GetFilesInDirectory(currentDirectory, @"*.sql|*.rdl|*.jpg|*.png|*.bmp");
            }
            catch (Exception exception)
            {
                LogEvent($"Error in loading files. {exception.Message}","Error");
            }
            

        }

        private void RemoveFile()
        {
            Files.Remove(SelectedFile);
        }

        private async void GetFilesInDirectory(string currentDirectory,string pattern)
        {
            var wildcards = pattern.Split('|');

            foreach (var filter in wildcards)
            {
                var fileInfos = await Task.Run(() => FileHelper.GetFilesInDirectory(currentDirectory, filter));

                foreach (var fileInfo in fileInfos)
                {
                    Files.Add(new Model.File()
                    {
                        Name = fileInfo.Name,
                        Type = FileHelper.GetFileType(fileInfo.Name),
                        Status = "",
                        FullPath = fileInfo.FullName,
                        Icon = Icon.ExtractAssociatedIcon(fileInfo.FullName),
                        Extension = fileInfo.Extension
                    });
                }
            }
        }

        private async void DeployFiles(object parameter)
        {
            if (Files.Count == 0) LogEvent("Nothing to do!", "Error");

            IsDeploying = true;

            var values = (object[]) parameter;
            var sqlPasswordBox = values[0] as PasswordBox;
            var ssrsPasswordBox = values[1] as PasswordBox;

            if (Files.Any(file => file.Extension.Equals(".rdl") ||
                                file.Extension.Equals(".jpg") ||
                                file.Extension.Equals(".png") ||
                                file.Extension.Equals(".gif") ||
                                file.Extension.Equals(".bmp")))
            {
                await DeployRdlFiles(ssrsPasswordBox);
            }

            if (Files.Any(file => file.Extension.Equals(".sql")))
            {
                await DeploySqlFiles(sqlPasswordBox);
            }
           
            IsDeploying = false;
        }

        //dropinng files/folders into the listview
        private void HandlePreviewDrop(object dropedObject)
        {
            IDataObject ido = dropedObject as IDataObject;

            //string[] formats = ido.GetFormats();

            var fileDropped = ido?.GetData("FileDrop");

            if (fileDropped != null)
            {
                foreach (var fileName in (string[]) fileDropped)
                {
                    if (FileHelper.IsDirectory(fileName))
                    {
                        GetFilesInDirectory(fileName, @"*.rdl|*.sql|*.jpg|*.png|*.bmp");
                    }
                    else
                    {
                        var fileInfo = new FileInfo(fileName);
                        Files.Add(new Model.File()
                        {
                            Name = fileInfo.Name,
                            Type = FileHelper.GetFileType(fileInfo.Name),
                            Status = "",
                            FullPath = fileInfo.FullName,
                            Icon = Icon.ExtractAssociatedIcon(fileInfo.FullName),
                            Extension = fileInfo.Extension
                        });
                    }
                }
            }
        }

        public void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        private async Task DeploySqlFiles(PasswordBox sqlPasswordBox)
        {
            var sqlFiles = Files.Where(file => file.Extension.Equals(".sql"));

            var sqlService = _sqlService;

            LogEvent($"Connecting to SQL Server {SqlServerName}");

            var connection = await sqlService.GetConnection(GetConnectionString(sqlPasswordBox));

            LogEvent($"Connected to SQL Server {SqlServerName}");

            foreach (var sqlFile in sqlFiles)
            {
                LogEvent($"Deploying {sqlFile.Name}");

                SelectedFile = sqlFile;

                sqlFile.BusyIcon = "Spinner";
                sqlFile.IsProcessing = true;
                sqlFile.Status = "Processing...";
                try

                {
                    await sqlService.ExecuteSqlScript(sqlFile.FullPath, connection);
                    sqlFile.Status = "Success!";
                    sqlFile.BusyIcon = "Check";
                }
                catch (Exception ex)
                {
                    sqlFile.Status = $"Falied!, {ex.Message}";
                    sqlFile.BusyIcon = "Close";
                    LogEvent($"Failed to deploy {sqlFile.Name}","Error");
                    LogEvent(ex.Message,"Error");
                }
                
                sqlFile.IsProcessing = false;
            }
            
        }

        private async Task DeployRdlFiles(PasswordBox ssrsPasswordBox)
        {
            if (string.IsNullOrEmpty(SsrsServerUrl))
            {
                await _diallogService.ShowMessage("SSRS Server URL is required!", "Error...");
                return;
            }

            try
            {
                var reportingService = _reportingService;

                if (_folderList.Count == 0)
                {
                    LogEvent("Fetching list of report folders.");

                    var folders =
                        await
                            Task.Run(
                                () =>
                                    reportingService.GetFolderList(SsrsServerUrl, SsrsUserName, ssrsPasswordBox.Password));

                    _folderList = new ObservableCollection<string>(folders);

                    //if (_folderList.Count > 0 && string.IsNullOrEmpty(ReportFolder))
                    //{
                    //    ReportFolder = _folderList[0];
                    //}
                }

                Messenger.Default.Send(new NotificationMessage("ShowReportFolders"));
                
                //TODO What if the user wants to cancel deployment from the pop-up window
                if (string.IsNullOrEmpty(ReportFolder)) return;
            
                var reportFiles = Files.Where(file => file.Extension.Equals(".rdl") ||
                                                      file.Extension.Equals(".jpg") ||
                                                      file.Extension.Equals(".png") ||
                                                      file.Extension.Equals(".gif") ||
                                                      file.Extension.Equals(".bmp"));
                
                foreach (var reportFile in reportFiles)
                {
                    if (reportFile.IsDeployed)
                    {
                        LogEvent($"Skipping {reportFile.Name} as it is already deployed!");   
                        continue;
                    }

                    LogEvent($"Deploying {reportFile.Name}");

                    SelectedFile = reportFile;

                    reportFile.BusyIcon = "Spinner";
                    reportFile.IsProcessing = true;
                    reportFile.Status = "Processing...";

                    try
                    {
                        List<string> warnings = await Task.Run(()=> reportingService.DeployReport(reportFile.FullPath, ReportFolder, SsrsServerUrl, SsrsUserName,
                            ssrsPasswordBox.Password));

                        if (warnings != null)
                            foreach (var warning in warnings)
                            {
                                LogEvent($"Warning: {warning}");
                            }

                        reportFile.Status = "Success!";
                        reportFile.BusyIcon = "Check";
                        reportFile.IsDeployed = true;
                    }
                    catch (Exception ex)
                    {
                        reportFile.Status = $"Falied!, {ex.Message}";
                        reportFile.BusyIcon = "Close";
                        reportFile.IsDeployed = false;
                        LogEvent($"Failed to deploy {reportFile.Name}", "Error");
                        LogEvent(ex.Message, "Error");
                    }
                    
                    reportFile.IsProcessing = false;
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error in deploying reports. {ex.Message}");
            }
        }

        private void LogEvent(string message, string type="Information")
        {
            LogMessages.Add(new LogMessage() { EventDate = DateTime.Now, Type = type, Message = message });
        }

        private string GetConnectionString(PasswordBox sqlPasswordBox)
        {
            return SqlHelper.PrepareConnectionString(SqlServerName, SqlUserName, sqlPasswordBox?.Password, (SqlHelper.SqlAuthenticationType)SqlAuthenticateTypeSelectedIndex);
        }
    }
}
