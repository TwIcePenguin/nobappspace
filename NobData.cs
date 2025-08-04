using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NOBApp.Sports;
using static NOBApp.NobMainCodePage;

namespace NOBApp
{
    public class NOBDATA : NOBBehavior
    {
        public NOBDATA(Process proc) : base(proc)
        {
            bool _initWH = true;
            while (_initWH)
            {
                if (GetWindowRect(Proc.MainWindowHandle, out RECT rect))
                {
                    原視窗 = rect;
                    NowWidth = rect.Right - rect.Left;
                    NowHeight = rect.Bottom - rect.Top;
                    _initWH = false;
                }
                Task.Delay(100).Wait();
            }
            LogID = Account;
        }

        /// <summary>
        /// 取得應用程式畫面
        /// </summary>
        /// <param name="hWnd">程序</param>
        /// <param name="bounds">範圍</param>
        /// <returns></returns>
        #region DllImport
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        public Setting CodeSetting = new();
        public 自動技能組 AutoSkillSet = new();
        public DateTime 到期日 = DateTime.Now.AddYears(99);
        public RECT 原視窗;
        public int NowHeight;
        public int NowWidth;
        public bool VIPSP = false;
        public bool 追蹤 = false;
        private static readonly Random _random = new Random();

        #region 記憶體讀取位置
        private const string BASE_ADDRESS = "<nobolHD.bng> +";
        private readonly Dictionary<string, string> _addressCache = new();
        // 使用緩存減少重複字串連接的效能開銷
        private string GetFullAddress(string address) =>
            _addressCache.TryGetValue(address, out var fullAddress)
                ? fullAddress
                : _addressCache[address] = $"{BASE_ADDRESS}{address}";

        // 角色基本資訊
        public string Account => ReadString(GetFullAddress(AddressData.Acc), 0, 15);
        public string Password => ReadString(GetFullAddress(AddressData.Pas), 0, 15);
        public string PlayerName => ReadString(GetFullAddress(AddressData.角色名稱), 1, 12);

        // 位置和地圖資訊
        public int MAPID => ReadInt(GetFullAddress(AddressData.地圖位置), 1);
        public int PosX => ReadInt(GetFullAddress(AddressData.地圖座標X), 0);
        public int PosH => ReadInt(GetFullAddress(AddressData.地圖座標H), 0);
        public int PosY => ReadInt(GetFullAddress(AddressData.地圖座標Y), 0);
        public float CamX => ReadFloat(GetFullAddress(AddressData.攝影機角度A));
        public float CamY => ReadFloat(GetFullAddress(AddressData.攝影機角度B));

        // UI 狀態
        public string 取得最下面選項(int num = 4) => ReadString(GetFullAddress(AddressData.直選框文字), 1, num);
        public bool 任務選擇框 => IsInState(GameState.QuestSelect);
        public bool 對話與結束戰鬥 => IsInState(GameState.Dialog);
        public bool 待機 => IsInState(GameState.Idle);
        public bool 戰鬥中 => IsInState(GameState.InBattle);

        public int 戰鬥中判定 = -1;

        // 結算相關
        public bool 進入結算
        {
            get
            {
                if (戰鬥中判定 >= 0 && IsInState(GameState.Dialog))
                {
                    戰鬥中判定++;
                    Task.Delay(100).Wait();
                }
                return 戰鬥中判定 > 3;
            }
        }
        // 視角相關
        // 修改後
        public bool 第三人稱
        {
            get => ReadInt(GetFullAddress(AddressData.視角), 0) == 0;
            set
            {
                // 設為第三人稱視角 (0)
                // 設為第一人稱視角 (1)
                MainWindow.dmSoft?.WriteInt(Hwnd, GetFullAddress(AddressData.視角), 0, value ? 0 : 1);
            }
        }
        public bool 輸入數量視窗 => ReadInt(GetFullAddress(AddressData.輸入數量視窗), 0) == 39 || ReadInt(GetFullAddress(AddressData.輸入數量視窗), 0) == 34;
        // 觀察與交互系統
        public string 觀察對象Str => ReadData(GetFullAddress(AddressData.是否有觀察對象), 2);
        public bool 有觀察對象 => !ReadData(GetFullAddress(AddressData.是否有觀察對象), 2).Contains("FF FF");
        public int 確認選單 => ReadInt(GetFullAddress(AddressData.直選框), 1);
        public int 製作Index => ReadInt(GetFullAddress(AddressData.製作Index), 1);
        public bool 出現左右選單 => ReadInt(GetFullAddress(AddressData.直選框), 0) == 2;
        public bool 出現直式選單 => ReadInt(GetFullAddress(AddressData.直選框), 0) == 1;
        public string StateA => StateARaw;
        public bool ResetPoint = false;


