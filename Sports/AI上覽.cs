using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp.Sports
{
    internal class AI上覽 : BaseClass
    {
        public int mState = 0;
        public int 大黑天ID = 1610613226;
        public int 上覽小販 = 0;
        public int Point = 0;
        public int BattleNum = 0;
        public int 滿倉判別 = 0;
        public int 選擇難度 = 6;
        public int 線路 = 0;
        public bool 統計販賣戰鬥 = false;
        public bool 是否經過戰鬥 = false;
        public bool 是否進入場內 = false;
        int mBattleCheckDone = 0;
        private bool 入場正式NPC說話 = false;
        private int m進行官ID = 0;
        private int m目標ID = 0;
        private int m上一場目標ID = 0;
        private int mErrorCheck = 0;
        private int mTErrorCheck = 0;
        private int mENDCheck = 0;
        private int SPNUM = 0;
        private bool isSPVer = false;

        public override void 初始化()
        {
            統計販賣戰鬥 = false;
            mENDCheck = 0;
            if (MainNob != null)
            {
                大黑天ID = MainNob.CodeSetting.目標A;
                上覽小販 = MainNob.CodeSetting.目標B;
            }
        }

        public override void 腳本運作()
        {
            if (MainWindow.CodeRun && MainNob != null)
            {
                狀態刷新判斷2();
                switch (Point)
                {
                    case 0:
                        MainNob.目前動作 = $"{Point} - 上覽入場";
                        上覽入場();
                        break;
                    case 1:
                        if (MainNob.戰鬥中)
                        {
                            Task.Delay(500).Wait();
                            狀態刷新判斷();
                            return;
                        }
                        MainNob.目前動作 = $"{Point} - 尋找戰鬥";
                        尋找戰鬥();
                        break;
                    case 3:
                        MainNob.目前動作 = $"{Point} - 販賣武器";
                        販賣武器();
                        break;
                    default:
                        MainNob.目前動作 = $"{Point} {MainNob.MAPID} - 狀態刷新判斷";
                        狀態刷新判斷();
                        break;
                }
                Task.Delay(100).Wait();
            }
        }

        void 狀態刷新判斷2()
        {
            if (MainNob != null)
            {
                MainNob.目前動作 = $@"MAP : {MainNob.MAPID} {Point}";
                if (MainNob.MAPID > 3200 && MainNob.MAPID < 3500)
                {
                    if (Point < 2)
                        Point = 0;
                }
                if (MainNob.MAPID > 6300 && MainNob.MAPID < 6500)
                {
                    if (Point != 1)
                        Point = 1;
                }
            }
        }

        bool 狀態刷新判斷()
        {
            if (MainNob != null)
            {
                if (MainNob.MAPID > 3200 && MainNob.MAPID < 3500)
                {
                    Point = 0;
                    return false;
                }
                if (MainNob.MAPID > 6300 && MainNob.MAPID < 6500)
                {
                    if (MainNob.戰鬥中)
                    {
                        mBattleCheckDone = 0;
                        統計販賣戰鬥 = 是否經過戰鬥 = true;
                        Task.Delay(1000).Wait();
                        return false;
                    }

                    if (MainNob.結算中)
                    {
                        Task.Run(MainNob.離開戰鬥B).Wait();
                        return false;
                    }

                    return true;
                }

                MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 50);
            }

            return false;
        }

        void 尋找戰鬥()
        {
            if (MainNob != null)
            {
                Debug.WriteLine($"尋找戰鬥 {MainNob.GetSStatus} {m進行官ID} {m上一場目標ID} {m目標ID} MAP : {MainNob.MAPID}");

                MainNob.目前動作 = "尋找戰鬥:" + MainNob.GetSStatus + " : " + MainNob.MAPID;
                if (狀態刷新判斷())
                {
                    if (是否經過戰鬥)
                    {
                        if (m目標ID != 0)
                        {
                            m上一場目標ID = m目標ID;
                            MainWindow.AddNowFindNpcToSkip();
                        }
                        是否經過戰鬥 = false;
                        mErrorCheck = 0;
                        m目標ID = 0;

                        Task.Delay(500).Wait();
                    }

                    if (mErrorCheck > 15)
                    {
                        m目標ID = 0;
                        mErrorCheck = 0;
                        是否經過戰鬥 = false;

                        MainNob.KeyPress(VKeys.KEY_ENTER, 3);
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 4);
                    }

                    if (m進行官ID == 0)
                    {
                        MainNob.目前動作 = "搜尋 筆試官";
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                        尋找筆試官();
                    }
                    mErrorCheck++;
                    if (m目標ID != 0)
                    {
                        MainNob.目前動作 = $"有目標的狀態 找尋目標對戰中 {mErrorCheck}";
                        if (MainNob.對話與結束戰鬥)
                        {
                            Task.Delay(500).Wait();
                            if (MainNob.出現左右選單)
                            {
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_J);
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                            }
                            else
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            }
                        }
                        else
                        {
                            MainNob.MoveToNPC(m目標ID);
                            Task.Delay(400).Wait();
                        }
                        if (MainNob.出現直式選單 && MainNob.取得最下面選項().Contains("其他"))
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 3);
                            m目標ID = 0;
                        }
                    }
                    else
                    {
                        MainNob.目前動作 = $"前往 筆試官 {mErrorCheck}";
                        MainNob.MoveToNPC(m進行官ID);
                        Task.Delay(500).Wait();
                        if (MainNob.對話與結束戰鬥)
                        {
                            if (MainNob.出現直式選單)
                            {
                                if (MainNob.取得最下面選項().Contains("妙院"))
                                {
                                    Task.Delay(200).Wait();
                                    MainNob.直向選擇(線路);
                                    Task.Delay(200).Wait();
                                    for (int i = 0; i < 5; i++)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_J);
                                        Task.Delay(100).Wait();
                                        MainNob.KeyPress(VKeys.KEY_ENTER);
                                        Task.Delay(100).Wait();
                                    }
                                    return;
                                }
                                else
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 2, 100);
                                    Task.Delay(100).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 3, 100);
                                    Task.Delay(100).Wait();
                                    mErrorCheck = 0;
                                    尋找下一個目標ID();
                                    Task.Delay(500).Wait();
                                }
                            }
                            Task.Delay(200).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }
                    }

                    Task.Delay(100).Wait();
                }
            }
        }

        private void 尋找下一個目標ID()
        {
            if (MainNob != null)
            {
                // 找出符合條件的 NPC
                NPCData? nextTargetNPC = MainWindow.GetNPCWithMinID(0, 65534, m上一場目標ID);

                if (nextTargetNPC != null && nextTargetNPC.ID > m上一場目標ID && Math.Abs(nextTargetNPC.ID - m上一場目標ID) < 7)
                {
                    m目標ID = (int)nextTargetNPC.ID;
                    MainNob.MoveToNPC(m目標ID);
                }
            }
        }

        void 上覽入場()
        {
            if (MainNob != null)
            {
                Debug.WriteLine($"上覽入場中 {mErrorCheck} {BattleNum} MAP : {MainNob.MAPID}");
                MainNob.目前動作 = $"上覽入場中 {mErrorCheck} {BattleNum}  MAP : {MainNob.MAPID}";
                Task.Delay(300).Wait();
                if (統計販賣戰鬥)
                {
                    統計販賣戰鬥 = false;
                    BattleNum++;
                }
                if (BattleNum >= 10 || 滿倉判別 > 100)
                {
                    BattleNum = 0;
                    滿倉判別 = 0;
                    Point = 3;
                    mENDCheck++;

                    if (isSPVer)
                    {
                        SPNUM++;
                        if (SPNUM > 5)
                        {
                            var rand = new Random();
                            var iii = rand.Next(100);
                            if (iii + SPNUM > 60)
                            {
                                SPNUM = 0;
                                MainWindow.CodeRun = false;
                                MainNob.CloseGame();
                            }
                        }
                    }
                    return;
                }

                if (mENDCheck > 4)
                {
                    MainWindow.CodeRun = false;
                    MainNob.CloseGame();
                    MessageBox.Show($"{MainNob.PlayerName} 嘗試多次都沒有進入 戰局 多次卡住 自動關掉視窗 程式暫停避免其他問題");
                    Environment.Exit(0);
                }
                MainNob.MoveToNPC(大黑天ID);
                if (MainNob.出現直式選單)
                {
                    if (入場正式NPC說話 == false && MainNob.出現左右選單)
                    {
                        MainNob.KeyPress(VKeys.KEY_ESCAPE);
                    }

                    if (MainNob.取得最下面選項().Contains("交換"))
                    {
                        入場正式NPC說話 = true;
                        mTErrorCheck = 0;
                        滿倉判別 = 0;

                        MainNob.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(100).Wait();
                    }
                    if (MainNob.取得最下面選項().Contains("神魔"))
                    {
                        入場正式NPC說話 = true;
                        mTErrorCheck = 0;
                        滿倉判別 = 0;
                        MainNob.直向選擇(0);
                    }

                    var str = MainNob.取得最下面選項().Replace(" ", "");
                    if (str.Contains("甲") || str.Contains("乙") || str.Contains("丙") || str.Contains("丁") ||
                        str.Contains("戊") || str.Contains("己") || str.Contains("庚") || str.Contains("辛"))
                    {
                        MainNob.直向選擇(選擇難度);
                        Task.Delay(100).Wait();
                        MainNob.KeyPress(VKeys.KEY_ENTER, 10, 200);
                        Task.Delay(100).Wait();
                        Point = 1;
                        滿倉判別 = 0;
                        mTErrorCheck = 0;
                        mErrorCheck = 0;
                        m進行官ID = 0;
                        m目標ID = 0;
                        mENDCheck = 0;
                        MainWindow.skipNPCs.Clear();
                        入場正式NPC說話 = false;
                        是否經過戰鬥 = false;
                        Task.Delay(1000).Wait();
                    }
                }
                else if (MainNob.對話與結束戰鬥)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                }
                mTErrorCheck++;
                if (mTErrorCheck > 20)
                {
                    mTErrorCheck = 0;
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                    MainNob.KeyPress(VKeys.KEY_ENTER);
                }
                滿倉判別++;
            }
        }

        void 販賣武器()
        {
            do
            {
                if (MainNob != null)
                {
                    MainNob.MoveToNPC(上覽小販);
                    Task.Delay(100).Wait();
                    if (MainNob.出現直式選單)
                    {
                        if (MainNob.取得最下面選項().Contains("關於"))
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER, 100, 100);
                            Task.Delay(200).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 6, 100);
                            Task.Delay(200).Wait();

                            Point = 0;
                            BattleNum = 0;
                            滿倉判別 = 0;
                            break;
                        }
                        else
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }
                    }
                    else
                    {
                        Task.Delay(100).Wait();
                        if (MainNob.對話與結束戰鬥)
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                    }
                }
                else
                    break;
            }
            while (MainWindow.CodeRun);
        }

        void 尋找筆試官()
        {
            var minNpcData = MainWindow.GetNPCWithMaxID(65534, 65536);
            m進行官ID = (int)minNpcData.ID + 1;
            m目標ID = (int)minNpcData.ID - 1;
        }
    }
}
