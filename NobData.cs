using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NOBApp.Sports;
using static NOBApp.MainWindow;

namespace NOBApp
{
    public class NOBDATA
    {
        /// <summary>
        /// 取得應用程式畫面
        /// </summary>
        /// <param name="hWnd">程序</param>
        /// <param name="bounds">範圍</param>
        /// <returns></returns>
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

        public Setting CodeSetting = new();
        public 自動技能組 AutoSkillSet = new();
        //之後須要收費的話 直接使用
        public DateTime TimeUP;
        public Process Proc;
        public RECT 原視窗;
        public int NowHeight;
        public int NowWidth;
        public int Hwnd => (int)Proc.MainWindowHandle;

        static Random random = new Random();
        #region 記憶體讀取位置
        public string Account => ReadString("<nobolHD.bng> +" + AddressData.Acc, 0, 15);
        public string Password => ReadString("<nobolHD.bng> +" + AddressData.Pas, 0, 15);
        public string PlayerName => ReadString("<nobolHD.bng> + " + AddressData.角色名稱, 1, 12);
        public int MAPID => ReadInt("<nobolHD.bng> +" + AddressData.地圖位置, 1);
        public int PosX => ReadInt("<nobolHD.bng> +" + AddressData.地圖座標X, 0);
        public int PosY => ReadInt("<nobolHD.bng> +" + AddressData.地圖座標Y, 0);
        public float CamX => ReadFloat("<nobolHD.bng> +" + AddressData.攝影機角度A);
        public float CamY => ReadFloat("<nobolHD.bng> +" + AddressData.攝影機角度B);
        public string 取得最下面選項(int num = 4) => ReadString("<nobolHD.bng> + " + AddressData.直選框文字, 1, num);
        public bool 對話與結束戰鬥 => StateA.Contains("F0 F8");
        public bool 待機 => StateA.Contains("F0 B8");
        public bool 戰鬥中 => StateA.Contains("A0 98");
        public bool 結算中 => ReadInt("<nobolHD.bng> + " + AddressData.戰鬥結算UI, 0) > 0;
        public string 觀察對象Str => ReadData("<nobolHD.bng> + " + AddressData.是否有觀察對象, 2);
        public bool 有觀察對象 => !ReadData("<nobolHD.bng> + " + AddressData.是否有觀察對象, 2).Contains("FF FF");
        public bool 出現左右選單 => ReadInt("<nobolHD.bng> + " + AddressData.直選框, 0) == 2;
        public bool 出現直式選單 => ReadInt("<nobolHD.bng> + " + AddressData.直選框, 0) == 1;
        public int GetSStatus => ReadInt("<nobolHD.bng> + " + AddressData.特殊狀態判斷, 2);
        public string StateA => ReadData("<nobolHD.bng> + " + AddressData.判別狀態A, 2);

        public bool 驗證國家
        {
            get // 明確加上 get 關鍵字，雖然自動屬性可省略，但加上更清晰
            {
                string[] addressesToCheck = new string[] // 使用陣列儲存要檢查的位址
                {
                        AddressData.頻道認證B,
                        AddressData.頻道認證B.AddressAdd(192),
                        AddressData.頻道認證B.AddressAdd(384),
                        AddressData.頻道認證,
                        AddressData.頻道認證.AddressAdd(192),
                        AddressData.頻道認證.AddressAdd(384)
                };

                foreach (string address in addressesToCheck) // 使用迴圈迭代檢查位址
                {
                    if (驗證國家字串包含("胖鵝科技", address)) // 只要在任一位址找到字串，立即返回 true
                    {
                        return true;
                    }
                }

                return false; // 迴圈結束都沒找到，返回 false
            }
        }

        private bool 驗證國家字串包含(string 搜尋字串, string address)
        {
            return MainWindow.dmSoft?.ReadString(Hwnd, "<nobolHD.bng> + " + address, 1, 16)?.Contains(搜尋字串) ?? false;
        }

        /// <summary>
        /// 通過驗證 有特殊者或者贊助者或者驗證國家
        /// </summary>
        public bool 通過驗證 => 驗證完成 ? 贊助者 ? 贊助者 : 特殊者 && 驗證國家 : false;
        public int GetTargetIDINT()
        {
            var i = MainWindow.dmSoft!.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 4);
            return (int)i;
        }