        // 1. 首先新增遊戲狀態的枚舉類型
        public enum GameState
        {
            Unknown = 0,
            InBattle = 1,      // A0 98 - 戰鬥中
            Idle = 2,          // F0 B8 - 野外待機
            /// <summary>
            /// 對話框或戰鬥結束
            /// </summary>
            Dialog = 3,        // F0 F8 - 對話或戰鬥結束
            QuestSelect = 4    // E0 F0 - 任務選擇框
        }
        // 2. 修改 StateA 相關實現
        private string _rawStateA = string.Empty;
        private GameState _currentState = GameState.Unknown;
        private DateTime _lastStateCheck = DateTime.MinValue;
        private const int STATE_CACHE_MS = 50; // 狀態緩存 50 毫秒
        /// <summary>
        /// 獲取原始狀態字串
        /// </summary>
        public string StateARaw
        {
            get
            {
                //Log($"Update Raw - {_rawStateA}");
                // 檢查是否需要更新狀態緩存
                if ((DateTime.Now - _lastStateCheck).TotalMilliseconds > STATE_CACHE_MS)
                {
                    _rawStateA = ReadData(GetFullAddress(AddressData.判別狀態A), 2);
                    UpdateGameState();
                    _lastStateCheck = DateTime.Now;
                }
                return _rawStateA;
            }
        }
        /// <summary>
        /// 獲取當前遊戲狀態
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// 檢查當前是否處於指定狀態
        /// </summary>
        public bool IsInState(GameState state)
        {
            if ((DateTime.Now - _lastStateCheck).TotalMilliseconds > STATE_CACHE_MS)
            {
                速度();
                _rawStateA = ReadData(GetFullAddress(AddressData.判別狀態A), 2);
                UpdateGameState();
                _lastStateCheck = DateTime.Now;
            }
            //Debug.WriteLine($"Update -> {StateARaw}");
            return _currentState == state;
        }

        /// <summary>
        /// 更新遊戲狀態
        /// </summary>
        private void UpdateGameState()
        {
            if (_rawStateA.Contains("A0 98"))
            {
                _currentState = GameState.InBattle;
                戰鬥中判定 = 0;
            }
            else if (_rawStateA.Contains("F0 B8"))
            {
                _currentState = GameState.Idle;
                戰鬥中判定 = -1;
            }
            else if (_rawStateA.Contains("F0 F8"))
                _currentState = GameState.Dialog;
            else if (_rawStateA.Contains("E0 F0"))
                _currentState = GameState.QuestSelect;
            else
                _currentState = GameState.Unknown;
        }

