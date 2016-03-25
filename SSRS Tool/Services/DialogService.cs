using System;
using GalaSoft.MvvmLight.Views;
using System.Threading.Tasks;
using System.Windows;

namespace SSRSDeployTool.Services
{
    public class DialogService: IDialogService
    {
        public Task ShowError(string message, string title, string buttonText, Action afterHideCallback)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowError(Exception error, string title, string buttonText, Action afterHideCallback)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowMessage(string message, string title)
        {
            return Task.Factory.StartNew(()=>MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public Task<bool> ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText,
            Action<bool> afterHideCallback)
        {
            throw new NotImplementedException();
        }

        public Task ShowMessageBox(string message, string title)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowMessage(string message, string title, string buttonConfirmText, string buttonCancelText,
            Action afterHideCallback)
        {
            throw new System.NotImplementedException();
        }

        public Task ShowMessage(string message, string title, string buttonText, Action afterHideCallback)
        {
            throw new System.NotImplementedException();
        }
    }
}