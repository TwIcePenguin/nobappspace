using NOBApp.GoogleData;
using NOBApp.Sports;
using RegisterDmSoftConsoleApp.Configs;
using RegisterDmSoftConsoleApp.DmSoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NOBApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region DllImport
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        public static MainWindow? Instance { get; private set; }
        public static DmSoftCustomClassName? dmSoft;

        /// <summary>
        /// 所有視窗
        /// </summary>
        public static List<NOBDATA> AllNobWindowsList => nobWindowsList;
        static List<NOBDATA> nobWindowsList = new();

        public static string MainState = "";
        public static readonly Dictionary<string, string> stateAMapping = new()
        {
            { "A0 98", "戰鬥中" },
            { "F0 B8", "待機" },
            { "F0 F8", "對話與結束戰鬥" },
        };
        public static Point TxtToResolution = new();
        static ComboBox? Resolution;
        Thickness oThickness;
        double winHeight;
        bool PA_isExpanded = false, PB_isExpanded = false;
        public static int OrinX = 0;
        public static int OrinY = 0;

        public Setting CodeSetting = new();
        public MainWindow()
        {
            InitializeComponent();
            AdminRelauncher();
            企鵝之野望.Title = $"企鵝之野望 {VersionInfo.Version} KEY = {Tools.GetSerialNumber()}";

            var registerDmSoftDllResult = RegisterDmSoft.RegisterDmSoftDll();
            if (!registerDmSoftDllResult)
            {
                throw new Exception("註冊大漠插件失敗");
            }

            dmSoft = new DmSoftCustomClassName();
            if (!Directory.Exists(DmConfig.DmGlobalPath))
            {
                Directory.CreateDirectory(DmConfig.DmGlobalPath);
            }
            UIDefault();
            RegButtonEvent();

            //设置全局路径,设置了此路径后,所有接口调用中,相关的文件都相对于此路径.比如图片,字库等
            dmSoft.SetPath(DmConfig.DmGlobalPath);


            var list = Tools.InitResolution();
            foreach (var item in list)
            {
                CMB_Resolution.Items.Add(item);
            }

#if DEBUG && false
            企鵝專用測試A.Visibility = Visibility.Visible;
#endif

            TB_GamePadName.Text = Tools.GetGamePadStr();
            Resolution = CMB_Resolution;
            Instance = this;
            InitializeTabItems();
        }

        private void InitializeTabItems()
        {
            if (NBTabControl == null)
                return;

            NBTabControl.Items.Clear();

            for (int i = 0; i < 8; i++)
            {
                var tabItem = new TabItem();
                tabItem.Header = "TabItem";
                tabItem.MouseDoubleClick += OnTabFocus;

                var content = new NobMainCodePage();
                tabItem.Content = content;
                content.RootTabItem = tabItem;
                NBTabControl.Items.Add(tabItem);
            }
        }

        private void OnTabFocus(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tabItem)
            {

            }
        }

        public static void SetTitle(string txt)
        {
            if (Instance != null && Instance.企鵝之野望 != null)
            {
                Instance.企鵝之野望.Title = txt;
            }
        }

        public static void RefreshNOBID(ComboBox CB_HID, ComboBox[] comboBoxes)
        {
            Process[] localByName = Process.GetProcesses();
            nobWindowsList.Clear();
            CB_HID.Items.Clear();
            //快速切換.Items.Clear();
            foreach (var item in localByName)
            {
                //  MainNob.Log($"Name : {item.ProcessName} Title : {item.MainWindowTitle}");
                if (item.ProcessName.Contains("nobolHD"))
                {
                    var data = new NOBDATA(item);
                    Debug.WriteLine("新增 : " + data.Hwnd + " : " + data.Proc.Handle);
                    nobWindowsList.Add(data);
                    //快速切換.Items.Add(data.PlayerName);
                    CB_HID.Items.Add(data.PlayerName);
                }
            }
            //  MainNob.Log("共有 : " + nobList.Count);
            UIUpdate.RefreshNOBID_Sec(comboBoxes, nobWindowsList);
        }

        public static Point GetResolutioSize()
        {
            TxtToResolution = new Point();
            if (Resolution == null)
                return TxtToResolution;

            string str = Resolution.Text;
            var array = str.Split(',');
            if (array.Length > 1)
            {
                int.TryParse(array[0], out int x);
                int.TryParse(array[1], out int y);
                TxtToResolution = new Point(x, y);
            }
            return TxtToResolution;
        }

        private void AdminRelauncher()
        {
            if (!IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly()!.Location;

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                }
            }
        }

        private bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.WriteLine("-- Window_Closing --");
            Environment.Exit(0);
        }

        private void UIDefault()
        {
            winHeight = 企鵝之野望.Height;
            企鵝專用測試A.Visibility = 企鵝專用測試B.Visibility = 企鵝專用測試C.Visibility = Visibility.Hidden;


        }

        private void RegButtonEvent()
        {
            企鵝專用測試A.Click += 企鵝專用測試_Click;
            企鵝專用測試B.Click += 企鵝專用測試_Click;
            企鵝專用測試C.Click += 企鵝專用測試_Click;

            Btn_AutoRefresh.Click += Btn_AutoRefresh_Click;
        }

        private void UIRefrshSize(bool 進階腳本開啟, bool 戰鬥輔助開啟)
        {
            int offsetY = 100;
            if (PB_isExpanded != 進階腳本開啟 || PA_isExpanded != 戰鬥輔助開啟)
            {
                PB_isExpanded = 進階腳本開啟;
                PA_isExpanded = 戰鬥輔助開啟;

                double tA = PB_isExpanded ? 300 + offsetY : 0;
                double tB = PA_isExpanded ? 370 + offsetY : 0;
                Thickness nThickness = oThickness;
                nThickness.Top = oThickness.Top + tA;
                企鵝之野望.Height = winHeight + tA + tB;
            }
        }

        private void Button_Click_手把(object sender, RoutedEventArgs e)
        {
            TB_GamePadName.Text = "XInput-1";
        }
        private void 企鵝專用測試_Click(object sender, RoutedEventArgs e)
        {
            var MainNob = NobMainCodePage.MainNob;
            //WebRegistration.TestOnWebReg();
            if (MainNob != null)
            {
                Debug.WriteLine(sender.ToString());
                if (sender.ToString()!.Contains("企鵝A"))
                {
                    if (MainNob.GetTargetIDINT() != -1)
                        Debug.WriteLine("NPC ID=>" + MainNob.GetTargetIDINT());

                    //GetNPCIDs();

                    Debug.WriteLine($"1- {Dis(MainNob.PosX, MainNob.PosY, 14986, 14281)}");
                    Debug.WriteLine($"2- {Dis(MainNob.PosX, MainNob.PosY, 14716, 4492)}");
                    Debug.WriteLine($"3- {Dis(MainNob.PosX, MainNob.PosY, 5051, 4770)}");
                    Debug.WriteLine($"4- {Dis(MainNob.PosX, MainNob.PosY, 5246, 14497)}");

                    //效能測試
                    //PerformanceTest.TestGetColorCopNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    //橘 565ABD
                    var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    //藍 565ABD
                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "565ABD");
                    //紅 6363EE 
                    var c3 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "6363EE");
                    var c4 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "6363EE");
                    var c5 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "5959D8");

                    Debug.WriteLine($"Color : {c1} - {c2} - {c3} - {c4} - {c5}");
                }
            }
        }

        private void Btn_AutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (OrinX == 0 || OrinY == 0)
            {
                string str = CMB_Resolution.Text;
                var array = str.Split(',');
                if (array.Length == 2)
                {
                    int.TryParse(array[0], out OrinX);
                    int.TryParse(array[1], out OrinY);
                    OrinX = OrinX + 16;
                    OrinY = OrinY + 39;
                }
            }

            if (OrinX > 0 || OrinY > 0)
                for (int i = 0; i < nobWindowsList.Count; i++)
                {
                    nobWindowsList[i].MoveWindowTool(i);
                }
        }

        private void MuitOpen_Click(object sender, RoutedEventArgs e)
        {
            多開();
        }

        private void 多開()
        {
            string str = CMB_Resolution.Text;
            if (!string.IsNullOrEmpty(str))
            {
                var array = str.Split(',');
                if (array.Length > 1)
                {
                    Tools.SetResolution(array[0], array[1]);
                }
            }
            if (int.TryParse(OpenGameWindows.Text, out int i))
            {
                Tools.SetGamePad(TB_GamePadName.Text);
                for (int j = 0; j < i; j++)
                {
                    Task.Run(() => Tools.OpenNobMuit()).Wait();
                }
            }
        }

        private void Button_Click_遊戲視窗多開(object sender, RoutedEventArgs e)
        {
            Process.Start("NOBApp.exe");
        }


        private void QuickSelectShowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var name = 快速切換.SelectedItem.ToString();
            //var unob = nobWindowsList.Find(r => r.PlayerName == name);
            //if (unob != null && string.IsNullOrEmpty(name) == false)
            //{
            //    SetForegroundWindow(unob.Proc.MainWindowHandle);
            //    foreach (var item in Process.GetProcesses())
            //    {
            //        Debug.WriteLine($"item -> {item.MainWindowTitle}");
            //        if (item.MainWindowTitle.Contains(name))
            //        {
            //            unob.FoucsNobApp(item);
            //            break;
            //        }
            //    }
            //}
        }

        private void DonateBtn(object sender, RoutedEventArgs e)
        {
            OpenDonatePage();
        }

        private void OpenDonatePage()
        {
            Process myProcess = new();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = @"https://ko-fi.com/icetwpenguin";
            myProcess.Start();
        }

        public static string GetStateADescription(string stateA)
        {
            return stateAMapping.TryGetValue(stateA, out var description) ? description : stateA;
        }
    }
}
