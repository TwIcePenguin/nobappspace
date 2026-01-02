using NOBApp.GoogleData;
using NOBApp.Sports;
using NOBApp.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;
using System.IO.Compression;

namespace NOBApp
{
    /// <summary>
    /// NobMain.xaml 的互動邏輯
    /// </summary>
    public partial class NobMainCodePage : UserControl
    {
        internal AuthenticationManager _authManager;
        internal ScriptManager _scriptManager;
        internal TargetManager _targetManager;
        internal TeamManager _teamManager;

        internal BaseClass? useMenu = null;
        /// <summary>
        /// 本次一起掛網的隊伍 包含隊長自己
        /// </summary>
        public static List<NOBDATA> NobTeams = new();
        public static Dictionary<int, NOBDATA> AllPageNOBUser = new();
        public int PageIndex = 0;
        /// <summary>
        /// 鎖定的對象
        /// </summary>
        public NOBDATA? MainNob;
        DispatcherTimer _timer = new DispatcherTimer();
        internal static 隊伍技能紀錄 m隊伍技能紀錄 = new();
        Dictionary<string, Action> menuMapping = new Dictionary<string, Action>();

        /// <summary>
        /// 是否全隊追蹤 用在沒有腳本並且戰鬥結束的狀態下全隊自動追蹤
        /// </summary>
        public bool 全隊追蹤 = false;

        public int 限點數量 = 2;
        public bool UpdateNPCDataUI = false;
        public ComboBox[] comboBoxes;
        public TabItem RootTabItem;
        Thickness oThickness;
        bool pageA_isExpanded = false, pageB_isExpanded = false;
        // 用於標記是否需要自動恢復狀態
        public bool AutoRestoreState { get; set; } = false;

        private System.Threading.CancellationTokenSource? _selectionCts;

        // 需要恢復的玩家名稱
        public string PlayerToRestore { get; set; } = "";

        public NobMainCodePage()
        {
            InitializeComponent();
            _authManager = new AuthenticationManager(this);
            _scriptManager = new ScriptManager(this);
            _targetManager = new TargetManager(this);
            _teamManager = new TeamManager(this);

            UIInit();
            UIEventAdd();
        }

        void UIInit()
        {
            腳本區.IsEnabled =
            腳本展區.IsEnabled = 戰鬥輔助面.IsEnabled = false;

            #region 註冊UI 給其他物件使用
            comboBoxes = new ComboBox[] { SelectFID_1, SelectFID_2, SelectFID_3, SelectFID_4, SelectFID_5, SelectFID_6, SelectFID_7 };
            #endregion

            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += CustomUpdate;

            Tools.UpdateTimer(到期計時);

            UIStatus_Default();

            oThickness = 戰鬥輔助面.Margin;

            MainWindow.RefreshNOBID(CB_HID, comboBoxes);

            // 添加自動恢復邏輯
            if (AutoRestoreState && !string.IsNullOrEmpty(PlayerToRestore))
            {
                this.Loaded += async (s, e) =>
                {
                    await Task.Delay(500); // 給UI一些時間初始化
                    TryRestoreState();
                };
            }
        }

