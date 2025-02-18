using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 秀吉 : BaseClass
    {

        public int SetClass = 0;
        public int 秀吉ID = 1610616891;
        public int 目標ID = 1610613998;
        public int 起始地圖 = 6401;
        int 選擇關卡 = 8;       //柴田8   //伊勢9
        NOBDATA mUseNOB;

        public override void 初始化()
        {
            移動點 = new();
            //伊勢
            //移動點.Add(new(19478, 26294));
            //移動點.Add(new(17700, 28250));
            //移動點.Add(new(18128, 30768));
            //移動點.Add(new(21710, 30762));
            //移動點.Add(new(24191, 28885));
            //移動點.Add(new(27959, 34800));
            //移動點.Add(new(30900, 34970));
            //柴田
            移動點.Add(new(37888, 36436));
            移動點.Add(new(31830, 36638));
            移動點.Add(new(30237, 37377));
            移動點.Add(new(27766, 38138));

            //Task.Delay(200).Wait();
            //移動到定點();

            Point = 檢查點.未知;
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(7);
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
                        for (int i = 0; i < FIDList.Count; i++)
                        {
                            mUseNOB = FIDList[i];
                            if (i == FIDList.Count - 1)
                                Task.Run(入場).Wait();
                            else
                                Task.Run(入場);

                            Task.Delay(300).Wait();
                        }
                        Dictionary<NOBDATA, int> playErrorCheck = new();
                        while (MainWindow.CodeRun)
                        {
                            bool 全體離開確認 = true;
                            foreach (var nob in FIDList)
                            {
                                if (nob != null)
                                {
                                    if (nob.MAPID == 起始地圖)
                                    {
                                        if (playErrorCheck.ContainsKey(nob))
                                            playErrorCheck[nob]++;
                                        else
                                            playErrorCheck[nob] = 1;

                                        if (playErrorCheck[nob] > 100)
                                        {
                                            playErrorCheck[nob] = 0;
                                            mUseNOB = nob;
                                            Task.Run(入場);
                                            Task.Delay(300).Wait();
                                        }

                                        if (nob.目前動作.Contains("入場完成"))
                                        {
                                            nob.KeyPress(VKeys.KEY_W);
                                            Task.Delay(100).Wait();
                                            nob.KeyPress(VKeys.KEY_F8);
                                        }

                                        Task.Delay(200);
                                        全體離開確認 = false;
                                    }
                                }
                            }
                            if (全體離開確認)
                            {
                                playErrorCheck.Clear();
                                foreach (var nob in FIDList)
                                {
                                    if (nob != null)
                                    {
                                        nob.KeyPress(VKeys.KEY_F8);
                                        Task.Delay(50).Wait();
                                    }
                                }

                                Point = 檢查點.找目標;
                                break;
                            }
                            Task.Delay(500).Wait();
                        }

                        break;
                    case 檢查點.找目標:
                        Task.Delay(100).Wait();
                        for (int i = 0; i < 1; i++)
                        {
                            foreach (var nob in FIDList)
                            {
                                if (nob != null)
                                {
                                    nob.KeyPress(VKeys.KEY_F8);
                                    Task.Delay(50).Wait();
                                }
                            }
                        }
                        移動到定點();
                        bool isWaitDone = false;
                        for (int i = 0; i < 5; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(50).Wait();
                        }
                        while (MainWindow.CodeRun)
                        {
                            if (MainNob.對話與結束戰鬥)
                            {
                                MainNob.目前動作 = "目標ID:" + 目標ID;
                                if (MainNob.StateA.Contains("F0 F8"))
                                {
                                    while (MainWindow.CodeRun && isWaitDone == false)
                                    {
                                        bool allDone = true;
                                        foreach (var nob in FIDList)
                                        {
                                            if (nob.PlayerName.Contains(MainNob.PlayerName))
                                            {
                                                continue;
                                            }

                                            //有人未到點 等待
                                            if (Dis(nob.PosX, nob.PosY, MainNob.PosX, MainNob.PosY) > 400)
                                            {
                                                allDone = false;
                                                break;
                                            }
                                        }

                                        if (allDone)
                                        {
                                            isWaitDone = true;
                                            break;
                                        }
                                        else
                                            Task.Delay(300).Wait();
                                    }

                                    Task.Delay(200).Wait();
                                    MainNob.KeyPress(VKeys.KEY_J);
                                    Task.Delay(200).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                    Task.Delay(200).Wait();
                                }
                                else
                                {
                                    MainNob.MoveToNPC(目標ID);
                                    Task.Delay(200).Wait();
                                    if (MainNob.GetTargetIDINT() != 目標ID)
                                    {
                                        Debug.WriteLine("無法鎖定對象");
                                        Task.Delay(200).Wait();
                                    }
                                    if (MainNob.對話與結束戰鬥)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(100).Wait();
                                    }
                                }
                            }
                            else
                            {
                                MainNob.MoveToNPC(目標ID);
                            }

                            if (MainNob.戰鬥中)
                            {
                                Point = 檢查點.戰鬥中;
                                Task.Delay(300).Wait();
                                break;
                            }
                        }
                        break;
                    case 檢查點.戰鬥中:
                        Task.Delay(300).Wait();
                        int check = 0;
                        while (MainWindow.CodeRun)
                        {
                            if (MainNob.對話與結束戰鬥)
                            {
                                check++;
                                if (check > 5)
                                {
                                    Point = 檢查點.結束戰鬥;
                                    break;
                                }
                            }
                            else
                                check = 0;
                        }
                        break;
                    case 檢查點.結束戰鬥:
                        Task.Delay(300).Wait();
                        foreach (var nob in FIDList)
                        {
                            mUseNOB = nob;
                            mUseNOB.目前動作 = "離開戰鬥畫面";
                            Task.Run(離開戰鬥);
                            Task.Delay(300).Wait();
                        }
                        Task.Delay(300).Wait();
                        while (MainWindow.CodeRun)
                        {
                            bool allDone = true;

                            foreach (var nob in FIDList)
                            {
                                Task.Delay(100).Wait();
                                if (!nob.待機)
                                {
                                    nob.KeyPress(VKeys.KEY_ESCAPE);
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
                        break;
                    case 檢查點.出場:
                        //秀吉目前打完都是等待回去
                        while (MainWindow.CodeRun)
                        {
                            bool 全體離開確認 = true;
                            foreach (var nob in FIDList)
                            {
                                if (nob != null)
                                {
                                    if (nob.MAPID != 起始地圖)
                                    {
                                        if (nob.目前動作.Contains("離開完成"))
                                            nob.KeyPress(VKeys.KEY_W);
                                        全體離開確認 = false;
                                    }
                                    Task.Delay(200).Wait();
                                }
                            }
                            if (全體離開確認)
                            {
                                Point = 檢查點.入場;
                                Task.Delay(500).Wait();
                                break;
                            }
                            Task.Delay(500).Wait();
                        }
                        break;

                    case 檢查點.未知:
                    default:
                        MainWindow.MainState = "出現異常";
                        break;
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
                {
                    useNOB.目前動作 = "離開完成";
                    break;
                }
            }
            while (MainWindow.CodeRun);
        }
      
        private async Task 入場()
        {
            var useNOB = mUseNOB;

            Debug.WriteLine("入場 " + useNOB.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.目前動作 = "秀吉入場中..";
                bool m領獎完成 = false;
                bool isTaking = false;
                while (MainWindow.CodeRun)
                {
                    useNOB.MoveToNPC(秀吉ID);
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;
                    bool cstatus = useNOB.MAPID != 6401;
                    if (cstatus)
                    {
                        useNOB.目前動作 = "場內等待中";
                        Task.Delay(1000).Wait();
                        break;
                    }

                    if (useNOB.StateA.Contains("E0 F0") && isTaking)
                    {
                        isTaking = false;
                        Task.Delay(200).Wait();
                        useNOB.目前動作 = "隊長選擇關卡";
                        useNOB.直向選擇(選擇關卡);
                        Task.Delay(500).Wait();
                        for (int i = 0; i < 7; i++)
                        {
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(300).Wait();
                        }
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        bool hasTalk = false;
                        useNOB.MoveToNPC(秀吉ID);
                        if (useNOB.出現直式選單)
                        {
                            Task.Delay(100).Wait();
                            if (useNOB.取得最下面選項().Contains("自秀") && isTaking == false)
                            {
                                isTaking = true;
                                hasTalk = true;
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(300).Wait();
                            }

                            Task.Delay(300).Wait();
                            if (useNOB.取得最下面選項().Contains("山崎"))
                            {
                                hasTalk = true;
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(300).Wait();
                            }
                        }

                        if (hasTalk == false)
                        {
                            Task.Delay(100).Wait();
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                        }

                        mErrorCheck++;
                        if (mErrorCheck > 20)
                        {
                            Debug.WriteLine(" ErrorCheck ");
                            mErrorCheck = 0;
                            for (int i = 0; i < 5; i++)
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                                Task.Delay(200).Wait();
                            }
                        }
                    }
                }
                useNOB.目前動作 = "入場完成";
            }
        }
    }
}
