using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace NOBApp.Sports
{
    internal class 地下町天地 : BaseClass
    {
        public int mState = 0;
        public bool 是否經過戰鬥 = false;
        public bool 是否進入場內 = false;
        public int 地下町大會嚮導ID = 0;
        public int 內部大會嚮導ID = 0;
        public int 對手目標ID = 0;
        public int 連續戰鬥 = 0;
        bool 未經過戰鬥出去 = true;
        public int checkDone = 0;
        public NOBDATA? mUseNOB;
        public bool mutIn = false;
        int 目前戰鬥場次 = 0;
        List<string> cacheName = new();
        List<int> IgnoredIDs = new();
        public bool LDIn = false;
        bool isRun = false;
        bool hasNext = false;
        public bool isSolo = false;
        int 戰鬥中確認 = 0;
        int errorCC = 0;
        public override void 初始化()
        {
            是否經過戰鬥 = true;
            所有NPCID.Clear();
            町內所有NPCID.Clear();

            地下町大會嚮導ID = 0;
            對手目標ID = 0;
            內部大會嚮導ID = 0;
        }

        public void 戰鬥中()
        {
            未經過戰鬥出去 = false;
            是否經過戰鬥 = true;
        }
        public override void 腳本運作()
        {
            if (MainNob != null && isRun == false)
            {
                isRun = true;

                checkDone = 0;
                if (Point == 檢查點.未知)
                {
                    //index = 0;
                    MainNob.KeyPress(VKeys.KEY_W);
                    //6401 - 城下町
                    if (MainNob.MAPID == 6401)
                    {
                        Point = 檢查點.入場;
                        目前戰鬥場次 = 0;
                    }
                    else if (MainNob.MAPID != 6401)
                    {
                        Point = 檢查點.找目標;
                    }
                    else if (MainNob.戰鬥中)
                    {
                        是否經過戰鬥 = true;
                        Point = 檢查點.戰鬥中;
                    }
                    else
                    {
                        if (MainNob.MAPID == 6401)
                            Point = 檢查點.入場;
                        else
                            Point = 檢查點.出場;
                    }
                }

                MainNob.Log($"MainNob.MAPID {MainNob.MAPID} Point : {Point.ToString()}");
                MainWindow.MainState = "狀態:" + Point.ToString();
                switch (Point)
                {
                    case 檢查點.入場:
                        狀態入場();
                        break;
                    case 檢查點.找目標:
                        尋找目標();

                        foreach (var nob in NobTeam)
                        {
                            if (nob != null)
                            {
                                nob.KeyPress(VKeys.KEY_F8, 2);
                                Task.Delay(50).Wait();
                            }
                        }
                        Task.Delay(150).Wait();
                        int checkBattleDone = -1;
                        while (MainNob.StartRunCode)
                        {
                            if (MainNob.待機)
                            {
                                checkBattleDone = -1;
                                MainNob.MoveToNPC(對手目標ID);
                                Task.Delay(500).Wait();
                                continue;
                            }
                            if (MainNob.戰鬥中)
                            {
                                checkBattleDone = 0;
                                Task.Delay(500).Wait();
                                continue;
                            }
                            if (MainNob.對話與結束戰鬥 && checkBattleDone > -1)
                            {
                                checkBattleDone++;
                                if (checkBattleDone < 3)
                                {
                                    Task.Delay(500).Wait();
                                }
                                else
                                {
                                    checkBattleDone = -1;
                                    if (NobTeam != null)
                                    {
                                        foreach (var item in NobTeam)
                                        {
                                            MainNob.Log($"{item.PlayerName} --> 離開戰鬥");
                                            Task.Run(item.離開戰鬥A);
                                        }

                                        while (MainNob.StartRunCode)
                                        {
                                            bool alldone = true;
                                            foreach (var item in NobTeam)
                                            {
                                                if (item.離開戰鬥確認 == false)
                                                {
                                                    alldone = false;
                                                    break;
                                                }
                                            }
                                            if (alldone)
                                                break;
                                        }

                                        Point = 檢查點.出場;
                                        Task.Delay(500).Wait();
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                MainNob.KeyPress(VKeys.KEY_J);
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                            }
                        }
                        break;
                    case 檢查點.戰鬥中:
                        MainWindow.MainState = "地下町 戰鬥中";
                        //戰鬥中
                        是否經過戰鬥 = true;
                        未經過戰鬥出去 = false;
                        Task.Delay(1000).Wait();
                        if (MainNob.戰鬥中)
                        {
                            MainNob.目前動作 = "戰鬥中..";
                            戰鬥中確認 = 0;
                            Task.Delay(500).Wait();
                        }
                        else if (戰鬥中確認 > 2)
                        {
                            Point = 檢查點.結束戰鬥;
                        }
                        戰鬥中確認++;
                        break;
                    case 檢查點.出場:
                    default:
                        繼續或結束();
                        break;
                }

                isRun = false;
            }
            //城下町 ==> 會場
            async void 狀態入場()
            {
                MainWindow.MainState = "地下町 進場中 大會嚮導ID: " + 地下町大會嚮導ID;
                Task.Delay(100).Wait();
                while (MainNob.StartRunCode)
                {
                    MainNob.鎖定NPC(地下町大會嚮導ID);
                    Task.Delay(200).Wait();
                    if (地下町大會嚮導ID == MainNob.GetTargetIDINT() || 町內所有NPCID.Contains(地下町大會嚮導ID))
                    {
                        break;
                    }

                    町內所有NPCID.Clear();
                    尋找地下町嚮導(MainNob);
                }

                if (isSolo)
                {
                    Task.Delay(100).Wait();
                    mUseNOB = MainNob;
                    入場();
                }
                else
                {
                    MainNob.Log("mutIn" + mutIn);
                    if (mutIn == false)
                    {
                        mutIn = true;
                        foreach (var nob in NobTeam)
                        {
                            if (!cacheName.Contains(nob.PlayerName) && !nob.PlayerName.Contains(MainNob.PlayerName))
                            {
                                cacheName.Add(nob.PlayerName);
                                mUseNOB = nob;
                                await Task.Run(入場);
                                Task.Delay(500).Wait();
                            }
                        }
                        Task.Delay(2000).Wait();
                        mUseNOB = MainNob;
                        await Task.Run(入場);

                    }
                    while (MainNob.StartRunCode && NobTeam.Count > checkDone)
                    {
                        bool allDone = true;
                        List<string> msg = new();
                        foreach (var nob in NobTeam)
                        {
                            Task.Delay(100).Wait();

                            if (nob.MAPID == 6401)
                            {
                                msg.Add(nob.PlayerName + "未完成");

                                if (nob.目前動作.Contains("完成入場"))
                                {
                                    nob.KeyPress(VKeys.KEY_W);
                                    Task.Delay(100).Wait();
                                }

                                allDone = false;
                            }
                        }

                        MainWindow.MainState = "等待數量:" + NobTeam.Count + " 完成數量:" + checkDone + Environment.NewLine + "進場等待中 " + string.Join('|', msg);
                        if (allDone)
                        {
                            break;
                        }

                        Task.Delay(200).Wait();
                    }
                    cacheName.Clear();

                    MainWindow.MainState = "全部入場完成 All in Done";
                    LDIn = false;
                    mutIn = false;
                }

                目前戰鬥場次 = 0;
                Point = 檢查點.找目標;
                未經過戰鬥出去 = true;
                是否經過戰鬥 = false;
                內部大會嚮導ID = 0;
                對手目標ID = 0;
            }

            void 繼續或結束()
            {
                //戰鬥結束
                MainWindow.MainState = "戰鬥結束準備離開嚮導ID:" + 內部大會嚮導ID;
                if (是否經過戰鬥)
                {
                    是否經過戰鬥 = false;
                    目前戰鬥場次 = 目前戰鬥場次 + 1;
                    hasNext = MainNob!.CodeSetting.連續戰鬥 == 0 || MainNob.CodeSetting.連續戰鬥 > 目前戰鬥場次;
                }
                if (hasNext)
                {
                    int cacheID = 0;
                    //等待消失才能往下一步
                    MainNob.Log("等待消失才能往下一步");
                    while (MainNob.StartRunCode)
                    {
                        Task.Delay(500).Wait();
                        if (MainNob.待機)
                        {
                            break;
                        }
                    }
                    Task.Delay(100).Wait();
                    int checkTime = 0;
                    while (MainNob.StartRunCode)
                    {
                        MainNob.鎖定NPC(對手目標ID);
                        Task.Delay(200).Wait();
                        cacheID = MainNob.GetTargetIDINT();
                        if (cacheID != 對手目標ID || checkTime > 20)
                        {
                            break;
                        }
                        //  MainNob.Log("等待可以往下一關卡中");
                        MainNob.目前動作 = "等待可以往下一關卡中";
                        Task.Delay(1000).Wait();
                        checkTime = checkTime + 1;
                    }
                    MainNob.Log($"內部大會嚮導ID 判斷 : {內部大會嚮導ID}");
                    int moveErrorCheck = 0;
                    while (MainNob.StartRunCode)
                    {
                        MainNob.目前動作 = $"找大會對話 前往下一個 {moveErrorCheck}";
                        if (MainNob.GetTargetIDINT() == -1)
                        {
                            尋找目標並對話(17, E_TargetColor.藍NPC, ref 內部大會嚮導ID);
                        }

                        if (MainNob.對話與結束戰鬥)
                        {
                            if (MainNob.出現直式選單 && MainNob.取得最下面選項().Contains("返回"))
                            {
                                MainNob.直向選擇(0);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 6);
                                對手目標ID = 0;
                                Task.Delay(2000).Wait();
                                MainNob.Log($"完成對話");
                                break;
                            }
                            else
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }
                        else
                        {
                            MainNob.MoveToNPC(內部大會嚮導ID);
                            Task.Delay(200).Wait();
                        }
                        moveErrorCheck++;
                        if (moveErrorCheck > 20)
                        {
                            moveErrorCheck = 0;
                            IgnoredIDs.Add(內部大會嚮導ID);
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                            尋找目標並對話(17, E_TargetColor.藍NPC, ref 內部大會嚮導ID);
                        }
                    }
                    MainNob.Log($"找新目標 !!");
                    Point = 檢查點.找目標;
                }
                else
                {

                    if (isSolo && mutIn == false)
                    {
                        mUseNOB = MainNob;
                        出場();
                    }
                    else
                    {
                        if (mutIn == false)
                        {
                            mutIn = true;
                            foreach (var nob in NobTeam)
                            {
                                if (!cacheName.Contains(nob.PlayerName) && !nob.PlayerName.Contains(MainNob.PlayerName))
                                {
                                    mUseNOB = nob;
                                    mUseNOB.目前動作 = "出場";
                                    Task.Run(出場);
                                    Task.Delay(500).Wait();
                                }
                            }
                            Task.Delay(500).Wait();
                            mUseNOB = MainNob;
                            Task.Run(出場).Wait();
                            Task.Delay(500).Wait();
                        }
                        while (MainNob.StartRunCode && NobTeam.Count > checkDone)
                        {
                            bool allDone = true;
                            List<string> msg = new();
                            foreach (var nob in NobTeam)
                            {
                                Task.Delay(100).Wait();
                                if (nob != null && nob.MAPID != 6401)
                                {
                                    msg.Add(nob.PlayerName + "未完成");
                                    if (nob.目前動作.Contains("完成出場"))
                                    {
                                        nob.KeyPress(VKeys.KEY_W);
                                        Task.Delay(200).Wait();
                                        mUseNOB = nob;
                                        mUseNOB.目前動作 = "出場";
                                        Task.Run(出場).Wait();
                                        Task.Delay(500).Wait();
                                    }
                                    Task.Delay(200).Wait();
                                    allDone = false;
                                }
                            }
                            MainWindow.MainState = "等待數量:" + NobTeam.Count + " 完成數量:" + checkDone + Environment.NewLine + "出場等待中 " + string.Join('|', msg);
                            if (allDone)
                            {
                                break;
                            }

                            Task.Delay(500).Wait();
                        }
                        errorCC = 0;
                    }

                    MainNob.Log("出場完成");
                    Task.Delay(1000).Wait();

                    Point = 檢查點.入場;
                    是否經過戰鬥 = false;
                    內部大會嚮導ID = 0;
                    mutIn = false;
                    checkDone = 0;
                }
            }

            errorCC++;
            if (errorCC > 20)
            {
                errorCC = 0;
                isRun = false;
            }

        }

        private void 出場()
        {
            var useNOB = mUseNOB;
            useNOB!.目前動作 = "等待出場";
            int errorCheck = 0;
            if (useNOB != null)
            {
                useNOB.KeyPress(VKeys.KEY_W);
                while (MainNob.StartRunCode)
                {
                    useNOB.目前動作 = "離開" + 內部大會嚮導ID;
                    MainNob.Log(useNOB.PlayerName + "(出場) 尋找 出場 內部大會嚮導ID : " + 內部大會嚮導ID);

                    if (內部大會嚮導ID > 999)
                    {
                        useNOB.鎖定NPC(內部大會嚮導ID);
                        Task.Delay(200).Wait();
                        MainNob.Log("對手目標ID: " + 內部大會嚮導ID + " : " + useNOB.GetTargetIDINT());

                        if (內部大會嚮導ID.Equals(useNOB.GetTargetIDINT()))
                        {
                            MainNob.Log("出場 前往對話中");
                            Task.Delay(100).Wait();
                            useNOB.MoveToNPC(內部大會嚮導ID);
                            Task.Delay(200).Wait();
                        }
                        else
                        {
                            Task.Delay(100).Wait();
                            MainNob.Log("出場無法鎖定對象");
                            errorCheck += 2;
                        }
                    }

                    if (useNOB.MAPID == 6401)
                    {
                        useNOB.目前動作 = "已經在外面";
                        Task.Delay(1000).Wait();
                        break;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        useNOB.目前動作 = "離開大會" + 內部大會嚮導ID;
                        bool hasTalk = false;
                        if (useNOB.出現直式選單)
                        {
                            Task.Delay(300).Wait();
                            if (useNOB.取得最下面選項().Contains("返"))
                            {
                                errorCheck = 0;
                                hasTalk = true;
                                Task.Delay(500).Wait();
                                useNOB.直向選擇(5);
                                Task.Delay(500).Wait();
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                                if (未經過戰鬥出去)
                                    for (int k = 0; k < 2; k++)
                                    {
                                        useNOB.KeyPress(VKeys.KEY_J);
                                        Task.Delay(200).Wait();
                                        useNOB.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(200).Wait();
                                    }
                                Task.Delay(2000).Wait();
                                useNOB.KeyPress(VKeys.KEY_W);
                                if (useNOB.MAPID == 6401)
                                {
                                    Task.Delay(1000).Wait();
                                    break;
                                }
                                else
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                        Task.Delay(200).Wait();
                                    }
                                }
                            }
                        }

                        if (hasTalk == false)
                        {
                            Task.Delay(200).Wait();
                            useNOB.KeyPress(VKeys.KEY_ESCAPE);
                        }
                    }

                    if (MainNob!.PlayerName.Contains(useNOB.PlayerName))
                        errorCheck++;

                    if (errorCheck > 15)
                    {
                        useNOB.KeyPress(VKeys.KEY_W);
                        Task.Delay(100).Wait();
                        errorCheck = 0;
                        useNOB.目前動作 = "離開找不到嚮導";
                        內部大會嚮導ID = 顏色尋目標(17, 0, 2000);
                    }
                }

                Task.Delay(500).Wait();
                useNOB.KeyPress(VKeys.KEY_W);
                checkDone++;
                useNOB.目前動作 = "完成出場";
                MainNob.Log(" 完成功能 ");

            }

        }
        private void 入場()
        {
            var useNOB = mUseNOB;

            MainNob.Log("入場 " + useNOB!.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.目前動作 = "入場中.." + 地下町大會嚮導ID;
                bool m領獎完成 = false;
                while (MainNob.StartRunCode)
                {
                    if (地下町大會嚮導ID < 99)
                    {
                        Task.Delay(500).Wait();
                        continue;
                    }

                    useNOB.MoveToNPC(地下町大會嚮導ID);
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;
                    bool cstatus = useNOB.MAPID != 6401;
                    if (cstatus)
                    {
                        if (useNOB.PlayerName.Contains(MainNob!.PlayerName))
                            LDIn = true;
                        useNOB.目前動作 = "場內等待中";
                        Task.Delay(1000).Wait();
                        break;
                    }

                    if (useNOB.StateA.Contains("E0 F0"))
                    {
                        if (useNOB.PlayerName.Contains(MainNob!.PlayerName))
                        {
                            useNOB.目前動作 = "隊長選擇關卡";
                            useNOB.直向選擇(MainNob!.CodeSetting.選擇關卡);
                            Task.Delay(200).Wait();
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                        else
                        {
                            useNOB.目前動作 = "隊員入場中";
                            for (int i = 0; i < 10; i++)
                            {
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                            }
                            Task.Delay(1000).Wait();
                            useNOB.KeyPress(VKeys.KEY_W);
                            cstatus = useNOB.MAPID != 6401;
                            if (cstatus)
                            {
                                useNOB.目前動作 = "場內等待.";
                                Task.Delay(1000).Wait();
                                break;
                            }
                            else
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                    Task.Delay(200).Wait();
                                }
                            }
                        }
                    }

                    if (地下町大會嚮導ID > 0 && useNOB.對話與結束戰鬥)
                    {
                        useNOB.目前動作 = "前往嚮導對話" + 地下町大會嚮導ID;
                        bool hasTalk = false;
                        useNOB.MoveToNPC(地下町大會嚮導ID);
                        if (useNOB.出現直式選單)
                        {
                            Task.Delay(100).Wait();
                            if (useNOB.取得最下面選項().Contains("關於"))
                            {
                                hasTalk = true;
                                if (m領獎完成 == false)
                                {
                                    useNOB.目前動作 = "開始領獎";
                                    Task.Delay(200).Wait();
                                    useNOB.直向選擇(2);
                                    Task.Delay(300).Wait();
                                    useNOB.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(300).Wait();

                                    for (int i = 0; i < 5; i++)
                                    {
                                        useNOB.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(300).Wait();
                                    }
                                    for (int i = 0; i < 5; i++)
                                    {
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                        Task.Delay(200).Wait();
                                    }
                                    useNOB.MoveToNPC(地下町大會嚮導ID);
                                    Task.Delay(100).Wait();
                                    while (MainNob.StartRunCode && !LDIn && !useNOB.PlayerName.Contains(MainNob!.PlayerName))
                                    {
                                        useNOB.目前動作 = "領獎完成等待隊長";
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                        Task.Delay(500).Wait();
                                        if (LDIn) break;
                                    }
                                    m領獎完成 = true;
                                    useNOB.目前動作 = "領獎完成準備進入";
                                }

                                if (m領獎完成)
                                {
                                    useNOB.目前動作 = "準備進入";
                                    useNOB.直向選擇(0);
                                    Task.Delay(300).Wait();
                                    useNOB.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(300).Wait();
                                }
                            }

                            if (useNOB.取得最下面選項().Contains("聽"))
                            {
                                mErrorCheck = 0;
                                useNOB.目前動作 = "難度選擇 EC:" + mErrorCheck;
                                hasTalk = true;
                                Task.Delay(300).Wait();
                                useNOB.直向選擇(MainNob!.CodeSetting.選擇難度);
                                Task.Delay(300).Wait();
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(300).Wait();

                                for (int i = 0; i < 7; i++)
                                {
                                    useNOB.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(300).Wait();
                                }
                                Task.Delay(1000).Wait();

                                cstatus = useNOB.MAPID != 6401;
                                bool isLD = useNOB.PlayerName.Contains(MainNob!.PlayerName);
                                if (cstatus && isLD)
                                {
                                    useNOB.KeyPress(VKeys.KEY_W);
                                    mErrorCheck = 0;
                                    內部大會嚮導ID = 0;
                                    IgnoredIDs.Clear();
                                    Task.Delay(1000).Wait();
                                    LDIn = true;
                                    useNOB.目前動作 = "隊長完成進入";
                                    break;
                                }
                                else
                                {
                                    for (int i = 0; i < 5; i++)
                                    {
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                        Task.Delay(200).Wait();
                                    }
                                }
                            }
                        }

                        if (hasTalk == false)
                        {
                            Task.Delay(300).Wait();
                            useNOB.KeyPress(VKeys.KEY_ESCAPE);
                        }

                        mErrorCheck++;
                        useNOB.目前動作 = "難度選擇 EC:" + mErrorCheck;
                        if (mErrorCheck > 20)
                        {
                            MainNob.Log(" ErrorCheck ");
                            mErrorCheck = 0;
                            for (int i = 0; i < 5; i++)
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                Task.Delay(200).Wait();
                            }
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                    }
                }
                checkDone++;
                useNOB.目前動作 = "完成入場動作";
            }
        }

        //嚮導 17
        private void 尋找地下町嚮導(NOBDATA nob)
        {
            if (nob != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                List<long> skipIDs = new();
                long fminID = long.MaxValue;
                for (int i = 0; i < 64; i++)
                {
                    long findID = MainWindow.dmSoft?.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str, 4) ?? 0;
                    if (findID <= 10)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }

                    long chid = MainWindow.dmSoft?.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2) ?? 0;
                    long dis = MainWindow.dmSoft?.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(4), 4) ?? 0;
                    MainNob.Log("dis : " + dis + " findID : " + findID + " chid : " + chid);

                    if (skipIDs.Contains(findID) || chid != 96 || dis > 9000) { str = str.AddressAdd(12); continue; }
                    skipIDs.Add(findID);
                    if (fminID > findID && chid == 96 && dis < 65535 && !町內所有NPCID.Contains(findID))
                    {
                        fminID = findID;
                        //町內所有NPCID.Add(findID);
                    }

                    str = str.AddressAdd(12);
                }

                地下町大會嚮導ID = (int)fminID;
            }
        }

        List<long> 町內所有NPCID = new();
        List<long> 所有NPCID = new();

        void 尋找目標()
        {
            對手目標ID = -1;
            while (MainNob != null && MainNob.StartRunCode)
            {
                NobMainCodePage.NpcCountToRead = 60;
                var npcsDatas = NobMainCodePage.GetFilteredNPCs(TargetTypes.NPC, 0, 70000);
                NPCData? targetNPC = npcsDatas.FirstOrDefault(npc => npc.Distance == 3);

                if (targetNPC != null)
                {
                    MainNob.Log($"D={targetNPC.Distance} 對手目標ID: {targetNPC.ID} ");
                    對手目標ID = (int)targetNPC.ID;
                    內部大會嚮導ID = 對手目標ID + 1;
                }
                else
                {
                    //觀眾 9 嚮導 17
                    內部大會嚮導ID = 顏色尋目標(17, 0, 70000);
                    int findCheck = 0;
                    var allNPCIDs = NobMainCodePage.GetFilteredNPCs(TargetTypes.NPC, 0, 20000);
                    while (MainNob.StartRunCode)
                    {
                        bool isFind = false;
                        foreach (var npc in allNPCIDs)
                        {
                            MainNob!.鎖定NPC((int)npc.ID);
                            Task.Delay(200).Wait();
                            var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                            if (c1 > 0 && (c1 != 9 && c1 != 17))
                            {
                                MainNob.目前動作 = $"找到目標{npc.ID}";
                                對手目標ID = (int)npc.ID;
                                isFind = true;
                                break;
                            }
                        }
                        if (isFind)
                            break;

                        MainNob!.KeyPressT(VKeys.KEY_E, 500);
                        findCheck++;
                        if (findCheck > 3)
                        {
                            allNPCIDs = NobMainCodePage.GetFilteredNPCs(TargetTypes.NPC, 0, 5000);
                            findCheck = 0;
                            skipIDs.Clear();
                        }
                    }
                    MainNob.Log($"找顏色: {內部大會嚮導ID}");
                }

                if (對手目標ID > 0)
                {
                    MainNob!.鎖定NPC(對手目標ID);
                    Task.Delay(200).Wait();
                    var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    if (c1 > 0 && (c1 != 9 && c1 != 17))
                    {
                        break;
                    }
                    else if (c1 == 17)
                    {
                        內部大會嚮導ID = 對手目標ID;
                        對手目標ID = -1;
                    }
                    else
                    {
                        IgnoredIDs.Add(對手目標ID);
                        對手目標ID = -1;
                    }
                }
            }
        }

    }
}