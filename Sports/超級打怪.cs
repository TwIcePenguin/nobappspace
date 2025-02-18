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
    internal class 超級打怪 : BaseClass
    {
        bool inBattleState = false;
        static int checkDone = 0;
        static NOBDATA mUseNOB;
        public override void 初始化() { }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                Debug.WriteLine("StateA : " + MainNob.StateA);
                switch (MainNob.StateA)
                {
                    //戰鬥中
                    case "A0 98":
                        Task.Delay(1000).Wait();
                        inBattleState = true;
                        break;
                    //沒有任何視窗野外
                    case "F0 B8":
                        if (MainNob.CodeSetting.目標A != 0)
                        {
                            inBattleState = false;
                            MainNob.MoveToNPC(MainNob.CodeSetting.目標A);
                            Task.Delay(200).Wait();
                            MainNob.KeyPress(VKeys.KEY_W);
                        }
                        break;
                    //開寶 出現對話框 戰鬥結束
                    case "F0 F8":
                        if (inBattleState)
                        {
                            Debug.WriteLine("inBattleState : ---");
                            inBattleState = false;
                            checkDone = 0;
                            foreach (var nob in FIDList)
                            {
                                mUseNOB = nob;
                                new Thread(離開戰鬥).Start();
                                Task.Delay(200).Wait();
                            }
                            while (MainWindow.CodeRun && checkDone > FIDList.Count)
                            {
                                Task.Delay(200).Wait();
                                break;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("F0 F8 : ---");
                            Task.Delay(200).Wait();
                            MainNob.KeyPress(VKeys.KEY_J);
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(100).Wait();
                        }
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
                    break;
            }
            while (MainWindow.CodeRun);

            checkDone++;
        }
    }
}
