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
            Messenger.Default.Register<NotificationMessageAction<bool>>(this, msg =>
            {
                if (msg.Notification == "ShowReportFolders")
                {
                    var reportListWindow = new ReportsFolderList {Owner = this};
                    var result = reportListWindow.ShowDialog();
                    msg.Execute(result);
                }
            });
        }
    }
}
