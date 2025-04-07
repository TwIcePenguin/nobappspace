using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 戰場製炮 : BaseClass
    {
        public List<int> skipCID = new();
        int mPoint = 0;
        int mCachePoint = 0;
        public int mErrorCheck = 0;
        public override void 初始化()
        {
            mPoint = 0;
            if (MainNob != null)
            {
                if (MainNob.CodeSetting.使用定位點)
                {
                    移動點 = new();
                    移動點.Add(new(MainNob.CodeSetting.定位點X, MainNob.CodeSetting.定位點Y));
                }
                MainNob.選擇目標類型(7);
            }
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                  MainNob.Log(" - " + MainNob.PlayerName + " : " + mPoint + " : " + MainNob.CodeSetting.目標A);
                if (mPoint == 0)
                {
                    MainNob.目前動作 = mPoint + "前往目";
                    MainNob.MoveToNPC(MainNob.CodeSetting.目標A);
                    Task.Delay(200).Wait();
                    if (MainNob.出現左右選單)
                    {
                        MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        Task.Delay(100).Wait();
                        MainNob.KeyPress(VKeys.KEY_ENTER);
                    }
                    if (MainNob.出現直式選單)
                    {
                        if (MainNob.取得最下面選項().Contains("進行") || MainNob.取得最下面選項().Contains("合戰"))
                        {
                            MainNob.直向選擇(1);
                        }
                        if (MainNob.取得最下面選項().Contains("關於"))
                        {
                            MainNob.直向選擇(4);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 5);

                            if (MainNob.CodeSetting.後退時間 > 0)
                            {
                                MainNob.後退(MainNob.CodeSetting.後退時間);
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_C);
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                MainNob.MoveToNPC(MainNob.CodeSetting.目標B);
                                Task.Delay(100).Wait();
                            }
                            Task.Delay(2900).Wait();
                            MainNob.目前動作 = "前往砲";
                            mPoint = 1;
                        }
                        else
                        {
                            if (MainNob.出現直式選單)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                Task.Delay(100).Wait();
                            }
                        }
                    }
                    else
                    {
                        if (MainNob.對話與結束戰鬥)
                        {
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                            Task.Delay(200).Wait();
                        }
                    }
                }

                if (mPoint == 1)
                {
                    MainNob.目前動作 = mPoint + "前往砲";
                    MainNob.MoveToNPC(MainNob.CodeSetting.目標B);
                    Task.Delay(200).Wait();
                    if (MainNob.出現左右選單)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_J, 2);
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_ENTER, 2);
                        }
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 4);
                        Task.Delay(100).Wait();
                        MainNob.後退(3000);
                        Task.Delay(100).Wait();
                        MainNob.KeyPress(VKeys.KEY_C);
                        //MainNob.StartRunCode = false;
                        mPoint = 0;
                        MainNob.目前動作 = "前往目";
                        return;
                    }

                    if (MainNob.出現直式選單)
                    {
                        if (MainNob.取得最下面選項().Contains("大砲"))
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE, 3);
                            }
                            skipCID.Add(MainNob.CodeSetting.目標B);
                            mPoint = 2;
                        }
                        return;
                    }
                }

                if (mPoint == 2)
                {
                    MainNob.目前動作 = MainNob.CodeSetting.目標C + " : " + mPoint + "產生新砲 重新定義新砲";
                    if (MainNob.CodeSetting.目標C != -1)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            MainNob.MoveToNPC(MainNob.CodeSetting.目標C);
                            Task.Delay(500).Wait();
                        }
                        MainNob.KeyPress(VKeys.KEY_ENTER, 5);
                        Task.Delay(300).Wait();
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);

                    }
                    skipCID.Add(MainNob.CodeSetting.目標C);
                    skipCID.Add(MainNob.CodeSetting.目標B);
                    //  MainNob.Log("*----*");
                    var ccid = MainNob.CodeSetting.目標B;
                    尋找下一門砲();
                    Task.Delay(100).Wait();
                    if (MainNob.CodeSetting.目標B != ccid)
                    {
                        MainNob.KeyPressT(VKeys.KEY_D, 500);

                        if (MainNob.CodeSetting.使用定位點)
                        {
                            Task.Delay(1000).Wait();
                            移動到定點();
                        }
                        mPoint = 0;
                    }
                }

                if (mCachePoint == mPoint)
                {
                    mErrorCheck++;
                    Task.Delay(200).Wait();
                    if (mErrorCheck > 80)
                    {
                        mErrorCheck = 0;
                        if (MainNob.CodeSetting.使用定位點)
                            移動到定點();
                        mPoint = 0;
                        mCachePoint = mPoint;
                    }
                }
                else
                {
                    mErrorCheck = 0;
                    mCachePoint = mPoint;
                }
            }
        }

        void 尋找下一門砲()
        {
            if (MainNob != null)
            {
                var str = AddressData.搜尋身邊NPCID起始;
                long findID, chid;

                List<long> skipIDs = new();
                for (int i = 0; i < 150; i++)
                {
                    findID = MainWindow.dmSoft?.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str, 4) ?? 0;
                    chid = MainWindow.dmSoft?.ReadInt(MainNob.Hwnd, "<nobolHD.bng> + " + str.AddressAdd(3), 2) ?? 0;
                    if (chid != 254 || findID == -1)
                    {
                        skipIDs.Add(findID);
                        str = str.AddressAdd(12); continue;
                    }
                    if (skipCID.Contains((int)findID) || skipIDs.Contains(findID)) { str = str.AddressAdd(12); continue; }

                    skipIDs.Add(findID);

                    if (skipCID.Contains((int)findID) == false)
                    {
                        MainNob.CodeSetting.目標B = (int)findID;
                    }

                    str = str.AddressAdd(12);
                }
            }
        }
    }
}
