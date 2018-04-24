using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.Threading.Tasks;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ScreenResizeTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
       
        System.Threading.Tasks.Task t;
        private ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        Windows.Storage.StorageFile file = null;

        public MainPage()
        {
            t = new System.Threading.Tasks.Task(LoggerDaemon);
            t.Start();
            this.InitializeComponent();

          
        }

        private void Web_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Then the application is running "normally" after a reboot, then
            // I see log lines from the line below.
            // When it stops working correctly, I do NOT see log messages from 
            // the WebView, but I do see them from Grid.
            LogSizeChanged("WebView", e);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LogSizeChanged("Grid", e);
            double remainingWidth = Windows.UI.Xaml.Window.Current.Bounds.Width;
            // The next 2 lines should trigger a WebView resize when the size changes
            // in the debugger I can see that I am assigning a different size.
            myWebView.Width = remainingWidth;
            myWebView.Height = e.NewSize.Height;
        }

        private void LogSizeChanged(string fromWhere, SizeChangedEventArgs e)
        {
            StringBuilder msg = new StringBuilder();
            msg.Append(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.zzz"));
            msg.Append(" [").Append(fromWhere).Append("] ");
            msg.AppendLine($"SizeChangedEventArgs: {{ PreviousSize:{{Width:{e.PreviousSize.Width},Height:{e.PreviousSize.Height}}}, NewSize:{{Width:{e.NewSize.Width},Height:{e.NewSize.Height}}},Source: {e.OriginalSource}}}");

            messages.Enqueue(msg.ToString());
        }

        public async void LoggerDaemon() {
            while (true)
            {
                if (file != null)
                {
                    if (messages.Count > 0)
                    {
                        using (Stream ws = await file.OpenStreamForWriteAsync())
                        {
                            ws.Seek(0, SeekOrigin.End);
                            while (messages.TryDequeue(out string msg))
                            {
                                byte[] bytes = ASCIIEncoding.ASCII.GetBytes(msg);
                                await ws.WriteAsync(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }
                await System.Threading.Tasks.Task.Delay(1000);
            }
        }

     
        private async void myWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();

            // Dropdown of file types the user can save the file as
            picker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            picker.SuggestedFileName = "log";

            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            file = await picker.PickSaveFileAsync();

            //Windows.Storage.StorageFolder storageFolder =
            //         Windows.Storage.ApplicationData.Current.LocalFolder;
            //file =
            //    await storageFolder.CreateFileAsync("log.txt",
            //        Windows.Storage.CreationCollisionOption.ReplaceExisting);

        }
    }
}
