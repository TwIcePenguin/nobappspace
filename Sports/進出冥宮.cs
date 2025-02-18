using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 進出冥宮 : BaseClass
    {
        public int mState = 0;
        public int 大黑天ID = 0;
        public int 上覽小販 => 大黑天ID + 65;
        public int Point = 0;
        /// <summary>
        /// 計算戰鬥場次 10場販賣一次
        /// </summary>
        public int BattleNum = 0;
        public int 滿倉判別 = 0;
        public int 選擇難度 = 0;
        public bool 是否經過戰鬥 = false;
        public bool 是否進入場內 = false;
        private int m進行官ID = 0;
        private int m目標ID = 0;

        public override void 初始化() { }

        public override async void 腳本運作()
        {
            if (MainNob != null)
            {
                var index = MainNob.GetSStatus;
                switch (index)
                {
                    case 7:
                        //在最外面 - 找大黑天
                        Point = 0;

                        break;
                    case 9:
                        //
                        是否進入場內 = true;
                        if (是否經過戰鬥)
                        {

                        }
                        break;
                    case 25:
                        //戰鬥結束
                        MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        await Task.Delay(100);
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                        await Task.Delay(100);
                        break;
                    case 105:
                        //戰鬥中
                        是否經過戰鬥 = true;
                        await Task.Delay(1000);
                        break;
                    default:
                        for (int i = 0; i < 10; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            await Task.Delay(50);
                        }
                        break;
                }
            }
        }

        private void 尋找下一個目標ID()
        {
            if (MainNob != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long tID = long.MaxValue;
                long minID = long.MinValue;
                long findID = 0;
                long diffID = 0;
                for (int i = 0; i < 32; i++)
                {
                    findID = MainWindow.dmSoft.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str, 4);
                    if (findID > minID)
                    {
                        minID = findID;
                    }
                    str = str.AddressAdd(12);
                }
                str = AddressData.搜尋身邊NPCID起始;
                for (int i = 0; i < 20; i++)
                {
                    findID = MainWindow.dmSoft.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str, 4);
                    if (minID > 0)
                        diffID = findID - minID;

                    if (findID > m進行官ID && tID > findID && Math.Abs(diffID) < 7)
                    {
                        tID = findID;
                    }

                    str = str.AddressAdd(12);
                }
                if (tID > 0)
                {
                    m目標ID = (int)tID;

                }
            }
        }
        private async void 上覽入場()
        {
            if (MainNob != null)
            {
                Task.Delay(100).Wait();
                if (是否經過戰鬥)
                {
                    是否經過戰鬥 = false;
                    BattleNum++;
                }
                if (BattleNum >= 10)
                {
                    BattleNum = 0;
                    滿倉判別 = 0;
                    Point = 3;
                    return;
                }
                MainNob.MoveToNPC(大黑天ID);
                if (MainNob.出現直式選單)
                {
                    if (MainNob.取得最下面選項().Contains("交換"))
                    {
                        滿倉判別 = 0;
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                        await Task.Delay(100);
                    }
                    var str = MainNob.取得最下面選項().Replace(" ", "");
                    if (str == "甲" || str == "乙" || str == "丙" || str == "丁" ||
                        str == "戊" || str == "己" || str == "庚" || str == "辛")
                    {
                        MainNob.直向選擇(選擇難度);
                        await Task.Delay(100);
                        for (int i = 0; i < 10; i++)
                        {
                            MainNob.KeyPress(VKeys.KEY_ENTER);
                            await Task.Delay(200);
                        }

                        Point = 1;
                        m進行官ID = 0;
                    }
                }
                else if (MainNob.對話與結束戰鬥)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                }
                滿倉判別++;
            }
        }

        private async void 販賣武器()
        {
            do
            {
                if (MainNob != null)
                {
                    MainNob.MoveToNPC(上覽小販);
                    await Task.Delay(100);
                    if (MainNob.出現直式選單)
                    {
                        if (MainNob.取得最下面選項().Contains("關於"))
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                await Task.Delay(100);
                            }
                            for (int i = 0; i < 6; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                await Task.Delay(100);
                            }
                            await Task.Delay(200);
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
                        await Task.Delay(100);
                        if (MainNob.對話與結束戰鬥)
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                    }
                }
                else
                    break;
            }
            while (true);
        }

    }
}
