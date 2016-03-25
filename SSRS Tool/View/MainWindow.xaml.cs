using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace SSRSDeployTool.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, (NotificationMessage msg) =>
            {
                if (msg.Notification == "ShowReportFolders")
                {
                    var reportListWindow = new ReportsFolderList {};
                    reportListWindow.Owner = this;
                    var result = reportListWindow.ShowDialog();

                    //if (result != null && !result.Value)
                    //{
                    //    this.d
                    //}
                }
            });
        }
    }
}
