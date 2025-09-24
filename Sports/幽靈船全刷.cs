using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 幽靈船全刷 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int 對手目標ID = 0;
        public int 接任務NPCID = 0;
        static int 出場NPCID = 0;
        public int cacheMap = 0;
        public int inMaxLoopCount = 3;
        public int nowInCount = 0;
        public int nowClassCount = 0;
        public override void 初始化()
        {
            移動點 = new();

            Point = 檢查點.入場;
            for (int i = 0; i < NobTeam.Count; i++)
            {
                NobTeam[i].選擇目標類型(1);
                MainWindow.dmSoft!.WriteString(NobTeam[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F11, 1, "／自動移動:NPC");
                MainWindow.dmSoft!.WriteString(NobTeam[i].Hwnd, "<nobolHD.bng> + " + AddressData.快捷F12, 1, "／自動移動:GOM");
            }

            nowClassCount = MainNob?.CodeSetting.線路 ?? 0;
        }

        public override Task 腳本運作()
        {
            if (MainNob != null)
            {
                MainWindow.MainState = $"目前場次 {nowClassCount} {nowInCount} / {inMaxLoopCount}";
                switch (Point)
                {
                    case 檢查點.入場:
                        mUseNOB = MainNob;
                        Task.Run(接任務);
                        MainNob.Log($"---- {mUseNOB!.PlayerName}");
                        Task.Delay(5000).Wait();
                        for (int i = 0; i < NobTeam.Count; i++)
                        {
                            if (NobTeam[i].PlayerName.Contains(MainNob.PlayerName) == false)
                            {
                                mUseNOB = NobTeam[i];
                                mUseNOB.副本進入完成 = false;
                                Task.Run(接任務);
                                Task.Delay(500).Wait();
                            }
                        }
                        Task.Delay(500).Wait();
                        Dictionary<NOBDATA, int> playErrorCheck = new();
                        while (MainNob.StartRunCode)
                        {
                            bool done = true;

                            foreach (var nob in NobTeam)
                            {
                                if (nob.副本進入完成 == false)
                                {
                                    done = false;
                                    break;
                                }
                            }
                            if (done)
                                break;
                            Task.Delay(500).Wait();
                        }
                        Point = 檢查點.找目標;
                        break;
                    case 檢查點.找目標:
                        Task.Delay(100).Wait();
                        尋找目標();

                        if (nowClassCount == 7)
                        {
                            尋找目標並對話(對手目標ID, 5);
                            MainNob.KeyPress(VKeys.KEY_ENTER, 10);
                            NobMainCodePage.IgnoredIDs.Add(對手目標ID);
                            Task.Delay(100).Wait();

                            尋找並清除目標(9);
                        }
                        else
                        {
                            尋找並清除目標(new List<int> { 對手目標ID });
                        }

                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);
                        Point = 檢查點.出場;
                        break;
                    case 檢查點.出場:
                        mUseNOB = MainNob;
                        nowInCount++;
                        離開副本();
                        Point = 檢查點.入場;
                        break;

                    case 檢查點.未知:
                    default:
                        MainWindow.MainState = "出現異常";
                        break;
                }
            }

            return base.腳本運作();
        }

        void 尋找目標()
        {
            if (MainNob == null)
                return;

            int findCheck = 0;
            NobMainCodePage.NpcCountToRead = 30;
            var allNPCIDs = NobMainCodePage.GetFilteredNPCs(MainNob, TargetTypes.NPC, 0, 70000);
            對手目標ID = -1;
            出場NPCID = -1;
            while (MainNob != null && MainNob.StartRunCode)
            {
                foreach (var npc in allNPCIDs)
                {
                    MainNob!.鎖定NPC((int)npc.ID);
                    Task.Delay(300).Wait();
                    //32 九鬼水軍 九鬼13
                    var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                    if (對手目標ID == -1 && c1 > 0 && (c1 != 32 && c1 != 13))
                    {
                        MainNob.目前動作 += $"找到目標{npc.ID}";
                        對手目標ID = (int)npc.ID;
                    }
                    if (出場NPCID == -1 && (c1 == 32 || c1 == 13))
                    {
                        MainNob.目前動作 += $"出場NPCID{npc.ID}";
                        出場NPCID = (int)npc.ID;
                    }

                    NobMainCodePage.IgnoredIDs.Add((int)npc.ID);
                }
                if (對手目標ID != -1 && 出場NPCID != -1)
                {
                    MainNob.Log($"找到目標 {對手目標ID} 出場 {出場NPCID}");
                    break;
                }

                MainNob!.KeyPressT(VKeys.KEY_Q, 500);
                allNPCIDs = NobMainCodePage.GetFilteredNPCs(MainNob, TargetTypes.NPC, 0, 70000);
                findCheck++;
                if (findCheck > 6)
                {
                    findCheck = 0;
                    NobMainCodePage.IgnoredIDs.Clear();
                }
            }
        }

        private void 接任務()
        {
            var useNOB = mUseNOB;
            bool 領完獎勵 = false;
            MainNob.Log("接任務 " + useNOB!.PlayerName);
            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB.副本進入完成 = false;
                useNOB.目前動作 = "尋找NPC對話..";

                while (MainNob.StartRunCode)
                {
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = "入場中.." + useNOB.StateA;

                    if (useNOB.對話與結束戰鬥)
                    {
                        int nowID = useNOB.GetTargetIDINT();
                        //如果目標還沒鎖定
                        if (nowID != MainNob!.CodeSetting.目標A)
                        {
                            useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                        }
                        else
                        {
                            if (useNOB.出現左右選單)
                            {
                                Task.Delay(200).Wait();
                                useNOB!.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(200).Wait();
                                cacheMap = useNOB.MAPID;
                                break;
                            }
                            if (useNOB.出現直式選單)
                            {
                                Task.Delay(100).Wait();

                                if (useNOB.取得最下面選項(24).Contains("解放"))
                                {
                                    if (nowInCount >= inMaxLoopCount)
                                    {
                                        nowInCount = 0;
                                        nowClassCount++;
                                        if (nowClassCount >= 8)
                                        {
                                            MessageBox.Show($"{MainNob.PlayerName} 已經跑玩了喔");
                                            MainNob.StartRunCode = false;
                                            break;
                                        }
                                    }
                                    Task.Delay(200).Wait();
                                    useNOB.直向選擇(nowClassCount);
                                    Task.Delay(500).Wait();
                                    continue;
                                }

                                if (useNOB.取得最下面選項().Contains("說明"))
                                {
                                    if (領完獎勵)
                                    {
                                        useNOB.直向選擇(0);
                                        Task.Delay(300).Wait();
                                        continue;
                                    }
                                    else
                                    {
                                        useNOB.直向選擇(1);
                                        Task.Delay(300).Wait();
                                        useNOB.KeyPress(VKeys.KEY_ENTER, 5);
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                                        領完獎勵 = true;
                                        continue;
                                    }
                                }
                                MainNob.Log(useNOB.取得最下面選項());
                                if (useNOB.取得最下面選項(24).Contains("財寶"))
                                {
                                    useNOB.直向選擇(0);
                                    Task.Delay(300).Wait();
                                    continue;
                                }

                                Task.Delay(200).Wait();
                            }
                            else
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                            }
                        }
                        mErrorCheck++;
                        if (mErrorCheck > 20)
                        {
                            MainNob.Log(" ErrorCheck ");
                            mErrorCheck = 0;
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }

                }
                mErrorCheck = 0;
                while (MainNob.StartRunCode)
                {
                    Task.Delay(300).Wait();
                    if (useNOB.出現直式選單)
                    {
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(500).Wait();
                    }
                    if (useNOB.出現左右選單)
                    {
                        Task.Delay(300).Wait();
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(500).Wait();
                    }
                    if (useNOB.MAPID != cacheMap)
                    {
                        Task.Delay(1000).Wait();
                        useNOB.副本進入完成 = true;
                        break;
                    }
                    mErrorCheck++;
                    if (mErrorCheck > 60)
                    {
                        mErrorCheck = 0;
                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        Task.Delay(200).Wait();
                    }
                }

                useNOB.目前動作 = "入場完成";
            }
        }

        private void 離開副本()
        {
            var useNOB = mUseNOB;
            MainNob.Log($"{useNOB != null} 離開副本 " + useNOB?.PlayerName);
            if (useNOB != null)
            {
                useNOB.副本離開完成 = false;
                useNOB.目前動作 = $"尋找NPC對話.. 離開副本 -> {useNOB.副本離開完成}";
                int x = 0;
                int y = 0;
                int map = 0;

                尋找目標並對話(出場NPCID, 32);
                int moveIndex = 0;
                while (MainNob.StartRunCode)
                {
                    Task.Delay(200).Wait();
                    useNOB.目前動作 = $"出去中 {useNOB.StateA} {useNOB.MAPID} {useNOB.副本離開完成}";
                    MainNob.Log($"{出場NPCID} - {x} {useNOB.PosX} {y} {useNOB.PosY}");
                    if ((map > 0) && (map != useNOB.MAPID))
                    {
                        useNOB.副本離開完成 = true;
                        break;
                    }

                    if (useNOB.出現左右選單)
                    {
                        useNOB.KeyPress(VKeys.KEY_J);
                        useNOB.KeyPress(VKeys.KEY_ENTER);
                        map = useNOB.MAPID;
                        continue;
                    }

                    if (useNOB.取得最下面選項().Contains("返回"))
                    {
                        useNOB.直向選擇(3);
                        Task.Delay(300).Wait();
                        continue;
                    }

                    if (useNOB.GetTargetIDINT() == 出場NPCID && useNOB.對話與結束戰鬥)
                    {
                        useNOB.KeyPress(VKeys.KEY_ESCAPE);
                    }
                    else
                    {
                        moveIndex++;
                        if (moveIndex % 5 == 0)
                        {
                            useNOB.KeyPress(VKeys.KEY_C);
                        }
                        useNOB!.MoveToNPC(出場NPCID);
                        Task.Delay(500).Wait();
                    }

                }
                useNOB.目前動作 = "出場完成";
            }
        }

    }
}
