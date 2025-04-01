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
    internal class 冥宮 : BaseClass
    {
        private int mErrorCheck = 0;
        private int mMaxErrorCheck = 40;
        int inBattleState = 0;
        bool inBattle = false;
        public override void 初始化()
        {
              MainNob.Log("明宮 最先運作");
            MainNob?.KeyPress(VKeys.KEY_W);
            mErrorCheck = 0;
            for (int i = 0; i < NobTeam.Count; i++)
            {
                NobTeam[i].選擇目標類型(1);
            }
        }

        public void 戰鬥中()
        {
            MainNob!.目前動作 = "戰鬥中";
            mErrorCheck = 0;
            inBattleState = 0;
            inBattle = true;
            Task.Delay(500).Wait();
        }

        public void 對話與結束戰鬥()
        {
              MainNob.Log($"戰鬥結束 - {inBattle}");
            if (MainNob != null)
            {
                Task.Delay(200).Wait();
                if (inBattle)
                {
                    if (inBattleState > 3)
                    {
                        inBattle = false;
                        inBattleState = 0;
                        foreach (var nob in NobTeam)
                        {
                            if (nob != null)
                                Task.Run(nob.離開戰鬥B);
                        }
                        int escCheck = 0;
                        while (MainNob.StartRunCode)
                        {
                            bool allDoneCheck = true;

                            foreach (var nob in NobTeam)
                            {
                                if (nob.離開戰鬥確認 == false)
                                {
                                    allDoneCheck = false;
                                    if (escCheck > 10)
                                    {
                                        nob.KeyPress(VKeys.KEY_ESCAPE);
                                    }
                                }
                            }
                            escCheck = escCheck + 1;
                            MainNob!.目前動作 = "結算中 : " + allDoneCheck;
                            if (allDoneCheck)
                            {
                                MainWindow.MainState = "完成離開";
                                break;
                            }
                            else
                            {
                                MainWindow.MainState = "等待玩家離開";
                                Task.Delay(500).Wait();
                            }
                        }
                        inBattle = false;
                    }
                    else
                    {
                        inBattleState++;
                    }
                }

            }
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                if (MainNob.待機)
                {
                    inBattle = false;
                    MainNob.目前動作 = "待機";
                    bool allDone = true;
                    foreach (var nob in NobTeam)
                    {
                        if (nob.待機 == false)
                        {
                            nob.KeyDown(VKeys.KEY_ESCAPE);
                            Task.Delay(100).Wait();
                            allDone = false;
                        }
                    }

                    if (!allDone)
                    {
                        return;
                    }

                    MainNob.目前動作 = "前進" + mErrorCheck;
                    前進(300);
                    Task.Delay(200).Wait();
                    MainNob.KeyPress(VKeys.KEY_J);
                    Task.Delay(200).Wait();
                    MainNob.KeyPress(VKeys.KEY_ENTER);
                    mErrorCheck++;
                    if (mErrorCheck > mMaxErrorCheck)
                    {
                        mErrorCheck = 0;
                        後退(2000);
                        Task.Delay(200).Wait();
                        MainNob.KeyPress(VKeys.KEY_J);
                        Task.Delay(200).Wait();
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                    }
                }

                if (MainNob.戰鬥中)
                    戰鬥中();

                if (MainNob.對話與結束戰鬥)
                {
                    if (inBattle)
                        對話與結束戰鬥();
                    else
                    {
                        bool anyDo = false;
                        if (MainNob.出現直式選單)
                        {
                            mErrorCheck = 0;
                            anyDo = true;
                            for (int i = 0; i < 4; i++)
                            {
                                選擇();
                            }
                        }
                        if (MainNob.出現左右選單)
                        {
                            mErrorCheck = 0;
                            anyDo = true;
                            選擇();
                        }
                        if (!anyDo)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }
                    }
                }
            }
        }

        public void 選擇()
        {
            if (MainNob != null)
            {
                MainNob?.KeyPress(VKeys.KEY_J);
                Task.Delay(300).Wait();
                MainNob?.KeyPress(VKeys.KEY_ENTER);
                Task.Delay(300).Wait();
                前進(300);
                Task.Delay(200).Wait();
            }
        }

    }
}
