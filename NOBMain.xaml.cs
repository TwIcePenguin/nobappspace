using NOBApp.GoogleData;
using NOBApp.Sports;
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
using System.Windows.Interop;
using System.Windows.Threading;

namespace NOBApp
{
    /// <summary>
    /// NobMain.xaml 的互動邏輯
    /// </summary>
    public partial class NobMainCodePage : UserControl
    {
        BaseClass? useMenu = null;
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
        static 隊伍技能紀錄 m隊伍技能紀錄 = new();
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

        // 需要恢復的玩家名稱
        public string PlayerToRestore { get; set; } = "";

        public NobMainCodePage()
        {
            InitializeComponent();
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

        void UIStatus_Default()
        {

            其他選項A.Visibility =
            Btn移除名單.Visibility = Btn鎖定目標添加.Visibility =
            List_鎖定名單.Visibility =
            CB_定位點.Visibility = 武技設定頁面.Visibility =
            SMENU2.Visibility = 後退時間.Visibility = TargetViewPage.Visibility = CB_AllIn.Visibility = TB_選擇關卡.Visibility = TB_選擇難度.Visibility = TB_SetCNum.Visibility =
            FNPCID.Visibility = SMENU1.Visibility =
            Btn_TargetF.Visibility = Btn_TargetD.Visibility = Btn_TargetE.Visibility =
            Btn_TargetC.Visibility = Btn_TargetB.Visibility = Btn_TargetA.Visibility = Visibility.Hidden;
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

            儲存套路.Click += 儲存套路_Click;

            直接下指令_1.TextChanged += 直接下指令_TextChanged;
            同步_1.Click += 同步_Click;

            戰鬥輔助面.LayoutUpdated += 戰鬥輔助面_LayoutUpdated;

            List_鎖定名單.SelectionChanged += 排序_SelectionChanged;
            List_忽略名單.SelectionChanged += 排序_SelectionChanged;
            List_目前名單.SelectionChanged += 排序_SelectionChanged;

            VIPSP.Checked += (s, e) =>
            {
                if (MainNob != null)
                    MainNob.VIPSP = true;
                Task.Run(MainNob!.速度);
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
        }

        #region 目標選擇UI
        // 在「鎖定目標添加」按鈕的事件中添加：
        private void 鎖定目標添加_Click(object sender, RoutedEventArgs e)
        {
            var id = MainNob!.GetTargetIDINT();
            if (!id.Equals(4294967295) && id > 0)
            {
                if (!TargetsID.Contains(id))
                {
                    TargetsID.Add(id);
                }

                filteredNPCs = allNPCs.Where(npc => !IgnoredIDs.Contains(npc.ID) || !TargetsID.Contains(npc.ID)).ToList();
                List_目前名單.ItemsSource = filteredNPCs;
                List_鎖定名單.ItemsSource = TargetsID;
                List_鎖定名單.Items.Refresh();
                List_目前名單.Items.Refresh();
            }
        }

        private void 切換名單_Click(object sender, RoutedEventArgs e)
        {
            if (List_目前名單 == null || List_鎖定名單 == null || List_忽略名單 == null)
                return;

            if (List_目前名單 != null && List_目前名單.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(List_目前名單.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(List_目前名單.SelectedValue.ToString(), out int id))
                    {
                        if (IgnoredIDs.Contains(id) == false)
                            IgnoredIDs.Add(id);
                        if (AllNPCIDs.Contains(id))
                            AllNPCIDs.Remove(id);
                    }
                }
            }

            if (List_忽略名單 != null && List_忽略名單.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(List_忽略名單.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(List_忽略名單.SelectedValue.ToString(), out int id))
                    {
                        if (IgnoredIDs.Contains(id))
                            IgnoredIDs.Remove(id);
                        if (AllNPCIDs.Contains(id) == false)
                            AllNPCIDs.Add(id);
                    }
                }
            }

            //filteredNPCs = allNPCs.Where(npc => !IgnoredIDs.Contains(npc.ID) || !TargetsID.Contains(npc.ID)).ToList();
            // 更新列表顯示
            List_鎖定名單!.ItemsSource = TargetsID;
            List_目前名單!.ItemsSource = AllNPCIDs;
            List_忽略名單!.ItemsSource = IgnoredIDs;
            List_忽略名單.Items.Refresh();
            List_目前名單.Items.Refresh();
            List_鎖定名單.Items.Refresh();
        }

        private void 添加名單_Click(object sender, RoutedEventArgs e)
        {
            if (List_目前名單 == null || List_鎖定名單 == null || List_忽略名單 == null)
                return;

            if (List_目前名單 != null && List_目前名單.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(List_目前名單.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(List_目前名單.SelectedValue.ToString(), out int id))
                    {
                        if (IgnoredIDs.Contains(id) == false)
                            IgnoredIDs.Add(id);
                        if (AllNPCIDs.Contains(id))
                            AllNPCIDs.Remove(id);
                    }
                }
            }

