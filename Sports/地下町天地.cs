using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 地下町天地 : BaseClass
    {
        public int mState = 0;
        public bool 是否經過戰鬥 = false;
        public bool 是否進入場內 = false;
        public int 選擇關卡 = 0;
        public int 選擇難度 = 0; // 0-弱 
        public int 外設大會內嚮導ID = 0;
        public int 大會嚮導ID = 0;
        public int 內部大會嚮導ID = 0;
        public int 對手目標ID = 0;
        public int 家臣數量 = 0;
        public int 連續戰鬥 = 0;
        public bool 大會地 = false;
        bool 未經過戰鬥出去 = true;
        public int checkDone = 0;
        public NOBDATA mUseNOB;
        public bool mutIn = false;
        int 目前戰鬥場次 = 0;
        int 遞減檢查 = 0;
        List<string> cacheName = new();
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

            遞減檢查 = 0;
            大會嚮導ID = 0;
            對手目標ID = 0;
            if (MainNob.CodeSetting.目標C > 10)
            {
                內部大會嚮導ID = MainNob.CodeSetting.目標C;
            }
            //Point = 檢查點.未知;
            //for (int i = 0; i < FIDList.Count; i++)
            //{
            //    FIDList[i].選擇目標類型(7);
            //}
        }

        public void 戰鬥中()
        {
            未經過戰鬥出去 = false;
            是否經過戰鬥 = true;
            遞減檢查 = 0;
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

                Debug.WriteLine($"MainNob.MAPID {MainNob.MAPID} Point : {Point.ToString()}");
                MainWindow.MainState = "狀態:" + Point.ToString();
                switch (Point)
                {
                    case 檢查點.入場:
                        狀態入場();
                        break;
                    case 檢查點.找目標:
                        戰鬥中確認 = 0;
                        狀態找目標();
                        break;
                    case 檢查點.戰鬥中:
                        MainWindow.MainState = "地下町 戰鬥中";
                        //戰鬥中
                        是否經過戰鬥 = true;
                        未經過戰鬥出去 = false;
                        遞減檢查 = 0;
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
                    case 檢查點.結束戰鬥:
                        狀態出戰鬥();
                        if (MainNob.待機)
                        {
                            Point = 檢查點.出場;
                            是否經過戰鬥 = true;
                        }
                        break;
                    case 檢查點.出場:
                    default:
                        狀態出場();
                        break;
                }

                isRun = false;
            }
            //城下町 ==> 會場
            async void 狀態入場()
            {
                MainWindow.MainState = "地下町 進場中 大會嚮導ID: " + 大會嚮導ID;
                Task.Delay(100).Wait();
                while (MainWindow.CodeRun)
                {
                    MainNob.鎖定NPC(大會嚮導ID);
                    Task.Delay(200).Wait();
                    if (大會嚮導ID == MainNob.GetTargetIDINT() || 町內所有NPCID.Contains(大會嚮導ID))
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
                    await 入場();
                }
                else
                {
                    Debug.WriteLine("mutIn" + mutIn);
                    if (mutIn == false)
                    {
                        mutIn = true;
                        foreach (var nob in FIDList)
                        {
                            if (!cacheName.Contains(nob.PlayerName) && !nob.PlayerName.Contains(MainNob.PlayerName))
                            {
                                cacheName.Add(nob.PlayerName);
                                mUseNOB = nob;
                                Task.Run(入場);
                                Task.Delay(500).Wait();
                            }
                        }
                        Task.Delay(2000).Wait();
                        mUseNOB = MainNob;
                        await Task.Run(入場);

                    }
                    while (MainWindow.CodeRun && FIDList.Count > checkDone)
                    {
                        bool allDone = true;
                        List<string> msg = new();
                        foreach (var nob in FIDList)
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

                        MainWindow.MainState = "等待數量:" + FIDList.Count + " 完成數量:" + checkDone + Environment.NewLine + "進場等待中 " + string.Join('|', msg);
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
                遞減檢查 = 0;
            }
            //會場 找目標 ==> 戰鬥
            void 狀態找目標()
            {
                //未戰鬥 找人準備進入戰鬥
                int errorChk = 0;
                int ffcheck = 0;
                Task.Delay(100).Wait();
                //MainNob.KeyPress(VKeys.KEY_W);
                場內找對手();

                foreach (var nob in FIDList)
                {
                    nob.KeyPress(VKeys.KEY_F8);
                    Task.Delay(100).Wait();
                }

                while (MainWindow.CodeRun && MainNob != null)
                {
                    MainWindow.MainState = $"EC:{errorChk} FC:{ffcheck}" + Environment.NewLine + "目標ID:" + 對手目標ID;
                    Debug.WriteLine($"ST:{MainNob.StateA} EC:{errorChk} FC:{ffcheck}" + Environment.NewLine + "目標ID:" + 對手目標ID);
                    Task.Delay(200).Wait();
                    MainNob.目前動作 = @$" EC:{errorChk} NT:{遞減檢查} {Environment.NewLine} 嚮導{內部大會嚮導ID} {Environment.NewLine} 戰鬥目標{對手目標ID}";
                    //確認框框
                    if (MainNob.StateA.Contains("F0 F8"))
                    {
                        MainNob.KeyPress(VKeys.KEY_J);
                        Task.Delay(100).Wait();
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(100).Wait();
                    }
                    else
                    {
                        MainNob.MoveToNPC(對手目標ID);
                        Task.Delay(300).Wait();
                        if (MainNob.GetTargetIDINT() != 對手目標ID)
                        {
                            Debug.WriteLine("無法鎖定對象");
                            Task.Delay(200).Wait();

                            errorChk += 1;
                        }
                        if (MainNob.對話與結束戰鬥)
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(100).Wait();
                        }
                    }

                    if (MainNob != null && MainNob.戰鬥中)
                    {
                        errorCC = 0;
                        errorChk = 0;
                        ffcheck = 0;
                        Point = 檢查點.戰鬥中;
                        是否經過戰鬥 = true;
                        MainWindow.MainState = "地下町 戰鬥中";
                        break;
                    }

                    errorChk++;
                    if (errorChk > 20 && MainNob != null)
                    {
                        MainNob.目前動作 = "嘗試重新尋找對手";
                        for (int i = 0; i < 7; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }
                        errorChk = 0;
                        遞減檢查++;
                        if (遞減檢查 > 4)
                        {
                            遞減檢查 = 0;
                            ffcheck++;
                        }
                        Debug.WriteLine("Error 重置");
                        if (內部大會嚮導ID > -1)
                        {
                            內部大會嚮導ID = 內部大會嚮導ID - 1;
                            對手目標ID = 內部大會嚮導ID - 1;
                        }
                        尋找目標(MainNob);
                        MainNob.鎖定NPC(對手目標ID);
                        if (ffcheck > 2)
                        {
                            遞減檢查 = 0;
                            未經過戰鬥出去 = true;
                            Point = 檢查點.出場;
                            break;
                        }
                    }
                }
            }

            void 狀態出戰鬥()
            {
                是否經過戰鬥 = true;
                //戰鬥結束
                if (isSolo)
                {
                    mUseNOB = MainNob;
                    離開戰鬥();
                }
                else
                {
                    foreach (var nob in FIDList)
                    {
                        mUseNOB = nob;
                        mUseNOB.目前動作 = "離開戰鬥畫面";
                        new Thread(離開戰鬥).Start();
                        Task.Delay(300).Wait();
                    }

                    while (MainWindow.CodeRun && FIDList.Count > checkDone)
                    {
                        bool allDone = true;

                        foreach (var nob in FIDList)
                        {
                            Task.Delay(100).Wait();
                            if (!nob.待機)
                            {
                                allDone = false;
                            }
                        }

                        if (allDone)
                        {
                            Point = 檢查點.出場;
                            break;
                        }
                        Task.Delay(200).Wait();
                    }
                }
                Point = 檢查點.出場;
            }

            async void 狀態出場()
            {
                //戰鬥結束
                MainWindow.MainState = "戰鬥結束準備離開嚮導ID:" + 內部大會嚮導ID;

                if (內部大會嚮導ID < 99)
                {
                    場內找對手(true);
                }

                if (是否經過戰鬥)
                {
                    是否經過戰鬥 = false;
                    目前戰鬥場次 = 目前戰鬥場次 + 1;
                    hasNext = 連續戰鬥 > 目前戰鬥場次;
                }
                Debug.WriteLine($@"目前戰鬥場次:{目前戰鬥場次} - 連續戰鬥:{連續戰鬥} - hasNext : " + hasNext);
                if (hasNext)
                {
                    int cacheID = 0;
                    //等待消失才能往下一步
                    Debug.WriteLine("等待消失才能往下一步");
                    while (MainWindow.CodeRun)
                    {
                        Task.Delay(500).Wait();
                        if (MainNob.待機)
                        {
                            break;
                        }
                    }
                    Task.Delay(100).Wait();
                    int checkTime = 0;
                    while (MainWindow.CodeRun)
                    {
                        MainNob.鎖定NPC(對手目標ID);
                        Task.Delay(200).Wait();
                        cacheID = MainNob.GetTargetIDINT();
                        if (cacheID != 對手目標ID || checkTime > 20)
                        {
                            break;
                        }
                        Debug.WriteLine("等待可以往下一關卡中");
                        MainNob.目前動作 = "等待可以往下一關卡中";
                        Task.Delay(1000).Wait();
                        checkTime = checkTime + 1;
                    }
                    Debug.WriteLine($"內部大會嚮導ID 判斷 : {內部大會嚮導ID}");
                    while (MainWindow.CodeRun)
                    {
                        Task.Delay(100).Wait();
                        MainNob.鎖定NPC(內部大會嚮導ID);
                        Task.Delay(100).Wait();
                        cacheID = MainNob.GetTargetIDINT();
                        Debug.WriteLine($"{對手目標ID} {cacheID} : {內部大會嚮導ID} : {MainNob.CodeSetting.目標C}");
                        if (cacheID == 內部大會嚮導ID)
                        {
                            break;
                        }
                        else
                        {
                            if (MainNob.CodeSetting.目標C > 10)
                            {
                                內部大會嚮導ID = MainNob.CodeSetting.目標C;
                                MainNob.CodeSetting.目標C = 0;
                            }
                            else
                                內部大會嚮導ID = 對手目標ID + 1;
                        }
                    }

                    Debug.WriteLine("前進往大會嚮導");
                    int errorCheck = 0;
                    while (MainWindow.CodeRun)
                    {
                        MainNob.目前動作 = "前進往大會嚮導" + 內部大會嚮導ID;
                        MainNob.MoveToNPC(內部大會嚮導ID);
                        Task.Delay(100).Wait();
                        if (MainNob.對話與結束戰鬥)
                        {
                            if (MainNob.出現直式選單 && MainNob.取得最下面選項().Equals("返回"))
                            {
                                MainNob.直向選擇(0);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 6);
                                對手目標ID = 0;
                                Task.Delay(3000).Wait();
                                break;
                            }
                            else
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }
                        errorCheck++;
                        if (errorCheck > 20)
                        {
                            errorCheck = 0;
                            內部大會嚮導ID = 對手目標ID + 1;
                        }
                    }
                    Point = 檢查點.找目標;
                }
                else
                {

                    if (isSolo && mutIn == false)
                    {
                        mUseNOB = MainNob;
                        await 出場();
                    }
                    else
                    {
                        if (mutIn == false)
                        {
                            mutIn = true;
                            foreach (var nob in FIDList)
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
                        while (MainWindow.CodeRun && FIDList.Count > checkDone)
                        {
                            bool allDone = true;
                            List<string> msg = new();
                            foreach (var nob in FIDList)
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
                            MainWindow.MainState = "等待數量:" + FIDList.Count + " 完成數量:" + checkDone + Environment.NewLine + "出場等待中 " + string.Join('|', msg);
                            if (allDone)
                            {
                                break;
                            }

                            Task.Delay(500).Wait();
                        }
                        errorCC = 0;
                    }

                    Debug.WriteLine("出場完成");
                    Task.Delay(1000).Wait();

                    Point = 檢查點.入場;
                    是否經過戰鬥 = false;
                    內部大會嚮導ID = 0;
                    mutIn = false;
                    checkDone = 0;
                    遞減檢查 = 0;
                }
            }

            errorCC++;
            if (errorCC > 20)
            {
                errorCC = 0;
                isRun = false;
            }

            void 場內找對手(bool clear = false)
            {
                while (MainNob != null && MainWindow.CodeRun)
                {
                    if (內部大會嚮導ID < 99 || 對手目標ID < 99)
                    {
                        尋找目標(MainNob);
                    }

                    if (對手目標ID > 99)
                    {
                        break;
                    }
                }
            }
        }

        void 離開戰鬥()
        {
            var useNOB = mUseNOB;
            do
            {
                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                Task.Delay(100).Wait();
                useNOB.KeyPress(VKeys.KEY_ENTER);
                Task.Delay(100).Wait();
                if (useNOB.待機)
                    break;
            }
            while (MainWindow.CodeRun);

            checkDone++;
            //離開戰鬥結束
        }

        private async Task 出場()
        {
            var useNOB = mUseNOB;
            useNOB.目前動作 = "等待出場";
            int errorCheck = 0;
            int fferrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.KeyPress(VKeys.KEY_W);
                while (MainWindow.CodeRun)
                {
                    useNOB.目前動作 = "離開" + 內部大會嚮導ID;
                    Debug.WriteLine(useNOB.PlayerName + "(出場) 尋找 出場 內部大會嚮導ID : " + 內部大會嚮導ID);

                    if (內部大會嚮導ID > 999)
                    {
                        //if (對手目標ID != 0)
                        //{
                        //    內部大會嚮導ID = 對手目標ID + 1;
                        //}

                        useNOB.鎖定NPC(內部大會嚮導ID);
                        Task.Delay(200).Wait();
                        Debug.WriteLine("對手目標ID: " + 內部大會嚮導ID + " : " + useNOB.GetTargetIDINT());

                        if (內部大會嚮導ID.Equals(useNOB.GetTargetIDINT()))
                        {
                            Debug.WriteLine("出場 前往對話中");
                            Task.Delay(100).Wait();
                            useNOB.MoveToNPC(內部大會嚮導ID);
                            Task.Delay(200).Wait();
                        }
                        else
                        {
                            Task.Delay(100).Wait();
                            Debug.WriteLine("出場無法鎖定對象");
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
                        if (對手目標ID != 0)
                        {
                            內部大會嚮導ID = 對手目標ID + 1;
                            對手目標ID = 0;
                        }
                        else
                        {
                            遞減檢查++;
                            if (遞減檢查 > 3)
                            {
                                遞減檢查 = 0;
                                fferrorCheck++;
                            }
                            if (fferrorCheck > 1)
                            {
                                fferrorCheck = 0;
                                所有NPCID.Clear();
                            }

                            尋找目標(MainNob);
                        }
                    }
                }

                Task.Delay(500).Wait();
                useNOB.KeyPress(VKeys.KEY_W);
                checkDone++;
                useNOB.目前動作 = "完成出場";
                Debug.WriteLine(" 完成功能 ");

            }

        }
        private async Task 入場()
        {
            var useNOB = mUseNOB;

            Debug.WriteLine("入場 " + useNOB.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.目前動作 = "入場中.." + 大會嚮導ID;
                bool m領獎完成 = false;
                while (MainWindow.CodeRun)
                {
                    if (大會嚮導ID < 99)
                    {
                        Task.Delay(500).Wait();
                        continue;
                    }

                    useNOB.MoveToNPC(大會嚮導ID);
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;
                    bool cstatus = useNOB.MAPID != 6401;
                    if (cstatus)
                    {
                        if (useNOB.PlayerName.Contains(MainNob.PlayerName))
                            LDIn = true;
                        useNOB.目前動作 = "場內等待中";
                        Task.Delay(1000).Wait();
                        break;
                    }

                    if (useNOB.StateA.Contains("E0 F0"))
                    {
                        if (useNOB.PlayerName.Contains(MainNob.PlayerName))
                        {
                            useNOB.目前動作 = "隊長選擇關卡";
                            useNOB.直向選擇(選擇關卡);
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

                    if (大會嚮導ID > 0 && useNOB.對話與結束戰鬥)
                    {
                        useNOB.目前動作 = "前往嚮導對話" + 大會嚮導ID;
                        bool hasTalk = false;
                        useNOB.MoveToNPC(大會嚮導ID);
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
                                    useNOB.MoveToNPC(大會嚮導ID);
                                    Task.Delay(100).Wait();
                                    while (MainWindow.CodeRun && !LDIn && !useNOB.PlayerName.Contains(MainNob.PlayerName))
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
                                useNOB.直向選擇(選擇難度);
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
                                bool isLD = useNOB.PlayerName.Contains(MainNob.PlayerName);
                                if (cstatus && isLD)
                                {
                                    useNOB.KeyPress(VKeys.KEY_W);
                                    mErrorCheck = 0;
                                    內部大會嚮導ID = 0;
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
                            Debug.WriteLine(" ErrorCheck ");
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

        private void 尋找地下町嚮導(NOBDATA nob)
        {
            if (nob != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                List<long> skipIDs = new();
                long fminID = long.MaxValue;
                for (int i = 0; i < 64; i++)
                {
                    long findID = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str, 4);
                    if (findID <= 10)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }

                    long chid = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
                    long dis = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(4), 4);
                    Debug.WriteLine("dis : " + dis + " findID : " + findID + " chid : " + chid);

                    if (skipIDs.Contains(findID) || chid != 96 || dis > 9000) { str = str.AddressAdd(12); continue; }
                    skipIDs.Add(findID);
                    if (fminID > findID && chid == 96 && dis < 65535 && !町內所有NPCID.Contains(findID))
                    {
                        fminID = findID;
                        //町內所有NPCID.Add(findID);
                    }

                    str = str.AddressAdd(12);
                }

                大會嚮導ID = (int)fminID;
            }
        }

        List<long> 町內所有NPCID = new();
        List<long> 所有NPCID = new();
        private void 尋找目標(NOBDATA nob)
        {
            if (nob != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long findID = 0, maxID = 0;
                if (大會地 && 目前戰鬥場次 > 0)
                    maxID = int.MaxValue;
                bool find3dis = false;
                List<long> skipIDs = new();
                for (int i = 0; i < 32; i++)
                {
                    findID = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str, 4);
                    if (findID <= 10)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }
                    long chid = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
                    if (findID < 99 || skipIDs.Contains(findID) || chid != 96) { str = str.AddressAdd(12); continue; }
                    long dis = MainWindow.dmSoft.ReadInt(nob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(4), 4);
                    if (dis > 10 && dis < 1000)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }
                    if (dis > 10 && dis >= 65534)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }
                    if (dis == 3)
                    {
                        Debug.WriteLine("找到目標 : " + 對手目標ID);
                        對手目標ID = (int)findID;
                        if (內部大會嚮導ID == 0)
                        {
                            內部大會嚮導ID = 對手目標ID + 1;
                        }
                        find3dis = true;
                        break;
                    }
                    if (大會地 && 目前戰鬥場次 > 0)
                    {
                        if (maxID > findID)
                        {
                            maxID = findID;
                        }
                    }
                    else
                    {
                        if (findID > maxID)
                        {
                            maxID = findID;
                        }
                    }

                    skipIDs.Add(findID);
                    if (findID > 0 && 所有NPCID.Contains(findID) == false)
                        所有NPCID.Add(findID);

                    str = str.AddressAdd(12);
                }

                if (MainNob != null &&
                    find3dis == false)
                {
                    MainNob.鎖定NPC(內部大會嚮導ID);
                    Task.Delay(100).Wait();
                    var getCid = MainNob.GetTargetIDINT();
                    Task.Delay(100).Wait();
                    if (getCid == 內部大會嚮導ID)
                    {
                        對手目標ID = (int)maxID;
                    }
                    else
                    {
                        內部大會嚮導ID = (int)maxID;
                        對手目標ID = 內部大會嚮導ID - 1;
                        Debug.WriteLine("找到內部大會嚮導ID : " + 內部大會嚮導ID);
                    }
                }

                //str = AddressData.搜尋身邊NPCID起始;
            }
        }
    }
}


