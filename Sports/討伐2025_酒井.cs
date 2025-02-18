using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 討伐2025_酒井 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 接任務NPCID = 0;
        int cache地圖 = 7800;
        int TalkID_1 = 0;
        int checkIDC1 = 13;     //松平元康
        int checkIDC2 = 17;     //酒井
        int checkIDC3 = 41;     //水滴
        public override void 初始化()
        {
            移動點 = new();

            Point = 檢查點.入場;
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(1);
            }
            cache地圖 = MainNob!.MAPID;
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                if (Point == 檢查點.未知)
                {
                    if (MainNob.MAPID == cache地圖) { Point = 檢查點.入場; }
                    if (MainNob.MAPID != cache地圖) { Point = 檢查點.找目標; }
                }

                MainWindow.MainState = Point.ToString();
                switch (Point)
                {
                    case 檢查點.入場:
                        {
                            mUseNOB = UseLockNOB;
                            Task.Run(接任務);
                            Debug.WriteLine($"---- {mUseNOB!.PlayerName}");
                            Task.Delay(5000).Wait();
                            for (int i = 0; i < FIDList.Count; i++)
                            {
                                if (FIDList[i].PlayerName.Contains(MainNob.PlayerName) == false)
                                {
                                    mUseNOB = FIDList[i];
                                    mUseNOB.副本進入完成 = false;
                                    Task.Run(接任務);
                                    Task.Delay(500).Wait();
                                }
                            }
                            Task.Delay(500).Wait();
                            Dictionary<NOBDATA, int> playErrorCheck = new();
                            while (MainWindow.CodeRun)
                            {
                                bool done = true;

                                foreach (var nob in FIDList)
                                {
                                    if (nob.副本進入完成 == false)
                                    {
                                        done = false;
                                        break;
                                    }
                                }
                                if (done)
                                    break;
                                Task.Delay(500).Wait();
                            }
                        }
                        Point = 檢查點.找目標;
                        break;
                    case 檢查點.找目標:
                        Task.Delay(100).Wait();
                        //if (false)
                        {
                            MainNob.KeyPress(VKeys.KEY_W, 3);
                            foreach (var nob in FIDList)
                            {
                                if (nob != null)
                                {
                                    nob.KeyPress(VKeys.KEY_F8, 2);
                                    Task.Delay(50).Wait();
                                }
                            }
                            //使用道具
                            while (MainWindow.CodeRun)
                            {
                                MainNob.KeyPress(VKeys.KEY_F7);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 3, 500);
                                if (MainNob.出現直式選單 && MainNob.取得最下面選項().Contains("獎勵"))
                                {
                                    Task.Delay(500).Wait();
                                    MainNob.直向選擇(2);
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 5, 200);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 100);
                                    break;
                                }
                            }
                            ////信長 20 對話開始
                            while (MainWindow.CodeRun)
                            {
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 100), "F6F67A");
                                    if (c2 == checkIDC1)
                                    {
                                        TalkID_1 = MainNob.GetTargetIDINT();
                                        MainNob.MoveToNPC(TalkID_1);
                                    }
                                    else
                                    {
                                        MainNob.KeyPressT(VKeys.KEY_Q, 100);
                                        MainNob.KeyPress(VKeys.KEY_J);
                                        Task.Delay(100).Wait();
                                    }
                                    if (MainNob.GetTargetIDINT() == TalkID_1 && MainNob.對話與結束戰鬥)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                                        Task.Delay(100);
                                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                        break;
                                    }
                                }
                                else
                                {
                                    MainNob.KeyPressT(VKeys.KEY_Q, 100);
                                    MainNob.KeyPress(VKeys.KEY_J);
                                    Task.Delay(100).Wait();
                                }

                            }

                            //井伊
                            移動點.Clear();
                            移動點.Add(new(24282, 52366));
                            移動點.Add(new(26802, 52202));
                            移動點.Add(new(28421, 48006));
                            移動點.Add(new(25291, 43715));
                            移動點.Add(new(18774, 48106));
                            移動點.Add(new(18864, 50827));
                            移動到定點();
                        }
                        int battleCheck = 0;
                        int battleIn = 0;
                        while (MainWindow.CodeRun)
                        {
                            if (MainNob.戰鬥中)
                            {
                                battleIn = 1;
                            }
                            if (MainNob.結算中)
                            {
                                foreach (var item in FIDList)
                                {
                                    Task.Run(item.離開戰鬥B);
                                }
                                Task.Delay(200).Wait();
                                while (CodeRun)
                                {
                                    bool alldone = true;
                                    foreach (var item in FIDList)
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
                            }
                            if (MainNob.待機)
                            {
                                //等待戰鬥
                                Task.Delay(100).Wait();
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    Task.Delay(200).Wait();
                                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                    //朝比奈
                                    if (c2 == checkIDC2)
                                    {
                                        battleCheck = 0;
                                        MainNob!.KeyPress(VKeys.KEY_C);
                                        MainNob!.MoveToNPC(MainNob.GetTargetIDINT());
                                        Task.Delay(200).Wait();
                                    }
                                    else
                                    {
                                        MainNob!.KeyPress(VKeys.KEY_J);
                                        Task.Delay(200).Wait();
                                        battleCheck = battleCheck + 1;
                                    }
                                }
                                else
                                {
                                    MainNob!.KeyPressT(VKeys.KEY_Q, 100);
                                    Task.Delay(200).Wait();
                                    MainNob!.KeyPress(VKeys.KEY_J);
                                    Task.Delay(200).Wait();
                                    battleCheck = battleCheck + 1;
                                }
                                if (battleIn == 1)
                                {
                                    battleCheck = 0;
                                    battleIn = 0;
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    break;
                                }
                                if (battleCheck > 10)
                                {
                                    battleCheck = 0;
                                    MainNob!.KeyPress(VKeys.KEY_ESCAPE, 5);
                                    Task.Delay(200).Wait();
                                    MainNob!.KeyPressT(VKeys.KEY_Q, 100);
                                }
                            }
                            if (MainNob.對話與結束戰鬥)
                            {
                                if (MainNob.GetTargetIDINT() != -1 && battleIn == 0)
                                {
                                    MainNob.KeyPress(VKeys.KEY_J);
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                }
                                else
                                {
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                }
                            }
                        }

                        //水滴回信長
                        {
                            int talkNPCID = 0;
                            int talkCheck = 0;
                            while (MainWindow.CodeRun)
                            {
                                if (MainNob.待機)
                                {
                                    Task.Delay(100).Wait();
                                    if (MainNob.GetTargetIDINT() != -1)
                                    {
                                        Task.Delay(200).Wait();
                                        var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                        //水滴
                                        if (c2 == checkIDC3)
                                        {
                                            talkNPCID = MainNob.GetTargetIDINT();
                                            MainNob!.KeyPress(VKeys.KEY_C);
                                            MainNob!.MoveToNPC(talkNPCID);
                                            Task.Delay(200).Wait();
                                        }
                                        else
                                        {
                                            MainNob!.KeyPress(VKeys.KEY_J);
                                            Task.Delay(200).Wait();
                                            talkCheck = talkCheck + 1;
                                        }
                                    }
                                    else
                                    {
                                        MainNob.後退(200);
                                        MainNob!.KeyPress(VKeys.KEY_J);
                                        Task.Delay(200).Wait();
                                        talkCheck = talkCheck + 1;
                                    }

                                }
                                if (MainNob.GetTargetIDINT() == talkNPCID && MainNob.對話與結束戰鬥)
                                {
                                    if (MainNob.出現左右選單)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_J);
                                        MainNob.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(1500).Wait();
                                        MainNob.鎖定NPC(TalkID_1);
                                        if (MainNob.GetTargetIDINT() == TalkID_1)
                                        {
                                            MainNob.MoveToNPC(TalkID_1);
                                            Task.Delay(500);
                                            break;
                                        }
                                    }
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                }
                                else if (MainNob.對話與結束戰鬥)
                                {
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                }
                                if (talkCheck > 20)
                                {
                                    talkCheck = 0;
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 3);
                                    MainNob.KeyPress(VKeys.KEY_S, 3);

                                }
                                Task.Delay(200).Wait();
                            }
                        }
                        //信長對話
                        Task.Delay(2000).Wait();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);

                        Task.Delay(1000).Wait();
                        {
                            //目前打完 等待回去
                            cache地圖 = MainNob.MAPID;
                            while (MainWindow.CodeRun)
                            {
                                Task.Delay(500);
                                Debug.WriteLine($"離開 尋找絕ID = {TalkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}");
                                MainNob.目前動作 = $"離開 尋找絕 = {TalkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}";
                                if (TalkID_1 == -1)
                                {
                                    TalkID_1 = 顏色尋目標(15, E_TargetColor.藍NPC);
                                }
                                else
                                {
                                    if (MainNob.GetTargetIDINT() == TalkID_1 && MainNob.對話與結束戰鬥)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                                        Task.Delay(100);
                                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                        Point = 檢查點.出場;
                                        break;
                                    }
                                    else
                                    {
                                        MainNob.MoveToNPC(TalkID_1);
                                        Task.Delay(500).Wait();
                                    }
                                }


                            }
                        }
                        break;
                    case 檢查點.出場:
                        Task.Delay(100);
                        int checkTimeOut = 0;
                        while (CodeRun)
                        {
                            bool done = true;

                            foreach (var nob in FIDList)
                            {
                                //Debug.WriteLine($"{nob.PlayerName} 副本離開 -> {nob.MAPID} -> {cache地圖}");
                                nob.目前動作 = $"副本離開 -> {nob.MAPID} -> {cache地圖}";
                                if (nob.MAPID == cache地圖)
                                {
                                    done = false;
                                    break;
                                }
                            }
                            if (done)
                                break;

                            if (checkTimeOut > 20)
                            {
                                checkTimeOut = 0;
                                {
                                    //再次找信長對話
                                    while (MainWindow.CodeRun)
                                    {
                                        Task.Delay(500).Wait();
                                        Debug.WriteLine($"離開 尋找信長 = {TalkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}");
                                        MainNob.目前動作 = $"離開 尋找信長 = {TalkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}";
                                        if (MainNob.GetTargetIDINT() == TalkID_1 && MainNob.對話與結束戰鬥)
                                        {
                                            MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                                            Task.Delay(100);
                                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                            break;
                                        }
                                        else
                                        {
                                            MainNob.MoveToNPC(TalkID_1);
                                            Task.Delay(500).Wait();
                                        }
                                    }
                                }
                            }
                            checkTimeOut = checkTimeOut + 1;
                            Task.Delay(500).Wait();
                        }


                        Point = 檢查點.入場;
                        Task.Delay(1000).Wait();
                        break;

                    case 檢查點.未知:
                    default:
                        MainWindow.MainState = "出現異常";
                        break;
                }
            }
        }

        private void 接任務()
        {
            var useNOB = mUseNOB;
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                Debug.WriteLine("接任務 " + useNOB?.PlayerName);
                useNOB.副本進入完成 = false;
                useNOB.目前動作 = "尋找NPC對話..";
                //入場對話
                int x = 0;
                int y = 0;
                while (MainWindow.CodeRun)
                {
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;
                    if (useNOB.StateA.Contains("E0 F0"))
                    {
                        //等待隊長完成進入
                        if (useNOB.PlayerName.Contains(MainNob!.PlayerName) == false && MainNob!.副本進入完成 == false)
                            continue;
                        //紀錄目前座標 變動後視同進入
                        x = useNOB.PosX;
                        y = useNOB.PosY;
                        cache地圖 = useNOB.MAPID;

                        Task.Delay(500).Wait();
                        useNOB.直向選擇(7);
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER, 7, 200);
                        break;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        if (useNOB.出現直式選單)
                        {
                            Task.Delay(100).Wait();

                            if (useNOB.取得最下面選項().Contains("賤岳") ||
                                useNOB.取得最下面選項().Contains("京都") ||
                                useNOB.取得最下面選項().Contains("富士"))
                            {
                                useNOB.直向選擇(0);
                                Task.Delay(300).Wait();
                            }
                            if (useNOB.取得最下面選項().Contains("不做"))
                            {
                                useNOB.直向選擇(0);
                                Task.Delay(300).Wait();
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                            }
                            if (useNOB.出現左右選單)
                            {
                                useNOB.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                            }

                            if (useNOB.取得最下面選項().Contains("返回"))
                            {
                                useNOB.直向選擇(0);
                                Task.Delay(300).Wait();
                            }

                            Task.Delay(200).Wait();
                        }
                        else
                        {
                            useNOB.KeyPress(VKeys.KEY_ESCAPE);
                        }

                        mErrorCheck++;
                        if (mErrorCheck > 20)
                        {
                            Debug.WriteLine(" ErrorCheck ");
                            mErrorCheck = 0;
                            if (useNOB.出現左右選單)
                            {
                                Task.Delay(500).Wait();
                                useNOB.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                            }
                            else
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            }

                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        if (useNOB.出現左右選單)
                        {
                            Task.Delay(500).Wait();
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                        }
                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }

                }
                //等待轉換地圖入場
                int inQSCheck = 0;
                while (MainWindow.CodeRun)
                {
                    if ((x > 0 && y > 0 && useNOB.PosX != x && useNOB.PosY != y) ||
                        useNOB.MAPID != cache地圖)
                    {
                        Task.Delay(200).Wait();
                        useNOB.副本進入完成 = true;
                        useNOB.目前動作 = "入場完成";
                        break;
                    }

                    inQSCheck = inQSCheck + 1;
                    if (inQSCheck > 25)
                    {
                        //進任務失敗 重新進入
                        useNOB.副本進入完成 = false;
                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        useNOB.目前動作 = "進場失敗";
                        break;
                    }
                    Task.Delay(500).Wait();
                }

                if (useNOB.副本進入完成 == false)
                {
                    mUseNOB = useNOB;
                    接任務();
                }

            }
        }

    }
}
