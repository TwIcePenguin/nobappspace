using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 討伐2025_白石 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 起始地圖 = 7800; //白石 地圖 -10015
        public int 接任務NPCID = 0;
        int 出場NPCID = 0;
        public override void 初始化()
        {
            移動點 = new();

            Point = 檢查點.入場;
            for (int i = 0; i < NobTeam.Count; i++)
            {
                NobTeam[i].WriteStringValue("<nobolHD.bng> + " + AddressData.快捷F11, 1, "／自動移動:NPC");
                NobTeam[i].WriteStringValue("<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:GOM");
                NobTeam[i].KeyPress(VKeys.KEY_F11);
            }
        }

        public override Task 腳本運作()
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
                        mUseNOB = MainNob;
                        Task.Run(接任務);
                          MainNob.Log($"---- {mUseNOB!.PlayerName}");
                        Task.Delay(5000).Wait();
                        for (int i = 0; i < NobTeam.Count; i++)
                        {
                            if (NobTeam[i].PlayerName.Contains(MainNob.PlayerName) == false)
                            {
                                mUseNOB = NobTeam[i];
                                mUseNOB.副本進入完成 = false;
                                Task.Run(接任務);
                                Task.Delay(500).Wait();
                            }
                        }
                        Task.Delay(500).Wait();
                        Dictionary<NOBDATA, int> playErrorCheck = new();
                        while (MainNob.StartRunCode)
                        {
                            bool done = true;

                            foreach (var nob in NobTeam)
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
                            foreach (var nob in NobTeam)
                            {
                                if (nob != null)
                                {
                                    nob.KeyPress(VKeys.KEY_F8, 2);
                                    Task.Delay(50).Wait();
                                }
                            }
                            //使用道具
                            while (MainNob.StartRunCode)
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
                            while (MainNob.StartRunCode)
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
                            移動點.Add(new(44984, 28604));
                            移動點.Add(new(45243, 30404));
                            移動點.Add(new(39549, 31225));
                            移動點.Add(new(37013, 28806));
                            移動點.Add(new(32294, 29443));
                            移動點.Add(new(32201, 33837));
                            移動到定點();
                        }
                        //需打兩個小兵
                        int battleCheck = 0;
                        int checkBattle = 0;
                        bool 離開結算 = false;
                        while (MainNob.StartRunCode)
                        {
                            if (MainNob.戰鬥中)
                            {
                                離開結算 = false;
                            }
                            if (MainNob.進入結算)
                            {
                                if (離開結算 == false)
                                {
                                    checkBattle = checkBattle + 6;
                                    離開結算 = true;
                                    foreach (var item in NobTeam)
                                    {
                                        Task.Run(item.離開戰鬥B);
                                    }
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
                                離開結算 = false;
                                //等待戰鬥
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(50, 150), "565ABD");
                                    var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(50, 130), "47ADE8");
                                    if (c1 == 8 && cr > 30)
                                    {
                                        battleCheck = 0;
                                        MainNob!.KeyPress(VKeys.KEY_C);
                                        Task.Delay(100).Wait();
                                        MainNob!.MoveToNPC(MainNob.GetTargetIDINT());
                                        Task.Delay(200).Wait();
                                    }
                                    else
                                    {
                                        MainNob!.KeyPress(VKeys.KEY_J);
                                        Task.Delay(200).Wait();
                                        MainNob.KeyPressT(VKeys.KEY_Q, 600);
                                        battleCheck = battleCheck + 1;
                                    }
                                }
                                else
                                {
                                    MainNob!.KeyPress(VKeys.KEY_J);
                                    Task.Delay(200).Wait();
                                    MainNob.KeyPressT(VKeys.KEY_Q, 500);
                                    battleCheck = battleCheck + 1;
                                }
                                if (battleCheck > 20 - checkBattle)
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
                        移動點.Clear();
                        移動點.Add(new(32315, 38548));
                        移動點.Add(new(29654, 38908));
                        移動到定點();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                        離開結算 = false;
                        int battleIn = 0;
                        while (MainNob.StartRunCode)
                        {
                            if (MainNob.戰鬥中)
                            {
                                battleIn = 1;
                                離開結算 = false;
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
                                離開結算 = false;
                                //等待戰鬥
                                Task.Delay(100).Wait();
                                if (MainNob.GetTargetIDINT() != -1)
                                {
                                    Task.Delay(200).Wait();
                                    var c2 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(50, 130), "F6F67A");
                                    if (c2 == 9)
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
                                    MainNob!.KeyPress(VKeys.KEY_J);
                                    Task.Delay(200).Wait();
                                    battleCheck = battleCheck + 1;
                                }
                                if (battleIn == 1)
                                {
                                    battleIn = 0;
                                    Task.Delay(5000).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                                    Point = 檢查點.出場;
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
                                    foreach (var nob in NobTeam)
                                    {
                                        nob.副本離開完成 = false;
                                        mUseNOB = nob;
                                        Task.Run(離開副本);
                                        Task.Delay(500).Wait();
                                    }
                                    Task.Delay(300).Wait();
                                    while (MainNob.StartRunCode)
                                    {
                                        bool done = true;

                                        foreach (var nob in NobTeam)
                                        {
                                              MainNob.Log($"{nob.PlayerName} 副本離開完成 -> {nob.副本離開完成}");
                                            if (nob.副本離開完成 == false)
                                            {
                                                done = false;
                                                break;
                                            }
                                        }
                                        if (done)
                                            break;
                                        Task.Delay(500).Wait();
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

            return base.腳本運作();
        }

        private void 接任務()
        {
            var useNOB = mUseNOB;
              MainNob.Log("接任務 " + useNOB!.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.副本進入完成 = false;
                useNOB.目前動作 = "尋找NPC對話..";
                int movePress = 0;
                bool isOK = false;
                while (MainNob.StartRunCode)
                {
                    if (movePress % 50 == 0)
                    {
                        useNOB.KeyPress(VKeys.KEY_F11);
                    }
                    movePress++;
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;
                    if (useNOB.StateA.Contains("E0 F0") && isOK)
                    {
                        Task.Delay(500).Wait();
                        useNOB.直向選擇(3);
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER, 7, 200);
                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 15, 100);
                        break;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        if (useNOB.出現左右選單)
                        {
                            Task.Delay(200).Wait();
                            useNOB!.KeyPress(VKeys.KEY_J);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
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
                            }
                            if (useNOB.取得最下面選項().Contains("故事") ||
                                useNOB.取得最下面選項().Contains("攻城"))
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
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }

                }
                movePress = 0;
                while (MainNob.StartRunCode)
                {
                    if (movePress % 50 == 0)
                    {
                        useNOB.KeyPress(VKeys.KEY_F12);
                    }
                    else
                    {
                        if (!MainNob!.PlayerName.Contains(useNOB.PlayerName) && MainNob.副本進入完成 == false)
                        {
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
                }
                useNOB.目前動作 = "入場完成";
            }
        }

        private void 離開副本()
        {
            var useNOB = mUseNOB;
              MainNob.Log("離開副本 " + useNOB!.PlayerName);
            if (useNOB != null)
            {
                useNOB.副本離開完成 = false;
                useNOB.目前動作 = $"尋找NPC對話.. 離開副本 -> {useNOB.副本離開完成}";
                int x = 0;
                int y = 0;
                while (MainNob.StartRunCode)
                {
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = $"出去中 {useNOB.StateA} {useNOB.MAPID} {useNOB.副本離開完成}";

                    if (x > 0 && y > 0 && useNOB.PosX != x && useNOB.PosY != y)
                    {
                        useNOB.副本離開完成 = true;
                        break;
                    }

                    if (useNOB.GetTargetIDINT() == 出場NPCID && useNOB.對話與結束戰鬥)
                    {
                        x = useNOB.PosX;
                        y = useNOB.PosY;
                        for (int i = 0; i < 7; i++)
                        {
                            useNOB.KeyPress(VKeys.KEY_J);
                            Task.Delay(100).Wait();
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        x = 0;
                        y = 0;
                        useNOB!.MoveToNPC(出場NPCID);
                    }
                }
                useNOB.目前動作 = "出場完成";
            }
        }

    }
}
