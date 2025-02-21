using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 討伐2025_後藤 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 起始地圖 = 7800; //白石 地圖 -10015
        public int 接任務NPCID = 0;
        static int 出場NPCID = 0;
        List<int> skipID = new();

        public override void 初始化()
        {
            移動點 = new();

            Point = 檢查點.入場;
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(1);
                MainWindow.dmSoft!.WriteString(FIDList[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F11, 1, "／自動移動:NPC");
                MainWindow.dmSoft!.WriteString(FIDList[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:GOM");
            }
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                if (Point == 檢查點.未知)
                {
                    if (MainNob.MAPID == 起始地圖) { Point = 檢查點.入場; }
                    if (MainNob.MAPID != 起始地圖) { Point = 檢查點.找目標; }
                }

                MainWindow.MainState = Point.ToString();
                switch (Point)
                {
                    case 檢查點.入場:
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
                            ////對話開始
                            while (MainWindow.CodeRun)
                            {
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER);

                                    if (MainNob.對話與結束戰鬥)
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
                                }
                            }

                            //白石
                            移動點.Clear();
                            移動點.Add(new(52391, 26443));
                            移動點.Add(new(53128, 26863));
                            移動點.Add(new(53249, 30717));
                            移動點.Add(new(53123, 31449));
                            移動點.Add(new(53091, 35307));
                            移動到定點();
                        }
                        //需打兩個小兵
                        int battleCheck = 0;
                        int checkBattle = 0;
                        bool 離開結算 = false;

                        targetIDs.Clear();
                        skipID.Clear();
                        int finalTargetID = 0;
                        int findCheck = 0;
                        //搜尋目標
                        while (CodeRun)
                        {
                            var allNPCIDs = GetAllNPCIDs();
                            foreach (var id in allNPCIDs)
                            {
                                if (skipID.Contains(id))
                                    continue;

                                MainNob.鎖定NPC(id);
                                Task.Delay(300).Wait();
                                var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "47ADE8");
                                if (c1 == 20 && targetIDs.Contains(id) == false)
                                {
                                    targetIDs.Add(id);
                                }
                                var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                //後藤
                                if (c2 == 15)
                                {
                                    finalTargetID = id;
                                }

                                skipID.Add(id);
                            }

                            if (targetIDs.Count >= 2)
                            {
                                Debug.WriteLine($"敵人搜尋完成");
                                break;
                            }
                            else
                            {
                                MainNob.KeyPressT(VKeys.KEY_E, 500);
                            }
                            findCheck++;
                            if (findCheck > 3)
                            {
                                findCheck = 0;
                                skipID.Clear();
                            }
                        }

                        int thisTargetID = 0;

                        while (CodeRun)
                        {
                            MainNob.目前動作 = $"小兵數量 -> {targetIDs.Count} 結算中 -> {MainNob.結算中}";
                            if (MainNob.戰鬥中)
                            {
                                if (targetIDs.Contains(thisTargetID))
                                    targetIDs.Remove(thisTargetID);

                                離開結算 = false;

                            }

                            if (MainNob.結算中)
                            {
                                MainNob.目前動作 = $"結算中 -> {離開結算} 小兵數量 -> {targetIDs.Count}";
                                if (離開結算 == false)
                                {
                                    if (targetIDs.Contains(thisTargetID))
                                        targetIDs.Remove(thisTargetID);
                                    checkBattle = checkBattle + 1;
                                    離開結算 = true;
                                    foreach (var item in FIDList)
                                    {
                                        Task.Run(item.離開戰鬥B);
                                    }
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
                                離開結算 = false;
                                //等待戰鬥
                                //新找法
                                if (targetIDs.Count > 0)
                                {
                                    thisTargetID = targetIDs[0];
                                    MainNob.MoveToNPC(thisTargetID);
                                    Task.Delay(500).Wait();
                                }

                                if (targetIDs.Count == 0)
                                {
                                    break;
                                }

                                if (checkBattle >= 2)
                                {
                                    Task.Delay(5000).Wait();
                                    break;
                                }
                            }
                            if (MainNob.對話與結束戰鬥 && !離開結算)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                            }
                            Task.Delay(200).Wait();
                        }
                        ///BOSS
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                        離開結算 = false;
                        if (finalTargetID == 0)
                        {
                            移動點.Clear();
                            移動點.Add(new(52456, 36402));
                            移動到定點();
                        }
                        targetIDs.Clear();
                        thisTargetID = 0;
                        //搜尋目標
                        if (finalTargetID == 0)
                        {
                            while (CodeRun)
                            {
                                var allNPCIDs = GetAllNPCIDs();
                                foreach (var id in allNPCIDs)
                                {
                                    if (skipID.Contains(id))
                                        continue;

                                    MainNob.鎖定NPC(id);
                                    Task.Delay(500).Wait();
                                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                    //後藤
                                    if (c2 == 15)
                                    {
                                        targetIDs.Add(id);
                                    }
                                    skipID.Add(id);
                                    if (targetIDs.Count >= 1)
                                    {
                                        Debug.WriteLine($"敵人搜尋完成");
                                        break;
                                    }
                                }

                                if (targetIDs.Count >= 1)
                                {
                                    Debug.WriteLine($"敵人搜尋完成");
                                    break;
                                }
                                else
                                {
                                    MainNob.KeyPressT(VKeys.KEY_E, 500);
                                }

                                findCheck++;
                                if (findCheck > 3)
                                {
                                    findCheck = 0;
                                    skipID.Clear();
                                }
                            }
                        }
                        else
                        {
                            targetIDs.Add(finalTargetID);

                        }
                        int battleIn = 0;

                        while (MainWindow.CodeRun)
                        {
                            MainNob.目前動作 = $"目標數量 -> {targetIDs.Count}";

                            if (MainNob.戰鬥中)
                            {
                                battleIn = 1;
                                離開結算 = false;
                            }
                            if (MainNob.結算中)
                            {
                                MainNob.目前動作 = $"結算中 -> {離開結算} 目標數量 -> {targetIDs.Count}";

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
                                離開結算 = false;
                                //等待戰鬥
                                Task.Delay(100).Wait();

                                if (targetIDs.Count > 0)
                                {
                                    thisTargetID = targetIDs[0];
                                    MainNob.MoveToNPC(thisTargetID);
                                    Task.Delay(500).Wait();
                                }
                                if (battleIn == 1 || targetIDs.Count == 0)
                                {
                                    battleIn = 0;
                                    Task.Delay(5000).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    Point = 檢查點.出場;
                                    break;
                                }
                                if (battleCheck > 25)
                                {
                                    battleCheck = 0;
                                    MainNob!.KeyPress(VKeys.KEY_ESCAPE, 5);
                                    MainNob!.KeyPress(VKeys.KEY_ENTER);
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

                        break;
                    case 檢查點.出場:
                        //秀吉目前打完都是等待回去
                        if (MainNob.待機)
                        {
                            離開結算 = false;
                            //等待戰鬥
                            if (MainNob.GetTargetIDINT() != -1)
                            {
                                var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                if (cr == 35)
                                {
                                    出場NPCID = MainNob.GetTargetIDINT();
                                    battleCheck = 0;
                                    foreach (var nob in FIDList)
                                    {
                                        nob.副本離開完成 = false;
                                        mUseNOB = nob;
                                        Task.Run(離開副本);
                                        Task.Delay(500).Wait();
                                    }
                                    Task.Delay(300).Wait();
                                    while (CodeRun)
                                    {
                                        bool done = true;

                                        foreach (var nob in FIDList)
                                        {
                                            Debug.WriteLine($"{nob.PlayerName} 副本離開完成 -> {nob.副本離開完成}");
                                            if (nob.副本離開完成 == false)
                                            {
                                                done = false;
                                                break;
                                            }
                                        }
                                        if (done)
                                            break;
                                        Task.Delay(1000).Wait();
                                    }

                                    Point = 檢查點.入場;
                                    Task.Delay(1000).Wait();
                                }
                                else
                                {
                                    MainNob.KeyPressT(VKeys.KEY_Q, 600);
                                    Task.Delay(200).Wait();
                                    MainNob!.KeyPress(VKeys.KEY_J);
                                }
                            }
                            else
                            {
                                MainNob!.KeyPress(VKeys.KEY_J);
                                Task.Delay(200).Wait();
                                MainNob.KeyPressT(VKeys.KEY_Q, 600);
                            }
                        }

                        if (MainNob.對話與結束戰鬥)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(100).Wait();
                        }
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
            Debug.WriteLine("接任務 " + useNOB!.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.副本進入完成 = false;
                useNOB.目前動作 = "尋找NPC對話..";
                int movePress = 0;
                bool isOK = false;
                while (MainWindow.CodeRun)
                {
                    if (movePress % 50 == 0)
                    {
                        useNOB.KeyPress(VKeys.KEY_F11);
                    }
                    if (isOK == false)
                        movePress++;

                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;

                    if (useNOB.StateA.Contains("E0 F0") && isOK)
                    {
                        Task.Delay(500).Wait();
                        useNOB.直向選擇(2);
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER, 7, 200);
                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 15, 100);
                        break;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        int nowID = useNOB.GetTargetIDINT();
                        if (nowID != MainNob!.CodeSetting.目標A)
                        {
                            useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                        }
                        else
                        {
                            if (useNOB.出現左右選單)
                            {
                                Task.Delay(200).Wait();
                                useNOB!.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                                continue;
                            }
                            if (useNOB.出現直式選單)
                            {
                                Task.Delay(100).Wait();

                                if (useNOB.取得最下面選項().Contains("武裝"))
                                {
                                    Task.Delay(200).Wait();
                                    useNOB.直向選擇(1);
                                    Task.Delay(500).Wait();
                                    isOK = true;
                                    continue;
                                }
                                if (useNOB.取得最下面選項().Contains("故事") ||
                                    useNOB.取得最下面選項().Contains("攻城"))
                                {
                                    useNOB.直向選擇(0);
                                    Task.Delay(300).Wait();
                                    continue;
                                }

                                Task.Delay(200).Wait();
                            }
                            else
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                            }
                        }
                        mErrorCheck++;
                        if (mErrorCheck > 20)
                        {
                            Debug.WriteLine(" ErrorCheck ");
                            mErrorCheck = 0;
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }

                }
                movePress = 0;
                mErrorCheck = 0;
                while (MainWindow.CodeRun)
                {
                    if (movePress % 50 == 0)
                    {
                        useNOB.KeyPress(VKeys.KEY_F12);
                    }
                    else
                    {
                        if (!UseLockNOB!.PlayerName.Contains(useNOB.PlayerName) && UseLockNOB.副本進入完成 == false)
                        {
                            UseLockNOB.目前動作 = $"等待隊長 -> {UseLockNOB!.PlayerName}.{UseLockNOB.副本進入完成}.";
                            continue;
                        }
                    }
                    movePress++;
                    Task.Delay(300).Wait();
                    if (useNOB.出現直式選單)
                    {
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(500).Wait();
                    }
                    if (useNOB.出現左右選單)
                    {
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(500).Wait();
                    }
                    if (useNOB.MAPID != 起始地圖)
                    {
                        Task.Delay(1000).Wait();
                        useNOB.副本進入完成 = true;
                        break;
                    }
                    mErrorCheck++;
                    if (mErrorCheck > 60)
                    {
                        mErrorCheck = 0;
                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(200).Wait();
                        useNOB.KeyPress(VKeys.KEY_F11);
                        Task.Delay(1000).Wait();
                        movePress = 0;
                    }
                }
                useNOB.目前動作 = "入場完成";
            }
        }

        private void 離開副本()
        {
            var useNOB = mUseNOB;
            Debug.WriteLine($"{useNOB != null} 離開副本 " + useNOB?.PlayerName);
            if (useNOB != null)
            {
                useNOB.副本離開完成 = false;
                useNOB.目前動作 = $"尋找NPC對話.. 離開副本 -> {useNOB.副本離開完成}";
                int x = 0;
                int y = 0;
                while (MainWindow.CodeRun)
                {
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = $"出去中 {useNOB.StateA} {useNOB.MAPID} {useNOB.副本離開完成}";
                    Debug.WriteLine($"{出場NPCID} - {x} {useNOB.PosX} {y} {useNOB.PosY}");
                    if ((x > 0 && y > 0) && (useNOB.PosX != x && useNOB.PosY != y))
                    {
                        useNOB.副本離開完成 = true;
                        break;
                    }

                    if (useNOB.GetTargetIDINT() == 出場NPCID && useNOB.對話與結束戰鬥)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            useNOB.KeyPress(VKeys.KEY_J);
                            Task.Delay(100).Wait();
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }

                        x = useNOB.PosX;
                        y = useNOB.PosY;
                    }
                    else
                    {
                        useNOB!.MoveToNPC(出場NPCID);
                    }
                }
                useNOB.目前動作 = "出場完成";
            }
        }

    }
}