//if (ramFind == false && 所有NPCID != null && 所有NPCID.Count > 0)
//{
//    所有NPCID.Sort((x, y) => -x.CompareTo(y));
//    Debug.WriteLine(String.Join(" , ", 所有NPCID));
//    int index = 家臣數量 + 遞減檢查;
//    while (MainWindow.CodeRun)
//    {
//        if (所有NPCID.Count > index)
//        {
//            內部大會嚮導ID = (int)所有NPCID[index];
//            對手目標ID = 內部大會嚮導ID - 1;
//            if (所有NPCID.Contains(對手目標ID))
//            {
//                break;
//            }
//            index++;
//        }
//        else
//        {
//            break;
//        }
//    }
//}

//if (文字搜尋 && findID > 0)
//{
//    nob.鎖定NPC((int)findID);
//    Task.Delay(100).Wait();
//    for (int j = 0; j < 目標轉名稱.Count; j++)
//    {
//        if (nob.目標記憶體文字(目標轉名稱[j]))
//        {
//            對手目標ID = (int)findID;
//        }
//    }
//    if (nob.目標記憶體文字("大會"))
//    {
//        Debug.WriteLine("找到嚮導");
//        內部大會嚮導ID = (int)findID;
//        對手目標ID = 內部大會嚮導ID - 1;
//        MainWindow.MainState = "搜尋到文字" + (int)findID;
//        ramFind = true;
//        return;
//    }
//}