        // 驗證功能
        public bool 驗證國家
        {
            get
            {
                // 需要檢查的六個位址
                string[][] addressPairs = new string[][]
                {
            new[] { AddressData.頻道認證B, "0", "192", "384" },
            new[] { AddressData.頻道認證A, "0", "192", "384" }
                };

                foreach (var basePair in addressPairs)
                {
                    string baseAddr = basePair[0];
                    for (int i = 1; i < basePair.Length; i++)
                    {
                        string offset = basePair[i];
                        string addr = offset == "0" ? baseAddr : baseAddr.AddressAdd(int.Parse(offset));

                        if (驗證國家字串包含("胖鵝科技", addr))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        private bool 驗證國家字串包含(string 搜尋字串, string address)
        {
            Debug.WriteLine(MainWindow.dmSoft?.ReadString(Hwnd, $"{BASE_ADDRESS}{address}", 1, 16));
            return MainWindow.dmSoft?.ReadString(Hwnd, $"{BASE_ADDRESS}{address}", 1, 16)?.Contains(搜尋字串) ?? false;
        }

        // 目標處理
        public int GetTargetIDINT() => (int)(MainWindow.dmSoft?.ReadInt(Hwnd, GetFullAddress(AddressData.選擇項目), 4) ?? 0);
        public int GetTargetClass() => (int)(MainWindow.dmSoft?.ReadInt(Hwnd, GetFullAddress(AddressData.選擇項目.AddressAdd(3)), 2) ?? 0);

        // 基礎讀取方法
        private string ReadString(string address, int type, int length)
        {
            return MainWindow.dmSoft?.ReadString(Hwnd, address, type, length) ?? string.Empty;
        }

        private int ReadInt(string address, int type)
        {
            return (int)(MainWindow.dmSoft?.ReadInt(Hwnd, address, type) ?? 0);
        }

        private float ReadFloat(string address)
        {
            return MainWindow.dmSoft?.ReadFloat(Hwnd, address) ?? 0.0f;
        }

        private string ReadData(string address, int type)
        {
            return MainWindow.dmSoft?.ReadData(Hwnd, address, type) ?? string.Empty;
        }
        #endregion

        public bool 特殊者 = false;
        public bool 贊助者 = false;
        public bool 驗證完成 = false;
        public float 比例 = 1;
        public bool 副本進入完成 = false;
        public bool 副本離開完成 = false;
        public string NOBCDKEY = "";
        public BaseClass? RunCode;
        public List<BTData> MYTeamData = new List<BTData>();
        public List<BTData> EMTeamData = new List<BTData>();
        public List<long> SetSkillsID = new List<long>();
        public List<string> SKNames = new List<string>();
        public bool 離開戰鬥確認 = false;
        public bool 完成必須對話 = false;
        public bool 啟動自動輔助中 = false;
        public bool 準備完成 = false;
        public bool 自動追蹤隊長 = false;
        public bool 自動結束_A = false;
        public bool 自動結束_B = false;
        public bool 希望取得 = false;
        /// <summary>
        /// 判斷是否開始隨機地圖上打怪
        /// </summary>
        public bool 開打 = false;
        public bool F5解無敵 = false;
        //使用 Enter 點怪(舊式)
        public bool isUseEnter = false;
        int errorCheckCount = 0;
        string cacheStatus = "";

        public void CloseGame()
        {
            Proc.Kill();
        }
        public void ClearBTData()
        {
            SetSkillsID.Clear();
            MYTeamData.Clear();
            EMTeamData.Clear();
            SKNames.Clear();
        }
        public void CodeUpdate()
        {
            if (RunCode == null)
                return;

            bool _init = false;
            while (StartRunCode && RunCode != null)
            {
                if (StartRunCode == false)
                    break;
                if (!_init)
                {
                    _init = true;
                    RunCode.SetMainUser(this);
                    RunCode.初始化();
                }

                RunCode.腳本運作();

                if (cacheStatus != StateARaw)
                {
                    cacheStatus = StateARaw;
                    errorCheckCount = 0;
                }
                else
                {
                    Task.Delay(50).Wait();
                    errorCheckCount++;
                    if (errorCheckCount > 10000)
                    {
                        errorCheckCount = 0;
                        StartRunCode = false;
                        string msg = $"{RunCode?.GetType().Name ?? "無腳本"} 狀態長時間沒有變化 需要請企鵝確認";
                        DiscordNotifier.SendNotificationAsync(PlayerName, msg);
                        MessageBox.Show($"{PlayerName} -> {msg}");//TelegramNotifier.SendNotificationAsync(PlayerName, "狀態長時間沒有變化 需要確認");
                    }
                }
            }
            //RunCode?.SetClickThrough(false);
        }

        public async Task BattleUpdate()
        {
            Log($"1 啟動自動輔助中");
            if (啟動自動輔助中)
                return;
            Log($"2 啟動自動輔助中");
            啟動自動輔助中 = true;

            bool 希望完成 = false;
            bool 已經放過一次 = false;
            bool 放技能完成 = false;
            while (IsUseAutoSkill)
            {
                if (MainWindow.dmSoft == null)
                    return;

                //Log($"IsUseAutoSkill->{IsUseAutoSkill}");

                if (AutoSkillSet.背景Enter)
                {
                    KeyPressPP(VKeys.KEY_ENTER);
                    Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
                    continue;
                }

                if (戰鬥中)
                {
                    Log($"進入戰鬥中 {AutoSkillSet.一次放 || AutoSkillSet.重複放}");
                    #region 戰鬥中
                    //目前選數量 
                    希望完成 = false;
                    var index = MainWindow.dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷, 2);
                    if (index > 0 && (MYTeamData == null || MYTeamData.Count == 0))
                    {
                        BtDataUpdate();
                    }

                    if (AutoSkillSet.一次放 || AutoSkillSet.重複放)
                    {
                        do
                        {
                            index = MainWindow.dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷, 2);
                            string supDataCheck = MainWindow.dmSoft.ReadData(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷II, 1);
                            if (supDataCheck.Substring(supDataCheck.Length - 1).Contains("4"))
                            {
                                string newD = supDataCheck[0] + "0";
                                MainWindow.dmSoft.WriteData(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷II, newD);
                            }

                            if (index > 0)
                            {
                                if (已經放過一次)
                                    break;

                                if (AutoSkillSet.延遲 > 0)
                                {
                                    Task.Delay(AutoSkillSet.延遲).Wait();
                                }
                                if (AutoSkillSet.特殊運作)
                                {
                                    int setindex = (int)MainWindow.dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥輸入, 2);
                                    Log("setindex : " + setindex);

                                    Task.Delay(AutoSkillSet.程式速度).Wait();
                                    if (setindex == 0)
                                    {
                                        直向選擇(AutoSkillSet.技能段1, AutoSkillSet.程式速度);
                                    }
                                    if (setindex == 1 && AutoSkillSet.技能段2 >= 0)
                                    {
                                        直向選擇(AutoSkillSet.技能段2, AutoSkillSet.程式速度);
                                    }

                                    int setNum = -1;
                                    if (setindex == 2)
                                    {
                                        if (AutoSkillSet.技能段3 >= 0)
                                            直向選擇(AutoSkillSet.技能段3, AutoSkillSet.程式速度);

                                        if (string.IsNullOrEmpty(AutoSkillSet.施放A) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放A);
                                            直向選擇(setNum == -1 ? 0 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            KeyPress(VKeys.KEY_ENTER);
                                            KeyPress(VKeys.KEY_K);
                                        }
                                    }

                                    if (setindex == 3)
                                    {
                                        if (string.IsNullOrEmpty(AutoSkillSet.施放A) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放A);
                                            直向選擇(setNum == -1 ? 0 : setNum, AutoSkillSet.程式速度);
                                        }

                                        if (string.IsNullOrEmpty(AutoSkillSet.施放B) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放B);
                                            直向選擇(setNum == -1 ? 0 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            KeyPress(VKeys.KEY_ENTER);
                                            KeyPress(VKeys.KEY_K);
                                        }
                                    }

                                    if (setindex == 4)
                                    {
                                        if (string.IsNullOrEmpty(AutoSkillSet.施放B) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放B);
                                            直向選擇(setNum == -1 ? 1 : setNum, AutoSkillSet.程式速度);
                                        }

                                        if (string.IsNullOrEmpty(AutoSkillSet.施放C) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放C);
                                            直向選擇(setNum == -1 ? 1 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            KeyPress(VKeys.KEY_ENTER);
                                            KeyPress(VKeys.KEY_K);
                                        }
                                    }

                                    if (setindex == 5)
                                    {
                                        if (string.IsNullOrEmpty(AutoSkillSet.施放C) == false)
                                        {
                                            setNum = check(MYTeamData, AutoSkillSet.施放C);
                                            直向選擇(setNum == -1 ? 2 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            KeyPress(VKeys.KEY_ENTER);
                                            KeyPress(VKeys.KEY_K);
                                        }
                                    }

                                }
                                else
                                {
                                    BT_Cmd();
                                    Task.Delay(100).Wait();
                                    if (AutoSkillSet.需選擇)
                                    {
                                        Task.Delay(100).Wait();
                                        KeyPress(VKeys.KEY_ENTER);
                                    }
                                }

                                放技能完成 = true;
                            }

                            if (index <= 0)
                            {
                                if (放技能完成 && AutoSkillSet.一次放)
                                    已經放過一次 = true;
                                break;
                            }
                            Task.Delay(AutoSkillSet.程式速度).Wait();
                        }
                        while (IsUseAutoSkill && index > 0);
                    }

                    #endregion 戰鬥中
                }
                #region 對話框出現 + 戰鬥結束
                if (進入結算)
                {
                    Debug.WriteLine($"進入結算");
                    放技能完成 = false;
                    已經放過一次 = false;
                    if (希望取得 && 希望完成 == false)
                    {
                        希望完成 = true;
                        Task.Delay(1000).Wait();
                        KeyPress(VKeys.KEY_ENTER, 6, 300);
                        Task.Delay(100).Wait();
                    }
                    if (自動結束_A)
                    {
                        離開戰鬥A();
                    }
                    if (自動結束_B)
                    {
                        離開戰鬥B();
                    }

                    if (追蹤)
                    {
                        KeyPressPP(VKeys.KEY_F8);
                    }
                }
                #endregion
                if (對話與結束戰鬥)
                {
                    #region 對話與結束戰鬥
                    await Task.Delay(AutoSkillSet.程式速度);
                    #endregion
                }


                Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
            }
            //Log($"啟動自動輔助中 關閉->{IsUseAutoSkill}");
            啟動自動輔助中 = false;
        }

