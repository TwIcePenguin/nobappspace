using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 生產剛破 : BaseClass
    {
        int mPoint = 0;
        int mSpBuy = 0;
        int mSpBuyNum = 0;
        public override void 初始化()
        {
            if (MainNob != null)
            {
                MainNob.選擇目標類型(7);
                mPoint = MainNob.CodeSetting.MPoint;
            }
        }
        public override void 腳本運作()
        {
            if (MainNob == null)
                return;
            SetClickThrough(true);
            switch (mPoint)
            {
                default:
                case 0:
                    採購物品();
                    break;
                case 1:
                    製作();
                    break;
                case 2:
                    販賣();
                    break;
            }
        }
        void 製作()
        {
            if (MainNob == null)
                return;
            int tryCheck = 0;
            int workindex = 0;
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCD");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(10000).Wait();
                }
                tryCheck++;
                if (MainNob.有觀察對象)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10, 300);
                    MainNob.KeyPress(VKeys.KEY_ENTER, 10, 300);
                    Task.Delay(200).Wait();
                    if (workindex == 0)
                    {
                        MainNob.KeyPress(VKeys.KEY_ENTER, 8000, 20);
                        workindex++;
                    }
                    else if (workindex == 1)
                    {
                        MainNob.KeyPress(VKeys.KEY_ENTER, 11000, 20);
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 20);
                        mPoint = 2;
                        break;
                    }
                    continue;
                }


                if (tryCheck > 10)
                {
                    tryCheck = 0;
                }
                Task.Delay(50).Wait();
            }

            mSpBuy++;
        }
        void 採購物品()
        {
            mSpBuyNum = /*mSpBuy % 3 == 0 ? 6 :*/ 5;
            if (MainNob == null)
                return;
            int cc = 0;
            int butDelay = 300;
            int tryCheck = 0;
            //npc A 
            //箭矢 10
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
                        for (int i = 0; i < 10; i++)
                        {
                            while (MainNob.StartRunCode)
                            {
                                cc = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(440, 260), new System.Drawing.Point(160, 130), "D5F1F1");

                                MainNob.直向選擇(9);
                                Task.Delay(50).Wait();
                                if (MainNob.輸入數量視窗 || cc == 33)
                                    break;
                            }
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 2, 200);
                            Task.Delay(butDelay).Wait();
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
            //npc B
            //銀箔 5 漆箔 7
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCB");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(10000).Wait();
                }
                MainNob.MoveToNPC(MainNob.CodeSetting.目標B);
                if (MainNob.對話與結束戰鬥)
                {
                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");

                    if (cr == 9)
                    {
                        //點買
                        Task.Delay(200).Wait();
                        MainNob.ML_Click(225, 205, 2);
                        Task.Delay(500).Wait();
                        for (int i = 0; i < mSpBuyNum; i++)
                        {
                            while (MainNob.StartRunCode)
                            {
                                cc = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(440, 260), new System.Drawing.Point(160, 130), "D5F1F1");

                                MainNob.直向選擇(8);
                                Task.Delay(50).Wait();
                                if (MainNob.輸入數量視窗 || cc == 33)
                                    break;
                            }
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 2, 200);
                            Task.Delay(butDelay).Wait();
                        }
                        Task.Delay(200).Wait();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        Task.Delay(200).Wait();
                        for (int i = 0; i < (mSpBuy % 4 == 0 ? 12 : 8); i++)
                        {
                            while (MainNob.StartRunCode)
                            {
                                cc = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(440, 260), new System.Drawing.Point(160, 130), "D5F1F1");

                                MainNob.直向選擇(14);
                                Task.Delay(50).Wait();
                                if (MainNob.輸入數量視窗 || cc == 33)
                                    break;
                            }
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 2, 200);
                            Task.Delay(butDelay).Wait();
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
            //npc C
            //銀 5
            while (MainNob.StartRunCode)
            {
                if (tryCheck == 0)
                {
                    MainWindow.dmSoft!.WriteString(MainNob.Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:NPCC");
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(10000).Wait();
                }
                MainNob.MoveToNPC(MainNob.CodeSetting.目標C);
                if (MainNob.對話與結束戰鬥)
                {
                    var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
                    if (cr == 9)
                    {
                        //點買
                        Task.Delay(200).Wait();
                        MainNob.ML_Click(225, 205, 2);
                        Task.Delay(200).Wait();
                        for (int i = 0; i < mSpBuyNum; i++)
                        {
                            while (MainNob.StartRunCode)
                            {
                                cc = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(440, 260), new System.Drawing.Point(160, 130), "D5F1F1");

                                MainNob.直向選擇(1);
                                Task.Delay(50).Wait();
                                if (MainNob.輸入數量視窗 || cc == 33)
                                    break;
                            }
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_J, 5, 100);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 2, 200);
                            Task.Delay(butDelay).Wait();
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
        void 販賣()
        {
            if (MainNob == null)
                return;

            MainNob.MoveToNPC(MainNob.CodeSetting.目標D);
            if (MainNob.對話與結束戰鬥)
            {
                var cr = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(200, 190), new System.Drawing.Point(45, 55), "FFFFFF");
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
                    mPoint = 0;
                }
                else
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                }
            }
        }
    }
}
