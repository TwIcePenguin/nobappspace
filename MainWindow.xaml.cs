using Microsoft.VisualBasic.Logging;
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
        static TextBox? 主窗狀態;
        static ComboBox? Resolution;
        double winHeight;
        public static int OrinX = 0;
        public static int OrinY = 0;

        private bool _updateAvailable = false;
        private string _latestVersion = string.Empty;

        private const string TAB_STATE_FILENAME = "TabState.json";
        private TabControlState _tabState = new TabControlState();

        public Setting CodeSetting = new();

        #region Initialize
        public MainWindow()
        {
            InitializeComponent();
            AdminRelauncher();
            企鵝之野望.Title = $"企鵝之野望 v{VersionInfo.Version} KEY = {Tools.GetSerialNumber()}";

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
            InitUIStatus();
            InitButtonEvent();

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
            // 檢查更新是否成功完成
            CheckUpdateSuccess();

            // 在其他初始化之前加載 GitHub 配置
            this.Loaded += (s, e) => CheckForUpdatesAsync();
        }

        private void InitializeTabItems()
        {
            if (NBTabControl == null)
                return;

            NBTabControl.Items.Clear();
            // 嘗試載入之前的標籤頁狀態
            LoadTabState();

            // 確保始終有8個標籤頁
            // 如果載入的標籤頁少於8個，添加到8個
            while (_tabState.TabItems.Count < 8)
            {
                var index = _tabState.TabItems.Count;
                var tabItemState = new TabItemState
                {
                    Header = $"角色{index}",
                    PlayerName = "",
                    IsVerified = false
                };
                _tabState.TabItems.Add(tabItemState);
            }

            // 如果載入的標籤頁超過8個，只保留前8個
            if (_tabState.TabItems.Count > 8)
            {
                _tabState.TabItems = _tabState.TabItems.Take(8).ToList();
            }

            // 根據狀態創建標籤頁
            for (int i = 0; i < _tabState.TabItems.Count; i++)
            {
                var state = _tabState.TabItems[i];
                CreateTabItem(state, i);
            }

            // 設置活動標籤頁
            if (NBTabControl.Items.Count > 0)
            {
                NBTabControl.SelectedIndex = Math.Min(_tabState.ActiveTabIndex, NBTabControl.Items.Count - 1);
            }
        }

        private void InitUIStatus()
        {
            主窗狀態 = MainWindowsStatusMsgBox;
            winHeight = 企鵝之野望.Height;
            企鵝專用測試A.Visibility = 企鵝專用測試B.Visibility = 企鵝專用測試C.Visibility = Visibility.Hidden;

#if DEBUG
            企鵝專用測試A.Visibility = Visibility.Visible;
#endif
        }

        private void InitButtonEvent()
        {
            企鵝專用測試A.Click += 企鵝專用測試_Click;
            企鵝專用測試B.Click += 企鵝專用測試_Click;
            企鵝專用測試C.Click += 企鵝專用測試_Click;

            Btn_AutoRefresh.Click += Btn_AutoRefresh_Click;
            Btn_SetLoad.Click += Btn_SetLoad_Click;
            Btn_SetApply.Click += Btn_SetApply_Click;
        }

        private void Btn_SetApply_Click(object sender, RoutedEventArgs e)
        {
            if (NBTabControl.SelectedItem is TabItem tabItem && ((TabItem)NBTabControl.SelectedItem).Content != null)
            {
                NobMainCodePage page = ((TabItem)NBTabControl.SelectedItem).Content as NobMainCodePage;
                if (page != null && page.MainNob != null)
                {
                    page.MainNob.CodeSetting = CodeSetting;
                    page.SetToUI();
                }
            }
        }
        private void Btn_SetLoad_Click(object sender, RoutedEventArgs e)
        {
            if (NBTabControl.SelectedItem is TabItem tabItem && ((TabItem)NBTabControl.SelectedItem).Content != null)
            {
                NobMainCodePage page = ((TabItem)NBTabControl.SelectedItem).Content as NobMainCodePage;
                if (page != null && page.MainNob != null)
                {
                    Btn_SetLoad.Content = $"取得{page.MainNob.PlayerName}設定";
                    CodeSetting = page.MainNob.CodeSetting;
                }
            }
        }

        #endregion Initialize

        #region static public functions
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
        public static string GetStateADescription(string stateA)
        {
            return stateAMapping.TryGetValue(stateA, out var description) ? description : stateA;
        }

        public static void 狀態訊息(string msg, bool clear = false)
        {
            if (主窗狀態 == null)
                return;

            if (clear)
                主窗狀態.Text = "";
            主窗狀態?.AppendText(msg);
        }
        public static void 清除狀態訊息()
        {
            主窗狀態?.Clear();
        }

        #endregion static public functions

        #region public functions
        public void UIRefrshSize(bool 進階腳本開啟, bool 戰鬥輔助開啟)
        {
            int offsetY = 100;
            double tA = 進階腳本開啟 ? 300 + offsetY : 0;
            double tB = 戰鬥輔助開啟 ? 370 + offsetY : 0;
            企鵝之野望.Height = winHeight + tA + tB;
        }

        /// <summary>
        /// 保存標籤頁狀態
        /// </summary>
        public void SaveTabState()
        {
            try
            {
                _tabState.ActiveTabIndex = NBTabControl.SelectedIndex;
                _tabState.TabItems.Clear();

                foreach (TabItem tabItem in NBTabControl.Items)
                {
                    if (tabItem.Content is NobMainCodePage page)
                    {
                        var state = new TabItemState
                        {
                            Header = tabItem.Header.ToString() ?? "",
                            PlayerName = page.MainNob?.PlayerName ?? "",
                            IsVerified = page.MainNob != null && page.MainNob.驗證完成
                        };
                        _tabState.TabItems.Add(state);
                    }
                }

                // 確保有8個標籤頁記錄
                while (_tabState.TabItems.Count < 8)
                {
                    var index = _tabState.TabItems.Count;
                    _tabState.TabItems.Add(new TabItemState
                    {
                        Header = $"角色{index}",
                        PlayerName = "",
                        IsVerified = false
                    });
                }

                string json = JsonSerializer.Serialize(_tabState);
                File.WriteAllText(TAB_STATE_FILENAME, json);
                Debug.WriteLine("保存標籤頁狀態成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存標籤頁狀態失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 載入標籤頁狀態
        /// </summary>
        private void LoadTabState()
        {
            try
            {
                if (File.Exists(TAB_STATE_FILENAME))
                {
                    string json = File.ReadAllText(TAB_STATE_FILENAME);
                    _tabState = JsonSerializer.Deserialize<TabControlState>(json) ?? new TabControlState();
                    Debug.WriteLine("載入標籤頁狀態成功");
                }
                else
                {
                    _tabState = new TabControlState();
                    Debug.WriteLine("未找到標籤頁狀態文件，使用默認設置");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"載入標籤頁狀態失敗: {ex.Message}");
                _tabState = new TabControlState();
            }
        }
        // 在應用程式關閉時保存標籤頁狀態
        protected override void OnClosing(CancelEventArgs e)
        {
            SaveTabState();
            base.OnClosing(e);
        }

        #endregion public functions

        #region private founctions
        private void CreateTabItem(TabItemState state, int index)
        {
            var tabItem = new TabItem();
            tabItem.Header = state.Header;
            tabItem.MouseDoubleClick += OnTabFocus;

            var content = new NobMainCodePage();
            tabItem.Content = content;
            content.RootTabItem = tabItem;
            content.PageIndex = index; // 設置頁面索引

            NBTabControl.Items.Add(tabItem);

            // 如果有已保存的遊戲角色，且已驗證，則恢復狀態
            if (!string.IsNullOrEmpty(state.PlayerName) && state.IsVerified)
            {
                var nobData = AllNobWindowsList.Find(n => n.PlayerName == state.PlayerName);
                if (nobData != null)
                {
                    // 設置自動恢復狀態屬性
                    content.AutoRestoreState = true;
                    content.PlayerToRestore = state.PlayerName;
                }
            }
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

        private void Button_Click_手把(object sender, RoutedEventArgs e)
        {
            TB_GamePadName.Text = TB_GamePadName.Text.Contains("XInput") ? "None" : "XInput-1";
        }

        private void 企鵝專用測試_Click(object sender, RoutedEventArgs e)
        {
            if (NBTabControl.SelectedItem is TabItem tabItem && ((TabItem)NBTabControl.SelectedItem).Content != null)
            {
                NobMainCodePage page = ((TabItem)NBTabControl.SelectedItem).Content as NobMainCodePage;
                if (page != null && page.MainNob != null)
                {
                    var speed = page.MainNob.速度;
                    狀態訊息(speed.ToString());
                    //效能測試
                    //PerformanceTest.TestGetColorCopNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    //橘 565ABD
                    var c1 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    //藍 565ABD
                    var c2 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "565ABD");
                    //紅 6363EE 
                    var c3 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "6363EE");
                    var c4 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
                    var c5 = ColorTools.GetColorNum(page.MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "5959D8");
                    Debug.WriteLine($"Color : {c1} - {c2} - {c3} - {c4} - {c5}");
                    page.MainNob.ML_Click(125, 260);
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
        /// <summary>
        /// 多開視窗
        /// </summary>
        private void MuitOpen_Click(object sender, RoutedEventArgs e)
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

        private void CheckUpdateSuccess()
        {
            string successMarkerPath = Path.Combine(Environment.CurrentDirectory, "update_success.txt");
            if (File.Exists(successMarkerPath))
            {
                try
                {
                    string successMessage = File.ReadAllText(successMarkerPath);
                    File.Delete(successMarkerPath); // 讀取後立即刪除標記檔

                    // 清理更新文件，以防未被清理
                    string updateFilePath = Path.Combine(Environment.CurrentDirectory, "update.zip");
                    if (File.Exists(updateFilePath))
                    {
                        try
                        {
                            File.Delete(updateFilePath);
                            Debug.WriteLine("清理遺留的更新文件");
                        }
                        catch
                        {
                            // 忽略錯誤
                        }
                    }

                    MessageBox.Show($"應用程式已成功更新！\n\n{successMessage}",
                        "更新成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"讀取更新狀態檔案發生錯誤: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 檢查更新並設置更新按鈕的可見性
        /// </summary>
        /// <summary>
        /// 檢查更新並設置更新按鈕的可見性
        /// </summary>
        private async void CheckForUpdates()
        {
            try
            {
                var release = await GetLatestRelease();
                _updateAvailable = IsUpdateAvailable(release.tag_name);
                _latestVersion = release.tag_name;

                // 使用 Dispatcher.Invoke 確保 UI 更新在 UI 執行緒上進行
                this.Dispatcher.Invoke(() =>
                {
                    清除狀態訊息();
                    if (_updateAvailable)
                    {
                        Btn_Update.Content = $"更新至 {_latestVersion}";
                        Btn_Update.Visibility = Visibility.Visible;
                        狀態訊息($"發現新版本: {_latestVersion}，當前版本: {VersionInfo.Version}");
                    }
                    else
                    {
                        狀態訊息("當前已是最新版本");
                        //Debug.WriteLine($"當前已是最新版本: {VersionInfo.Version}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"檢查更新失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新按鈕點擊事件
        /// </summary>
        private async void Btn_Update_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdatesAsync();
            if (!_updateAvailable)
                return;

            var result = MessageBox.Show($"是否更新至 {_latestVersion} 版本？\n\n更新後應用將重新啟動。",
                "更新確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Root.IsEnabled =
                Btn_Update.IsEnabled = false;
                Btn_Update.Content = "正在更新...";

                try
                {
                    // 下載更新
                    await UpdateDownloader.DownloadUpdate(_latestVersion);
                    Btn_Update.Content = "正在安裝更新...";

                    // 創建一個專用的 PowerShell 更新腳本，使用修改過的解壓方法
                    string psScriptPath = Path.Combine(Environment.CurrentDirectory, "RunUpdate.ps1");
                    string psContent = @"
# 設置腳本編碼為 UTF-8，解決中文顯示問題
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host ""=============================================="" -ForegroundColor Cyan
Write-Host ""          企鵝之野望 - 更新程序          "" -ForegroundColor Cyan
Write-Host ""=============================================="" -ForegroundColor Cyan
Write-Host """"
Write-Host ""正在執行更新，請勿關閉此視窗..."" -ForegroundColor Yellow
Write-Host """"

# 記錄到日誌文件
$logFile = ""update_log.txt""
""[$(Get-Date)] 開始更新過程"" | Out-File -FilePath $logFile -Encoding utf8

# 檢查更新文件
if (-not (Test-Path ""update.zip"")) {
    Write-Host ""錯誤: 找不到更新文件 'update.zip'"" -ForegroundColor Red
    ""[$(Get-Date)] 錯誤: 找不到更新文件"" | Out-File -Append -FilePath $logFile -Encoding utf8
    Write-Host """"
    Write-Host ""按任意鍵退出..."" -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey(""NoEcho,IncludeKeyDown"")
    exit 1
}

# 顯示文件大小
$fileSize = (Get-Item ""update.zip"").Length / 1KB
Write-Host ""找到更新文件，大小: $([Math]::Round($fileSize, 2)) KB"" -ForegroundColor Green
""[$(Get-Date)] 找到更新文件，大小: $([Math]::Round($fileSize, 2)) KB"" | Out-File -Append -FilePath $logFile -Encoding utf8

# 等待原應用程式退出
Write-Host """"
Write-Host ""正在等待原應用程式退出..."" -ForegroundColor Yellow
Start-Sleep -Seconds 2

# 解壓縮文件
try {
    Write-Host """"
    Write-Host ""正在解壓縮更新文件..."" -ForegroundColor Yellow
    ""[$(Get-Date)] 開始解壓縮更新文件"" | Out-File -Append -FilePath $logFile -Encoding utf8
    
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead(""update.zip"")
    Write-Host ""更新包含 $($zip.Entries.Count) 個文件"" -ForegroundColor Cyan
    
    # 手動解壓縮文件，而不使用 ExtractToDirectory 方法
    Write-Host ""正在解壓文件..."" -ForegroundColor Yellow
    foreach ($entry in $zip.Entries) {
        $targetPath = [System.IO.Path]::Combine(""."", $entry.FullName)
        $targetDir = [System.IO.Path]::GetDirectoryName($targetPath)
        
        # 確保目標目錄存在
        if (-not [System.IO.Directory]::Exists($targetDir)) {
            [System.IO.Directory]::CreateDirectory($targetDir)
        }
        
        # 如果是文件（不是目錄）才解壓
        if (-not $entry.FullName.EndsWith('/')) {
            # 檢查文件是否已存在，如果存在則刪除
            if ([System.IO.File]::Exists($targetPath)) {
                try {
                    [System.IO.File]::Delete($targetPath)
                } catch {
                    # 使用 ${} 括號正確引用變數，避免與冒號衝突
                    Write-Host ""無法刪除文件 ${targetPath}: $_"" -ForegroundColor Yellow
                }
            }
            
            # 解壓文件
            try {
                $entryStream = $entry.Open()
                $targetStream = [System.IO.File]::Create($targetPath)
                $entryStream.CopyTo($targetStream)
                $targetStream.Close()
                $entryStream.Close()
            } catch {
                # 同樣使用 ${} 括號
                Write-Host ""解壓文件 ${entry.FullName} 失敗: $_"" -ForegroundColor Red
                throw
            }
        }
    }
    
    $zip.Dispose()
    Write-Host ""解壓縮完成！"" -ForegroundColor Green
    ""[$(Get-Date)] 解壓縮完成"" | Out-File -Append -FilePath $logFile -Encoding utf8
}
catch {
    Write-Host ""解壓縮失敗: $($_.Exception.Message)"" -ForegroundColor Red
    ""[$(Get-Date)] 解壓縮失敗: $($_.Exception.Message)"" | Out-File -Append -FilePath $logFile -Encoding utf8
    Write-Host """"
    Write-Host ""按任意鍵退出..."" -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey(""NoEcho,IncludeKeyDown"")
    exit 1
}

# 刪除更新文件
Write-Host """"
Write-Host ""正在刪除更新文件..."" -ForegroundColor Yellow
try {
    Remove-Item ""update.zip"" -Force
    Write-Host ""更新文件已刪除"" -ForegroundColor Green
    ""[$(Get-Date)] 更新文件已刪除"" | Out-File -Append -FilePath $logFile -Encoding utf8
}
catch {
    Write-Host ""警告: 無法刪除更新文件: $($_.Exception.Message)"" -ForegroundColor Yellow
    ""[$(Get-Date)] 警告: 無法刪除更新文件"" | Out-File -Append -FilePath $logFile -Encoding utf8
}

# 寫入更新成功標記
""更新成功於 $(Get-Date)"" | Out-File -FilePath ""update_success.txt"" -Encoding utf8
""[$(Get-Date)] 寫入更新成功標記"" | Out-File -Append -FilePath $logFile -Encoding utf8

# 重新啟動應用程式
Write-Host """"
Write-Host ""正在重新啟動應用程式..."" -ForegroundColor Yellow
""[$(Get-Date)] 準備重新啟動應用程式"" | Out-File -Append -FilePath $logFile -Encoding utf8

try {
    Start-Process ""NOBApp.exe""
    Write-Host ""應用程式已重新啟動"" -ForegroundColor Green
    ""[$(Get-Date)] 應用程式已重新啟動"" | Out-File -Append -FilePath $logFile -Encoding utf8
}
catch {
    Write-Host ""警告: 無法直接啟動 NOBApp.exe，嘗試尋找其它可執行文件..."" -ForegroundColor Yellow
    
    $exeFiles = Get-ChildItem -Path ""."" -Filter ""*.exe"" | Where-Object { $_.Name -ne ""NOBApp.exe"" }
    if ($exeFiles.Count -gt 0) {
        $exePath = $exeFiles[0].FullName
        Write-Host ""找到可能的可執行文件: $($exeFiles[0].Name)"" -ForegroundColor Green
        Start-Process $exePath
        ""[$(Get-Date)] 啟動替代執行檔: $($exeFiles[0].Name)"" | Out-File -Append -FilePath $logFile -Encoding utf8
    }
    else {
        Write-Host ""警告: 無法找到可執行文件，請手動啟動應用程式"" -ForegroundColor Red
        ""[$(Get-Date)] 警告: 無法找到可執行文件"" | Out-File -Append -FilePath $logFile -Encoding utf8
    }
}

# 結束
Write-Host """"
Write-Host ""=============================================="" -ForegroundColor Green
Write-Host ""                更新成功完成!                "" -ForegroundColor Green
Write-Host ""=============================================="" -ForegroundColor Green
Write-Host """"
""[$(Get-Date)] 更新過程完成"" | Out-File -Append -FilePath $logFile -Encoding utf8

Write-Host ""按任意鍵關閉此視窗..."" -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey(""NoEcho,IncludeKeyDown"")
";

                    // 使用 UTF-8 編碼寫入文件，確保中文正確顯示
                    File.WriteAllText(psScriptPath, psContent, System.Text.Encoding.UTF8);

                    // 記錄更新標記
                    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "update_pending.txt"),
                        $"更新開始於 {DateTime.Now}，版本: {_latestVersion}");

                    // 直接啟動 PowerShell 並保持視窗開啟
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoExit -ExecutionPolicy Bypass -NoProfile -File \"{psScriptPath}\"",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    Process.Start(psi);

                    // 延遲一秒後退出
                    await Task.Delay(1000);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"更新失敗: {ex.Message}");
                    File.AppendAllText("update_error.log", $"[{DateTime.Now}] 更新失敗: {ex.Message}\n{ex.StackTrace}\n");

                    MessageBox.Show($"更新失敗: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    Root.IsEnabled = true;
                    Btn_Update.IsEnabled = true;
                    Btn_Update.Content = $"更新至 {_latestVersion}";
                }
            }
        }

        /// <summary>
        /// 異步檢查更新
        /// </summary>
        private async Task CheckForUpdatesAsync()
        {
            await Task.Run(() => CheckForUpdates());
        }

        private void OnTabFocus(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tabItem)
            {
                NobMainCodePage page = ((TabItem)sender).Content as NobMainCodePage;
                if (page != null)
                {
                    Debug.WriteLine($"{page.RootTabItem.Header}");
                    page.FocusUserWindows();
                }
            }
        }

        #endregion private founctions
    }
}