        // 處理戰鬥結束動作
        private void ProcessBattleEndActions()
        {
            if (自動結束_A)
            {
                離開戰鬥A();
            }

            if (自動結束_B)
            {
                離開戰鬥B();
            }
        }

        // 處理技能施放
        private async Task ProcessSkillExecution(long index, bool 已放過, bool 放完成)
        {
            do
            {
                // 讀取戰鬥狀態
                index = ReadInt(GetFullAddress(AddressData.戰鬥可輸入判斷), 2);

                // 檢查是否需要更新輔助數據
                ProcessSupplementaryData();

                // 判斷是否可以輸入
                if (index > 0 && !已放過)
                {
                    // 處理技能延遲
                    if (AutoSkillSet.延遲 > 0)
                    {
                        await Task.Delay(AutoSkillSet.延遲);
                    }

                    // 根據不同模式執行技能
                    if (AutoSkillSet.特殊運作)
                    {
                        await ExecuteSpecialSkills();
                    }
                    else
                    {
                        await ExecuteStandardSkills();
                    }

                    放完成 = true;
                }

                // 如果無法輸入，或已完成後需退出
                if (index <= 0 || (放完成 && AutoSkillSet.一次放))
                {
                    if (放完成 && AutoSkillSet.一次放)
                        已放過 = true;
                    break;
                }

                await Task.Delay(AutoSkillSet.程式速度);
            }
            while (IsUseAutoSkill && index > 0);
        }
        // 處理輔助數據
        private void ProcessSupplementaryData()
        {
            string supDataCheck = ReadData(GetFullAddress(AddressData.戰鬥可輸入判斷II), 1);
            if (supDataCheck.Length > 0 && supDataCheck[supDataCheck.Length - 1].ToString().Contains("4"))
            {
                string newD = supDataCheck.Length > 0 ? supDataCheck[0] + "0" : "00";
                MainWindow.dmSoft.WriteData(Hwnd, GetFullAddress(AddressData.戰鬥可輸入判斷II), newD);
            }
        }

