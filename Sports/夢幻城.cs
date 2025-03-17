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
    internal class 夢幻城 : BaseClass
    {

        int anyDoCheck = 0;
        int mPoint = 0;
        int checkRoomIndex = 0;
        List<long> skipID = new();
        bool 進行任務 = false;
        public override void 初始化()
        {
            Debug.WriteLine("夢幻城 最先運作");
            MainNob!.KeyPress(VKeys.KEY_W);
            MainNob!.選擇目標類型(1);
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(1);
            }
        }

        public void 對話與結束戰鬥()
        {
            if (MainNob != null)
            {
                Debug.WriteLine("夢幻城 最先運作");
                Task.Delay(100).Wait();
                bool anyDo = false;
                if (MainNob.出現直式選單)
                {
                    anyDo = true;
                    for (int i = 0; i < 4; i++)
                    {
                        選擇();
                    }
                }

                if (MainNob.出現左右選單)
                {
                    anyDo = true;
                    選擇();
                }

                if (!anyDo)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE);
                }
            }
        }

        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                //Debug.WriteLine($" Point : {mPoint}");
                MainNob.目前動作 = $"P:{mPoint}Q:{進行任務}";
                if (進行任務)
                {
                    if (MainNob.待機)
                    {
                        anyDoCheck = 0;
                        尋找任務();
                    }

                    if (MainNob.對話與結束戰鬥)
                    {
                        if (hasBox == false)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                MainNob!.KeyPress(VKeys.KEY_J);
                                Task.Delay(300).Wait();
                                MainNob!.KeyPress(VKeys.KEY_ENTER);
                            }
                        }
                        尋找任務();
                    }
                }

                if (MainNob.待機 && 進行任務 == false)
                {
                    boxGetCheck = 0;
                    anyDoCheck = 0;
                    skipID.Clear();
                    hasBox = false;
                    夢幻成移動();
                }

                if (MainNob.結算中)
                {
                    anyDoCheck = 0;
                    Task.Run(MainNob.離開戰鬥B).Wait();
                }

                if (MainNob.戰鬥中)
                {
                    anyDoCheck = 0;
                }

                //沒做任何事情卡住了
                if (anyDoCheck > 5)
                {
                    Debug.WriteLine("沒做任何事情卡住了 -- ");
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                }
                if (hasBox && boxGetCheck > 10)
                {
                    while (CodeRun)
                    {
                        MainNob.MoveToNPC(MainNob.GetTargetIDINT());
                        Task.Delay(1000).Wait();

                        if (MainNob.出現左右選單)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                MainNob!.KeyPress(VKeys.KEY_J);
                                Task.Delay(300).Wait();
                                MainNob!.KeyPress(VKeys.KEY_ENTER);
                            }
                            break;
                        }
                        Task.Delay(300).Wait();
                    }
                    Debug.WriteLine("外部確認 拿完已經拿完寶相");
                    boxGetCheck = 0;
                    進行任務 = false;
                    hasBox = false;
                    mPoint = mPoint + 1;
                }

                if (hasBox)
                {
                    boxGetCheck++;
                }
                anyDoCheck++;
                Task.Delay(100).Wait();
            }
        }

        public void 夢幻成移動()
        {
            //十層
            //Debug.WriteLine($"{MainNob!.CamX} {MainNob.CamY}");
            if (mPoint == 0 && MainNob!.CamX > 0.97 && MainNob.CamX < 1.01 && MainNob.CamY < 0.05 && MainNob.CamY > -0.05)
            {
                Debug.WriteLine("進入十層階段");
                mPoint = 8;
            }

            移動點 = new List<座標>();
            switch (mPoint)
            {
                case 0:
                    Debug.WriteLine($"前往第一個門");
                    移動點.Add(new(12957, 14464)); //第一個門
                    break;
                case 1:
                    Debug.WriteLine($"前往第一房間出口");
                    移動點.Add(new(14986, 14281));
                    移動點.Add(new(14991, 12783)); //第一房間出口
                    break;
                case 2:
                    Debug.WriteLine($"前往第二個門");
                    移動點.Add(new(15011, 6542));  //第二個門
                    break;
                case 3:
                    Debug.WriteLine($"前往第二房間出口");
                    移動點.Add(new(14716, 4492)); //第二房間出口
                    移動點.Add(new(13072, 4493)); //第二房間出口
                    break;
                case 4:
                    Debug.WriteLine($"第三個門");
                    移動點.Add(new(7042, 4509));  //第三個門
                    break;
                case 5:
                    Debug.WriteLine($"第三房間出口");
                    移動點.Add(new(5051, 4770)); //第三房間出口
                    移動點.Add(new(5005, 6427)); //第三房間出口
                    break;
                case 6:
                    Debug.WriteLine($"第四個門");
                    移動點.Add(new(5016, 12457));  //第四個門
                    break;
                case 7:
                    Debug.WriteLine($"第四房間出口");
                    移動點.Add(new(5246, 14497));  //第四房間出口
                    移動點.Add(new(6937, 14484));  //第四房間出口
                    break;
            }

            移動到定點();
            Task.Delay(100).Wait();
            int findCheck = 0;
            while (MainWindow.CodeRun)
            {
                //Debug.WriteLine($"{mPoint} - 觀察對象 : {MainNob!.有觀察對象} : {MainNob.觀察對象Str}");
                if (mPoint == 8)
                {
                    if (MainNob!.戰鬥中)
                        break;

                    if (MainNob!.出現直式選單)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            選擇();
                        }
                        Debug.WriteLine("完成 該樓");
                        mPoint = 0;
                        Task.Delay(3000).Wait();
                        break;
                    }

                    if (MainNob!.出現左右選單)
                    {
                        //開箱
                        Task.Delay(200).Wait();
                        選擇();
                        Task.Delay(200).Wait();
                        MainNob.前進(200);
                    }

                    if (MainNob!.出現直式選單 == false && MainNob!.出現左右選單 == false)
                    {
                        MainNob.前進(200);
                        MainNob?.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(400).Wait();
                    }
                    Task.Delay(100).Wait();
                }
                else
                {
                    if (MainNob!.有觀察對象)
                    {
                        MainNob!.KeyPress(VKeys.KEY_ENTER, 7, 200);
                        進行任務 = mPoint == 0 || mPoint == 2 || mPoint == 4 || mPoint == 6;

                        Debug.WriteLine($"進行開門-->{mPoint}");

                        if (進行任務 == false)
                            mPoint = mPoint + 1;
                        else
                            MainNob.前進(1000);
                        break;
                    }
                    else
                    {
                        MainNob.前進(100);
                        findCheck++;
                        if (findCheck > 10)
                        {
                            Debug.WriteLine("重新找路徑");
                            break;
                        }
                    }
                    Task.Delay(100).Wait();
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
            }
        }

        bool hasBox = false;
        int boxGetCheck = 0;
        //寶相 第四碼 FE
        void 尋找任務()
        {
            if (MainNob != null)
            {
                //Debug.WriteLine("尋找任務" + checkRoomIndex);
                MainNob.KeyPress(VKeys.KEY_J);
                Task.Delay(100).Wait();
                if (MainNob.GetTargetIDINT() != -1)
                {
                    //Check寶相
                    //Debug.WriteLine("Class " + MainNob.GetTargetClass());
                    checkRoomIndex = 0;
                    hasBox = false;
                    if (!skipID.Contains(MainNob.GetTargetIDINT()) && MainNob.GetTargetClass() == 254)
                    {
                        Debug.WriteLine("找到寶相");
                        hasBox = true;
                    }

                    MainNob.MoveToNPC(MainNob.GetTargetIDINT());
                    Task.Delay(1000).Wait();
                    if (hasBox && MainNob.對話與結束戰鬥 || MainNob.出現左右選單)
                    {
                        Debug.WriteLine($"檢測寶相 準備打開");
                        for (int i = 0; i < 3; i++)
                        {
                            MainNob!.KeyPress(VKeys.KEY_J);
                            Task.Delay(300).Wait();
                            MainNob!.KeyPress(VKeys.KEY_ENTER);
                        }
                        MainNob!.KeyPress(VKeys.KEY_ESCAPE, 3);
                        if (!skipID.Contains(MainNob.GetTargetIDINT()))
                            skipID.Add(MainNob.GetTargetIDINT());
                        checkRoomIndex = 0;
                        進行任務 = false;
                        mPoint = mPoint + 1;
                    }
                    else if (hasBox)
                    {
                        MainNob.前進(100);
                        MainNob.MoveToNPC(MainNob.GetTargetIDINT());
                        Task.Delay(1000).Wait();
                    }
                }
                else
                {
                    MainNob.KeyPressT(VKeys.KEY_Q, 600);
                    MainNob.KeyPress(VKeys.KEY_J);
                    checkRoomIndex++;
                    if (checkRoomIndex > 8)
                    {
                        checkRoomIndex = 0;
                        進行任務 = false;
                        mPoint = mPoint + 1;
                    }
                    Task.Delay(100).Wait();
                }
            }
        }
    }
}
