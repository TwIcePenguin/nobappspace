using Microsoft.VisualBasic.Logging;
using NOBApp.Managers;
using NOBApp.Sports;
using RegisterDmSoftConsoleApp.Configs;
using RegisterDmSoftConsoleApp.DmSoft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NOBApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public static DmSoftCustomClassName? dmSoft;

        // 使用 Instance 存取 GameWindowManager
        public static List<NOBDATA> AllNobWindowsList => Instance?._gameWindowManager.NobWindows ?? new List<NOBDATA>();

        public static string MainState = "";
        public static readonly Dictionary<string, string> stateAMapping = new()
        {
            { "A0 98", "戰鬥中" },
            { "F0 B8", "待機" },
            { "F0 F8", "對話與結束戰鬥" },
        };
        public static Point TxtToResolution = new();
        static TextBox? 主窗狀態;
        static ComboBox? Resolution;
        double winHeight;
        public static int OrinX = 0;
        public static int OrinY = 0;

        public Setting CodeSetting = new();

        private readonly DmSoftManager _dmManager = new();
        private readonly TabManager _tabManager = new();
        private readonly UpdateManager _updateManager = new();
        private readonly GameWindowManager _gameWindowManager = new();

        #region Initialize
        public MainWindow()
        {
            InitializeComponent();
            企鵝之野望.Title = $"企鵝之野望 v{VersionInfo.Version} KEY = {Tools.GetSerialNumber()}";
            Instance = this;

            // 初始化大漠插件
            _dmManager.InitializeDmSoft();
            dmSoft = _dmManager.DmSoft;

            // 初始化解析度下拉
            var list = Tools.InitResolution();
            foreach (var item in list) CMB_Resolution.Items.Add(item);
            Resolution = CMB_Resolution;

            TB_GamePadName.Text = Tools.GetGamePadStr();
            InitUIStatus();
            InitButtonEvent();
            if (winHeight <= 0) winHeight = 企鵝之野望.Height;

            // 初始化分頁
            _tabManager.Initialize(NBTabControl, _gameWindowManager.NobWindows);

            // 更新成功檢查
            _updateManager.CheckUpdateSuccessAndNotify();

            this.Loaded += async (s, e) =>
            {
                await _updateManager.CheckForUpdatesAsync(msg => 狀態訊息(msg, true), latest =>
                {
                    Btn_Update.Content = $"更新至 {latest}";
                    Btn_Update.Visibility = Visibility.Visible;
                });
            };
        }

        private void InitUIStatus()
        {
            主窗狀態 = MainWindowsStatusMsgBox;
            winHeight = 企鵝之野望.Height;
            企鵝專用測試A.Visibility = 企鵝專用測試B.Visibility = 企鵝專用測試C.Visibility = Visibility.Hidden;

#if DEBUG
			企鵝專用測試B.Visibility = 企鵝專用測試A.Visibility = Visibility.Visible;
#endif
        }

        private void InitButtonEvent()
        {
            企鵝專用測試A.Click +=企鵝專用測試_Click;
            企鵝專用測試B.Click +=企鵝專用測試_Click;
            企鵝專用測試C.Click +=企鵝專用測試_Click;

            Btn_AutoRefresh.Click += Btn_AutoRefresh_Click;
            Btn_SetLoad.Click += Btn_SetLoad_Click;
            Btn_SetApply.Click += Btn_SetApply_Click;
            Btn_Update.Click += Btn_Update_Click;
            // 移除不存在的 Btn_MultiOpen (若 XAML 有定義再行加入)
        }

        private void Btn_SetApply_Click(object sender, RoutedEventArgs e)
        {
            if (NBTabControl.SelectedItem is TabItem tabItem && tabItem.Content is NobMainCodePage page && page.MainNob != null)
            {
                page.MainNob.CodeSetting = CodeSetting;
                page.SettingLoadToUI();
            }
        }

        private void Btn_SetLoad_Click(object sender, RoutedEventArgs e)
        {
            if (NBTabControl.SelectedItem is TabItem tabItem && tabItem.Content is NobMainCodePage page && page.MainNob != null)
            {
                Btn_SetLoad.Content = $"取得{page.MainNob.PlayerName}設定";
                CodeSetting = page.MainNob.CodeSetting;
            }
        }

        #endregion Initialize

        #region static public functions
        public static void RefreshNOBID(ComboBox CB_HID, ComboBox[] comboBoxes)
        {
            // 委派到 GameWindowManager (保留舊靜態 API兼容性)
            Instance?._gameWindowManager.RefreshNOBID(CB_HID, comboBoxes);
        }

        public static Point GetResolutioSize()
        {
            TxtToResolution = new Point();
            if (Resolution == null)
                return TxtToResolution;

            var arr = Resolution.Text.Split(',');
            if (arr.Length >1 && int.TryParse(arr[0], out int x) && int.TryParse(arr[1], out int y))
            { TxtToResolution = new Point(x, y); }
            return TxtToResolution;
        }

        public static string GetStateADescription(string stateA) => stateAMapping.TryGetValue(stateA, out var d) ? d : stateA;

        public static void 狀態訊息(string msg, bool clear = false)
        {
            if (主窗狀態 == null)
                return;

            if (clear) 主窗狀態.Text = string.Empty;
            主窗狀態.AppendText(msg);
        }
        public static void 清除狀態訊息() => 主窗狀態?.Clear();

        #endregion static public functions

        #region public functions
        public void UIRefrshSize(bool advancedScriptEnabled, bool battleAssistEnabled)
        {
            if (winHeight <=0) winHeight =企鵝之野望.Height;

			int offsetY = 100;
			double tA = advancedScriptEnabled ? 300 + offsetY : 0;
			double tB = battleAssistEnabled ? 370 + offsetY : 0;
			企鵝之野望.Height = winHeight + tA + tB;
 }

        /// <summary>
        /// 保存標籤頁狀態
        /// </summary>
        public void SaveTabState()
        {
            try
            {
                _tabManager.Save(NBTabControl);
                Debug.WriteLine("保存標籤頁狀態成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存標籤頁狀態失敗: {ex.Message}");
            }
        }

        // 在應用程式關閉時保存標籤頁狀態
        protected override void OnClosing(CancelEventArgs e)
        {
            _tabManager.Save(NBTabControl);
            base.OnClosing(e);
        }

        #endregion public functions

        #region private founctions
        private void 企鵝專用測試_Click(object sender, RoutedEventArgs e)
        {
            // 如果點擊 Penguin B，打開輸入對話視窗
            if (sender is Button btn && btn.Name == "企鵝專用測試B")
            {
                var dlg = new PenguinTestDialog();
                dlg.Owner = this;
                dlg.ShowDialog();
                return;
            }

            if (NBTabControl.SelectedItem is TabItem tabItem && tabItem.Content is NobMainCodePage page && page.MainNob != null)
            {
                var speed = page.MainNob.速度;
                狀態訊息(speed.ToString());
                var c1 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900,70), new System.Drawing.Point(100,70), "F6F67A");
                var c2 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900,70), new System.Drawing.Point(100,70), "565ABD");
                var c3 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900,70), new System.Drawing.Point(100,70), "6363EE");
                var c4 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200,190), new System.Drawing.Point(45,55), "FFFFFF");
                var c5 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900,70), new System.Drawing.Point(100,70), "5959D8");
                var c6 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900,70), new System.Drawing.Point(50,130), "47ADE8");
                var c7 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(440,260), new System.Drawing.Point(160,130), "D5F1F1");

                Debug.WriteLine($"Color : {c1} - {c2} - {c3} - {c4} - {c5} - {c6} - {c7}");
            }
        }

        private void Btn_AutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            _gameWindowManager.AutoArrangeWindows(CMB_Resolution.Text);
        }

        private async void MuitOpen_Click(object? sender, RoutedEventArgs e)
        {
            if (int.TryParse(OpenGameWindows.Text, out int count))
            {
                await _gameWindowManager.MultiOpenAsync(count, CMB_Resolution.Text, TB_GamePadName.Text);
            }
        }

        private async void Btn_Update_Click(object? sender, RoutedEventArgs e)
        {
            if (!_updateManager.UpdateAvailable)
            {
                await _updateManager.CheckForUpdatesAsync(msg => 狀態訊息(msg, true), latest =>
                {
                    Btn_Update.Content = $"更新至 {latest}";
                    Btn_Update.Visibility = Visibility.Visible;
                });
            }
            if (!_updateManager.UpdateAvailable) return;
            var result = MessageBox.Show($"是否更新至 {_updateManager.LatestVersion}版本？\n\n更新後應用將重新啟動。", "更新確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _updateManager.PerformUpdateAsync(_updateManager.LatestVersion, Root, Btn_Update);
            }
        }

        private void Button_Click_手把(object sender, RoutedEventArgs e)
        {
			TB_GamePadName.Text = TB_GamePadName.Text.Contains("XInput") ? "None" : "XInput-1";
		}

        #endregion private founctions
    }
}
