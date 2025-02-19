using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 刷熊本城 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 接任務NPCID = 0;
        int cache地圖 = 7800; //白石 地圖 -10015
        int talkID_1 = 0;
        /// <summary>
        /// 加藤
        /// </summary>
        int checkIDC1 = 16;

        int checkIDC2 = 21;     //酒井
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
                        int findCheck = 0;
                        //if (false)
                        {

                            MainNob.KeyPress(VKeys.KEY_W, 3);
                            Task.Delay(500).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                            //foreach (var nob in FIDList)
                            //{
                            //    if (nob != null)
                            //    {
                            //        nob.KeyPress(VKeys.KEY_F8, 2);
                            //        Task.Delay(50).Wait();
                            //    }
                            //}
                            ////清正說話

                            while (MainWindow.CodeRun)
                            {
                                talkID_1 = 顏色尋目標前往(checkIDC1, E_TargetColor.藍NPC);

                                if (MainNob.GetTargetIDINT() == talkID_1 && MainNob.對話與結束戰鬥)
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 20);
                                    Task.Delay(100);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    break;
                                }
                            }
                            目標地移動(new List<座標> {
                                new(26822 , 69544),
                                new(28117 , 70814),
                                new(32852 , 68236),
                                new(35503 , 64971),
                                new(39108, 64867),
                                new(42870, 63634),
                                new(43623, 57812),
                                new(44953, 57837),
                             });

                            ////開門
                            確認開門(new 座標(44953, 57837));
                            //打小兵
                            目標地移動(new List<座標> {
                                new(43354, 58750)
                             });
                            尋找並清除目標(24, 1, E_TargetColor.紅NPC);
                            //打完小兵去開門
                            目標地移動(new List<座標> {
                                new(43354, 58750),
                                new(44953, 57837),
                             });
                            Task.Delay(2000).Wait();
                            確認開門(new 座標(44953, 57837));
                            //開門後往下一個
                            目標地移動(new List<座標> {
                                new(46012, 57369),
                                new(44436, 56047),
                                new(44245, 52467),
                                new(46303, 52218),
                                new(46166, 48907),
                                new(41183, 46482),
                                new(29686, 45318),
                             });
                            //點門
                            確認開門(new 座標(29686, 45318));
                            //前往目標點
                            目標地移動(new List<座標> {
                                new(35332, 44644),
                                new(42790, 44558),
                                new(44900, 37793),
                                new(41117, 33248),
                                new(40977, 30944),
                             });
                            //11 藍 飯田
                            尋找並清除目標(11, 1, E_TargetColor.藍NPC);
                            //回到第二門點
                            目標地移動(new List<座標> {
                                new(40977, 30944),
                                new(41117, 33248),
                                new(44900, 37793),
                                new(42790, 44558),
                                new(35332, 44644),
                                new(29686, 45318),
                             });
                            確認開門(new 座標(29686, 45318));

                            //第三階段
                            目標地移動(new List<座標> {
                                new(24900, 44654),
                                new(25012, 27852),
                                new(42942, 27872),
                             });
                            確認開門(new 座標(42942, 27872));


                            //找NPC 森本 -24
                            尋找並清除目標(24, 1, E_TargetColor.藍NPC);
                            //打完回去 點開門
                            目標地移動(new List<座標> {
                                new(42942, 27872),
                             });
                            確認開門(new 座標(42942, 27872));
                            //走道這邊找結束
                            目標地移動(new List<座標> {
                                new(46857, 29237),
                             });
                            //第一次對話 清正
                            while (MainWindow.CodeRun)
                            {
                                talkID_1 = 顏色尋目標前往(checkIDC1, E_TargetColor.藍NPC);

                                if (MainNob.GetTargetIDINT() == talkID_1 && MainNob.對話與結束戰鬥)
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 20);
                                    Task.Delay(100);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    break;
                                }
                            }
                            Task.Delay(2000).Wait();
                            while (MainWindow.CodeRun)
                            {
                                talkID_1 = 顏色尋目標前往(checkIDC1, E_TargetColor.藍NPC);
                                if (MainNob.GetTargetIDINT() == talkID_1 && MainNob.對話與結束戰鬥)
                                {
                                    cache地圖 = MainNob.MAPID;
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 20);
                                    Task.Delay(100);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    break;
                                }
                            }
                            //等待切換地點
                            while (CodeRun)
                            {
                                MainNob.目前動作 = $"等待切換地點 A:{cache地圖} - B:{MainNob.MAPID}";
                                if (cache地圖 == MainNob.MAPID)
                                    Task.Delay(500).Wait();
                                else
                                {
                                    break;
                                }
                            }
                            Task.Delay(1000).Wait();
                            cache地圖 = MainNob.MAPID;
                            尋找並清除目標(checkIDC1, 1, E_TargetColor.藍NPC);
                            Task.Delay(1000).Wait();
                            Point = 檢查點.出場;
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
                                        Debug.WriteLine($"離開 尋找信長 = {talkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}");
                                        MainNob.目前動作 = $"離開 尋找信長 = {talkID_1} | {MainNob.GetTargetIDINT()} | {MainNob.對話與結束戰鬥}";
                                        if (MainNob.GetTargetIDINT() == talkID_1 && MainNob.對話與結束戰鬥)
                                        {
                                            MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                                            Task.Delay(100);
                                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                            break;
                                        }
                                        else
                                        {
                                            MainNob.MoveToNPC(talkID_1);
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
            if (MainWindow.CodeRun && useNOB != null)
            {
                Debug.WriteLine("接任務 " + useNOB!.PlayerName);
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
                        useNOB.直向選擇(2);
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER, 7, 200);
                        break;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        if (useNOB.出現直式選單)
                        {
                            Task.Delay(100).Wait();

                            if (useNOB.取得最下面選項().Contains("聽說"))
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
