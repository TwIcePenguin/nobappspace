using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 夢幻城 : BaseClass
    {
        int anyDoCheck = 0;
        int mPoint = 0;
        int checkRoomIndex = 0;
        int mCheckbattleCount = -1;

        List<long> skipID = new();
        bool 進行任務 = false;
        public override void 初始化()
        {
            MainNob.Log("夢幻城 最先運作");
            MainNob!.KeyPress(VKeys.KEY_W);
            MainNob!.選擇目標類型(1);
            for (int i = 0; i < FIDList.Count; i++)
            {
                FIDList[i].選擇目標類型(1);
            }
        }

        public override async void 腳本運作()
        {
            if (MainNob != null)
            {
                if (mCheckbattleCount > 3)
                {
                    MainNob.Log($"{mCheckbattleCount} - 結算中");
                    mCheckbattleCount = -1;
                    anyDoCheck = 0;
                    Task.Run(MainNob.離開戰鬥A).Wait();
                    return;
                }
                if (MainNob.戰鬥中)
                {
                    MainNob.Log("戰鬥中");
                    anyDoCheck = 0;
                    findNPCCheck = 0;
                    mCheckbattleCount = 0;
                    Task.Delay(500).Wait();
                    return;
                }
                if (mCheckbattleCount >= 0)
                {
                    mCheckbattleCount++;
                    MainNob.Log("戰鬥中");
                    Task.Delay(500).Wait();
                    return;
                }

                MainNob.Log($"Point:{mPoint} resetPoint:{MainNob!.ResetPoint} Ready:{MainNob.準備完成} InNext:{準備進入下一階段}");
                if (MainNob!.ResetPoint)
                {
                    MainNob.Log("ResetPoint Done");
                    MainNob.ResetPoint = false;
                    進行任務 = false;
                    mPoint = 0;
                    return;
                }

                if (MainNob!.出現直式選單)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        選擇();
                    }
                    MainNob.Log($"完成該樓");
                    foreach (var item in NobTeams)
                    {
                        item.Log($"ResetPoint");
                        item.ResetPoint = true;
                    }
                    //進樓梯的人 直接完成
                    準備進入下一階段 = false;
                    MainNob.ResetPoint = false;
                    mPoint = 0;
                    Task.Delay(3000).Wait();
                }

                if (mPoint <= 7 && 準備進入下一階段)
                {
                    if (Dis(MainNob.PosX, MainNob.PosY, 14986, 14281) < 6000)
                    {
                        MainNob.Log($"已經 到下一 層樓了");
                        //進樓梯的人 直接完成
                        準備進入下一階段 = false;
                        mPoint = 0;
                        Task.Delay(3000).Wait();
                    }

                    if (MainNob.待機)
                    {
                        MainNob.準備完成 = true;
                        return;
                    }
                    else
                    {
                        MainNob.準備完成 = false;
                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                        return;
                    }
                }

                MainNob.目前動作 = $"Idle:{MainNob.待機} P:{mPoint} Q:{進行任務} M:{MainNob.ResetPoint}";

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

                //沒做任何事情卡住了
                if (anyDoCheck > 5)
                {
                    MainNob.Log("沒做任何事情卡住了 -- ");
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
                    MainNob.Log("外部確認 拿完已經拿完寶箱 -- ");
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
            if (MainNob == null)
            {
                Debug.WriteLine("MainNob Is Null");
                return;
            }

            //十層
            if (mPoint == 0 && MainNob!.CamX > 0.97 && MainNob.CamX < 1.01 && MainNob.CamY < 0.05 && MainNob.CamY > -0.05)
            {
                MainNob.Log("進入十層階段");
                mPoint = 8;
            }

            移動點 = new List<座標>();
            switch (mPoint)
            {
                case 0:
                    MainNob.Log($"前往第一個門");
                    移動點.Add(new(12957, 14464)); //第一個門
                    break;
                case 1:
                    MainNob.Log($"前往第一房間出口");
                    移動點.Add(new(14986, 14281));
                    移動點.Add(new(14991, 12783)); //第一房間出口
                    break;
                case 2:
                    MainNob.Log($"前往第二個門");
                    移動點.Add(new(15011, 6542));  //第二個門
                    break;
                case 3:
                    MainNob.Log($"前往第二房間出口");
                    移動點.Add(new(14716, 4492)); //第二房間出口
                    移動點.Add(new(13072, 4493)); //第二房間出口
                    break;
                case 4:
                    MainNob.Log($"第三個門");
                    移動點.Add(new(7042, 4509));  //第三個門
                    break;
                case 5:
                    MainNob.Log($"第三房間出口");
                    移動點.Add(new(5051, 4770)); //第三房間出口
                    移動點.Add(new(5005, 6427)); //第三房間出口
                    break;
                case 6:
                    MainNob.Log($"第四個門");
                    移動點.Add(new(5016, 12457));  //第四個門
                    break;
                case 7:
                    MainNob.Log($"第四房間出口");
                    移動點.Add(new(5246, 14497));  //第四房間出口
                    移動點.Add(new(6937, 14484));  //第四房間出口
                    break;
            }

            if (移動到定點())
            {
                Task.Delay(100).Wait();
                int findCheck = 0;
                while (MainWindow.CodeRun && MainNob != null)
                {
                    if (mPoint == 8)
                    {
                        if (MainNob!.戰鬥中)
                            break;

                        while (CodeRun)
                        {
                            bool allDone = true;
                            foreach (var item in NobTeams)
                            {
                                if (item!.準備完成 == false)
                                {
                                    if (item.待機)
                                    {
                                        item.準備完成 = true;
                                    }

                                    item.Log("未完成");
                                    item.KeyPress(VKeys.KEY_ESCAPE);
                                    Task.Delay(500).Wait();
                                    allDone = false;
                                    break;

                                }
                            }
                            Task.Delay(200).Wait();
                            if (allDone)
                                break;
                        }
                        MainNob.Log("準備完成 前往下一樓");

                        if (MainNob!.出現直式選單)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                選擇();
                            }
                            MainNob.Log($"完成該樓");
                            foreach (var item in NobTeams)
                            {
                                item.Log($"ResetPoint");
                                item.ResetPoint = true;
                            }
                            //進樓梯的人 直接完成
                            準備進入下一階段 = false;
                            MainNob.ResetPoint = false;
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

                        if (Dis(MainNob.PosX, MainNob.PosY, 14986, 14281) < 6000)
                        {
                            MainNob.Log($"已經 到下一 層樓了");
                            //進樓梯的人 直接完成
                            準備進入下一階段 = false;
                            mPoint = 0;
                            Task.Delay(3000).Wait();
                            break;
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
                        //移動中出現 上下選單
                        if (MainNob.出現直式選單)
                        {
                            MainNob.Log($"移動其他樓層中出現選單->{MainNob.取得最下面選項}");

                            for (int i = 0; i < 3; i++)
                            {
                                選擇();
                            }
                            MainNob.Log($"完成該樓");
                            foreach (var item in NobTeams)
                            {
                                item.Log($"ResetPoint");
                                item.ResetPoint = true;
                            }
                            //進樓梯的人 直接完成
                            準備進入下一階段 = false;
                            MainNob.ResetPoint = false;
                            mPoint = 0;
                            Task.Delay(3000).Wait();
                            break;
                        }

                        if (MainNob.有觀察對象)
                        {
                            MainNob!.KeyPress(VKeys.KEY_ENTER, 7, 200);
                            進行任務 = mPoint == 0 || mPoint == 2 || mPoint == 4 || mPoint == 6;

                            MainNob.Log($"進行開門--> {mPoint}");

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
                                MainNob.Log($"重新找路徑 --> {mPoint}");
                                break;
                            }
                        }
                        Task.Delay(100).Wait();
                    }
                }

            }
            else
            {
                MainNob.Log($"Point {mPoint} 移動失敗 重新嘗試");
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
        int findNPCCheck = 0;
        //寶相 第四碼 FE
        void 尋找任務()
        {
            if (MainNob != null)
            {
                //  MainNob.Log("尋找任務" + checkRoomIndex);
                if (!hasBox)
                    MainNob.KeyPress(VKeys.KEY_J);
                Task.Delay(100).Wait();

                if (MainNob.GetTargetIDINT() != -1)
                {
                    //Check寶相
                    //  MainNob.Log("Class " + MainNob.GetTargetClass());
                    checkRoomIndex = 0;
                    hasBox = false;
                    if (!skipID.Contains(MainNob.GetTargetIDINT()) && MainNob.GetTargetClass() == 254)
                    {
                        MainNob.Log("找到寶相");
                        hasBox = true;
                    }

                    MainNob.MoveToNPC(MainNob.GetTargetIDINT());
                    Task.Delay(2000).Wait();
                    if (hasBox && MainNob.對話與結束戰鬥 || MainNob.出現左右選單)
                    {
                        findNPCCheck = 0;
                        MainNob.Log($"檢測寶相 準備打開");
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
                        findNPCCheck = 0;
                        MainNob.前進(100);
                        MainNob.MoveToNPC(MainNob.GetTargetIDINT());
                        Task.Delay(1000).Wait();
                    }

                    findNPCCheck++;
                    if (findNPCCheck > 3)
                    {
                        findNPCCheck = 0;
                        MainNob.KeyPressT(VKeys.KEY_Q, 600);
                        MainNob.KeyPress(VKeys.KEY_J);
                    }
                }
                else
                {
                    MainNob.KeyPressT(VKeys.KEY_Q, 600);
                    MainNob.KeyPress(VKeys.KEY_J);
                    checkRoomIndex++;
                    MainNob.Log($"確認房間狀態 {checkRoomIndex}");
                    if (checkRoomIndex > 8)
                    {
                        checkRoomIndex = 0;
                        進行任務 = false;
                        mPoint = mPoint + 1;
                        if (mPoint > 6)
                        {
                            //準備進入下一層
                            MainNob.準備完成 = true;
                            準備進入下一階段 = true;
                        }
                    }
                    Task.Delay(100).Wait();
                }
            }
        }
    }
}
