using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 生產破魔 : BaseClass
    {
        int mPoint = 0;
        int mSpBuy = 0;
        int mSpBuyNum = 0;
        public override void 初始化()
        {
            //MainWindow.dmSoft!.WriteString(NobTeam[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F11, 1, "／自動移動:NPC");
            //MainWindow.dmSoft!.WriteString(NobTeam[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:GOM");
            mPoint = 0;
            if (MainNob != null)
                MainNob.選擇目標類型(7);
        }
        public override void 腳本運作()
        {
            if (MainNob == null)
                return;
            SetClickThrough(true);
            if (mPoint == 0)
                採購物品();
            else
                製作();
        }
        void 製作()
        {
            if (MainNob == null)
                return;
            int tryCheck = 0;
            int stepIndex = 0;
            int workindex = 0;
            while (MainNob.StartRunCode)
            {
                if (stepIndex == 0)
                {
                    if (tryCheck == 0)
                    {
                        MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCD");
                        Task.Delay(100).Wait();
                        MainNob.KeyPress(VKeys.KEY_F12);
                        Task.Delay(10000).Wait();
                    }

                    if (MainNob.有觀察對象)
                    {
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                        MainNob.KeyPress(VKeys.KEY_ENTER, 10, 300);
                        Task.Delay(200).Wait();
                        if (workindex == 0)
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER, 8500, 20);
                            workindex++;
                        }
                        else if (workindex == 1)
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER, 12000, 20);
                            workindex++;
                        }
                        else if (workindex == 2)
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER, 14000, 20);
                            stepIndex = 1;
                        }
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                    }

                    tryCheck++;
                    if (tryCheck > 10)
                    {
                        tryCheck = 0;
                    }
                    Task.Delay(200).Wait();
                }
                else if (stepIndex == 1)
                {
                    MainNob.MoveToNPC(MainNob.CodeSetting.目標E);
                    if (MainNob.對話與結束戰鬥)
                    {
                        var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(100, 240), new System.Drawing.Point(35, 70), "FFFFFF");
                        if (cr == 9)
                        {
                            Task.Delay(200).Wait();
                            //點賣
                            MainNob.ML_Click(225, 225);

                            Task.Delay(200).Wait();
                            MainNob.KeyPress(VKeys.KEY_L, 10);
                            for (int i = 0; i < 35; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_ENTER, 2, 100);
                            }
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 5, 200);
                            MainNob.KeyPress(VKeys.KEY_ENTER);
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 5, 200);
                            break;
                        }
                        else
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER);
                        }
                    }
                }
                Task.Delay(50).Wait();
            }
            mPoint = 0;

            mSpBuy++;
        }
        void 採購物品()
        {
            mSpBuyNum = mSpBuy % 3 == 0 ? 7 : 5;
            if (MainNob == null)
                return;
            int tryCheck = 0;
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCA");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(10000).Wait();
                }
                MainNob.MoveToNPC(MainNob.CodeSetting.目標A);
                if (MainNob.對話與結束戰鬥)
                {
                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
                    if (cr == 9)
                    {
                        //點買
                        Task.Delay(200).Wait();
                        MainNob.ML_Click(225, 205, 2);
                        Task.Delay(200).Wait();
                        MainNob.直向選擇(13);
                        for (int i = 0; i < 5; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                            Task.Delay(200).Wait();
                        }
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                        break;
                    }
                    else
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                }
                tryCheck++;
                if (tryCheck > 10)
                {
                    tryCheck = 0;
                }
                Task.Delay(200).Wait();
            }
            tryCheck = 0;
            int buyIndex = 0;
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCB");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(15000).Wait();
                }
                MainNob.MoveToNPC(buyIndex == 0 ? MainNob.CodeSetting.目標B : MainNob.CodeSetting.目標C);
                if (MainNob.對話與結束戰鬥)
                {
                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
                    if (cr == 9)
                    {
                        //點買
                        Task.Delay(200).Wait();
                        MainNob.ML_Click(225, 205, 2);
                        Task.Delay(200).Wait();
                        if (buyIndex == 0)
                        {
                            MainNob.直向選擇(5);
                            for (int i = 0; i < 5; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                                Task.Delay(200).Wait();
                            }
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            MainNob.直向選擇(8);
                            for (int i = 0; i < 2; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                                Task.Delay(200).Wait();
                            }

                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                            tryCheck = 0;
                            Task.Delay(500).Wait();
                        }
                        else
                        {
                            MainNob.直向選擇(1);
                            for (int i = 0; i < mSpBuyNum; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                                MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                                Task.Delay(200).Wait();
                            }

                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                        }
                        buyIndex++;
                        if (buyIndex >= 2)
                            break;
                    }
                    else
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                }
                tryCheck++;
                if (tryCheck > 10)
                {
                    tryCheck = 0;
                }
                Task.Delay(200).Wait();
            }
            tryCheck = 0;
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCC");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(10000).Wait();
                }
                MainNob.MoveToNPC(MainNob.CodeSetting.目標D);
                if (MainNob.對話與結束戰鬥)
                {
                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
                    if (cr == 9)
                    {
                        //點買
                        Task.Delay(200).Wait();
                        MainNob.ML_Click(225, 205, 2);
                        Task.Delay(200).Wait();
                        MainNob.直向選擇(8);
                        Task.Delay(500).Wait();
                        for (int i = 0; i < mSpBuyNum; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                            Task.Delay(200).Wait();
                        }
                        Task.Delay(200).Wait();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        Task.Delay(200).Wait();
                        MainNob.直向選擇(14);
                        for (int i = 0; i < (mSpBuy % 4 == 0 ? 12 : 9); i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 3, 200);
                            Task.Delay(200).Wait();
                        }

                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                        break;
                    }
                    else
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                }
                tryCheck++;
                if (tryCheck > 10)
                {
                    tryCheck = 0;
                }
                Task.Delay(200).Wait();
            }
            mPoint = 1;
        }
    }
}
