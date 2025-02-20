using NOBApp.GoogleData;
using NtpClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp
{
    public partial class MainWindow
    {
        private static WebRegistration webRegistration = new WebRegistration();
        private static UIUpdate uiUpdate = new UIUpdate();

        public static List<NOBDATA> useNobList = new List<NOBDATA>();

        private DateTime GetNetworkTime()
        {
            return NetworkTime.GetNetworkTimeAsync();
        }

    }
}