        // 執行特殊技能
        private async Task ExecuteSpecialSkills()
        {
            int setindex = (int)ReadInt(GetFullAddress(AddressData.戰鬥輸入), 2);

            // 延遲以確保操作同步
            await Task.Delay(AutoSkillSet.程式速度);

            // 根據不同階段選擇技能
            switch (setindex)
            {
                case 0:
                    直向選擇(AutoSkillSet.技能段1, AutoSkillSet.程式速度);
                    break;
                case 1:
                    if (AutoSkillSet.技能段2 >= 0)
                        直向選擇(AutoSkillSet.技能段2, AutoSkillSet.程式速度);
                    break;
                case 2:
                    await HandleSkillStage(2);
                    break;
                case 3:
                    await HandleSkillStage(3);
                    break;
                case 4:
                    await HandleSkillStage(4);
                    break;
                case 5:
                    await HandleSkillStage(5);
                    break;
            }
        }

        // 處理各階段技能
        private async Task HandleSkillStage(int stage)
        {
            switch (stage)
            {
                case 2:
                    if (AutoSkillSet.技能段3 >= 0)
                        直向選擇(AutoSkillSet.技能段3, AutoSkillSet.程式速度);

                    await SelectTeamMember(AutoSkillSet.施放A, 0);
                    break;

                case 3:
                    await SelectTeamMember(AutoSkillSet.施放A, 0);

                    if (!string.IsNullOrEmpty(AutoSkillSet.施放B))
                    {
                        await SelectTeamMember(AutoSkillSet.施放B, 0);
                    }
                    else
                    {
                        直向選擇(1, AutoSkillSet.程式速度);
                        直向選擇(0, AutoSkillSet.程式速度);
                    }
                    break;

                case 4:
                    await SelectTeamMember(AutoSkillSet.施放B, 1);

                    if (!string.IsNullOrEmpty(AutoSkillSet.施放C))
                    {
                        await SelectTeamMember(AutoSkillSet.施放C, 1);
                    }
                    else
                    {
                        直向選擇(2, AutoSkillSet.程式速度);
                        直向選擇(1, AutoSkillSet.程式速度);
                    }
                    break;

                case 5:
                    await SelectTeamMember(AutoSkillSet.施放C, 2);
                    break;
            }
        }

