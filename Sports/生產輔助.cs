using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 生產輔助 : BaseClass
    {
        public int CreatNum = 80;
        public int SPoint = 0;
        public int SellNPC = 1610613420;
        public override void 初始化()
        {
            SPoint = 0;
            if (MainNob.CodeSetting.使用定位點)
            {
                移動點 = new();
                移動點.Add(new(MainNob.PosX, MainNob.PosY));
            }
            SellNPC = MainNob.CodeSetting.目標A;
        }
        public override void 腳本運作()
        {
            if (SPoint == 0)
            {
                if (MainNob!.CodeSetting.使用定位點)
                {
                    移動到定點();
                }
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_TAB, 1);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_ENTER, 1);
                Task.Delay(500).Wait();
                MainNob.直向選擇(4);
                MainNob.KeyPress(VKeys.KEY_ENTER, 500, 50);
                MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                Task.Delay(500).Wait();
                SPoint = 1;
            }

            if (SPoint == 1)
            {
                Task.Delay(500).Wait();
                MainNob.MoveToNPC(SellNPC);
                if (MainNob.對話與結束戰鬥)
                {
                    Task.Delay(500).Wait();
                    MainNob.KeyPress(VKeys.KEY_ENTER, 200);
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                    SPoint = 0;
                }
            }
        }

    }
}
