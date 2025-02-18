using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 鬼島 : BaseClass
    {
        public Action UpdateUI = null;

        bool F5解無敵 = false;
        bool 補符 = true;
        int mBCHCount = 0;
        int 戰鬥回合 = 5;
        int 休息回合 = 80;
        int spCheck = 0;
        座標 掛網點 = new(0, 0);
        bool initPos = false;
        int tryFindNum = 0;

        List<long> skipID = new();
        public override void 初始化()
        {
            initPos = false;
            tryFindNum = 0;
            skipID = new List<long>();
        }

        void 移動倒掛網點()
        {
            全部追隨();
            移動點 = new();
            移動點.Add(掛網點);
            移動到定點();
        }

        void 回村長定位點()
        {
            全部追隨();
            移動點 = new();
            Debug.WriteLine($"回村長定位 掛網點 :  {掛網點.X} , {掛網點.Y}");
            //移動點.Add(new(21987, 24883));
            移動點.Add(掛網點);
            Task.Delay(100).Wait();
            移動到定點();
            skipID.Clear();
        }

        Dictionary<int, int> cacheIGID = new();

        static int waitDone = 0;
        public override async void 腳本運作()
        {
            if (MainNob != null)
            {
                if (initPos == false)
                {
                    initPos = true;
                    掛網點 = new(MainNob.PosX, MainNob.PosY);
                    Debug.WriteLine($"A 掛網點 : {掛網點.X} , {掛網點.Y}");
                    休息回合 = MainNob.CodeSetting.其他選項A == 0 ? 80 : MainNob.CodeSetting.其他選項A;
                    Debug.WriteLine($"休息回合 {休息回合}");
                    全部追隨();
                }

                if (戰鬥回合 >= 休息回合)
                {
                    補符 = true;
                }

                if (補符)
                {
                    回村長定位點();
                    NOBDATA useUser;
                    waitDone = 0;
                    int tryDone = 0;
                    useUser = MainNob;
                    Task.Run(村長補符);
                    Task.Delay(200).Wait();
                    foreach (var nobuser in 隊員智能功能組)
                    {
                        if (nobuser.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                        {
                            useUser = null;
                            useUser = nobuser.NOB;
                            Task.Run(村長補符);
                            Task.Delay(200).Wait();
                        }
                    }
                    Debug.WriteLine($"隊員智能功能組 -> {隊員智能功能組.Count}");
                    while (MainWindow.CodeRun)
                    {
                        Task.Delay(500).Wait();
                        bool 全部完成 = true;
                        if (MainNob.完成必須對話 == false)
                        {
                            全部完成 = false;
                        }
                        foreach (var 成員 in 隊員智能功能組)
                        {
                            if (成員.NOB.完成必須對話 == false)
                            {
                                全部完成 = false;
                                break;
                            }
                        }
                        if (全部完成)
                        {
                            Debug.WriteLine("補符正常完成");
                            break;
                        }
                        tryDone = tryDone + 1;
                        if (tryDone > 60)
                        {
                            Debug.WriteLine("強制結束");
                            break;
                        }

                    }

                    Debug.WriteLine("全部補完");
                    MainWindow.忽略名單IDs.Clear();
                    全部追隨();
                    Task.Delay(1000).Wait();
                    戰鬥回合 = 0;
                    補符 = false;
                    移動倒掛網點();
                    void 村長補符()
                    {
                        NOBDATA user = useUser;
                        Debug.WriteLine($"{user.PlayerName} 補符");
                        int tryNum = 0;
                        user.完成必須對話 = false;
                        while (MainWindow.CodeRun)
                        {
                            if (user.出現直式選單)
                            {
                                if (user.取得最下面選項().Contains("沒什"))
                                {
                                    Task.Delay(200);
                                    user.直向選擇(7);
                                    Task.Delay(100);
                                    user.KeyPress(VKeys.KEY_ENTER, 2);
                                    Task.Delay(100);
                                    user.KeyPress(VKeys.KEY_ESCAPE, 5);
                                    waitDone = waitDone + 1;
                                    Debug.WriteLine($"{user.PlayerName} 補符 完成");
                                    user.完成必須對話 = true;
                                    break;
                                }
                            }
                            else if (user.對話與結束戰鬥 || user.出現左右選單)
                            {
                                user.KeyPress(VKeys.KEY_ESCAPE);
                            }
                            else
                            {
                                user.MoveToNPC(MainNob.CodeSetting.目標A);
                            }
                            tryNum = tryNum + 1;
                            Task.Delay(200);
                            if (tryNum > 200 && 補符 == false)
                            {
                                Debug.WriteLine($"{user.PlayerName} 補符過程出現異常 強制結束");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (MainNob.待機)
                    {
                        Debug.WriteLine($"搜尋新敵人");
                        MainNob.目前動作 = "搜尋新敵人";
                        MainWindow.目標IDs.Clear();
                        Task.Delay(300).Wait();
                        UpdateUI?.Invoke();
                        Task.Delay(100).Wait();
                        tryFindNum++;
                        if (tryFindNum > 20)
                        {
                            tryFindNum = 0;
                            skipID = new List<long>();
                        }
                    }

                    if (MainWindow.目標IDs != null && MainWindow.開打)
                    {
                        MainNob.目前動作 = "待機";
                        if (MainNob.待機)
                        {
                            MainNob.目前動作 = "待機 準備找怪";
                            if (MainWindow.F5解無敵 && F5解無敵 == false)
                            {
                                F5解無敵 = true;
                                MainNob.KeyPress(VKeys.KEY_F5);
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            }

                            var dis = Dis(MainNob.PosX, MainNob.PosY, 掛網點.X, 掛網點.Y);
                            if (dis > 4000)
                            {
                                移動倒掛網點();
                            }

                            Debug.WriteLine($" 目前目標數量 : {MainWindow.目標IDs.Count}");
                            if (MainWindow.目標IDs.Count == 0)
                            {
                                MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                            }
                            else
                            {
                                MainNob.目前動作 = $"尋找目標..SPCheck {spCheck}.";
                                bool battleIn = false;
                                for (int i = 0; i < MainWindow.目標IDs.Count; i++)
                                {
                                    var emID = MainWindow.目標IDs[i];
                                    if (skipID.Contains(emID))
                                    {
                                        continue;
                                    }
                                    MainNob.鎖定NPC((int)emID);
                                    Task.Delay(100).Wait();
                                    var c = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(50, 130), "47ADE8");
                                    //赤鬼 3
                                    //枯瘦 11
                                    //粗暴 5
                                    //肥胖 15
                                    //岩石
                                    if (0 > emID || emID > 4261412000)
                                    {
                                        if (MainWindow.cacheNPCID.ContainsKey(emID))
                                        {
                                            Debug.WriteLine($"目標 : {emID} 範圍 : {MainWindow.cacheNPCID[emID]}");
                                        }
                                        MainNob.MoveToNPC((int)emID);
                                        foreach (var nob in 隊員智能功能組)
                                        {
                                            if (nob != null && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                                            {
                                                nob.NOB.MoveToNPC((int)emID);
                                            }
                                        }

                                        Task.Delay(3500).Wait();
                                        dis = Dis(MainNob.PosX, MainNob.PosY, 掛網點.X, 掛網點.Y);
                                        if (dis > 4000)
                                        {
                                            for (int j = 0; j < 3; j++)
                                            {
                                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                                foreach (var nob in 隊員智能功能組)
                                                {
                                                    if (nob != null && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                                                    {
                                                        if (nob.NOB.戰鬥中 == false)
                                                        {
                                                            nob.NOB.KeyPress(VKeys.KEY_ESCAPE);
                                                        }
                                                    }
                                                }
                                                Task.Delay(1000).Wait();
                                            }
                                            if (skipID.Contains(emID) == false)
                                            {
                                                skipID.Add(emID);
                                            }
                                            移動倒掛網點();
                                        }
                                        else
                                        {
                                            for (int j = 0; j < 3; j++)
                                            {
                                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                                foreach (var nob in 隊員智能功能組)
                                                {
                                                    if (nob != null && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                                                    {
                                                        if (nob.NOB.戰鬥中 == false)
                                                        {
                                                            nob.NOB.KeyPress(VKeys.KEY_ESCAPE);
                                                        }
                                                    }
                                                }
                                                Task.Delay(1000).Wait();
                                            }

                                            Task.Delay(5000).Wait();
                                        }

                                        if (skipID.Contains(emID) == false)
                                        {
                                            skipID.Add(emID);
                                        }
                                        全部追隨();
                                        Task.Delay(2000).Wait();
                                        if (MainWindow.忽略名單IDs.Contains(emID) == false)
                                            MainWindow.忽略名單IDs.Add(emID);
                                    }
                                    else
                                    {
                                        switch (c)
                                        {
                                            case 8:
                                            case 3:
                                            case 11:
                                            case 5:
                                            case 15:
                                                int tryNum = 0;
                                                if (MainWindow.cacheNPCID.ContainsKey(emID))
                                                {
                                                    Debug.WriteLine($"目標 : {emID} 範圍 : {MainWindow.cacheNPCID[emID]}");
                                                }
                                                while (MainWindow.CodeRun)
                                                {
                                                    全部追隨();
                                                    Task.Delay(500).Wait();

                                                    int lockID = MainNob.GetTargetIDINT();
                                                    if (lockID == (int)emID)
                                                    {
                                                        MainNob.MoveToNPC((int)emID);
                                                        foreach (var nob in 隊員智能功能組)
                                                        {
                                                            if (nob != null && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                                                            {
                                                                if (nob.NOB.戰鬥中 == false)
                                                                {
                                                                    nob.NOB.MoveToNPC((int)emID);
                                                                }
                                                            }
                                                        }
                                                        Task.Delay(500).Wait();
                                                        for (int j = 0; j < 3; j++)
                                                        {
                                                            foreach (var nob in 隊員智能功能組)
                                                            {
                                                                if (nob != null
                                                                    && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false
                                                                    && nob.NOB.戰鬥中 == false)
                                                                {
                                                                    nob.NOB.MoveToNPC((int)emID);
                                                                }
                                                            }
                                                        }

                                                        if (MainNob.戰鬥中)
                                                        {
                                                            Task.Delay(1000).Wait();
                                                            if (MainWindow.忽略名單IDs.Contains(emID) == false)
                                                                MainWindow.忽略名單IDs.Add(emID);
                                                            spCheck = 0;
                                                            tryFindNum = 0;

                                                            bool allinBattle = true;
                                                            while (CodeRun)
                                                            {
                                                                foreach (var nob in 隊員智能功能組)
                                                                {
                                                                    if (nob != null && nob.NOB.PlayerName.Contains(MainNob.PlayerName) == false)
                                                                    {
                                                                        if (nob.NOB.戰鬥中 == false)
                                                                        {
                                                                            allinBattle = false;
                                                                            nob.NOB.MoveToNPC((int)emID);
                                                                        }
                                                                    }
                                                                    Task.Delay(100).Wait();
                                                                }
                                                                if (allinBattle || MainNob.待機)
                                                                    break;
                                                            }
                                                            battleIn = true;
                                                            break;
                                                        }
                                                        tryNum = tryNum + 1;
                                                        if (tryNum > 6)
                                                        {
                                                            tryNum = 0;
                                                            if (MainWindow.忽略名單IDs.Contains(emID) == false)
                                                                MainWindow.忽略名單IDs.Add(emID);
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (MainWindow.忽略名單IDs.Contains(emID) == false)
                                                            MainWindow.忽略名單IDs.Add(emID);
                                                        break;
                                                    }
                                                }
                                                break;
                                            default:
                                                if (MainWindow.忽略名單IDs.Contains(emID) == false)
                                                    MainWindow.忽略名單IDs.Add(emID);
                                                continue;
                                        }

                                    }
                                    if (battleIn)
                                    {
                                        if (skipID.Contains(emID) == false)
                                        {
                                            skipID.Add(emID);
                                        }
                                        break;
                                    }
                                }

                                Debug.WriteLine("選轉尋找目標");
                                MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                            }

                            if (戰鬥回合 > 3)
                            {
                                spCheck = spCheck + 1;
                                if (spCheck > 20)
                                {
                                    spCheck = 0;
                                    補符 = true;
                                }
                            }
                            else
                            {
                                spCheck = spCheck + 1;
                                if (spCheck > 20)
                                {
                                    spCheck = 0;
                                    移動倒掛網點();
                                }
                            }
                        }

                        if (MainNob.戰鬥中)
                        {
                            MainNob.目前動作 = "戰鬥中";
                            cacheIGID.Clear();
                            F5解無敵 = false;
                            mBCHCount = 0;
                        }

                        if (MainNob.對話與結束戰鬥)
                        {
                            MainNob.目前動作 = "結算對話中";
                            mBCHCount++;
                            Task.Delay(100).Wait();
                            if (mBCHCount > 3)
                            {
                                mBCHCount = 0;
                                Debug.WriteLine($"隊員智能功能組 {隊員智能功能組.Count}");
                                foreach (var user in 隊員智能功能組)
                                {
                                    if (user != null)
                                        Task.Run(user.NOB.離開戰鬥B);
                                }
                                int tryNum = 0;
                                while (MainWindow.CodeRun)
                                {
                                    bool allDoneCheck = true;
                                    foreach (var user in 隊員智能功能組)
                                    {
                                        if (user.NOB.離開戰鬥確認 == false)
                                        {
                                            allDoneCheck = false;
                                        }
                                    }
                                    tryNum = tryNum + 1;
                                    if (tryNum > 120 || allDoneCheck)
                                    {
                                        全部追隨();
                                        戰鬥回合 = 戰鬥回合 + 1;
                                        mBCHCount = 0;
                                        MainWindow.MainState = "完成離開";
                                        break;
                                    }
                                    else
                                    {
                                        MainWindow.MainState = "等待玩家離開";
                                        Task.Delay(500).Wait();
                                    }

                                }
                            }
                        }
                    }

                    MainNob.KeyPressT(VKeys.KEY_Q, 1000);
                }
            }
        }
    }
}
