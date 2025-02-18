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
    internal class 刷大名物 : BaseClass
    {

        public int 藤岡屋ID = 1610615511;
        public int 目標ID = 0;
        // 0- F7 用掉道具
        public int Point = 1;
        static int CountDone = 0;
        public bool 已戰鬥 = false;
        static NOBDATA mUseNOB;

        bool canRun = true;

        public override void 初始化()
        {
            var npcName = MainWindow.dmSoft.ReadString(MainNob.Hwnd, "[[[<nobolHD.bng>+05C45568] + B0]+10]+154", 1, 15);
            Debug.WriteLine($"NpcName {npcName} ");

            //尋找目標NPCID();
            SPCheck();
        }

        public void 待機()
        {
            if (已戰鬥)
            {
                已戰鬥 = false;
                Point = 4;
            }
        }

        public void SPCheck()
        {
            if (MainNob != null)
            {
                Debug.WriteLine("GetSStatus : " + MainNob.GetSStatus);
                if (Point != 5)
                {
                    if (MainNob.GetSStatus == 91)
                    {
                        Point = 2;
                    }
                }

                if (MainNob.GetSStatus == 7)
                {
                    Point = 0;
                    目標ID = 0;
                }
                Task.Delay(500).Wait();
            }
        }

        public void 戰鬥中()
        {
            Point = 3;
            已戰鬥 = true;
        }

        public void 對話與結束戰鬥()
        {
            if (已戰鬥)
            {
                foreach (var nob in MainWindow.NobList)
                {
                    mUseNOB = nob;
                    new Thread(離開戰鬥).Start();
                    Task.Delay(300).Wait();
                }
            }
        }


        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                //Debug.WriteLine(@$"Point {Point} CountDone {CountDone}");
                switch (Point)
                {
                    case 0:
                        if (canRun)
                        {
                            canRun = false;
                            for (int i = 0; i < MainWindow.NobList.Count; i++)
                            {
                                mUseNOB = MainWindow.NobList[i];
                                new Thread(使用任務道具).Start();
                                Task.Delay(300).Wait();
                            }
                        }
                        if (CountDone >= MainWindow.NobList.Count)
                        {
                            CountDone = 0;
                            目標ID = 0;
                            Point = 1;
                            canRun = true;
                        }
                        break;
                    case 1:
                        if (canRun)
                        {
                            canRun = false;
                            mUseNOB = MainNob;
                            new Thread(點NPC進入).Start();
                            Debug.WriteLine($"Time {DateTime.Now}");
                            Task.Delay(7000).Wait();
                            Debug.WriteLine($"Time {DateTime.Now}");
                            for (int i = 0; i < MainWindow.NobList.Count; i++)
                            {
                                mUseNOB = MainWindow.NobList[i];

                                if (MainNob.PlayerName == mUseNOB.PlayerName)
                                {
                                    continue;
                                }
                                new Thread(點NPC進入).Start();
                                Task.Delay(300).Wait();
                            }
                        }
                        if (CountDone >= MainWindow.NobList.Count)
                        {
                            CountDone = 0;
                            Point = 99;
                            canRun = true;
                        }
                        break;
                    case 2:
                        if (目標ID == 0)
                        {

                        }
                        else
                        {
                            if (MainNob.對話與結束戰鬥)
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                            else
                                MainNob.MoveToNPC(目標ID);
                        }
                        Task.Delay(200).Wait();
                        SPCheck();
                        break;
                    case 3:
                        //戰鬥中
                        Task.Delay(500).Wait();
                        break;
                    case 4:
                        if (MainNob.對話與結束戰鬥)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(500).Wait();
                            }
                            Task.Delay(20000).Wait();
                            Point = 5;
                        }
                        else
                        {
                            if (目標ID == 0)
                            {

                            }
                            MainNob.MoveToNPC(目標ID);
                        }
                        MainNob.KeyPress(VKeys.KEY_W);
                        Task.Delay(300).Wait();
                        break;
                    case 99:
                    default:
                        SPCheck();
                        break;
                }
            }

        }

        void 離開戰鬥()
        {
            var useNOB = mUseNOB;
            for (int i = 0; i < 10; i++)
            {
                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                Task.Delay(100).Wait();
                useNOB.KeyPress(VKeys.KEY_ENTER);
                Task.Delay(100).Wait();
            }

        }
        void 使用任務道具()
        {
            var useNOB = mUseNOB;
            useNOB.KeyPress(VKeys.KEY_W);
            for (int j = 0; j < 10; j++)
            {
                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                Task.Delay(300).Wait();
            }
            Task.Delay(300).Wait();
            useNOB.KeyPress(VKeys.KEY_F7);
            Task.Delay(300).Wait();
            for (int j = 0; j < 5; j++)
            {
                useNOB.KeyPress(VKeys.KEY_ENTER);
                Task.Delay(300).Wait();
            }
            for (int j = 0; j < 5; j++)
            {
                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                Task.Delay(300).Wait();
            }
            CountDone++;
        }

        void 點NPC進入()
        {
            bool run = true;
            bool canCheck = false;
            var useNOB = mUseNOB;
            int errorCheck = 0;
            Debug.WriteLine($"useNOB {useNOB.PlayerName} 開始對話");
            do
            {
                if (useNOB.GetSStatus == 91)
                {
                    CountDone++;
                    run = false;
                    break;
                }

                if (useNOB.出現直式選單)
                {
                    if (useNOB.取得最下面選項().Contains("真田"))
                    {
                        useNOB.直向選擇(0);
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(300).Wait();
                    }
                    if (useNOB.取得最下面選項().Contains("不做"))
                    {
                        useNOB.直向選擇(0);
                        Task.Delay(300).Wait();
                        for (int j = 0; j < 10; j++)
                        {
                            useNOB.KeyPress(VKeys.KEY_J);
                            Task.Delay(200).Wait();
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                        for (int j = 0; j < 10; j++)
                        {
                            useNOB.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }
                    }
                    if (useNOB.取得最下面選項().Contains("查看"))
                    {
                        Debug.WriteLine("準備進入");
                        Task.Delay(500).Wait();
                        useNOB.KeyPress(VKeys.KEY_J);
                        Task.Delay(500).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(1000).Wait();
                        useNOB.直向選擇(12);
                        Task.Delay(500).Wait();
                        for (int j = 0; j < 7; j++)
                        {
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(500).Wait();
                        }
                        Task.Delay(300).Wait();
                        Debug.WriteLine($"useNOB {useNOB.PlayerName} 完成");
                        canCheck = true;
                    }
                    if (useNOB.取得最下面選項().Contains("移動"))
                    {
                        Task.Delay(300).Wait();
                        useNOB.直向選擇(5);
                        Task.Delay(500).Wait();
                        for (int j = 0; j < 5; j++)
                        {
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(300).Wait();
                        }
                        canCheck = true;
                    }

                    if (canCheck && useNOB.GetSStatus == 91)
                    {
                        CountDone++;
                        run = false;
                    }

                    Task.Delay(300).Wait();
                    useNOB.KeyPress(VKeys.KEY_ENTER);
                }
                else
                {
                    if (useNOB.對話與結束戰鬥)
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                    else
                        useNOB.MoveToNPC(藤岡屋ID);
                    Task.Delay(500).Wait();
                }

                errorCheck++;
                if (errorCheck > 15)
                {
                    errorCheck = 0;
                    for (int j = 0; j < 10; j++)
                    {
                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                        Task.Delay(200).Wait();
                    }
                }

            }
            while (run);
        }

        public void 最後運作()
        {
        }

        //public int 尋找目標NPCID()
        //{
        //    if (MainNob != null)
        //    {
        //        var str = AddressData.搜尋身邊NPCID起始;
        //        long maxID = long.MaxValue;
        //        long findID = 0;

        //        for (int i = 0; i < 32; i++)
        //        {
        //            findID = MainWindow.dmSoft.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str, 4);
        //            var idCheck = MainWindow.dmSoft.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2);
        //            Debug.WriteLine("idCheck : " + idCheck + " : " + findID);
        //            if (idCheck == 96)
        //            {
        //                MainNob.鎖定NPC((int)findID);
        //                Task.Delay(500).Wait();
        //                var npcName = MainWindow.dmSoft.ReadString(MainNob.Hwnd, "[[<nobolHD.bng>+05C45568] + 8C]+154", 1, 15);
        //                Debug.WriteLine($"1NpcName {npcName} findID:{findID}");
        //                if (npcName.Contains("明智秀滿"))
        //                {
        //                    Debug.WriteLine("Find Done--");
        //                    目標ID = (int)findID;
        //                    return (int)findID;
        //                }
        //                npcName = MainWindow.dmSoft.ReadString(MainNob.Hwnd, "[[[<nobolHD.bng>+05C45568] + A0] + 10 ]+154", 1, 15);
        //                Debug.WriteLine($"2NpcName {npcName} findID:{findID}");
        //                if (npcName.Contains("明智秀滿"))
        //                {
        //                    Debug.WriteLine("Find Done--");
        //                    目標ID = (int)findID;
        //                    return (int)findID;
        //                }
        //                //maxID = findID;
        //            }
        //            str = str.AddressAdd(12);
        //        }
        //        //Debug.WriteLine("minID : " + maxID);
        //        return 0;
        //    }
        //    return 0;
        //}

    }
}
