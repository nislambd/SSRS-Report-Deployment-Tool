using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;

namespace SSRSDeployTool.Model
{
    public class File : ViewModelBase
    {
        public string Name { get; set; }
        
        public string Type { get; set; }
        
        public Icon Icon { get; set; }

        public ImageSource Bitmap
        {
            get
            {
                Bitmap bitmap = Icon.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();

                return Imaging.CreateBitmapSourceFromHBitmap(
                          hBitmap, IntPtr.Zero, Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
            }
        }


        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(); }
        }

        private string _busyIcon;
        public string BusyIcon
        {
            get { return _busyIcon; }
            set { _busyIcon = value; RaisePropertyChanged(); }
        }
        
        private Boolean _isProcessing;
        public Boolean IsProcessing
        {
            get {return _isProcessing;}
            set { _isProcessing = value; RaisePropertyChanged(); }
        }

        public string FullPath { get; set; }

        public string Extension { get; set; }

        public Boolean IsDeployed { get; set; }

    }
}