        public int GetTargetClass()
        {
            return (int)MainWindow.dmSoft!.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目.AddressAdd(3), 2);
        }

        private string ReadString(string address, int type, int length)
        {
            return MainWindow.dmSoft!.ReadString(Hwnd, address, type, length);
        }

        private int ReadInt(string address, int type)
        {
            return (int)MainWindow.dmSoft!.ReadInt(Hwnd, address, type);
        }

        private float ReadFloat(string address)
        {
            return MainWindow.dmSoft!.ReadFloat(Hwnd, address);
        }

        private string ReadData(string address, int type)
        {
            return MainWindow.dmSoft!.ReadData(Hwnd, address, type);
        }
        #endregion
        //
        public void 前進(int time)
        {
            KeyPressT(VKeys.KEY_W, time);
        }

        public void 後退(int time)
        {
            KeyPressT(VKeys.KEY_S, time);
        }

        public bool 特殊者 = false;
        public bool 贊助者 = false;
        public bool 驗證完成 = false;
        public float 比例 = 1;
        public bool 副本進入完成 = false;
        public bool 副本離開完成 = false;
        public string 目前動作 = "";
        public BaseClass? RunCode;
        public List<BTData> MYTeamData = new List<BTData>();
        public List<BTData> EMTeamData = new List<BTData>();
        public List<long> SetSkillsID = new List<long>();
        public List<string> SKNames = new List<string>();
        public bool 離開戰鬥確認 = false;
        public bool 完成必須對話 = false;
        public bool 啟動自動輔助中 = false;

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

        public void CodeRunUpdate()
        {
            bool _init = false;
            while (MainWindow.CodeRun && RunCode != null)
            {
                if (MainWindow.CodeRun == false)
                    break;
                if (!_init)
                {
                    _init = true;
                    RunCode.SetMainUser(this);
                    RunCode.初始化();
                }

                RunCode.腳本運作();
            }
        }

        public void BattleUpdate()
        {
            if (啟動自動輔助中)
                return;

            啟動自動輔助中 = true;
            Debug.WriteLine($"Nob {PlayerName} Update ");
            int endBattleCheckNum = 0;
            bool 希望完成 = false;
            bool 進入過戰鬥畫面 = false;
            bool 已經放過一次 = false;
            bool 放技能完成 = false;
            while (MainWindow.UseAutoSkill && dmSoft != null)
            {
                if (AutoSkillSet.背景Enter)
                {
                    KeyPressPP(VKeys.KEY_ENTER);
                    Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
                    continue;
                }

                if (戰鬥中)
                {
                    #region 戰鬥中
                    //目前選數量 
                    endBattleCheckNum = 0;
                    希望完成 = false;
                    進入過戰鬥畫面 = true;
                    var index = dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷, 2);
                    if (index > 0 && (MYTeamData == null || MYTeamData.Count == 0))
                    {
                        BtDataUpdate();
                    }

                    if (AutoSkillSet.一次放 || AutoSkillSet.重複放)
                    {
                        do
                        {
                            index = dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷, 2);
                            string supDataCheck = dmSoft.ReadData(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷II, 1);
                            if (supDataCheck.Substring(supDataCheck.Length - 1).Contains("4"))
                            {
                                string newD = supDataCheck[0] + "0";
                                dmSoft.WriteData(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥可輸入判斷II, newD);
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
                                    int setindex = (int)dmSoft.ReadInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥輸入, 2);
                                    Debug.WriteLine("setindex : " + setindex);

                                    Task.Delay(AutoSkillSet.程式速度).Wait();
                                    Debug.WriteLine("setindex : " + setindex);
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
                                            setNum = check(UseLockNOB!.MYTeamData, AutoSkillSet.施放A);
                                            直向選擇(setNum == -1 ? 0 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            直向選擇(0, AutoSkillSet.程式速度);
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
                                            直向選擇(1, AutoSkillSet.程式速度);
                                            直向選擇(0, AutoSkillSet.程式速度);
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
                                            直向選擇(2, AutoSkillSet.程式速度);
                                            直向選擇(1, AutoSkillSet.程式速度);
                                        }
                                    }

                                    if (setindex == 5)
                                    {
                                        if (string.IsNullOrEmpty(AutoSkillSet.施放C) == false)
                                        {
                                            setNum = check(UseLockNOB!.MYTeamData, AutoSkillSet.施放C);
                                            直向選擇(setNum == -1 ? 2 : setNum, AutoSkillSet.程式速度);
                                        }
                                        else
                                        {
                                            直向選擇(2, AutoSkillSet.程式速度);
                                            直向選擇(0, AutoSkillSet.程式速度);
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
                        while (MainWindow.UseAutoSkill && index > 0);
                    }

                    #endregion 戰鬥中
                }

                if (對話與結束戰鬥)
                {
                    #region 對話與結束戰鬥

                    #endregion
                }

                switch (StateA)
                {
                    // 戰鬥中
                    case "A0 98":
                        break;
                    //沒有任何視窗野外
                    case "F0 B8":
                        #region 野外
                        已經放過一次 = false;
                        進入過戰鬥畫面 = false;

                        if (UseLockNOB!.MYTeamData.Count > 0)
                        {
                            UseLockNOB.ClearBTData();
                        }

                        if (全隊追蹤)
                        {
                            KeyPress(VKeys.KEY_F8);
                            Task.Delay(500).Wait();
                        }
                        #endregion 野外
                        break;
                    //開寶 出現對話框 戰鬥結束
                    case "F0 F8":
                        #region 對話框出現 + 戰鬥結束
                        if (進入過戰鬥畫面)
                        {
                            if (endBattleCheckNum < 3)
                            {
                                endBattleCheckNum = endBattleCheckNum + 1;
                                Task.Delay(100).Wait();
                            }
                            else
                            {
                                endBattleCheckNum = 0;
                                放技能完成 = false;
                                已經放過一次 = false;
                                if (隊長希望取得 && 希望完成 == false)
                                {
                                    希望完成 = true;
                                    Task.Delay(1000).Wait();
                                    UseLockNOB!.KeyPress(VKeys.KEY_ENTER, 6, 300);
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
                            }
                        }
                        #endregion
                        break;
                }

                Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
            }

            啟動自動輔助中 = false;
        }

        public void 更改F8追隨() => MainWindow.dmSoft!.WriteString(Hwnd, "<nobolHD.bng> + " + AddressData.快捷F8, 1, "／追蹤：％Ｌ");

        public void MoveToNPC(int npcID)
        {
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.移動對象, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.開始移動到目標對象, 0, npcID);
        }
        public void 鎖定NPC(int npcID)
        {
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.選擇項目, 0, npcID);
            MainWindow.dmSoft?.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.移動對象, 0, npcID);
        }
        public void 直向選擇(int num, int delay = 300, bool passCheck = false)
        {
            MainWindow.dmSoft!.WriteInt(Hwnd, "[<nobolHD.bng>+4C4C154] + C4", 0, num);
            Task.Delay(delay).Wait();
            if (passCheck)
                KeyPressPP(VKeys.KEY_ENTER);
            else
                KeyPress(VKeys.KEY_ENTER);
        }

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
                //Debug.WriteLine($"baseStr {baseStr} {baseStr.AddressAdd(8)} {baseStr.AddressAdd(26)} {name1} {name2} ID : {data.UID} FName : {data.FullName} SID : {data.SortID} ");
                //Debug.WriteLine("\n");
            }
        }

        public void BT_Cmd()
        {
            MainWindow.dmSoft!.WriteInt(Hwnd, "<nobolHD.bng> + " + AddressData.戰鬥輸入, 1, 6);
            // Proc.Handle.WriteInt(UseAddress(AddressData.戰鬥輸入), 4);
        }

        public void MR_Clik(int x, int y)
        {
            Proc.MainWindowHandle.M_RClick(x, y);
        }

        public void KeyPress(VKeys keyCode, int loopNum = 1, int delay = 100)
        {
            for (int i = 0; i < loopNum; i++)
            {
                if (MainWindow.CodeRun == false && MainWindow.UseAutoSkill == false)
                    break;

                Proc.MainWindowHandle.KeyPress(keyCode);
                if (loopNum > 1)
                    Task.Delay(delay).Wait();
            }
        }

        public void KeyPressPP(VKeys keyCode, int loopNum = 1, int delay = 100)
        {
            for (int i = 0; i < loopNum; i++)
            {
                Proc.MainWindowHandle.KeyPress(keyCode);
                if (loopNum > 1)
                    Task.Delay(delay).Wait();
            }
        }

        public void KeyPressT(VKeys keyCode, int ss)
        {
            Proc.MainWindowHandle.KeyPress(keyCode, ss);
        }

        public void KeyDown(VKeys keyCode)
        {
            Proc.MainWindowHandle.KeyDown(keyCode);
        }
        public void KeyUp(VKeys keyCode)
        {
            Proc.MainWindowHandle.KeyUp(keyCode);
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
                    離開戰鬥確認 = true;
                    break;
                }
            }
            while ((MainWindow.CodeRun || MainWindow.UseAutoSkill) && true);
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
            Task.Delay(100).Wait();
            do
            {
                if (戰鬥中) { break; }
                //Debug.WriteLine($"結算中 : {checkDoneCount}");
                if (對話與結束戰鬥)
                {
                    checkDoneCount = 0;
                    var (x, y) = GetRandomPosition(inPosX, inPosY, 100, 50);
                    MR_Clik(x, y);
                    Task.Delay(50).Wait();
                }
                else
                {
                    checkDoneCount++;
                    if (checkDoneCount > 3)
                    {
                        離開戰鬥確認 = true;
                        break;
                    }
                }
            } while ((MainWindow.CodeRun || MainWindow.UseAutoSkill) && true);

            (int x, int y) GetRandomPosition(int centerX, int centerY, int rangeX, int rangeY)
            {
                int x = centerX + random.Next(-rangeX, rangeX);
                int y = centerY + random.Next(-rangeY, rangeY);
                return (x, y);
            }

        }

        public NOBDATA(Process proc)
        {
            Proc = proc;
            GetWindowRect(Proc.MainWindowHandle, out RECT rect);
            原視窗 = rect;
            NowWidth = rect.Right - rect.Left;
            NowHeight = rect.Bottom - rect.Top;
            //Debug.WriteLine($"W : {width} H : {height} - {rect}");
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
