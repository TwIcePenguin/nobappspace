using System;
using System.Collections.Generic;

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