        // 選擇團隊成員
        private async Task SelectTeamMember(string memberName, int defaultIndex)
        {
            if (!string.IsNullOrEmpty(memberName))
            {
                int setNum = check(MYTeamData, memberName);
                直向選擇(setNum == -1 ? defaultIndex : setNum, AutoSkillSet.程式速度);
            }
            else
            {
                直向選擇(defaultIndex, AutoSkillSet.程式速度);
            }
        }

        // 執行標準技能
        private async Task ExecuteStandardSkills()
        {
            BT_Cmd();
            await Task.Delay(100);

            if (AutoSkillSet.需選擇)
            {
                await Task.Delay(100);
                KeyPress(VKeys.KEY_ENTER);
            }
        }


        public void 更改F8追隨() => MainWindow.dmSoft!.WriteString(Hwnd, "<nobolHD.bng> + " + AddressData.快捷F8, 1, "／追蹤：％Ｌ");
        public void 更改字型(int i)
        {
            MainWindow.dmSoft?.WriteInt(Hwnd, AddressData.UI字型, 0, i);
        }
        public void MoveToNPC(int npcID)
        {
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目B, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.移動對象, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.開始移動到目標對象, 0, npcID);
        }

        public void MoveToNPC2(int npcID)
        {
            while (StartRunCode)
            {
                if (GetTargetIDINT() == npcID && 對話與結束戰鬥)
                {
                    break;
                }
                else
                {
                    MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 0, npcID);
                    MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.移動對象, 0, npcID);
                    MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目B, 0, npcID);
                    MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.開始移動到目標對象, 0, npcID);
                    Task.Delay(500).Wait();
                }
            }
        }

        public void 鎖定NPC(int npcID)
        {
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目B, 0, npcID);
            //MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.移動對象, 0, npcID);
        }

        public void 生產到底()
        {
            int checkDone = 0;
            int checkIndex = 製作Index;
            KeyPress(VKeys.KEY_ENTER, 5);
            while (StartRunCode)
            {
                KeyPress(VKeys.KEY_ENTER, 10, 10);
                //Debug.WriteLine($"出現左右選單 -- {出現左右選單}");
                if (checkIndex != 製作Index)
                {
                    checkIndex = 製作Index;
                    checkDone = 0;
                    continue;
                }

                checkDone++;
                Task.Delay(50).Wait();
                if (checkDone > 7)
                {
                    KeyPress(VKeys.KEY_ESCAPE, 10);
                    break;
                }
            }
        }

        public void 速度()
        {
            if (VIPSP)
            {
                MainWindow.dmSoft!.WriteInt(Hwnd, "[<nobolHD.bng>+AFC234] + 26a", 0, 3081718408);
                MainWindow.dmSoft!.WriteInt(Hwnd, "[<nobolHD.bng>+AFC234] + 260", 0, 3081718408);
                //await Task.Delay(500);
            }
        }

        //檢查是否有歸0

        const string SelectData = @"[<nobolHD.bng>+4C4D144] + C4";
        public void 直向選擇ZC(int num, int delay = 300, bool passCheck = false)
        {
            int indexCheck = -1;

            while (StartRunCode)
            {
                indexCheck = (int)MainWindow.dmSoft!.ReadInt(Hwnd, SelectData, 0);
                if (indexCheck == 0)
                {
                    MainWindow.dmSoft!.WriteInt(Hwnd, SelectData, 0, num);
                    Task.Delay(delay).Wait();
                    if (passCheck)
                        KeyPressPP(VKeys.KEY_ENTER);
                    else
                        KeyPress(VKeys.KEY_ENTER);

                    break;
                }
                else
                    KeyPress(VKeys.KEY_ESCAPE);

                Task.Delay(500).Wait();
            }
        }

        public void 直向選擇(int num, int delay = 300, bool passCheck = false)
        {
            MainWindow.dmSoft!.WriteInt(Hwnd, SelectData, 0, num);
            Task.Delay(delay).Wait();
            if (passCheck)
                KeyPressPP(VKeys.KEY_ENTER);
            else
                KeyPress(VKeys.KEY_ENTER);
        }

        public int 精準移動Index => (int)MainWindow.dmSoft!.ReadInt(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +54", 0);

        public void 準確目標移動(float x, float y, float z)
        {
            while (StartRunCode)
            {
                if (精準移動Index == 0 || 精準移動Index == 1)
                {
                    var x1 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +58");
                    var y1 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +5C");
                    var z1 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +60");
                    var y2 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +6C");
                    var z2 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +70");
                    var x2 = MainWindow.dmSoft!.ReadFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +68");

                    var ii = MainWindow.dmSoft!.ReadInt(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +54", 0);
                    float f = 0;
                    if (x1 < 0 || x2 < 0 || y1 < 0 || y2 < 0 || z1 < 0 || z2 < 0 ||
                        !float.TryParse(x1.ToString(), out f) ||
                        !float.TryParse(y1.ToString(), out f) ||
                        !float.TryParse(z1.ToString(), out f))
                    {
                        ML_Click((int)NowWidth / 2, NowHeight / 2);
                        Debug.WriteLine($"出現問題 重新修正位置");
                        continue;
                    }

                    Debug.WriteLine($"-- Read {x1} {y1} {z1} {x2} {y2} {z2} {ii}");

                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +58", x);
                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +5C", y);
                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +60", z);
                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +68", x);
                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +6C", y);
                    MainWindow.dmSoft!.WriteFloat(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +70", z);
                    Debug.WriteLine("寫入完成");
                    MainWindow.dmSoft!.WriteInt(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +54", 0, 1);
                    Debug.WriteLine("開始移動");
                    break;
                }
                else
                {
                    ML_Click((int)NowWidth / 2, NowHeight / 2);
                }
            }
        }

        public int 點移動 => (int)MainWindow.dmSoft!.ReadInt(Hwnd, "[[<nobolHD.bng> + 04C4D144] + 164] +54", 0);

        public void 選擇目標類型(int num)
        {
            MainWindow.dmSoft!.WriteInt(Hwnd, "<nobolHD.bng> + B5B604", 0, num);
        }

        public void BtDataUpdate()
        {
            ClearBTData();

            string baseStr = "";
            string n = "";
            long l = 0;
            for (int i = 0; i < 7; i++)
            {
                baseStr = AddressData.戰鬥可輸隊員.AddressAdd(4 * i);
                l = MainWindow.dmSoft?.ReadInt(Hwnd, "<nobolHD.bng> + " + baseStr, 4) ?? 0;
                if (l > 0)
                {
                    MYTeamData.Add(new BTData(l));
                }
            }
            for (int i = 0; i < 24; i++)
            {
                baseStr = AddressData.戰鬥技能編號起.AddressAdd(80 * i);
                l = MainWindow.dmSoft?.ReadInt(Hwnd, "<nobolHD.bng> + " + baseStr, 4) ?? 0;
                n = MainWindow.dmSoft?.ReadString(Hwnd, "<nobolHD.bng> + " + baseStr.AddressAdd(34), 1, 10);
                n = i + "_" + n;
                if (l > 0)
                {
                    SetSkillsID.Add(l);
                    SKNames.Add(n);
                }
            }

            for (int i = 1; i < 15; i++)
            {
                baseStr = AddressData.戰鬥列隊.AddressAdd(44 * (i - 1));
                //string nid = MainWindow.dmSoft.ReadData(Hwnd, "<nobolHD.bng> + " + baseStr.AddressAdd(8), 6); //Proc.Handle.ReadData(UseAddress(baseStr), new byte[4]);
                long ln = MainWindow.dmSoft?.ReadInt(Hwnd, "<nobolHD.bng> + " + baseStr, 4) ?? 0;

                string name1 = MainWindow.dmSoft?.ReadString(Hwnd, "<nobolHD.bng> + " + baseStr.AddressAdd(8), 1, 6) ?? string.Empty; // Proc.Handle.ReadStr(UseAddress(baseStr.AddressAdd(8)), new byte[8]);
                string name2 = MainWindow.dmSoft?.ReadString(Hwnd, "<nobolHD.bng> + " + baseStr.AddressAdd(26), 1, 6) ?? string.Empty; //Proc.Handle.ReadStr(UseAddress(baseStr.AddressAdd(26)), new byte[8]);
                var data = MYTeamData.Find(d => d.UID == ln);
                if (data != null)
                {
                    data.FirstName = name1;
                    data.LastName = name2;
                    MYTeamData.Add(data);
                    Debug.WriteLine("Data -> " + data.FullName);
                }
                else
                {
                    var EData = new BTData(ln);
                    EData.FirstName = name1;
                    EData.LastName = name2;
                    EMTeamData.Add(EData);
                }
                //  MainNob.Log($"baseStr {baseStr} {baseStr.AddressAdd(8)} {baseStr.AddressAdd(26)} {name1} {name2} ID : {data.UID} FName : {data.FullName} SID : {data.SortID} ");
                //  MainNob.Log("\n");
            }
        }

        public void BT_Cmd()
        {
            MainWindow.dmSoft!.WriteInt(Hwnd, GetFullAddress(AddressData.戰鬥輸入), 1, 6);
        }

        public void 離開戰鬥A()
        {
            離開戰鬥確認 = false;
            do
            {
                if (戰鬥中) { break; }

                if (對話與結束戰鬥)
                {
                    Task.Delay(100).Wait();
                    KeyPress(VKeys.KEY_ESCAPE);
                    Task.Delay(100).Wait();
                    KeyPress(VKeys.KEY_ESCAPE);
                    Task.Delay(100).Wait();
                    KeyPress(VKeys.KEY_ENTER);
                    Task.Delay(100).Wait();
                }
                if (待機)
                {
                    戰鬥中判定 = -1;
                    離開戰鬥確認 = true;
                    break;
                }
            }
            while (StartRunCode || IsUseAutoSkill);
            離開戰鬥確認 = true;
        }

        public void 離開戰鬥B()
        {
            var width = 原視窗.Right - 原視窗.Left;
            var height = 原視窗.Bottom - 原視窗.Top;

            int inPosX = width / 2;
            int inPosY = (height / 2) - 100;
            離開戰鬥確認 = false;
            int checkDoneCount = 0;
            Task.Delay(50).Wait();
            do
            {
                if (戰鬥中) { break; }
                //  MainNob.Log($"結算中 : {checkDoneCount}");

                if (待機)
                {
                    戰鬥中判定 = -1;
                    離開戰鬥確認 = true;
                    KeyPress(VKeys.KEY_ESCAPE, 3);
                    break;
                }

                if (對話與結束戰鬥)
                {
                    checkDoneCount = 0;
                    int x = inPosX + _random.Next(-100, 100);
                    int y = inPosY + _random.Next(-20, 80);
                    MR_Click(x, y);
                    Task.Delay(100).Wait();
                }
                else
                {
                    checkDoneCount++;
                    if (checkDoneCount > 5)
                    {
                        戰鬥中判定 = -1;
                        離開戰鬥確認 = true;
                        break;
                    }
                    Task.Delay(400).Wait();
                }
            } while (StartRunCode || IsUseAutoSkill);
        }

        public void 縮小(string str = "")
        {
            if (!string.IsNullOrEmpty(str))
            {
                var array = str.Split(',');
                if (array.Length > 1)
                {
                    if (int.TryParse(array[0], out int width) && int.TryParse(array[1], out int height))
                    {
                        UpdateWindowSize(width, height, -0.1f);
                    }
                }
            }
            else
            {
                var width = 原視窗.Right - 原視窗.Left;
                var height = 原視窗.Bottom - 原視窗.Top;
                UpdateWindowSize(width, height, -0.1f);
            }
        }
        private void UpdateWindowSize(int width, int height, float scaleChange)
        {
            比例 = Math.Max(比例 + scaleChange, 0.3f);
            float f = 比例;
            NowWidth = (int)((width + 16) * f);
            NowHeight = (int)((height + 39) * f);
            MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, NowWidth, NowHeight, true);
        }

        public void MoveWindowTool(int tlIndex)
        {
            int TopPos = tlIndex * 40;
            int LeftPos = tlIndex * 120;

            MoveWindow(Proc.MainWindowHandle, LeftPos, TopPos, MainWindow.OrinX, MainWindow.OrinY, true);
        }

        public void FoucsNobWindows()
        {
            if (Proc.MainWindowHandle != IntPtr.Zero)
            {
                GetWindowRect(Proc.MainWindowHandle, out RECT rect);
                var nowPos = rect;
                SetForegroundWindow(Proc.MainWindowHandle);
            }
        }

        public void FoucsNobApp(Process proc)
        {
            if (proc.MainWindowHandle != IntPtr.Zero)
            {
                GetWindowRect(Proc.MainWindowHandle, out RECT rect);
                var nowPos = rect;
                MoveWindow(proc.MainWindowHandle, nowPos.Left - 100, nowPos.Top, (int)MainWindow.Instance!.Width, (int)MainWindow.Instance.Height, true);
                SetForegroundWindow(proc.MainWindowHandle);
            }
        }

        public void 還原(string str = "")
        {
            比例 = 1;
            if (!string.IsNullOrEmpty(str))
            {
                var array = str.Split(',');
                if (array.Length > 1)
                {
                    if (int.TryParse(array[0], out int width) && int.TryParse(array[1], out int height))
                    {
                        MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, width + 16, height + 39, true);
                    }
                }
            }
            else
            {
                MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, 原視窗.Right - 原視窗.Left, 原視窗.Bottom - 原視窗.Top, true);
            }
        }
    }
}