        // 嘗試恢復狀態的方法
        private void TryRestoreState()
        {
            try
            {
                if (string.IsNullOrEmpty(PlayerToRestore))
                    return;

                // 檢查指定的玩家是否存在
                var player = MainWindow.AllNobWindowsList.Find(p => p.PlayerName == PlayerToRestore);
                if (player == null)
                {
                    Debug.WriteLine($"無法找到玩家: {PlayerToRestore}");
                    return;
                }

                // 設置下拉選單並觸發驗證流程
                if (CB_HID.Items.Contains(PlayerToRestore))
                {
                    CB_HID.SelectedItem = PlayerToRestore;

                    // 設置自動驗證
                    認證2CB.IsChecked = true;

                    // 自動點擊驗證按鈕
                    LockButton_Click(LockBtn, new RoutedEventArgs());

                    Debug.WriteLine($"自動恢復標籤頁 {RootTabItem.Header} 的狀態: {PlayerToRestore}");
                }
                else
                {
                    Debug.WriteLine($"下拉選單中找不到玩家: {PlayerToRestore}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"自動恢復標籤頁狀態失敗: {ex.Message}");
            }
        }

        public void UIStatus_Default()
        {
            Btn移除名單.Visibility = Btn鎖定目標添加.Visibility =
            List_鎖定名單.Visibility =
            CB_定位點.Visibility = 武技設定頁面.Visibility =
            SMENU2.Visibility = 後退時間.Visibility = TargetViewPage.Visibility = CB_AllIn.Visibility = TB_選擇關卡.Visibility = TB_選擇難度.Visibility = TB_SetCNum.Visibility =
            FNPCID.Visibility = SMENU1.Visibility =
            Btn_TargetF.Visibility = Btn_TargetD.Visibility = Btn_TargetE.Visibility =
            Btn_TargetC.Visibility = Btn_TargetB.Visibility = Btn_TargetA.Visibility = Visibility.Hidden;

            通用欄.Visibility = Visibility.Hidden;

            // 重置顏色提示
            var colorHint = GetColorHintBlock();
            if (colorHint != null)
            {
                colorHint.Visibility = Visibility.Collapsed;
                colorHint.Text = string.Empty;
            }
        }

        void UIEventAdd()
        {
            StartCode.Checked += (o, e) => StartCode_Checked(true);
            StartCode.Unchecked += (o, e) => StartCode_Checked(false);
            Btn添加名單.Click += 切換名單_Click;
            Btn移除名單.Click += 切換名單_Click;
            Btn鎖定目標添加.Click += 鎖定目標添加_Click;
            UseSkill_CB.Click += UseSkill_CB_Click;
            Btn_上.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_I); };
            Btn_下.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_K); };
            Btn_左.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_J); };
            Btn_右.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_L); };
            Btn_ALL_ENTER.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_ENTER); };
            Btn_ALL_ESC.Click += (s, e) => { 同步所有需要功能(VKeys.KEY_ESCAPE); };
            全體選擇.TextChanged += 直接下指令_TextChanged;

            自動結束一.Click += 自動結束一_Click;
            自動結束二.Click += 自動結束二_Click;
            希望取得.Click += 希望取得_Click;
            AutoFR_CB.Click += AutoFR_CB_Click;

            CB_開打.Click += CB_開打_Click;
            解無敵.Click += 解無敵_Click;
            E點怪.Click += E點怪_Click;

            A套路.Click += 套路_Click;
            B套路.Click += 套路_Click;
            C套路.Click += 套路_Click;
            D套路.Click += 套路_Click;
            E套路.Click += 套路_Click;
            F套路.Click += 套路_Click;

            儲存套路.Click += 儲存套路_Click;

            同步_1.Click += 同步_Click;

            戰鬥輔助面.LayoutUpdated -= 戰鬥輔助面_LayoutUpdated; // 移除舊 handler 避免疊加
            // 新增使用 Expanded/Collapsed事件
            腳本展區.Expanded += (s, e) => OnExpandersChanged();
            腳本展區.Collapsed += (s, e) => OnExpandersChanged();
            戰鬥輔助面.Expanded += (s, e) => OnExpandersChanged();
            戰鬥輔助面.Collapsed += (s, e) => OnExpandersChanged();

            List_鎖定名單.SelectionChanged += 排序_SelectionChanged;
            List_IgnoreList.SelectionChanged += 排序_SelectionChanged;
            List_目前名單.SelectionChanged += 排序_SelectionChanged;

            IGMouse.Checked += (s, e) =>
            {
                MainNob!.SetClickThrough(true);
            };
            IGMouse.Unchecked += (s, e) =>
            {
                MainNob!.SetClickThrough(false);
            };

            VIPSP.Checked += (s, e) =>
            {
                if (MainNob != null)
                    MainNob.VIPSP = true;
                //Task.Run(MainNob!.速度);
            };
            VIPSP.Unchecked += (s, e) =>
            {
                if (MainNob != null)
                    MainNob.VIPSP = false;
            };

            Btn_TargetA.Click += OnTargetClick;
            Btn_TargetB.Click += OnTargetClick;
            Btn_TargetC.Click += OnTargetClick;
            Btn_TargetD.Click += OnTargetClick;
            Btn_TargetE.Click += OnTargetClick;
            Btn_TargetF.Click += OnTargetClick;

            Btn_ReportIssue.Click += Btn_ReportIssue_Click;
            Btn_ContactPurchase.Click += Btn_ContactPurchase_Click;
        }

        private void IGMouse_Checked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #region 目標選擇UI
        // 在「鎖定目標添加」按鈕的事件中添加：
        private void 鎖定目標添加_Click(object sender, RoutedEventArgs e)
        {
            _targetManager.AddLockTarget();
        }


        private void 切換名單_Click(object sender, RoutedEventArgs e)
        {
            _targetManager.ToggleList();
        }




        #endregion

        #region UIEvent 
        // 新的統一調整方法
        private void OnExpandersChanged()
        {
            if (MainWindow.Instance == null) return;
            //直接使用當前狀態，不再修改 Expander Margin，避免在 ScrollViewer內造成位移疊加
            bool scriptExpanded = 腳本展區.IsExpanded;
            bool battleExpanded = 戰鬥輔助面.IsExpanded;
            if (pageB_isExpanded == scriptExpanded && pageA_isExpanded == battleExpanded)
                return; // 狀態沒變不重新調整
            pageB_isExpanded = scriptExpanded;
            pageA_isExpanded = battleExpanded;

            MainWindow.Instance.UIRefrshSize(scriptExpanded, battleExpanded);
            // 還原原始 Margin，避免因之前的 LayoutUpdated 疊加造成位移
            戰鬥輔助面.Margin = oThickness;
        }

        // 移除原本依賴 LayoutUpdated 的方法內容，避免被誤觸造成多次高度/邊距疊加
        private void 戰鬥輔助面_LayoutUpdated(object? sender, EventArgs e)
        {
            // 已改為使用 OnExpandersChanged + Expanded/Collapsed，不在此做任何事
        }

        private void FollowLeadLockTarget_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
            {
                int id = MainNob.GetTargetIDINT();
                foreach (var nob in MainWindow.AllNobWindowsList)
                {
                    if (nob != null)
                        nob.MoveToNPC(id);
                }
            }
        }

        private void Button_Click_離開戰鬥(object sender, RoutedEventArgs e)
        {
            全員離開戰鬥();
        }

        private void 希望取得_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
                MainNob.希望取得 = 希望取得.IsChecked == true;
        }

        private void 同步_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void 直接下指令_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                var tb = sender as TextBox;
                if (int.TryParse(tb!.Text, out int setInput))
                {
                    if (隊員智能功能組 != null && 隊員智能功能組.Count > 0)
                    {
                        for (int i = 0; i < 隊員智能功能組.Count; i++)
                        {
                            var nb = 隊員智能功能組[i];
                            if (nb.同步)
                            {
                                nb?.NOB?.直向選擇(setInput, 0);
                            }
                        }
                    }
                }
                if (tb.Text.Contains('R'))
                {
                    if (隊員智能功能組 != null && 隊員智能功能組.Count > 0)
                    {
                        for (int i = 0; i < 隊員智能功能組.Count; i++)
                        {
                            var nb = 隊員智能功能組[i];
                            if (nb.同步)
                            {
                                nb?.NOB?.KeyPressPP(VKeys.KEY_R);
                            }
                        }
                    }
                }

                tb.Text = "";
            }

        }

        private void 儲存套路_Click(object sender, RoutedEventArgs e)
        {
            儲存隊員技能組();
        }

        private void 套路_Click(object sender, RoutedEventArgs e)
        {
            讀取隊員技能組();
        }

        private void E點怪_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
            {
                MainNob.isUseEnter = E點怪.IsChecked ?? false;
            }
        }

        private void 解無敵_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
                MainNob.F5解無敵 = 解無敵.IsChecked ?? false;
        }

        private void CB_開打_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
                MainNob.開打 = CB_開打.IsChecked ?? false;
        }

        private void AutoFR_CB_Click(object sender, RoutedEventArgs e)
        {
            全隊追蹤 = AutoFR_CB.IsChecked ?? false;
        }

        private void 自動結束一_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
                MainNob.自動結束_A = 自動結束一.IsChecked ?? false;
        }

        private void 自動結束二_Click(object sender, RoutedEventArgs e)
        {
            if (MainNob != null)
                MainNob.自動結束_B = 自動結束二.IsChecked ?? false;
        }
        public void 同步所有需要功能(VKeys key)
        {
            _teamManager.SyncAllFunctions(key);
        }


        private void UseSkill_CB_Click(object sender, RoutedEventArgs e)
        {
            特殊功能開關Async();
        }

        public void 特殊功能開關Async()
        {
            if (MainNob == null)
                return;

            bool useAutoSkill = UseSkill_CB.IsChecked == true;
            MainNob.IsUseAutoSkill = useAutoSkill;
            SaveSetting();
            儲存隊員技能組();
            foreach (var item in 隊員智能功能組)
            {
                if (item == null || item.NOB == null)
                    continue;
                item.NOB.自動結束_A = MainNob.自動結束_A;
                item.NOB.自動結束_B = MainNob.自動結束_B;
                item.NOB.IsUseAutoSkill = useAutoSkill;
                item.NOB.VIPSP = MainNob.VIPSP;
                item.NOB.追蹤 = 全隊追蹤;
                if (item.NOB.啟動自動輔助中 == false)
                {
                    MainNob.Log($"啟動自動輔助中");
                    Task.Run(item.NOB.BattleUpdate);
                }
            }
        }

        /// <summary>
        /// 確認使用角色
        /// </summary>
        private async void LockButton_Click(object sender, RoutedEventArgs e)
        {
            await _authManager.HandleLockClick();
        }

        public void UpdateSelectMenu()
        {
            _scriptManager.UpdateSelectMenu();
        }

        void OnRefreshNOBID(object sender, RoutedEventArgs e)
        {
            MainWindow.RefreshNOBID(CB_HID, comboBoxes);
        }

        public void CustomUpdate(object? sender, EventArgs e)
        {
            // 基本檢查
            if (MainNob == null || (LockBtn != null && LockBtn.Content.ToString()!.Contains("驗證")))
                return;

            try
            {
                // 定期更新剩餘天數顯示
                UpdateRemainingDays();
                
                // 狀態視窗更新
                UpdateStatusWindow();

                // NPC 目標列表更新
                UpdateNPCTargetsIfNeeded();

                // 自動按 Enter
                if (CB_BKAutoEnter.IsChecked == true && MainNob != null)
                {
                    MainNob.KeyPress(VKeys.KEY_ENTER);
                }
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不中斷應用程序
                Debug.WriteLine($"CustomUpdate 發生錯誤: {ex.Message}");
            }




        }

        /// <summary>
        /// 更新狀態視窗，只在狀態變化時執行
        /// </summary>
        private void UpdateStatusWindow()
        {
            if (MainNob == null || useMenu == null || useMenu.NobTeam == null || 視窗狀態 == null)
                return;

            // 檢查狀態是否變化
            string stateADescription = MainWindow.GetStateADescription(MainNob.StateA);
            bool stateChanged = (_lastStateADescription != stateADescription);

            if (!stateChanged)
                return;

            _lastStateADescription = stateADescription;

            // 使用低優先級調度來更新UI，避免UI凍結
            this.Dispatcher.InvokeAsync(() =>
            {
                視窗狀態.Clear();
#if DEBUG
                視窗狀態.AppendText($@"POS: new({MainNob.PosX}, {MainNob.PosH}, {MainNob.PosY}),LDS:{stateADescription} S:{MainWindow.MainState} " + Environment.NewLine);
#else
                視窗狀態.AppendText($@"LDS:{stateADescription} S:{MainWindow.MainState} " + Environment.NewLine);
#endif
                // 幫助 GC，減少內存占用
                using (var teamEnum = useMenu.NobTeam.GetEnumerator())
                {
                    while (teamEnum.MoveNext())
                    {
                        var nob = teamEnum.Current;
                        if (nob != null)
                        {
                            視窗狀態.AppendText($@"{nob.PlayerName} : {nob.目前動作} " + Environment.NewLine);
                        }
                    }
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        // 定義私有成員用於快取狀態
        private string _lastStateADescription = string.Empty;
        private bool _lastNPCListUpdateStatus = false;
        private DateTime _lastNPCUpdateTime = DateTime.MinValue;
        private const int MIN_NPC_UPDATE_INTERVAL_MS = 300;

        /// <summary>
        /// 根據需要更新 NPC 目標列表，避免頻繁更新
        /// </summary>
        private void UpdateNPCTargetsIfNeeded()
        {
            _targetManager.UpdateNPCTargetsIfNeeded();
        }

        private async void CB_HID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_HID.SelectedValue == null)
                return;

            string idstr = CB_HID.SelectedValue.ToString();

            if (string.IsNullOrEmpty(idstr))
                return;

            // 取消上一次的任務
            _selectionCts?.Cancel();
            _selectionCts = new System.Threading.CancellationTokenSource();
            var token = _selectionCts.Token;

            try
            {
                // 延遲 200ms，避免快速滾动時頻繁觸發
                await Task.Delay(200, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested) return;

            var user = MainWindow.AllNobWindowsList.Find(r => r.PlayerName == idstr);
            if (user == null) return;

            // 立即更新狀態並滾動到底部
            視窗狀態.AppendText($"[{DateTime.Now:HH:mm:ss}] 正在讀取 {user.PlayerName} 的驗證記錄...\n");
            視窗狀態.ScrollToEnd();
            
            bool autoCheckin = false;
            try
            {
                autoCheckin = await Authentication.讀取認證訊息NameAsync(user) && string.IsNullOrEmpty(user.NOBCDKEY) == false;
            }
            catch
            {
                // 忽略錯誤
            }

            if (token.IsCancellationRequested) return;

            認證TBox.Text = autoCheckin ? user.NOBCDKEY : string.Empty;
            認證2CB.IsChecked = autoCheckin;
            
            if (autoCheckin)
            {
                視窗狀態.AppendText($"[{DateTime.Now:HH:mm:ss}] 已讀取本地驗證記錄\n");
                視窗狀態.ScrollToEnd();
            }
        }

        private void SelectMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = SelectMenu?.SelectedItem as ComboBoxItem;
            var str = item?.Content?.ToString() ?? SelectMenu?.SelectedItem?.ToString();
            _scriptManager.HandleSelectionChanged(str);

            UpdateColorHint();
        }

        private TextBlock? GetColorHintBlock() => this.FindName("ColorHintText") as TextBlock;

        private void UpdateColorHint()
        {
            var colorHint = GetColorHintBlock();
            if (colorHint == null) return;

            bool needsColor = useMenu?.需要顏色判斷 == true;
            if (needsColor)
            {
                colorHint.Text = "UI1: 此腳本需要顏色判斷，請將遊戲解析度設定為 1024x768。";
                colorHint.Visibility = Visibility.Visible;
            }
            else
            {
                colorHint.Text = string.Empty;
                colorHint.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 整合處理所有目標按鈕的點擊事件，將當前選定的目標ID儲存到對應的CodeSetting屬性中
        /// </summary>
        private void OnTargetClick(object sender, RoutedEventArgs e)
        {
            _targetManager.HandleTargetClick(sender);
        }



        private void 目前名單_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                var lbx = (ListBox)sender;
                if (lbx.SelectedValue != null)
                {
                    //   MainNob.Log("lbx " + lbx.SelectedValue.ToString());
                    if (int.TryParse(lbx.SelectedValue.ToString(), out int id))
                    {
                        MainNob!.鎖定NPC(id);
                    }
                }
            }
        }

        private void Button_Click_全隊員跟隨(object sender, RoutedEventArgs e)
        {
            foreach (var item in 隊員智能功能組)
            {
                Debug.WriteLine("_全隊員跟隨");
                //if (item.同步)
                {
                    item.NOB.更改F8追隨();
                    item.NOB.KeyPressPP(VKeys.KEY_F8);
                }
            }
        }

        private void Button_Click_視窗縮小(object sender, RoutedEventArgs e)
        {
            //string str = CMB_Resolution.Text;
            NowSelect()?.縮小();
        }

        private void Button_Click_視窗放大(object sender, RoutedEventArgs e)
        {
            //string str = CMB_Resolution.Text;
            NowSelect()?.還原();

        }

        private void 限制點怪_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(其他選項A.Text, out int num);
            限點數量 = Math.Abs(num);
        }

        private void 排序_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox)
            {
                var lbx = (ListBox)sender;
                if (lbx.SelectedValue == null)
                {
                    return;
                }
                Debug.WriteLine($"鎖定 -> {lbx.SelectedValue.ToString()}");
                if (lbx.SelectedValue != null && string.IsNullOrEmpty(lbx.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(lbx.SelectedValue.ToString(), out int id))
                    {
                        MainNob!.鎖定NPC(id);
                    }
                }
            }
        }

        #endregion

        public void 全員離開戰鬥()
        {
            _teamManager.LeaveBattle();
        }


        private NOBDATA? NowSelect()
        {
            string idstr = CB_HID.SelectedValue?.ToString();

            if (MainWindow.AllNobWindowsList != null && !string.IsNullOrEmpty(idstr))
            {
                return MainWindow.AllNobWindowsList.Find(r => r.PlayerName == idstr);
            }

            return null;
        }

        public void FocusUserWindows()
        {
            MainNob?.FoucsNobWindows();
        }

        //啟動腳本
        private void StartCode_Checked(bool mChecked)
        {
            if (MainNob == null)
                return;

            MainNob.StartRunCode = mChecked;
            if (mChecked && useMenu != null)
            {
                if (MainWindow.AllNobWindowsList != null)
                {
                    Task.Run(() => WebRegistration.OnWebReg());
                }

                儲存隊員技能組();

                MainNob!.CodeSetting.使用定位點 = CB_定位點.IsChecked ?? false;
                MainNob.CodeSetting.定位點X = MainNob.PosX;
                MainNob.CodeSetting.定位點Y = MainNob.PosY;
                if (int.TryParse(腳本Point.Text, out int mpoint))
                {
                    MainNob.CodeSetting.MPoint = mpoint;
                }
                int tryIntNum = 0;
                if (int.TryParse(其他選項A.Text, out tryIntNum))
                {
                    MainNob.CodeSetting.其他選項A = tryIntNum;
                }
                if (int.TryParse(其他選項B.Text, out tryIntNum))
                {
                    MainNob.CodeSetting.其他選項B = tryIntNum;
                }
                if (int.TryParse(FNPCID.Text, out int id))
                {
                    MainNob.CodeSetting.目標A = id;
                }
                if (int.TryParse(SMENU2.Text, out int line))
                {
                    MainNob.CodeSetting.線路 = line;
                }
                // Read difficulty: prefer SMENU1 (per-script small textbox) when visible, otherwise use TB_選擇難度
                int chosenDiff = 0;
                if (SMENU1.Visibility == Visibility.Visible && int.TryParse(SMENU1.Text, out int sm))
                {
                    chosenDiff = sm;
                    Debug.WriteLine("SMENU1 (選擇難度) : " + chosenDiff);
                }
                else if (!string.IsNullOrEmpty(TB_選擇難度.Text) && int.TryParse(TB_選擇難度.Text, out int tbv))
                {
                    chosenDiff = tbv;
                    Debug.WriteLine("TB_選擇難度 : " + chosenDiff);
                }
                MainNob.CodeSetting.選擇難度 = chosenDiff;
                if (int.TryParse(後退時間.Text, out int t))
                {
                    MainNob.CodeSetting.後退時間 = t;
                }
                if (int.TryParse(TB_選擇關卡.Text, out int cc))
                {
                    MainNob.CodeSetting.選擇關卡 = cc;
                }
                if (int.TryParse(TB_SetCNum.Text, out int ssb))
                {
                    MainNob.CodeSetting.連續戰鬥 = ssb;
                }
                if (int.TryParse(TBX搜尋範圍.Text, out int ssa))
                {
                    MainNob.CodeSetting.搜尋範圍 = ssa;
                }
                if (int.TryParse(延遲係數.Text, out int delay))
                {
                    MainNob.CodeSetting.延遲係數 = delay;
                }
                MainNob.CodeSetting.AllInTeam = (CB_AllIn.IsChecked ?? false);
                if (int.TryParse(TB_Set家臣.Text, out int sn))
                {

                }

                List<NOBDATA> list = new();
                if (CB_AllIn.Visibility == Visibility.Visible &&
                   MainNob.CodeSetting.AllInTeam)
                {
                    list = MainWindow.AllNobWindowsList;
                }

                foreach (var user in 隊員智能功能組)
                {
                    if (user != null && user.NOB != null && user.同步 && list!.Find(c => { return c.PlayerName == user.NOB!.PlayerName; }) == null)
                        list.Add(user.NOB);
                }
                if (list?.Find(c => c.PlayerName == MainNob.PlayerName) == null)
                    list?.Add(MainNob);

                if (list != null && list.Count > 0)
                    useMenu.AddNOBList(list);

                SaveSetting();

                if (useMenu != null && list != null)
                {
                    NobTeams = list;
                    MainNob.RunCode = useMenu;
                    Task.Run(MainNob.CodeUpdate);

                    // 通用方法處理所有支持多人同時執行的腳本
                    if (useMenu.多人同時執行)
                    {
                        Task.Delay(200).Wait();

                        // 獲取選擇的腳本類型以進行動態實例化
                        Type scriptType = useMenu.GetType();
                        string scriptName = scriptType.Name;
                        Debug.WriteLine($"scriptName -> {scriptName}");
                        MainNob.Log($"多人同時執行: {scriptName}, 隊員數量: {隊員智能功能組.Count}");

                        foreach (var user in 隊員智能功能組)
                        {
                            if (user != null && user.同步 && !user.NOB!.PlayerName.Contains(MainNob.PlayerName))
                            {
                                // 使用反射動態創建相同類型的腳本實例
                                BaseClass? newScript = null;
                                try
                                {
                                    // 反射創建實例
                                    newScript = (BaseClass)Activator.CreateInstance(scriptType);
                                    MainNob.Log($"為 {user.NOB.PlayerName} 創建 {scriptName} 腳本");
                                }
                                catch (Exception ex)
                                {
                                    MainNob.Log($"創建腳本實例失敗: {ex.Message}");
                                    continue;
                                }

                                if (newScript != null)
                                {
                                    user.NOB.StartRunCode = MainNob.StartRunCode;
                                    // 設置腳本並啟動
                                    user.NOB.RunCode = newScript;
                                    user.NOB.CodeSetting = MainNob.CodeSetting; // 共用設定

                                    Task.Run(user.NOB.CodeUpdate);
                                    Task.Delay(200).Wait();

                                }
                            }
                        }
                    }

                }
            }

            if (!mChecked)
            {
                foreach (var user in 隊員智能功能組)
                {
                    if (user != null && user.同步 && !user.NOB!.PlayerName.Contains(MainNob.PlayerName))
                    {
                        user.NOB.StartRunCode = false;
                    }
                }
            }

            Btn_Refresh.IsEnabled = !mChecked;
        }

        private DateTime _lastReportTime = DateTime.MinValue;

        private async void Btn_ReportIssue_Click(object sender, RoutedEventArgs e)
        {
            // 1. 檢查 VIP 權限
            if (!Tools.IsVIP)
            {
                MessageBox.Show("此功能僅限 VIP 會員使用。", "權限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. 檢查冷卻時間 (1分鐘)
            if ((DateTime.Now - _lastReportTime).TotalMinutes < 1)
            {
                MessageBox.Show($"請勿頻繁回報，請等待 {60 - (int)(DateTime.Now - _lastReportTime).TotalSeconds} 秒後再試。", "操作過快", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _lastReportTime = DateTime.Now;
            Btn_ReportIssue.IsEnabled = false;
            Btn_ReportIssue.Content = "回報中...";

            try
            {
                // 3. 收集資訊
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"NOBReport_{timestamp}");
                Directory.CreateDirectory(tempDir);

                // 3.1 視窗狀態 Log
                string statusLog = 視窗狀態.Text;
                await System.IO.File.WriteAllTextAsync(System.IO.Path.Combine(tempDir, "StatusLog.txt"), statusLog);

                // 3.2 其他 Log 檔案
                string[] logFiles = { "update_error.log", "dm_init.log", "startup_error.log" };
                foreach (var file in logFiles)
                {
                    string srcPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                    if (System.IO.File.Exists(srcPath))
                    {
                        System.IO.File.Copy(srcPath, System.IO.Path.Combine(tempDir, file), true);
                    }
                }

                // 3.3 壓縮
                string zipFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Report_{timestamp}.zip");
                ZipFile.CreateFromDirectory(tempDir, zipFilePath);

                // 4. 發送至 Discord
                string message = $"使用者: {MainNob?.PlayerName ?? "Unknown"}\n帳號: {MainNob?.Account ?? "Unknown"}\n版本: {VersionInfo.Version}\n時間: {DateTime.Now}";
                bool success = await DiscordNotifier.SendFileAsync("🛠️ 使用者問題回報", message, zipFilePath);

                if (success)
                {
                    MessageBox.Show("回報成功！我們會盡快查看您的問題。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("回報失敗，請稍後再試。", "失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // 清理暫存檔
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                if (System.IO.File.Exists(zipFilePath)) System.IO.File.Delete(zipFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"回報過程發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Btn_ReportIssue.IsEnabled = true;
                Btn_ReportIssue.Content = "回報問題 (附Log)";
            }
        }

        private async void Btn_ContactPurchase_Click(object sender, RoutedEventArgs e)
        {
            string contactId = TB_ContactID.Text.Trim();
            if (string.IsNullOrEmpty(contactId))
            {
                MessageBox.Show("請輸入您的 LINE 或 WeChat ID 以便我們聯繫您。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                TB_ContactID.Focus();
                return;
            }

            Btn_ContactPurchase.IsEnabled = false;
            Btn_ContactPurchase.Content = "傳送中...";

            try
            {
                string message = $"使用者想要購買或加時 VIP\n\n" +
                                 $"聯絡 ID: {contactId}\n" +
                                 $"目前帳號: {MainNob?.Account ?? "Unknown"}\n" +
                                 $"目前角色: {MainNob?.PlayerName ?? "Unknown"}\n" +
                                 $"目前到期日: {MainNob?.到期日.ToString("yyyy-MM-dd") ?? "Unknown"}";

                bool success = await DiscordNotifier.SendNotificationAsync("💰 購買/加時請求", message);

                if (success)
                {
                    MessageBox.Show("請求已發送！我們會盡快透過您提供的 ID 聯繫您。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    TB_ContactID.Text = ""; // 清空輸入框
                }
                else
                {
                    MessageBox.Show("發送失敗，請稍後再試。", "失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"發送過程發生錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Btn_ContactPurchase.IsEnabled = true;
                Btn_ContactPurchase.Content = "聯絡購買/加時";
            }
        }

        private async void CheckValidityButton_Click(object sender, RoutedEventArgs e)
        {
            await _authManager.CheckValidity(視窗狀態, 到期計時);
        }

        public void LoadSetting()
        {
            if (MainNob == null) return;

            try
            {
                // Ensure team member structures are initialized from current running windows
                _teamManager.InitializeTeamMembersFromAllNobs();

                // Load player-specific settings from disk into MainNob.CodeSetting
                _scriptManager.LoadSetting(MainNob.PlayerName);

                // Reflect loaded settings into the UI
                _scriptManager.SettingLoadToUI();

                // Apply saved team skill records (同步, 技能設定) to 隊員智能功能組
                _teamManager.UpdateAutoSkillTeamMembers();

                Debug.WriteLine("LoadSetting: completed initialization and applied team settings.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadSetting: failed to load/apply settings: {ex.Message}");
            }
        }

        public void SettingLoadToUI()
        {
            // Implementation based on original logic
            if (_scriptManager != null)
            {
                _scriptManager.SettingLoadToUI();
            }
        }

        // Expose a method to read current UI values back into MainNob.CodeSetting
        public void ReadSettingFromUI()
        {
            try
            {
                _scriptManager?.ReadSettingFromUI();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReadSettingFromUI failed: {ex.Message}");
            }
        }

        public void RestartTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public void SaveSetting()
        {
            if (MainNob != null)
            {
                _scriptManager.SaveSetting(MainNob.PlayerName);
            }
        }
        
        /// <summary>
        /// 更新剩餘天數顯示
        /// </summary>
        private async void UpdateRemainingDays()
        {
            if (_authManager != null)
            {
               await _authManager.UpdateRemainingDays(到期計時);
            }
        }

        public void 儲存隊員技能組()
        {
            _teamManager.SaveTeamSkillSet();
        }

        public void 讀取隊員技能組()
        {
            _teamManager.LoadTeamSkillSet();
        }

        public void 更新自動使用技能隊員名單()
        {
            _teamManager.UpdateAutoSkillTeamMembers();
        }

        public void AddRound_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && int.TryParse(tag, out int memberIndex))
            {
                var panelName = $"RoundsPanel_{memberIndex}";
                var panel = this.FindName(panelName) as StackPanel;

                // 檢查目前已有的回合數
                // 如果不是 VIP 且已經有 1 個回合，則禁止新增
                if (!Tools.IsVIP && panel != null && panel.Children.Count >= 1)
                {
                    MessageBox.Show("非 VIP 會員僅能設定一輪戰鬥。\n若需設定多輪戰鬥，請升級 VIP 會員。", "權限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                AddRoundRow(memberIndex);
            }
        }

        public void ClearRounds_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.Tag is string tag && int.TryParse(tag, out int memberIndex))
            {
                var panelName = $"RoundsPanel_{memberIndex}";
                var panel = this.FindName(panelName) as StackPanel;
                if (panel != null && panel.Children.Count > 0)
                {
                    panel.Children.RemoveAt(panel.Children.Count - 1);

                    // 當移除回合後，恢復上一個回合的重複選項功能
                    if (panel.Children.Count > 0)
                    {
                        var lastRow = panel.Children[panel.Children.Count - 1] as Canvas;
                        if (lastRow != null)
                        {
                            foreach (var child in lastRow.Children)
                            {
                                if (child is CheckBox cb && (cb.Tag as string) == "重複")
                                {
                                    cb.IsEnabled = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddRoundRow(int memberIndex, RoundConfig? config = null, int roundNum = -1)
        {
            var panelName = $"RoundsPanel_{memberIndex}";
            var panel = this.FindName(panelName) as StackPanel;
            if (panel == null) return;

            // 新增回合時，將上一個回合的重複選項取消並鎖定
            if (panel.Children.Count > 0)
            {
                var lastRow = panel.Children[panel.Children.Count - 1] as Canvas;
                if (lastRow != null)
                {
                    foreach (var child in lastRow.Children)
                    {
                        if (child is CheckBox cb && (cb.Tag as string) == "重複")
                        {
                            cb.IsChecked = false;
                            cb.IsEnabled = false;
                            break;
                        }
                    }
                }
            }

            Canvas row = new Canvas();
            row.Height = 30;

            // Helper to create TextBox
            TextBox CreateTextBox(string name, string def, double w, double l, string tip, string bg)
            {
                var tb = new TextBox
                {
                    Text = def,
                    Width = w,
                    Height = 20,
                    TextWrapping = TextWrapping.Wrap,
                    ToolTip = tip,
                    MaxLength = 4,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(bg),
                    Tag = name 
                };
                Canvas.SetLeft(tb, l);
                Canvas.SetTop(tb, 5);
                try { tb.Style = (Style)FindResource("CurrentInputStyle"); } catch {}
                return tb;
            }

            // Helper to create CheckBox
            CheckBox CreateCheckBox(string name, string content, double l)
            {
                var cb = new CheckBox
                {
                    Content = content,
                    Tag = name
                };
                Canvas.SetLeft(cb, l);
                Canvas.SetTop(cb, 8);
                return cb;
            }

            // Helper to create Label
            Label CreateLabel(string name, string content, double w, double l, string tip, string bg)
            {
                var lbl = new Label
                {
                    Content = content,
                    Width = w,
                    Height = 24,
                    ToolTip = tip,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(bg),
                    Tag = name,
                    Padding = new Thickness(0),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(lbl, l);
                Canvas.SetTop(lbl, 3);
                return lbl;
            }

            int rNum = roundNum != -1 ? roundNum : (panel.Children.Count + 1);
            var lblRound = CreateLabel("回合", rNum.ToString(), 30, 0, "回合數", "#FFFCE4EC");

            // CheckBoxes
            var cbOnce = CreateCheckBox("開場一", "一次", 35);
            var cbRepeat = CreateCheckBox("重複", "重複", 90);
            if (config != null)
            {
                cbOnce.IsChecked = config.一次放;
                cbRepeat.IsChecked = config.重複放;
            }
            
            // TextBoxes
            var tbSpeed = CreateTextBox("程式速度", config?.程式速度.ToString() ?? "100", 56, 145, "程式運作速度", "#FFE8F5E9");
            
            var tbDelay = CreateTextBox("延遲施放", config?.延遲.ToString() ?? "0", 56, 205, "延遲", "#FFFCE4EC");
            var tbInterval = CreateTextBox("間隔時間放", config?.間隔.ToString() ?? "0", 56, 265, "間隔", "#FFFCE4EC");
            
            var tbS1 = CreateTextBox("技能段1", config?.技能段1.ToString() ?? "", 32, 325, "技能段1", "#FFFFF9C4");
            var tbS2 = CreateTextBox("技能段2", config?.技能段2.ToString() ?? "", 32, 360, "技能段2", "#FFFFF9C4");
            var tbS3 = CreateTextBox("技能段3", config?.技能段3.ToString() ?? "", 32, 395, "技能段3", "#FFFFF9C4");
            
            var tbA = CreateTextBox("施放A", config?.施放A ?? "", 32, 430, "施放A", "#FFC8E6C9");
            var tbB = CreateTextBox("施放B", config?.施放B ?? "", 32, 465, "施放B", "#FFC8E6C9");
            var tbC = CreateTextBox("施放C", config?.施放C ?? "", 32, 500, "施放C", "#FFC8E6C9");

            row.Children.Add(lblRound);
            row.Children.Add(cbOnce);
            row.Children.Add(cbRepeat);
            row.Children.Add(tbSpeed);
            row.Children.Add(tbDelay);
            row.Children.Add(tbInterval);
            row.Children.Add(tbS1);
            row.Children.Add(tbS2);
            row.Children.Add(tbS3);
            row.Children.Add(tbA);
            row.Children.Add(tbB);
            row.Children.Add(tbC);

            panel.Children.Add(row);
        }

        public void SetTabHeader(string text)
        {
            if (RootTabItem == null) return;

            if (RootTabItem.Header is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Text = text;
                        return;
                    }
                }
            }
            else
            {
                // Recreate the structure if it's missing
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var headerText = new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center };
                var focusButton = new Button 
                { 
                    Content = "F", 
                    Width = 20, 
                    Height = 20, 
                    Margin = new Thickness(5, 0, 0, 0),
                    ToolTip = "Focus Window",
                    FontSize = 10,
                    Padding = new Thickness(0)
                };
                
                focusButton.Click += (s, e) => FocusUserWindows();

                headerPanel.Children.Add(headerText);
                headerPanel.Children.Add(focusButton);
                RootTabItem.Header = headerPanel;
            }
        }
    }
}
