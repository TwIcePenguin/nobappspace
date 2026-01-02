using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 全域例外攔截，避免啟動期未處理例外直接結束
            this.DispatcherUnhandledException += (s, e) =>
            {
                LogStartupError("DispatcherUnhandledException", e.Exception);
                MessageBox.Show("程式發生未處理例外:\n" + e.Exception.Message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true; // 嘗試繼續
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                LogStartupError("DomainUnhandledException", e.ExceptionObject as Exception);
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogStartupError("UnobservedTaskException", e.Exception);
                e.SetObserved();
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Enable code page encodings (e.g., BIG5/950) for name/account decoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            base.OnStartup(e);
            //直接顯示主視窗，不做提權自動重啟
            var mw = new MainWindow();
            mw.Show();
        }

        private void LogStartupError(string tag, Exception? ex)
        {
            try
            {
                var path = Path.Combine(Environment.CurrentDirectory, "startup_error.log");
                File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {tag} -> {ex}\n");
            }
            catch (Exception ioex)
            {
                Debug.WriteLine("Log write failed: " + ioex.Message);
            }
        }
    }
}
