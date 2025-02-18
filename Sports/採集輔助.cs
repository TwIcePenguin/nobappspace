using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 採集輔助 : BaseClass
    {
        public int Get = 12;
        public int SupA = 3;
        public int SupB = 2;

        public override void 初始化()
        {
            MainNob!.選擇目標類型(7);
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                for (int i = 0; i < Get; i++)
                {
                    MainNob.KeyPress(VKeys.KEY_F12);
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_ENTER, 10, 200);
                    Task.Delay(100).Wait();
                }
                MainNob.KeyPress(VKeys.KEY_ENTER, 5, 200);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_F7);
                Task.Delay(1000).Wait();
                MainNob.KeyPress(VKeys.KEY_ENTER, 1, 200);
                for (int i = 0; i < SupA; i++)
                {
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_ENTER, 2, 500);
                    Task.Delay(100).Wait();
                }
                MainNob.KeyPress(VKeys.KEY_ESCAPE , 10);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_F7);
                Task.Delay(1200).Wait();
                MainNob.KeyPress(VKeys.KEY_ENTER, 1, 200);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_K, 1, 200);
                Task.Delay(300).Wait();
                for (int i = 0; i < SupB; i++)
                {
                    Task.Delay(100).Wait();
                    MainNob.KeyPress(VKeys.KEY_ENTER, 2, 500);
                    Task.Delay(100).Wait();
                }
                MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_F7);
                Task.Delay(1000).Wait();
                MainNob.KeyPress(VKeys.KEY_ENTER, 1, 200);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_I, 1, 200);
                Task.Delay(500).Wait();
                MainNob.KeyPress(VKeys.KEY_ESCAPE, 5, 200);
            }
        }

    }
}
