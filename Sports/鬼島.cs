using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 鬼島 : BaseClass
    {
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
            MainNob.Log($"回村長定位 掛網點 :  {掛網點.X} , {掛網點.Y}");
            //移動點.Add(new(21987, 24883));
            移動點.Add(掛網點);
            Task.Delay(100).Wait();
            移動到定點();
            skipID.Clear();
        }

        Dictionary<int, int> cacheIGID = new();
        bool allDoneCheck = true;
        static int waitDone = 0;
        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                try
                {
                    if (initPos == false)
                    {
                        initPos = true;
                        掛網點 = new(MainNob.PosX, MainNob.PosY);
                        MainNob.Log($"A 掛網點 : {掛網點.X} , {掛網點.Y}");
                        休息回合 = MainNob.CodeSetting.其他選項A == 0 ? 80 : MainNob.CodeSetting.其他選項A;
                        MainNob.Log($"休息回合 {休息回合}");
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
                        foreach (var nobuser in NobTeam)
                        {
                            if (nobuser.PlayerName.Contains(MainNob.PlayerName) == false)
                            {
                                useUser = null;
                                useUser = nobuser;
                                Task.Run(村長補符);
                                Task.Delay(200).Wait();
                            }
                        }
                        MainNob.Log($"隊員智能功能組 -> {NobTeam.Count}");
                        while (MainNob.StartRunCode)
                        {
                            Task.Delay(500).Wait();
                            bool 全部完成 = true;
                            if (MainNob.完成必須對話 == false)
                            {
                                全部完成 = false;
                            }
                            foreach (var 成員 in NobTeam)
                            {
                                if (成員.完成必須對話 == false)
                                {
                                    全部完成 = false;
                                    break;
                                }
                            }
                            if (全部完成)
                            {
                                MainNob.Log("補符正常完成");
                                break;
                            }
                            tryDone = tryDone + 1;
                            if (tryDone > 60)
                            {
                                MainNob.Log("強制結束");
                                break;
                            }

                        }

                        MainNob.Log("全部補完");
                        NobMainCodePage.IgnoredIDs.Clear();
                        全部追隨();
                        Task.Delay(1000).Wait();
                        戰鬥回合 = 0;
                        補符 = false;
                        移動倒掛網點();
                        void 村長補符()
                        {
                            NOBDATA user = useUser;
                            MainNob.Log($"{user.PlayerName} 補符");
                            int tryNum = 0;
                            user.完成必須對話 = false;
                            while (MainNob.StartRunCode)
                            {
                                Task.Delay(100).Wait();
                                if (user.出現直式選單 && user.取得最下面選項().Contains("沒什"))
                                {
                                    Task.Delay(200).Wait();
                                    user.直向選擇(7);
                                    Task.Delay(200).Wait();
                                    user.KeyPress(VKeys.KEY_ENTER, 2);
                                    Task.Delay(100).Wait();
                                    user.KeyPress(VKeys.KEY_ESCAPE, 5);
                                    waitDone = waitDone + 1;
                                    MainNob.Log($"{user.PlayerName} 補符 完成");
                                    user.完成必須對話 = true;
                                    break;
                                }
                                else if (user.對話與結束戰鬥 || user.出現左右選單)
                                {
                                    user.KeyPress(VKeys.KEY_ESCAPE);
                                    continue;
                                }
                                else
                                {
                                    user.MoveToNPC(MainNob.CodeSetting.目標A);
                                }
                                tryNum = tryNum + 1;
                                if (tryNum > 200 && 補符 == false)
                                {
                                    MainNob.Log($"{user.PlayerName} 補符過程出現異常 強制結束");
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (MainNob.戰鬥中)
                        {
                            MainWindow.MainState = "戰鬥中";
                            MainNob.目前動作 = "戰鬥中";
                            MainNob.戰鬥中判定 = 0;
                            cacheIGID.Clear();
                            mBCHCount = 0;
                            allDoneCheck = false;
                            return;
                        }

                        if (MainNob.進入結算)
                        {
                            MainNob.目前動作 = "進入結算";
                            Task.Delay(200).Wait();
                            foreach (var user in NobTeam)
                            {
                                user.離開戰鬥確認 = false;
                                if (user != null)
                                    Task.Run(user.離開戰鬥B);
                            }
                            int tryNum = 0;
                            while (MainNob.StartRunCode)
                            {
                                allDoneCheck = true;
                                foreach (var user in NobTeam)
                                {
                                    Task.Delay(400).Wait();
                                    if (user.待機)
                                    {
                                        continue;
                                    }
                                    if (user.離開戰鬥確認 == false)
                                    {
                                        allDoneCheck = false;
                                    }
                                }
                                tryNum = tryNum + 1;
                                MainNob.Log($"LOG -- {allDoneCheck} {tryNum}");
                                if (tryNum > 120 || allDoneCheck)
                                {
                                    全部追隨();
                                    戰鬥回合 = 戰鬥回合 + 1;
                                    mBCHCount = 0;
                                    MainWindow.MainState = $"完成離開 {allDoneCheck} {tryNum}";
                                    allDoneCheck = true;
                                    break;
                                }
                                else
                                {
                                    MainWindow.MainState = "等待玩家離開";
                                    Task.Delay(500).Wait();
                                }

                            }

                            mBCHCount = 0;
                        }

                        if (MainNob.對話與結束戰鬥)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }

                        if (allDoneCheck && MainNob.待機)
                        {
                            MainNob.Log($"搜尋新敵人");
                            MainWindow.MainState = "搜尋新敵人";
                            NobMainCodePage.AllNPCIDs.Clear();
                            Task.Delay(300).Wait();
                            NobMainCodePage.GetFilteredNPCs(MainNob, TargetTypes.NPC | TargetTypes.TreasureBox, 4, MainNob.CodeSetting.搜尋範圍);

                            MainNob.Log($" 目前目標數量 : {NobMainCodePage.AllNPCIDs.Count}");

                            if (NobMainCodePage.AllNPCIDs != null)
                            {
                                MainNob.目前動作 = "待機 準備找怪";
                                var dis = Tools.Dis(MainNob.PosX, MainNob.PosY, 掛網點.X, 掛網點.Y);
                                if (dis > 4000)
                                {
                                    移動倒掛網點();
                                }
                                Task.Delay(100).Wait();

                                if (NobMainCodePage.AllNPCIDs.Count > 0)
                                {
                                    MainNob.目前動作 = $"尋找目標..SPCheck {spCheck}.";
                                    bool battleIn = false;
                                    for (int i = 0; i < NobMainCodePage.AllNPCIDs.Count; i++)
                                    {
                                        var emID = NobMainCodePage.AllNPCIDs[i];
                                        if (skipID.Contains(emID))
                                        {
                                            continue;
                                        }
                                        MainNob.鎖定NPC((int)emID);
                                        Task.Delay(100).Wait();
                                        var c = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(50, 130), "47ADE8");
                                        //赤鬼 6
                                        //枯瘦 11
                                        //粗暴 5
                                        //肥胖 15
                                        //岩石

                                        if (MainNob.GetTargetClass() == 254)
                                        {
                                            MainNob.MoveToNPC((int)emID);
                                            foreach (var nob in NobTeam)
                                            {
                                                if (nob != null && nob.PlayerName.Contains(MainNob.PlayerName) == false)
                                                {
                                                    nob.MoveToNPC((int)emID);
                                                }
                                            }

                                            Task.Delay(5000).Wait();
                                            dis = Tools.Dis(MainNob.PosX, MainNob.PosY, 掛網點.X, 掛網點.Y);
                                            if (dis > 7000)
                                            {
                                                for (int j = 0; j < 3; j++)
                                                {
                                                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                                    foreach (var nob in NobTeam)
                                                    {
                                                        if (nob != null && nob.PlayerName.Contains(MainNob.PlayerName) == false)
                                                        {
                                                            if (nob.戰鬥中 == false)
                                                            {
                                                                nob.KeyPress(VKeys.KEY_ESCAPE);
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
                                                    foreach (var nob in NobTeam)
                                                    {
                                                        if (nob != null && nob.PlayerName.Contains(MainNob.PlayerName) == false)
                                                        {
                                                            if (nob.戰鬥中 == false)
                                                            {
                                                                nob.KeyPress(VKeys.KEY_ESCAPE);
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
                                            if (NobMainCodePage.IgnoredIDs.Contains(emID) == false)
                                                NobMainCodePage.IgnoredIDs.Add(emID);
                                        }
                                        else
                                        {
                                            switch (c)
                                            {
                                                //case 8:
                                                case 6:
                                                case 11:
                                                case 5:
                                                case 15:
                                                    int tryNum = 0;
                                                    while (MainNob.StartRunCode)
                                                    {
                                                        全部追隨();
                                                        Task.Delay(500).Wait();

                                                        int lockID = MainNob.GetTargetIDINT();
                                                        if (lockID == (int)emID)
                                                        {
                                                            MainNob.MoveToNPC((int)emID);
                                                            foreach (var nob in NobTeam)
                                                            {
                                                                if (nob != null && nob.PlayerName.Contains(MainNob.PlayerName) == false)
                                                                {
                                                                    if (nob.戰鬥中 == false)
                                                                    {
                                                                        nob.MoveToNPC((int)emID);
                                                                    }
                                                                }
                                                            }
                                                            Task.Delay(500).Wait();
                                                            for (int j = 0; j < 5; j++)
                                                            {
                                                                foreach (var nob in NobTeam)
                                                                {
                                                                    if (nob != null
                                                                        && nob.PlayerName.Contains(MainNob.PlayerName) == false
                                                                        && nob.戰鬥中 == false)
                                                                    {
                                                                        nob.MoveToNPC((int)emID);
                                                                    }
                                                                }
                                                            }

                                                            if (MainNob.戰鬥中)
                                                            {
                                                                Task.Delay(1000).Wait();
                                                                if (NobMainCodePage.IgnoredIDs.Contains(emID) == false)
                                                                    NobMainCodePage.IgnoredIDs.Add(emID);
                                                                spCheck = 0;
                                                                tryFindNum = 0;

                                                                bool allinBattle = true;
                                                                while (MainNob.StartRunCode)
                                                                {
                                                                    foreach (var nob in NobTeam)
                                                                    {
                                                                        if (nob != null && nob.PlayerName.Contains(MainNob.PlayerName) == false)
                                                                        {
                                                                            if (nob.戰鬥中 == false)
                                                                            {
                                                                                allinBattle = false;
                                                                                nob.MoveToNPC((int)emID);
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
                                                                if (NobMainCodePage.IgnoredIDs.Contains(emID) == false)
                                                                    NobMainCodePage.IgnoredIDs.Add(emID);
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (NobMainCodePage.IgnoredIDs.Contains(emID) == false)
                                                                NobMainCodePage.IgnoredIDs.Add(emID);
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    if (NobMainCodePage.IgnoredIDs.Contains(emID) == false)
                                                        NobMainCodePage.IgnoredIDs.Add(emID);
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

                                    MainNob.Log("旋轉尋找目標");
                                    MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                                }


                                spCheck = spCheck + 1;
                                if (spCheck > 20)
                                {
                                    spCheck = 0;
                                    if (戰鬥回合 > 3)
                                    {
                                        補符 = true;
                                    }
                                    else
                                    {
                                        移動倒掛網點();
                                    }
                                }


                            }

                            MainNob.KeyPressT(VKeys.KEY_Q, 1000);
                            Task.Delay(100).Wait();
                            tryFindNum++;
                            if (tryFindNum > 20)
                            {
                                tryFindNum = 0;
                                skipID = new List<long>();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MainNob.Log($"Code Error {e.ToString()}");
                }
            }
        }
    }
}
