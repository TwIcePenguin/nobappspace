using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 討伐2025_今川氏真 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 接任務NPCID = 0;
        int cache地圖 = 7800; //白石 地圖 -10015
        int talkID_1 = 0;
        int checkIDC1 = 15;     //松平
        int checkIDC2 = 21;     //今川
        int checkIDC3 = 41;     //水滴

        public override void 初始化()
        {
            移動點 = new();

            Point = 檢查點.入場;
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(1);
                //MainWindow.dmSoft!.WriteString(FIDList[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F11, 1, "／自動移動:NPC");
                //MainWindow.dmSoft!.WriteString(FIDList[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:GOM");
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
                                if (尋找目標並對話(checkIDC1, E_TargetColor.藍NPC, ref talkID_1))
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 20);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    break;
                                }
                                Task.Delay(50).Wait();
                            }

                            移動點.Clear();

                            移動點.Add(new(22700, 43024));
                            移動點.Add(new(22662, 40514));
                            移動點.Add(new(27412, 40630));

                            移動點.Add(new(32815, 40420));
                            移動點.Add(new(32880, 38212));
                            移動點.Add(new(32352, 35967));
                            移動點.Add(new(33621, 33463));

                            移動到定點();
                        }
                        ///BOSS 今川
                        尋找並清除目標(checkIDC2, 1, E_TargetColor.藍NPC);

                        Task.Delay(300).Wait();
                        MainNob.KeyPress(VKeys.KEY_W, 3);
                        //原地找NPC 回到 回報處 找最後的回報NPC
                        {
                            bool talking = false; // 初始化 talking 變數，用於追蹤是否正在對話中
                            while (MainWindow.CodeRun)
                            {
                                if (talking) // 如果目前正在對話中
                                {
                                    if (MainNob.出現左右選單) // 檢查是否出現左右選單
                                    {
                                        MainNob.KeyPress(VKeys.KEY_J);
                                        MainNob.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(1000).Wait(); // 等待 1 秒

                                        MainNob.鎖定NPC(talkID_1); // 嘗試鎖定目標 NPC (talkID_1 變數應在外部定義和賦值)
                                        if (MainNob.GetTargetIDINT() == talkID_1) // 確認是否成功鎖定目標 NPC
                                        {
                                            MainNob.MoveToNPC(talkID_1); // 移動到目標 NPC 附近
                                            Task.Delay(500); // 短暫等待
                                            break; // 如果成功鎖定並移動，則跳出迴圈，結束對話流程
                                        }
                                        else
                                        {
                                            talking = false; // 如果鎖定目標失敗，設定 talking 為 false，可能需要重新尋找目標或處理錯誤
                                        }
                                    }
                                    else // 如果沒有出現左右選單，表示可能在對話中，需要繼續按 Enter 鍵
                                    {
                                        MainNob.KeyPress(VKeys.KEY_ENTER); // 模擬按下 Enter 鍵 (繼續對話)
                                    }
                                }
                                else // 如果目前不在對話中 (talking 為 false)
                                {
                                    talking = 尋找目標並對話(checkIDC3, E_TargetColor.藍NPC); // 呼叫 尋找目標並對話 函數，嘗試開始對話
                                    continue; // 無論 尋找目標並對話 結果如何，都繼續下一次迴圈迭代 (如果成功開始對話，talking 會變為 true，下次迴圈會進入 if (talking) 區塊)
                                }
                                Task.Delay(50).Wait(); // 迴圈末尾的短暫延遲
                            }
                        }
                        //信長對話
                        Task.Delay(2000).Wait();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 3);
                        Task.Delay(1000).Wait();
                        Point = 檢查點.出場;
                        break;
                    case 檢查點.出場:
                        {
                            //目前打完 等待回去
                            while (MainWindow.CodeRun)
                            {
                                Task.Delay(50).Wait();
                                if (talkID_1 == -1)
                                {
                                    尋找目標並對話(checkIDC1, E_TargetColor.藍NPC, ref talkID_1);
                                    continue;
                                }

                                //  MainNob.Log($"離開 尋找信長 = {talkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}");
                                MainNob.目前動作 = $"離開 尋找信長 = {talkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}";
                                if (MainNob.GetTargetIDINT() == talkID_1 && MainNob.對話與結束戰鬥)
                                {
                                    cache地圖 = MainNob.MAPID;
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                                    Task.Delay(100);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    int checkTimeOut = 0;
                                    while (CodeRun)
                                    {
                                        bool done = true;
                                        foreach (var nob in FIDList)
                                        {
                                            nob.目前動作 = $"副本離開 -> {nob.MAPID} -> {cache地圖}";
                                            if (nob.MAPID == cache地圖)
                                            {
                                                done = false;
                                                break;
                                            }
                                        }
                                        if (done)
                                        {
                                            Point = 檢查點.入場;
                                            break;
                                        }

                                        checkTimeOut = checkTimeOut + 1;
                                        if (checkTimeOut > 20)
                                        {
                                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                            checkTimeOut = 0;
                                            break;
                                        }
                                        Task.Delay(500).Wait();
                                    }

                                    if (Point == 檢查點.入場)
                                        break;
                                }
                                else
                                {
                                    MainNob.MoveToNPC(talkID_1);
                                    Task.Delay(500).Wait();
                                }
                            }
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
            int mErrorCheck = 0;
            if (MainWindow.CodeRun && useNOB != null)
            {
                  MainNob.Log("接任務 " + useNOB!.PlayerName);
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
                        useNOB.直向選擇(5);
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
                                useNOB.直向選擇(1);
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
                              MainNob.Log(" ErrorCheck ");
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
