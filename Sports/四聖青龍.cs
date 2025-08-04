using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 四聖青龍 : BaseClass
    {
        NOBDATA? mUseNOB;
        int cache地圖 = 7800;

        public override void 初始化()
        {
            移動點 = new();

            int mPoint = MainNob.CodeSetting.MPoint;
            switch (mPoint)
            {
                case 0:
                    Point = 檢查點.入場;
                    break;
                case 1:
                default:
                    Point = 檢查點.找目標;
                    break;
            }
            //Point = 檢查點.入場;
            for (int i = 0; i < NobTeam.Count; i++)
            {
                NobTeam[i].選擇目標類型(1);
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
                Debug.WriteLine($"Point--> {Point}");
                switch (Point)
                {
                    case 檢查點.入場:
                        {
                            //mUseNOB = MainNob;
                            //Task.Run(接任務);
                            if (MainNob.對話與結束戰鬥)
                            {
                                Task.Delay(200).Wait();
                                if (MainNob.出現左右選單)
                                {
                                    Debug.WriteLine("出現左右選單");
                                    MainNob.KeyPress(VKeys.KEY_J);
                                    Task.Delay(200).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(200).Wait();
                                    Point = 檢查點.找目標;
                                    Task.Delay(2000).Wait();
                                }
                                else
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(200).Wait();
                                }
                            }
                            else
                            {
                                Debug.WriteLine("自動移動-->");
                                MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCA");
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_F12);
                                Task.Delay(5000).Wait();
                                MainNob.MoveToNPC2(MainNob.CodeSetting.目標A);
                            }

                        }
                        break;
                    case 檢查點.找目標:
                        Task.Delay(100).Wait();
                        //if (false)
                        {
                            MainNob.KeyPress(VKeys.KEY_W, 3);
                            foreach (var nob in NobTeam)
                            {
                                if (nob != null)
                                {
                                    nob.KeyPress(VKeys.KEY_F8, 2);
                                    Task.Delay(50).Wait();
                                }
                            }

                            //四聖
                            移動點.Clear();

                            移動點.Add(new(5979, 21, 13975));
                            移動點.Add(new(5951, 601, 16082));
                            移動點.Add(new(3836, 601, 15689));
                            移動點.Add(new(3621, 926, 13590));
                            移動點.Add(new(2774, 926, 13866));
                            移動點.Add(new(3004, 1201, 16689));

                            移動點.Add(new(10342, 631, 16991));
                            移動點.Add(new(12989, 601, 19011));
                            移動點.Add(new(19150, 601, 18718));
                            移動點.Add(new(19146, 926, 13623));
                            移動點.Add(new(18098, 926, 13677));
                            移動點.Add(new(18305, 1201, 18774));
                            移動點.Add(new(7096, 1526, 18238));
                            移動點.Add(new(7070, 1526, 19429));
                            移動點.Add(new(19429, 2401, 19168));
                            移動點.Add(new(18970, 2401, 9390));

                            移動點.Add(new(16209, 2401, 9461));
                            移動點.Add(new(16091, 1801, 3000));
                            移動點.Add(new(8448, 1801, 3696));
                            移動點.Add(new(3701, 1801, 3763));
                            移動點.Add(new(3634, 1801, 6242));
                            移動點.Add(new(2693, 1801, 5718));

                            移動點.Add(new(2856, 2401, 3619));
                            移動點.Add(new(3869, 2401, 3785));
                            移動點.Add(new(3845, 2401, 5824));
                            移動點.Add(new(1724, 2401, 5919));
                            移動點.Add(new(1683, 2401, 936));
                            移動點.Add(new(820, 2401, 873));

                            移動點.Add(new(820, 1801, 6399));
                            移動點.Add(new(1412, 1801, 6409));
                            移動點.Add(new(1228, 1201, 18801));
                            移動點.Add(new(6366, 1526, 18968));
                            移動點.Add(new(6320, 1526, 18302));
                            移動點.Add(new(822, 1801, 18279));

                            移動點.Add(new(1314, 1801, 14075));
                            移動點.Add(new(3965, 1801, 14263));
                            移動點.Add(new(4285, 1801, 17177));
                            移動點.Add(new(6369, 2126, 17258));
                            移動點.Add(new(6302, 2126, 16133));
                            移動點.Add(new(3970, 2401, 16300));
                            //移動點.Add(new(3687, 2401, 14229));
                            //移動點.Add(new(2140, 2401, 11052));
                            移動點.Add(new(1726, 2401, 8485));
                            移動點.Add(new(1460, 2400, 9860));
                            try
                            {
                                bool ec = 移動到定點New();

                                if (!ec)
                                    Debug.WriteLine($"出現錯誤");
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.ToString());
                            }
                            Task.Delay(1000).Wait();
                            MainNob.Log("達到青龍定點");
                        }
                        int battleCheck = 0;
                        int battleIn = 0;
                        while (MainNob.StartRunCode)
                        {
                            if (MainNob.戰鬥中)
                            {
                                battleIn = 1;
                            }
                            if (MainNob.進入結算)
                            {
                                foreach (var item in NobTeam)
                                {
                                    Task.Run(item.離開戰鬥B);
                                }
                                Task.Delay(200).Wait();
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
                            }
                            if (MainNob.待機)
                            {
                                //等待戰鬥
                                Task.Delay(100).Wait();
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    Task.Delay(200).Wait();
                                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                                    //青龍
                                    if (c2 == 17)
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
                        Task.Delay(2000).Wait();
                        Point = 檢查點.出場;
                        break;
                    case 檢查點.出場:
                        int sIndex = 0;
                        int checkError = 0;
                        //離開
                        while (MainNob.StartRunCode)
                        {
                            Task.Delay(200).Wait();
                            if (sIndex == 0)
                            {
                                MainNob.KeyPress(VKeys.KEY_TAB, 2, 500);
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                sIndex = 1;
                                Task.Delay(500).Wait();
                                continue;
                            }

                            if (sIndex == 1 && MainNob.取得最下面選項().Contains("有效"))
                            {
                                Debug.WriteLine("有效");
                                checkError = 0;
                                MainNob.直向選擇ZC(0);
                                sIndex = 2;
                                Task.Delay(500).Wait();
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                Task.Delay(500).Wait();
                                MainNob.直向選擇ZC(6);
                                continue;
                            }


                            if (sIndex == 2 && MainNob.出現左右選單)
                            {
                                Debug.WriteLine("出現左右選單");
                                checkError = 0;
                                MainNob?.KeyPress(VKeys.KEY_J);
                                Task.Delay(300).Wait();
                                MainNob?.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(300).Wait();
                                Point = 檢查點.出場;
                                break;
                            }

                            if (checkError > 10)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                                checkError = 0;
                                sIndex = 0;
                            }
                            checkError++;
                            Task.Delay(500).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }

                        Point = 檢查點.入場;
                        Task.Delay(10000).Wait();
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
                useNOB.Log("接任務 " + useNOB?.PlayerName);
                useNOB.副本進入完成 = false;
                useNOB.目前動作 = "尋找NPC對話..";
                //入場對話
                int x = 0;
                int y = 0;
                while (MainNob != null && MainNob.StartRunCode)
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
                        useNOB.直向選擇(6);
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
                while (MainNob.StartRunCode)
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