            if (List_忽略名單 != null && List_忽略名單.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(List_忽略名單.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(List_忽略名單.SelectedValue.ToString(), out int id))
                    {
                        if (IgnoredIDs.Contains(id))
                            IgnoredIDs.Remove(id);
                        if (AllNPCIDs.Contains(id) == false)
                            AllNPCIDs.Add(id);
                    }
                }
            }

            //filteredNPCs = allNPCs.Where(npc => !IgnoredIDs.Contains(npc.ID) || !TargetsID.Contains(npc.ID)).ToList();
            List_鎖定名單!.ItemsSource = TargetsID;
            List_目前名單!.ItemsSource = AllNPCIDs;
            List_忽略名單!.ItemsSource = IgnoredIDs;
            List_忽略名單.Items.Refresh();
            List_目前名單.Items.Refresh();
            List_鎖定名單.Items.Refresh();
        }

        #endregion

        #region UIEvent 
        private void 戰鬥輔助面_LayoutUpdated(object? sender, EventArgs e)
        {
            int offsetY = 100;
            if (pageB_isExpanded != 腳本展區.IsExpanded || pageA_isExpanded != 戰鬥輔助面.IsExpanded)
            {
                pageB_isExpanded = 腳本展區.IsExpanded;
                pageA_isExpanded = 戰鬥輔助面.IsExpanded;

                double tA = pageB_isExpanded ? 300 + offsetY : 0;
                double tB = pageA_isExpanded ? 370 + offsetY : 0;
                Thickness nThickness = oThickness;
                nThickness.Top = oThickness.Top + tA;
                MainWindow.Instance!.UIRefrshSize(pageB_isExpanded, pageA_isExpanded);
                戰鬥輔助面.Margin = nThickness;
            }
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
                MainNob.isUseEnter = E點怪.IsChecked ?? false;
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
            if (隊員智能功能組 != null && 隊員智能功能組.Count > 0)
            {
                for (int i = 0; i < 隊員智能功能組.Count; i++)
                {
                    var nb = 隊員智能功能組[i];
                    if (nb.同步)
                    {
                        nb.NOB.KeyPressPP(key);
                    }
                }
            }
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

            SaveSetting();
            儲存隊員技能組();
            foreach (var item in 隊員智能功能組)
            {
                if (item == null || item.NOB == null)
                    continue;

                item.NOB.IsUseAutoSkill = useAutoSkill;

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
            bool reset = LockBtn.Content.ToString()!.Contains("解除");
            bool isPass = false;

#if DEBUG && false
            腳本區.IsEnabled = 腳本展區.IsEnabled = 戰鬥輔助面.IsEnabled = true;
            UpdateSelectMenu();
            return;
#endif

            // 確認 是否有選擇 CB_HID
            if (LockBtn != null && ControlGrid != null && CB_HID != null &&
                CB_HID.SelectedValue != null && MainWindow.AllNobWindowsList != null)
            {
                string idstr = CB_HID.SelectedValue.ToString();
                if (!reset && !string.IsNullOrEmpty(idstr))
                {
                    MainNob = MainWindow.AllNobWindowsList?.Find(r => r.PlayerName == idstr);

                    if (MainWindow.AllNobWindowsList == null || MainWindow.AllNobWindowsList.Count == 0)
                    {
                        MessageBox.Show("請先刷新角色資料");
                        return;
                    }

                    Debug.WriteLine($"Web Reg {MainWindow.AllNobWindowsList.Count}");
                    Task.Run(() => WebRegistration.OnWebReg());

                    if (MainNob != null)
                    {
                        視窗狀態.Clear();
                        視窗狀態.AppendText("驗證中.. 請稍後\n");

                        if (!MainNob.驗證完成)
                        {
                            if (認證2CB.IsChecked == true)
                            {
                                if (string.IsNullOrEmpty(認證TBox.Text))
                                {
                                    GoogleSheet.GoogleSheetInit();
                                    GoogleSheet.CheckDonate(MainNob);
                                }
                                else
                                {
                                    Authentication.讀取認證訊息Json(MainNob, 認證TBox.Text);
                                }
                            }

                            int checkCount = 0;
                            while (true)
                            {
                                Debug.WriteLine($"MainNob 驗證 {MainNob.驗證完成} Count {checkCount}");
                                if (MainNob.驗證完成)
                                {
                                    視窗狀態.Text = "驗證完成!";
                                    checkCount = 0;
                                    break;
                                }
                                else
                                {
                                    checkCount++;
                                    視窗狀態.Text = $"驗證中! -- {checkCount}";
                                }
                                if (checkCount >= 60)
                                {
                                    視窗狀態.Text = "等待超時 請重新點選驗證";
                                    return;
                                }
                                await Task.Delay(400);
                            }
                        }
                        視窗狀態.AppendText("取得相關資料 比對中..\n");
                        try
                        {
                            bool SPPass = MainNob.特殊者 ? MainNob.驗證國家 : MainNob.贊助者;
                            if (MainNob.特殊者 && !MainNob.驗證國家)
                            {
                                MessageBox.Show("免費使用者 需要加入遊戲頻道 請聯繫企鵝 取得加入的方式 或著請認識的朋友提供");
                                return;
                            }

                            MainNob.驗證完成 = SPPass;
                            isPass = SPPass;

                            Debug.WriteLine($"MainNob 驗證 {isPass} {MainNob.特殊者} {MainNob.驗證國家}");
                        }
                        catch (Exception err)
                        {
                            視窗狀態.AppendText($"資料錯誤.. \n{err}\n");
                        }

                        Tools.SetTimeUp(MainNob);
                        視窗狀態.AppendText($"驗證完成.. 更新時間 -> {MainNob.到期日}\n");
                        到期計時.Content = $"到期日:{MainNob.到期日}";

                        //暫時將到期關閉
                        if (MainNob.到期日 >= DateTime.Now)
                        {
                            VIPSP.IsEnabled = true;
                        }

                        if (isPass)
                        {
                            _timer.Stop();
                            _timer.Start();
                            視窗狀態.AppendText($"通過驗證..");
                            腳本區.IsEnabled = 腳本展區.IsEnabled = 戰鬥輔助面.IsEnabled = true;
                            UpdateSelectMenu();
                            LoadSetting();
                            讀取隊員技能組();

                            // 通知MainWindow保存標籤頁狀態
                            MainWindow.Instance?.SaveTabState();
                        }
                        else
                        {
                            視窗狀態.AppendText($"該帳號 驗證失敗.. 請聯繫企鵝處理");
                            MainNob.StartRunCode = false;
                            _timer.Stop();
                        }
                    }
                    else
                    {
                        StartCode.IsChecked = false;
                        StartCode.UpdateLayout();
                        MessageBox.Show("選擇異常 不存在的角色資料 或著該角色被關閉請刷新後 請重新嘗試驗證");
                    }
                }

                // 通過驗證開啟功能 並鎖定使用者
                {
                    隊員額外功能頁面.IsEnabled = isPass;
                    SkillDataGird.IsEnabled = isPass;
                    UseSkill_CB.IsEnabled = isPass;
                    ControlGrid.IsEnabled = isPass;
                    CB_HID.IsEnabled = !isPass;
                    if (isPass)
                    {
                        RootTabItem.Header = $"{MainNob!.PlayerName}";
                    }

                    LockBtn.Content = isPass ? "解除" : "驗證";
                    LockBtn.UpdateLayout();
                }
            }
            else
            {
                MessageBox.Show("請選擇角色 ，　如果清單沒有角色名稱，開啟遊戲登入選擇完角色後點［刷新］");

                LockBtn.Content = "驗證";
                CB_HID.IsEnabled = true;
                CB_HID.UpdateLayout();
                StartCode.IsChecked = false;
                StartCode.UpdateLayout();
            }
        }

        private void UpdateSelectMenu()
        {
            SelectMenu.Items.Clear();
            // 獲取 Sports 命名空間中的所有類別，並排除 BaseClass
            var sportsClasses = Assembly.GetExecutingAssembly()
                                        .GetTypes()
                                        .Where(t => t.IsClass && t.Namespace == "NOBApp.Sports" && t.Name != "BaseClass" && !t.Name.Contains("<"))
                                        .Select(t => t.Name)
                                        .ToList();

            // 將類別名稱加入 SelectMenu
            foreach (var className in sportsClasses)
            {
                SelectMenu.Items.Add(className);
            }

            menuMapping = new Dictionary<string, Action>
            {
                { "刷熊本城", () => { useMenu = new 刷熊本城(); Btn_TargetA.Content = "入場NPC"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "四聖青龍", () => { useMenu = new 四聖青龍(); Btn_TargetA.Content = "老頭"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_酒井", () => { useMenu = new 討伐2025_酒井(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_今川氏真", () => { useMenu = new 討伐2025_今川氏真(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_謎之怪", () => { useMenu = new 討伐2025_謎之怪(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_後藤", () => { useMenu = new 討伐2025_後藤(); Btn_TargetA.Content = "安土NPC"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_朝比奈", () => { useMenu = new 討伐2025_朝比奈(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_百地", () => { useMenu = new 討伐2025_百地(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_井伊", () => { useMenu = new 討伐2025_井伊(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_松", () => { useMenu = new 討伐2025_松(); Btn_TargetA.Content = "水滴"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_白石", () => { useMenu = new 討伐2025_白石(); Btn_TargetA.Content = "安土奉行"; Btn_TargetA.Visibility = Visibility.Visible; } },
                { "幽靈船全刷", () => { useMenu = new 幽靈船全刷(); Btn_TargetA.Content = "九鬼"; SMENU2.Visibility = Btn_TargetA.Visibility = Visibility.Visible; } },

                { "夢幻城", () => { useMenu = new 夢幻城(); } },
                { "採集輔助", () => { useMenu = new 採集輔助(); } },
                { "生產輔助", () => { useMenu = new 生產輔助(); CB_定位點.Visibility = Btn_TargetA.Visibility = Visibility.Visible; } },
                { "生產破魔", () => { useMenu = new 生產破魔(); Btn_TargetA.Visibility = Btn_TargetB.Visibility = Btn_TargetC.Visibility = Btn_TargetD.Visibility = Btn_TargetE.Visibility = Visibility.Visible; } },

                { "戰場製炮", () => {
                    useMenu = new 戰場製炮();
                    Btn_TargetA.Content = "目付";
                    Btn_TargetB.Content = "砲基座";
                    Btn_TargetC.Content = "生砲道具";
                    CB_定位點.Visibility = 後退時間.Visibility = Btn_TargetC.Visibility
                    = Btn_TargetB.Visibility = Btn_TargetA.Visibility = Visibility.Visible; } },
                { "冥宮", () => { useMenu = new 冥宮();  } },
                { "鬼島", () => { useMenu = new 鬼島(); Btn_TargetA.Content = "村長-補符"; TargetViewPage.Visibility = Visibility.Visible; CB自動鎖定PC.Visibility = CB鎖定後自動黑槍.Visibility = List_鎖定名單.Visibility = Visibility.Hidden; Btn_TargetA.Visibility = Visibility.Visible; 其他選項A.Text = "80"; 其他選項B.Text = "0"; } },
                { "上覽打錢", () => { useMenu = new 上覽打錢(); Btn_TargetA.Content = "目標大黑天"; Btn_TargetB.Visibility = Btn_TargetA.Visibility = SMENU1.Visibility = SMENU2.Visibility = Visibility.Visible; } },
                //{ "AI上覽", () => { useMenu = new AI上覽(); Btn_TargetA.Content = "目標大黑天"; Btn_TargetB.Visibility = Btn_TargetA.Visibility = SMENU1.Visibility = SMENU2.Visibility = Visibility.Visible; } },

                { "地下町天地", () => { useMenu = new 地下町天地(); 武技設定頁面.Visibility = CB_AllIn.Visibility = TB_選擇關卡.Visibility = Btn_TargetC.Visibility = TB_選擇難度.Visibility = TB_SetCNum.Visibility = Visibility.Visible; } },
                { "超級打怪", () => { useMenu = new 超級打怪(); Btn_TargetA.Content = "鎖定目標"; Btn_TargetA.Visibility = Visibility.Visible; } },
                //{ "黑槍特搜", () => { useMenu = new 黑槍特搜(); CB自動鎖定PC.Visibility = CB鎖定後自動黑槍.Visibility = List_鎖定名單.Visibility = TargetViewPage.Visibility = Visibility.Visible; } },
                { "隨機打怪", () => { useMenu = new 隨機打怪(); UpdateNPCDataUI = true; CB自動鎖定PC.Visibility = List_目前名單.Visibility = TargetViewPage.Visibility = Visibility.Visible; CB鎖定後自動黑槍.Visibility = Visibility.Hidden; } }
            };
            if (VIPSP.IsEnabled == false)
                SelectMenu.Items.Remove("生產破魔");
            SelectMenu.UpdateLayout();
        }

        void OnRefreshNOBID(object sender, RoutedEventArgs e)
        {
            MainWindow.RefreshNOBID(CB_HID, comboBoxes);
        }

        public void CustomUpdate(object? sender, EventArgs e)
        {
            if (MainNob == null)
            {
                return;
            }

            //if (DateTime.Now > MainNob.到期日)
            //{
            //    MainNob.Log($"到期日 : {MainNob.到期日}");
            //    腳本區.IsEnabled = 腳本展區.IsEnabled = 戰鬥輔助面.IsEnabled = false;
            //    MainNob.驗證完成 = false;
            //    MainNob.特殊者 = false;
            //    MainNob.贊助者 = false;
            //    MainNob.StartRunCode = false;
            //    MainNob.IsUseAutoSkill = false;
            //    useMenu = null;
            //    MainNob = null;
            //    StartCode.IsChecked = false;
            //    StartCode.UpdateLayout();
            //    LockBtn.Content = "驗證";
            //    LockBtn.UpdateLayout();
            //    _timer.Stop();
            //    MessageBox.Show("免費試用到期 : 請聯繫企鵝增加使用時間感謝 或 贊助企鵝幫讓這個科技可以繼續延續下去! 贊助後再按一次 [需鎖定] 會自動更新日期 感謝各位夥伴的支持");
            //}

            if ((LockBtn != null && LockBtn.Content.ToString()!.Contains("驗證")))
                return;

            if (MainNob != null && useMenu != null && useMenu.NobTeam != null && 視窗狀態 != null)
            {
                視窗狀態.Clear();
                string stateADescription = MainWindow.GetStateADescription(MainNob.StateA);
                視窗狀態.AppendText($@"LDS:{stateADescription} S:{MainWindow.MainState} " + Environment.NewLine);
                for (int i = 0; i < useMenu.NobTeam.Count; i++)
                {
                    NOBDATA nob = useMenu.NobTeam[i];
                    if (nob != null)
                    {
                        視窗狀態.AppendText($@"{nob.PlayerName} : {nob.目前動作} " + Environment.NewLine);
                    }
                }
            }
            //更新搜尋目標
            if (UpdateNPCDataUI && TargetViewPage.Visibility == Visibility.Visible)
            {
                List_忽略名單.ItemsSource = IgnoredIDs;
                List_鎖定名單.ItemsSource = TargetsID;
                List_目前名單.ItemsSource = AllNPCIDs;
                List_鎖定名單.Items.Refresh();
                List_目前名單.Items.Refresh();
                List_忽略名單.Items.Refresh();
            }

            if (CB_BKAutoEnter.IsChecked == true && MainNob != null)
            {
                MainNob.KeyPress(VKeys.KEY_ENTER);
            }
        }

        private void CB_HID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CB_HID.SelectedValue == null)
                return;

            string idstr = CB_HID.SelectedValue.ToString();

            if (string.IsNullOrEmpty(idstr))
                return;

            var user = MainWindow.AllNobWindowsList.Find(r => r.PlayerName == idstr);
            if (user == null) return;

            bool autoCheckin = Authentication.讀取認證訊息Name(user) && string.IsNullOrEmpty(user.NOBCDKEY) == false;

            認證TBox.Text = autoCheckin ? user.NOBCDKEY : string.Empty;
            認證2CB.IsChecked = autoCheckin;
        }

        private void SelectMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var str = SelectMenu?.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(str))
            {
                StartCode.IsEnabled = false;
                StartCode.UpdateLayout();
                return;
            }
            StartCode.IsChecked = false;
            StartCode.IsEnabled = !string.IsNullOrEmpty(str);
            useMenu = null;
            UIStatus_Default();
            UIUpdate.RefreshNOBID_Sec(comboBoxes, MainWindow.AllNobWindowsList);
            if (menuMapping.ContainsKey(str))
            {
                menuMapping[str].Invoke();
            }
            else
            {
                StartCode.IsEnabled = false;
            }

            StartCode.UpdateLayout();
        }
        /// <summary>
        /// 整合處理所有目標按鈕的點擊事件，將當前選定的目標ID儲存到對應的CodeSetting屬性中
        /// </summary>
        private void OnTargetClick(object sender, RoutedEventArgs e)
        {
            if (MainNob == null)
                return;

            // 獲取目標ID
            int tid = (int)MainNob.GetTargetIDINT();

            // 獲取按鈕引用
            Button clickedButton = sender as Button;
            if (clickedButton == null)
                return;

            // 根據按鈕名稱確定目標位置
            string buttonName = clickedButton.Name;

            if (buttonName.Contains("TargetA"))
            {
                MainNob.CodeSetting.目標A = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetB"))
            {
                MainNob.CodeSetting.目標B = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetC"))
            {
                MainNob.CodeSetting.目標C = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetD"))
            {
                // 需要在 Setting 類中添加目標D屬性
                MainNob.CodeSetting.目標D = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetE"))
            {
                // 需要在 Setting 類中添加目標E屬性
                MainNob.CodeSetting.目標E = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetF"))
            {
                // 需要在 Setting 類中添加目標F屬性
                MainNob.CodeSetting.目標F = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
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
            var r = MainWindow.GetResolutioSize();
            foreach (var nob in NobTeams)
            {
                if (nob != null)
                {
                    int inPosX = (int)r.X / 2;
                    int inPosY = (int)r.Y / 2 - 50;
                    nob.MR_Click(inPosX + 16, inPosY);
                    Task.Delay(50).Wait();
                    nob.MR_Click(inPosX - 100, inPosY);
                    Task.Delay(50).Wait();
                    nob.MR_Click(inPosX - 100, inPosY + 100);
                }
            }
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

                MainNob!.CodeSetting.使用定位點 = CB_定位點.IsChecked ?? false;
                MainNob.CodeSetting.定位點X = MainNob.PosX;
                MainNob.CodeSetting.定位點Y = MainNob.PosY;
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
                if (int.TryParse(SMENU1.Text, out int lv))
                {
                    Debug.WriteLine("SMENU1 : " + lv);
                    MainNob.CodeSetting.選擇難度 = lv;
                }
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
                    if (user != null && user.同步 && list!.Find(c => { return c.PlayerName == user.NOB!.PlayerName; }) == null)
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


                    if (useMenu is 夢幻城)
                    {
                        Task.Delay(200).Wait();
                        //  MainNob.Log("隊員智能功能組 : " + 隊員智能功能組.Count);
                        foreach (var user in 隊員智能功能組)
                        {
                            //  MainNob.Log($"隊員智能功能組 : {user.同步} {user.NOB.PlayerName}-");
                            if (user != null && user.同步 && !user.NOB!.PlayerName.Contains(MainNob.PlayerName))
                            {
                                user.NOB.RunCode = new 夢幻城();
                                user.NOB.CodeSetting = MainNob.CodeSetting;
                                Task.Run(user.NOB.CodeUpdate);
                                Task.Delay(200).Wait();
                            }
                        }
                    }

                    if (useMenu is 戰場製炮)
                    {
                        Task.Delay(200).Wait();
                        //  MainNob.Log("隊員智能功能組 : " + 隊員智能功能組.Count);
                        foreach (var user in 隊員智能功能組)
                        {
                            //  MainNob.Log($"隊員智能功能組 : {user.同步} {user.NOB.PlayerName}-");
                            if (user != null && user.同步 && !user.NOB!.PlayerName.Contains(MainNob.PlayerName))
                            {
                                user.NOB.RunCode = new 戰場製炮();
                                user.NOB.CodeSetting = MainNob.CodeSetting;
                                Task.Run(user.NOB.CodeUpdate);
                                Task.Delay(200).Wait();
                            }
                        }
                    }
                }
            }
            Btn_Refresh.IsEnabled = !mChecked;
        }

        void 儲存隊員技能組()
        {
            更新自動使用技能隊員名單();

            if (隊員智能功能組 != null && 隊員智能功能組.Count > 0)
            {
                List<隊員資料紀錄檔> list = new();
                foreach (var item in 隊員智能功能組)
                {
                    if (item == null || item.NOB == null)
                        continue;

                    隊員資料紀錄檔 user = new();
                    user.用名 = item.NOB.PlayerName;
                    user.同步 = item.同步;
                    user.一次放 = item.一次放;
                    user.重複放 = item.重複放;
                    user.延遲 = item.延遲;
                    user.技能段1 = item.技能段1;
                    user.技能段2 = item.技能段2;
                    user.技能段3 = item.技能段3;
                    user.施放A = item.施放A;
                    user.施放B = item.施放B;
                    user.施放C = item.施放C;
                    user.間隔 = item.間隔;
                    user.程式速度 = item.程式速度;
                    list.Add(user);
                }

                if (A套路.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成A");
                    m隊伍技能紀錄.方案A = list;
                }
                if (B套路.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成B");
                    m隊伍技能紀錄.方案B = list;
                }
                if (C套路.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成C");
                    m隊伍技能紀錄.方案C = list;
                }

                SaveSetting();
            }
        }

        void 讀取隊員技能組()
        {
            if (m隊伍技能紀錄 != null)
            {
                List<隊員資料紀錄檔> list = new();
                if (A套路.IsChecked == true)
                {
                    list = m隊伍技能紀錄.方案A;
                    Debug.WriteLine("讀取A");
                }
                if (B套路.IsChecked == true)
                {
                    list = m隊伍技能紀錄.方案B;
                    Debug.WriteLine("讀取B");
                }
                if (C套路.IsChecked == true)
                {
                    list = m隊伍技能紀錄.方案C;
                    Debug.WriteLine("讀取C");
                }
                int no = 1;
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        if (!string.IsNullOrEmpty(item.用名))
                        {
                            寫入技能設定(no, item);
                            no = no + 1;
                        }
                    }
                }
                void 寫入技能設定(int num, 隊員資料紀錄檔 set)
                {
                    Debug.WriteLine($"讀取 {隊員額外功能頁面.Children.Count}");
                    foreach (var c1 in 隊員額外功能頁面.Children)
                    {
                        if (c1 is Canvas)
                        {
                            Canvas c = (Canvas)c1;
                            if (c.Name.Contains($"_{num}"))
                            {
                                foreach (var item in c.Children)
                                {
                                    if (item is CheckBox)
                                    {
                                        CheckBox cb = (CheckBox)item;
                                        if (cb.Name.Contains("重複"))
                                            cb.IsChecked = set.重複放;
                                        if (cb.Name.Contains("開場"))
                                            cb.IsChecked = set.一次放;
                                        if (cb.Name.Contains("同步"))
                                            cb.IsChecked = set.同步;
                                    }

                                    if (item is TextBox)
                                    {
                                        TextBox tb = (TextBox)item;
                                        if (tb.Name.Contains("延遲"))
                                            tb.Text = set.延遲.ToString();
                                        if (tb.Name.Contains("間隔"))
                                            tb.Text = set.間隔.ToString();
                                        if (tb.Name.Contains("技能段1"))
                                            tb.Text = set.技能段1 == -1 ? "" : set.技能段1.ToString();
                                        if (tb.Name.Contains("技能段2"))
                                            tb.Text = set.技能段2 == -1 ? "" : set.技能段2.ToString();
                                        if (tb.Name.Contains("技能段3"))
                                            tb.Text = set.技能段3 == -1 ? "" : set.技能段3.ToString();
                                        if (tb.Name.Contains("施放A"))
                                            tb.Text = set.施放A;
                                        if (tb.Name.Contains("施放B"))
                                            tb.Text = set.施放B;
                                        if (tb.Name.Contains("施放C"))
                                            tb.Text = set.施放C;
                                        if (tb.Name.Contains("程式速度"))
                                            tb.Text = set.程式速度.ToString();
                                    }
                                }
                            }
                        }
                    }
                }
                Debug.WriteLine("讀取完成");
                更新自動使用技能隊員名單();
            }
        }

        private void 更新自動使用技能隊員名單()
        {
            List<string> names = new();
            隊員智能功能組.Clear();
            addList(同步_1, SelectFID_1, 開場一_1, 重複_1, 二段_1, 背景ENTER_1, 程式速度_1, 延遲施放_1, 間隔時間放_1, 技能段1_1, 技能段2_1, 技能段3_1, 施放A_1, 施放B_1, 施放C_1);
            addList(同步_2, SelectFID_2, 開場一_2, 重複_2, 二段_2, 背景ENTER_2, 程式速度_2, 延遲施放_2, 間隔時間放_2, 技能段1_2, 技能段2_2, 技能段3_2, 施放A_2, 施放B_2, 施放C_2);
            addList(同步_3, SelectFID_3, 開場一_3, 重複_3, 二段_3, 背景ENTER_3, 程式速度_3, 延遲施放_3, 間隔時間放_3, 技能段1_3, 技能段2_3, 技能段3_3, 施放A_3, 施放B_3, 施放C_3);
            addList(同步_4, SelectFID_4, 開場一_4, 重複_4, 二段_4, 背景ENTER_4, 程式速度_4, 延遲施放_4, 間隔時間放_4, 技能段1_4, 技能段2_4, 技能段3_4, 施放A_4, 施放B_4, 施放C_4);
            addList(同步_5, SelectFID_5, 開場一_5, 重複_5, 二段_5, 背景ENTER_5, 程式速度_5, 延遲施放_5, 間隔時間放_5, 技能段1_5, 技能段2_5, 技能段3_5, 施放A_5, 施放B_5, 施放C_5);
            addList(同步_6, SelectFID_6, 開場一_6, 重複_6, 二段_6, 背景ENTER_6, 程式速度_6, 延遲施放_6, 間隔時間放_6, 技能段1_6, 技能段2_6, 技能段3_6, 施放A_6, 施放B_6, 施放C_6);
            addList(同步_7, SelectFID_7, 開場一_7, 重複_7, 二段_7, 背景ENTER_7, 程式速度_7, 延遲施放_7, 間隔時間放_7, 技能段1_7, 技能段2_7, 技能段3_7, 施放A_7, 施放B_7, 施放C_7);

            void addList(CheckBox 同步, ComboBox combox, CheckBox 開場, CheckBox 重複, CheckBox 需選, CheckBox 背景ENTER, TextBox 程式速度, TextBox 延遲施放
                , TextBox 間隔施放, TextBox 技能_1, TextBox 技能_2, TextBox 技能_3, TextBox 對象A, TextBox 對象B, TextBox 對象C)
            {
                if (combox.SelectedItem != null && !string.IsNullOrEmpty(combox.SelectedItem.ToString()))
                {
                    var str = combox.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var n = MainWindow.AllNobWindowsList.Find(r => r.PlayerName.Contains(str) || r.PlayerName == str);
                        if (n != null && !names.Contains(str))
                        {
                            自動技能組 nb = new();
                            nb.NOB = n;
                            nb.背景Enter = 背景ENTER.IsChecked ?? false;
                            nb.同步 = 同步.IsChecked ?? false;
                            nb.一次放 = 開場.IsChecked ?? false;
                            nb.重複放 = 重複.IsChecked ?? false;
                            nb.需選擇 = 需選.IsChecked ?? false;
                            nb.背景Enter = 背景ENTER.IsChecked ?? false;
                            int t = 0;
                            int.TryParse(延遲施放.Text, out t);
                            nb.延遲 = t;
                            int.TryParse(間隔施放.Text, out t);
                            nb.間隔 = t;
                            if (int.TryParse(程式速度.Text, out t))
                                nb.程式速度 = t;
                            else
                                nb.程式速度 = 100;
                            nb.特殊運作 = false;
                            if (string.IsNullOrEmpty(技能_1.Text) == false)
                            {
                                if (int.TryParse(技能_1.Text, out t))
                                {
                                    nb.特殊運作 = true;
                                    nb.技能段1 = t;

                                    if (int.TryParse(技能_2.Text, out t))
                                        nb.技能段2 = t;
                                    else
                                        nb.技能段2 = -1;

                                    if (int.TryParse(技能_3.Text, out t))
                                        nb.技能段3 = t;
                                    else
                                        nb.技能段3 = -1;

                                    nb.施放A = 對象A.Text;
                                    nb.施放B = 對象B.Text;
                                    nb.施放C = 對象C.Text;
                                }
                            }
                            else
                            {
                                nb.技能段1 = -1;
                                nb.技能段2 = -1;
                                nb.技能段3 = -1;
                            }
                            n.AutoSkillSet = nb;
                            隊員智能功能組.Add(nb);
                            names.Add(str);
                        }
                    }
                }
            }
        }

        public void LoadSetting()
        {
            if (MainNob == null)
            {
                Debug.WriteLine("useNOB == null");
                return;
            }

            if (File.Exists($@"{MainNob.PlayerName}_LoadSK.sk"))
            {
                using StreamReader reader = new($@"{MainNob.PlayerName}_LoadSK.sk");
                if (reader == null)
                {
                    Debug.WriteLine("reader == null");
                    return;
                }

                string setJson = reader.ReadToEnd();

                Setting set = JsonSerializer.Deserialize<Setting>(setJson);
                if (set != null)
                {
                    MainNob.CodeSetting = set;
                    SetToUI();
                }
            }

            #region 舊版的技能使用
            //DirectoryInfo Dir = new DirectoryInfo(@".\");
            //var fileEntries = Dir.GetFiles("*.sJson");
            //foreach (var f in fileEntries)
            //{
            //    bool b = false;
            //    foreach (var item in SkillComTitle.Items)
            //    {
            //        if (item is ComboBoxItem)
            //            if (item != null && (item as ComboBoxItem).Content == f.Name.Replace(".sJson", ""))
            //            {
            //                b = true;
            //                break;
            //            }
            //        if (item is string && item == f.Name.Replace(".sJson", ""))
            //        {
            //            b = true;
            //            break;
            //        }
            //    }
            //    if (b == false)
            //        SkillComTitle.Items.Add(f.Name.Replace(".sJson", ""));
            //}
            //if (!string.IsNullOrEmpty(skDefName))
            //{
            //    if (skDefName != "預設")
            //    {
            //        var i = SkillComTitle.Items.Add(skDefName.Trim());
            //        SkillComTitle.SelectedIndex = i;
            //    }
            //      MainNob.Log("----------skDefName------------");
            //    using StreamReader reader2 = new($@"{skDefName.Trim()}.sJson");
            //    // Read the stream as a string.
            //    string jsonString = reader2.ReadToEnd();
            //    var sk2 = JsonSerializer.Deserialize<List<SkillData>>(jsonString);
            //    SkillDataGird.ItemsSource = sk2;
            //      MainNob.Log("----------jsonString------------ : " + jsonString);
            //}

            //if (SkillDataGird.Items == null || SkillDataGird.Items.Count == 0)
            //{
            //      MainNob.Log("-----SkillDataGird Add--------");
            //    List<SkillData> sklist = new() { };
            //    SkillDataGird.ItemsSource = sklist;
            //}
            #endregion
        }

        public void SetToUI()
        {
            if (MainNob == null)
            {
                Debug.WriteLine("useNOB == null");
                return;
            }

            Setting set = MainNob.CodeSetting;
            SelectMenu.SelectedValue = set.上次使用的腳本;
            其他選項A.Text = set.其他選項A.ToString();
            其他選項B.Text = set.其他選項B.ToString();
            TBX搜尋範圍.Text = set.搜尋範圍.ToString();
            TB_選擇關卡.Text = set.選擇關卡.ToString();
            TB_選擇難度.Text = set.選擇難度.ToString();
            TB_SetCNum.Text = set.連續戰鬥.ToString();
            //TB_Set家臣.Text = set.家臣數量.ToString();
            CB_AllIn.IsChecked = set.AllInTeam;

            if (set.隊伍技能 != null)
            {
                m隊伍技能紀錄 = set.隊伍技能;
            }

            if (set.組隊玩家技能 != null)
            {
                for (int i = 0; i < set.組隊玩家技能.Count && i < comboBoxes.Length; i++)
                {
                    comboBoxes[i].Text = set.組隊玩家技能[i];
                }
            }

            if (set.目標A != 0)
            {
                Btn_TargetA.Content = set.目標A;
            }
            if (set.目標B != 0)
            {
                Btn_TargetB.Content = set.目標B;
            }
            if (set.目標C != 0)
            {
                Btn_TargetC.Content = set.目標C;
            }
            if (set.目標D != 0)
            {
                Btn_TargetD.Content = set.目標D;
            }
            if (set.目標E != 0)
            {
                Btn_TargetE.Content = set.目標E;
            }
            if (set.目標F != 0)
            {
                Btn_TargetF.Content = set.目標F;
            }
            SMENU2.Text = set.線路.ToString();
        }

        public void SaveSetting()
        {
            if (MainNob == null)
            {
                Debug.WriteLine("useNOB is null");
                return;
            }

            MainNob.CodeSetting.上次使用的腳本 = SelectMenu.Text;
            MainNob!.CodeSetting.組隊玩家技能 = new List<string>();
            foreach (var cb in comboBoxes)
            {
                saveSkillUserList(cb);
            }

            void saveSkillUserList(ComboBox cb)
            {
                try
                {
                    if (cb.SelectedItem != null && !string.IsNullOrEmpty(cb.SelectedItem.ToString()))
                    {
                        MainNob!.CodeSetting.組隊玩家技能.Add(cb.SelectedItem.ToString()!);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("saveSkillUserList Error -> " + e.ToString());
                }
            }

            MainNob.CodeSetting.隊伍技能 = m隊伍技能紀錄;
            string jsonString = JsonSerializer.Serialize(MainNob.CodeSetting);
            try
            {
                using (StreamWriter outputFile = new StreamWriter($@"{MainNob.PlayerName}_LoadSK.sk"))
                {
                    outputFile.WriteLine(jsonString);
                    Debug.WriteLine("寫入檔案完成");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($@"{MainNob.PlayerName}_LoadSK.sk write Error -> {e.ToString()}");
            }
        }

    }
}
