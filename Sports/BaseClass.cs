using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    public class BaseClass
    {
        #region 移動相關常數 - 建議值，可根據遊戲調整
        private const int InitialDelayMs = 300;                  // 初始延遲 (毫秒)
        private const int BattleCheckDelayMs = 500;               // 戰鬥檢查延遲 (毫秒)
        private const int DistanceThresholdForWaypoint = 300;     // 中途點距離閾值 (像素)
        private const int DistanceThresholdForFinalWaypoint = 200;  // 最終點距離閾值 (像素)
        private const int AngleToleranceDegrees = 5;              // 角度容忍度 (度)
        private const int DistanceThresholdForSlowDown = 200;     // 減速距離閾值 (像素)
        private const int MoveErrorCheckLimit = 1000;             // 移動錯誤檢查次數上限
        private const int RetreatDelayMs = 1500;                  // 後退延遲 (毫秒)
        private const int SideStepDelayMs = 1500;                 // 側向移動延遲 (毫秒)
        private const int BattleEndCheckDelayMs = 500;            // 戰鬥結束檢查延遲 (毫秒)
        private const int BattleEndCheckRetries = 3;              // 戰鬥結束檢查重試次數
        private const int LoopDelayMs = 20;                       // 迴圈延遲 (毫秒)
        #endregion
        ~BaseClass()  // finalizer
        {
            // cleanup statements...
        }

        public NOBDATA? MainNob { get; private set; }
        /// <summary>
        /// 包含隊長自己
        /// </summary>
        public List<NOBDATA> FIDList = new();
        public List<座標> 移動點 = new();
        public int MoveIndex = 0;
        private int moveMode = 0;
        public enum 檢查點 : int
        {
            未知 = -1,
            入場 = 0,
            找目標 = 1,
            戰鬥中 = 2,
            結束戰鬥 = 3,
            出場 = 4,
        }
        public 檢查點 Point = 檢查點.未知;

        public void AddNOBList(List<NOBDATA> mList)
        {
            FIDList = mList;
            NobTeams = mList;
            if (FIDList != null)
            {
                foreach (var nob in FIDList)
                {
                    nob.更改F8追隨();
                }
            }
        }

        public void 全部追隨()
        {
            foreach (var item in 隊員智能功能組)
            {
                item.NOB.更改F8追隨();
                item.NOB.KeyPressPP(VKeys.KEY_F8);
            }
        }

        public void SetMainUser(NOBDATA u)
        {
            MainNob = u;
        }

        public virtual void 初始化() { }
        public virtual void 腳本運作() { }
        public void 前進(int time)
        {
            MainNob?.KeyPressT(VKeys.KEY_W, time);
        }

        public void 後退(int time)
        {
            MainNob?.KeyPressT(VKeys.KEY_S, time);
        }

        public void 移動到定點()
        {
            if (MainNob != null)
            {
                MainNob.KeyPress(VKeys.KEY_W); // 開始持續按住 W 鍵
                Task.Delay(InitialDelayMs).Wait(); // 初始延遲

                int dis = 0, oldDistance = int.MaxValue;
                MoveIndex = 0;
                int moveErrorCheck = 0;
                int battleCheckDone = -1;

                while (MainWindow.CodeRun && 移動點.Count > MoveIndex)
                {
                    if (MainNob.戰鬥中)
                    {
                        battleCheckDone = 1;
                        Task.Delay(BattleCheckDelayMs).Wait();
                        continue; // 戰鬥中，暫停移動邏輯
                    }

                    // 戰鬥結束處理邏輯 (與原程式碼相同)
                    if (battleCheckDone > 0 && MainNob.對話與結束戰鬥)
                    {
                        battleCheckDone++;
                        if (battleCheckDone > BattleEndCheckRetries)
                        {
                            battleCheckDone = -1;
                            MainNob.離開戰鬥A();
                            continue;
                        }
                        Task.Delay(BattleEndCheckDelayMs).Wait();
                        continue;
                    }

                    if (battleCheckDone > 0)
                    {
                        MainNob.目前動作 = "戰鬥中";
                        Task.Delay(BattleCheckDelayMs).Wait();
                        continue; // 戰鬥結束後延遲
                    }

                    int 目標座標X = 移動點[MoveIndex].X;
                    int 目標座標Y = 移動點[MoveIndex].Y;
                    dis = Dis(MainNob.PosX, MainNob.PosY, 目標座標X, 目標座標Y);

                    MainNob.目前動作 = $"index : {MoveIndex} dis : {dis} {Environment.NewLine} X {MainNob.PosX}, Y {MainNob.PosY} {Environment.NewLine} MX {目標座標X} MY{目標座標Y}";

                    int checkDoneDis = 移動點.Count != MoveIndex ? DistanceThresholdForWaypoint : DistanceThresholdForFinalWaypoint;

                    if (dis <= checkDoneDis) // 到達目標點
                    {
                        oldDistance = int.MaxValue; // 重置 oldDistance
                        MoveIndex++;
                        continue; // 移動到下一個目標點
                    }

                    var 攝影機角度 = GetAngleCam(MainNob.CamX, MainNob.CamY);
                    var 與目標角度 = GetAngle(MainNob.PosX, MainNob.PosY, 目標座標X, 目標座標Y);
                    var 角度正規 = 與目標角度 > 180 ? 與目標角度 - 360 : 與目標角度;
                    var 角度差 = 攝影機角度 + 角度正規 - 270;

                    if (MathF.Abs(角度差) > 270)
                        角度差 += 360;

                    // 更精細的角度控制
                    if (Math.Abs(角度差) > AngleToleranceDegrees)
                    {
                        if (角度差 > 0)
                            MainNob.KeyPress(VKeys.KEY_Q); // 微調角度 (向左)
                        else
                            MainNob.KeyPress(VKeys.KEY_E); // 微調角度 (向右)
                    }

                    // 更精細的距離控制 與 移動速度監控
                    if (Math.Abs(角度差) <= AngleToleranceDegrees) // 角度對準後才進行距離控制
                    {
                        if (dis >= DistanceThresholdForSlowDown) // 距離較遠時，持續前進
                        {
                            MainNob.KeyDown(VKeys.KEY_W); // 持續按下 W 鍵
                        }
                        else // 接近目標點時，間歇性前進
                        {
                            MainNob.KeyUp(VKeys.KEY_W); // 抬起 W 鍵
                            if (dis > checkDoneDis) // 如果還沒到達，短時間前進
                            {
                                前進(100); // 短時間前進, 減少 overshoot
                            }
                        }
                    }
                    else
                    {
                        MainNob.KeyUp(VKeys.KEY_W); // 角度偏差大時，先停止前進，專注調整角度
                    }


                    int oold = oldDistance == int.MaxValue ? oldDistance : oldDistance + 10;
                    if (oold > dis) // 距離持續縮短，重置 moveErrorCheck
                    {
                        moveErrorCheck = 0;
                        oldDistance = dis;
                    }
                    else
                    {
                        moveErrorCheck++;
                        if (moveErrorCheck > MoveErrorCheckLimit) // 移動停滯過久，觸發錯誤處理
                        {
                            moveErrorCheck = 0;
                            MainNob.KeyUp(VKeys.KEY_W); // 停止前進
                            oldDistance = int.MaxValue;

                            後退(RetreatDelayMs); // 後退一段時間
                            if (moveMode % 2 == 1)
                                MainNob.KeyPressT(VKeys.KEY_A, SideStepDelayMs); // 側向移動 (模式 1)
                            if (moveMode % 2 == 2)
                                MainNob.KeyPressT(VKeys.KEY_D, SideStepDelayMs); // 側向移動 (模式 2)

                            moveMode++; // 切換側向移動模式
                        }
                    }

                    Task.Delay(LoopDelayMs).Wait(); // 迴圈延遲
                }

                moveMode = 0; // 重置移動模式
                MoveIndex = 0; // 重置移動索引
                MainNob.KeyUp(VKeys.KEY_W); // 移動結束，抬起 W 鍵
            }
        }

        public int 夾角(int a, int b)
        {
            double d1 = a - b;
            double d2 = (2 * 3.14159) - Math.Abs(d1);
            if (d1 > 0)
                d2 = d2 * -1;
            if (Math.Abs(d1) < Math.Abs(d2))
                return (int)d1;
            else
                return (int)d2;

        }
        public int Dis(int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            //Debug.WriteLine($"x1:{x1} y1:{y1} x2:{x2} y2:{y2}");
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        public int GetAngle(float x1, float y1, float x2, float y2)
        {
            var xx = x2 - x1;
            var yy = y2 - y1;
            var angle = MathF.Round((float)(MathF.Atan(xx / yy) * 180 / 3.1415));

            if (yy < 0)
                angle = 180 + angle;
            else
                angle = 360 + angle;


            if (angle >= 360)
                angle = angle - 360;

            return (int)angle;
        }

        public int GetAngleCam(float x, float y)
        {

            var angle = MathF.Round((float)(MathF.Atan(x / y) * 180 / 3.1415));
            if (y < 0)
                angle = 180 + angle;
            else
                angle = 360 + angle;

            if (angle >= 360)
                angle = angle - 360;

            return (int)angle;
        }

        public List<int> targetIDs = new();
        public List<int> skipIDs = new();

        public List<int> 顏色尋目標群(int colorMath, int needCount = 1, E_TargetColor eTC = E_TargetColor.藍NPC)
        {
            List<int> targets = new();
            if (MainNob != null)
            {
                string colorStr = eTC switch
                {
                    E_TargetColor.紅NPC => "6363EE",
                    E_TargetColor.橘NPC => "565ABD",
                    _ => "F6F67A",
                };
                int findCheck = 0;

                var allNPCIDs = GetAllNPCs();
                while (CodeRun)
                {
                    foreach (var npc in allNPCIDs)
                    {
                        MainNob.鎖定NPC((int)npc.ID);
                        Task.Delay(300).Wait();
                        var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), colorStr);
                        if (c1 == colorMath && targets.Contains((int)npc.ID) == false)
                        {
                            targets.Add((int)npc.ID);
                        }

                        if (targets.Count >= needCount)
                        {
                            Debug.WriteLine($"敵人搜尋完成");
                            break;
                        }
                    }

                    if (targets.Count >= needCount)
                    {
                        Debug.WriteLine($"敵人搜尋完成");
                        break;
                    }
                    else
                    {
                        MainNob.KeyPressT(VKeys.KEY_E, 500);
                    }
                    findCheck++;
                    if (findCheck > 3)
                    {
                        Debug.WriteLine($"超出搜尋次數 增加搜尋範圍 {NpcCountToRead}");
                        NpcCountToRead = Math.Min(NpcCountToRead + 5, 150);
                        IgnoredIDs.Clear();
                        allNPCIDs = GetAllNPCs();
                        findCheck = 0;
                    }
                }
            }
            return targets;
        }


        public int 顏色尋目標(int colorMath, int minDistance = 0, int maxDistance = 65535, TargetTypes types = TargetTypes.NPC, E_TargetColor eTC = E_TargetColor.藍NPC)
        {
            int targetID = -1;
            if (MainNob != null)
            {
                string colorStr = eTC switch
                {
                    E_TargetColor.紅NPC => "6363EE",
                    E_TargetColor.橘NPC => "565ABD",
                    _ => "F6F67A",
                };
                int findCheck = 0;

                var allNPCIDs = MainWindow.GetFilteredNPCs(types, minDistance, maxDistance);
                while (CodeRun)
                {
                    foreach (var npc in allNPCIDs)
                    {
                        MainNob.鎖定NPC((int)npc.ID);
                        Task.Delay(200).Wait();
                        var c1 = ColorTools.GetColorNum(MainNob.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), colorStr);
                        if (c1 == colorMath)
                        {
                            targetID = (int)npc.ID;
                            return targetID;
                        }
                    }

                    if (targetID != -1)
                    {
                        Debug.WriteLine($"敵人搜尋完成");
                        return targetID;
                    }
                    else
                    {
                        MainNob.KeyPressT(VKeys.KEY_E, 500);
                    }
                    findCheck++;
                    if (findCheck > 3)
                    {
                        findCheck = 0;
                        allNPCIDs = MainWindow.GetFilteredNPCs(TargetTypes.NPC, minDistance, maxDistance + 2000); findCheck = 0;
                    }
                }
            }
            return targetID;
        }

        public int 顏色尋目標(int colorMath, E_TargetColor eTC = E_TargetColor.藍NPC)
        {
            int targetID = -1;
            if (MainNob != null)
            {
                var list = 顏色尋目標群(colorMath, 1, eTC);
                if (list.Count > 0)
                {
                    targetID = list[0];
                    return targetID;
                }
            }
            return targetID;
        }

        public int 顏色尋目標前往(int colorMath, E_TargetColor eTC = E_TargetColor.藍NPC)
        {
            int targetID = -1;
            if (MainNob != null)
            {
                targetID = 顏色尋目標(colorMath, eTC);
                if (targetID > 0)
                {
                    MainNob.MoveToNPC(targetID);
                    return targetID;
                }
            }
            return targetID;
        }

        public void 確認開門(座標 doomPos)
        {
            int findCheck = 0;
            while (CodeRun)
            {
                if (MainNob!.有觀察對象)
                {
                    MainNob!.KeyPress(VKeys.KEY_ENTER, 5);
                    break;
                }
                else
                {
                    MainNob.前進(500);
                    findCheck++;
                    if (findCheck > 10)
                    {
                        findCheck = 0;
                        移動點.Clear();
                        移動點.Add(doomPos);
                        移動到定點();
                    }
                }
            }
        }

        public void 目標地移動(List<座標> movePosList)
        {
            if (movePosList.Count > 0)
            {
                移動點.Clear();
                移動點.AddRange(movePosList);
                移動到定點();
            }
        }

        public void 尋找並清除目標(int colorMath, int needCount = 1, E_TargetColor eTC = E_TargetColor.藍NPC)
        {
            int thisTargetID = 0;
            int checkBattleDone = -1;
            targetIDs.Clear();
            targetIDs = 顏色尋目標群(colorMath, needCount, eTC);
            while (CodeRun)
            {
                if (MainNob == null)
                {
                    return;
                }

                MainNob.目前動作 = $"目標數量 -> {targetIDs.Count}";
                if (MainNob.戰鬥中)
                {
                    if (targetIDs.Contains(thisTargetID))
                        targetIDs.Remove(thisTargetID);

                    checkBattleDone = 0;
                }

                if (MainNob.待機)
                {
                    checkBattleDone = -1;
                    //等待戰鬥
                    if (targetIDs.Count > 0)
                    {
                        thisTargetID = targetIDs[0];
                        MainNob.MoveToNPC(thisTargetID);
                        Task.Delay(500).Wait();
                    }

                    if (targetIDs.Count == 0)
                    {
                        MainNob.目前動作 = "解決所有目標";
                        break;
                    }
                }
                if (MainNob.對話與結束戰鬥 && checkBattleDone > -1)
                {
                    checkBattleDone++;
                    if (checkBattleDone < 3)
                    {
                        Task.Delay(500).Wait();
                    }
                    else
                    {
                        checkBattleDone = -1;
                        if (NobTeams != null)
                        {
                            foreach (var item in NobTeams)
                            {
                                Debug.WriteLine($"{item.PlayerName} --> 離開戰鬥");
                                Task.Run(item.離開戰鬥A);
                            }

                            while (CodeRun)
                            {
                                bool alldone = true;
                                foreach (var item in NobTeams)
                                {
                                    if (item.離開戰鬥確認 == false)
                                    {
                                        alldone = false;
                                        break;
                                    }
                                }
                                if (alldone)
                                    break;
                            }
                        }
                        //MainNob.離開戰鬥B();
                        //MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                    }
                }
                else if (MainNob.對話與結束戰鬥 && thisTargetID == MainNob.GetTargetIDINT())
                {
                    MainNob.KeyPress(VKeys.KEY_J);
                    MainNob.KeyPress(VKeys.KEY_ENTER);
                    Task.Delay(200).Wait();
                }
                else if (MainNob.對話與結束戰鬥)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                }
                Task.Delay(200).Wait();
            }
        }

        public void 尋找並清除目標(List<int> targetList)
        {
            int thisTargetID = 0;
            int checkBattleDone = -1;
            targetIDs = targetList;
            while (CodeRun)
            {
                if (MainNob == null)
                {
                    return;
                }

                MainNob.目前動作 = $"目標數量 -> {targetIDs.Count}";
                if (MainNob.戰鬥中)
                {
                    if (targetIDs.Contains(thisTargetID))
                        targetIDs.Remove(thisTargetID);

                    checkBattleDone = 0;
                }

                if (MainNob.待機)
                {
                    checkBattleDone = -1;
                    //等待戰鬥
                    if (targetIDs.Count > 0)
                    {
                        thisTargetID = targetIDs[0];
                        MainNob.MoveToNPC(thisTargetID);
                        Task.Delay(500).Wait();
                    }

                    if (targetIDs.Count == 0)
                    {
                        MainNob.目前動作 = "解決所有目標";
                        break;
                    }
                }
                if (MainNob.對話與結束戰鬥 && checkBattleDone > -1)
                {
                    checkBattleDone++;
                    if (checkBattleDone < 3)
                    {
                        Task.Delay(500).Wait();
                    }
                    else
                    {
                        checkBattleDone = -1;
                        if (NobTeams != null)
                        {
                            foreach (var item in NobTeams)
                            {
                                Debug.WriteLine($"{item.PlayerName} --> 離開戰鬥");
                                Task.Run(item.離開戰鬥A);
                            }

                            while (CodeRun)
                            {
                                bool alldone = true;
                                foreach (var item in NobTeams)
                                {
                                    if (item.離開戰鬥確認 == false)
                                    {
                                        alldone = false;
                                        break;
                                    }
                                }
                                if (alldone)
                                    break;
                            }
                        }
                        //MainNob.離開戰鬥B();
                        //MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                    }
                }
                else if (MainNob.對話與結束戰鬥 && thisTargetID == MainNob.GetTargetIDINT())
                {
                    MainNob.KeyPress(VKeys.KEY_J);
                    MainNob.KeyPress(VKeys.KEY_ENTER);
                    Task.Delay(200).Wait();
                }
                else if (MainNob.對話與結束戰鬥)
                {
                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 2);
                }
                Task.Delay(200).Wait();
            }
        }

        public bool 尋找目標並對話(int talkID, int targetColorCheck, E_TargetColor targetNPCType = E_TargetColor.藍NPC)
        {
            int talkNPCID = talkID; // 目標NPC的ID，初始值為 -1 表示尚未找到
            int findTargetTimeoutCounter = 0; // 尋找目標的超時計數器
            int maxFindTargetTimeout = 100; // 最大尋找目標超時次數 (可根據需求調整)
            int moveIndex = 0;
            while (MainWindow.CodeRun) // 當程式碼運行時持續執行
            {
                if (talkNPCID == -1) // 如果尚未找到目標NPC的ID
                {
                    talkNPCID = 顏色尋目標前往(targetColorCheck, targetNPCType); // 呼叫顏色尋目標函數尋找目標

                    if (talkNPCID == -1) // 如果顏色尋目標函數找不到目標
                    {
                        findTargetTimeoutCounter++; // 增加超時計數器
                        if (findTargetTimeoutCounter > maxFindTargetTimeout) // 檢查是否超出最大超時次數
                        {
                            Debug.WriteLine($"尋找 {targetNPCType} 目標超時，可能發生異常。檢查碼: {targetColorCheck}");
                            MainWindow.MainState = $"尋找 {targetNPCType} 目標超時"; // 更新主視窗狀態 (如果需要)
                            return false; // 返回 false 表示尋找目標失敗
                        }
                        Task.Delay(100).Wait(); // 短暫延遲，避免過度消耗 CPU
                        continue; // 找不到目標，繼續下一次迴圈尋找
                    }
                    findTargetTimeoutCounter = 0; // 找到目標後重置超時計數器
                }

                // 條件判斷：是否已鎖定目標NPC且處於可以對話和結束戰鬥的狀態
                bool canTalkToTargetNPC = (MainNob!.GetTargetIDINT() == talkNPCID && MainNob.對話與結束戰鬥);

                if (canTalkToTargetNPC) // 如果可以與目標NPC對話
                {
                    return true; // 返回 true 表示成功與NPC對話
                }
                else // 如果還不能與目標NPC對話
                {
                    MainNob!.MoveToNPC(talkNPCID); // 移動到目標NPC
                    Task.Delay(500).Wait();
                    if (moveIndex % 10 == 0)
                    {
                        MainNob.KeyPress(VKeys.KEY_C);
                    }
                    moveIndex++;
                    talkNPCID = MainNob!.GetTargetIDINT();
                }
                Task.Delay(50).Wait(); // 增加短暫延遲，避免迴圈過快 (可根據需求調整)
            }

            return false; // 如果迴圈因為 MainWindow.CodeRun 為 false 而結束，也返回 false
        }

        /// <summary>
        /// 尋找指定顏色的目標NPC並與之對話。
        /// </summary>
        /// <param name="targetColorCheck">用於顏色識別的檢查碼 (例如 checkIDC1)。</param>
        /// <param name="targetNPCType">目標NPC的顏色類型 (E_TargetColor 枚舉)。</param>
        /// <returns>如果成功與NPC對話則返回 true，否則返回 false。</returns>
        public bool 尋找目標並對話(int targetColorCheck, E_TargetColor targetNPCType)
        {
            int talkNPCID = -1; // 目標NPC的ID，初始值為 -1 表示尚未找到
            int findTargetTimeoutCounter = 0; // 尋找目標的超時計數器
            int maxFindTargetTimeout = 100; // 最大尋找目標超時次數 (可根據需求調整)

            while (MainWindow.CodeRun) // 當程式碼運行時持續執行
            {
                if (talkNPCID == -1) // 如果尚未找到目標NPC的ID
                {
                    talkNPCID = 顏色尋目標前往(targetColorCheck, targetNPCType); // 呼叫顏色尋目標函數尋找目標

                    if (talkNPCID == -1) // 如果顏色尋目標函數找不到目標
                    {
                        findTargetTimeoutCounter++; // 增加超時計數器
                        if (findTargetTimeoutCounter > maxFindTargetTimeout) // 檢查是否超出最大超時次數
                        {
                            Debug.WriteLine($"尋找 {targetNPCType} 目標超時，可能發生異常。檢查碼: {targetColorCheck}");
                            MainWindow.MainState = $"尋找 {targetNPCType} 目標超時"; // 更新主視窗狀態 (如果需要)
                            return false; // 返回 false 表示尋找目標失敗
                        }
                        Task.Delay(100).Wait(); // 短暫延遲，避免過度消耗 CPU
                        continue; // 找不到目標，繼續下一次迴圈尋找
                    }
                    findTargetTimeoutCounter = 0; // 找到目標後重置超時計數器
                }

                // 條件判斷：是否已鎖定目標NPC且處於可以對話和結束戰鬥的狀態
                bool canTalkToTargetNPC = (MainNob?.GetTargetIDINT() == talkNPCID && MainNob.對話與結束戰鬥);

                if (canTalkToTargetNPC) // 如果可以與目標NPC對話
                {
                    return true; // 返回 true 表示成功與NPC對話
                }
                else // 如果還不能與目標NPC對話
                {
                    MainNob?.MoveToNPC(talkNPCID); // 移動到目標NPC
                }
                Task.Delay(50).Wait(); // 增加短暫延遲，避免迴圈過快 (可根據需求調整)
            }

            return false; // 如果迴圈因為 MainWindow.CodeRun 為 false 而結束，也返回 false
        }

        public bool 尋找目標並對話(int targetColorCheck, E_TargetColor targetNPCType, ref int r_talkNPCID)
        {
            int talkNPCID = -1; // 目標NPC的ID，初始值為 -1 表示尚未找到
            int findTargetTimeoutCounter = 0; // 尋找目標的超時計數器
            int maxFindTargetTimeout = 100; // 最大尋找目標超時次數 (可根據需求調整)
            int talkcheckMax = 0;
            while (MainWindow.CodeRun) // 當程式碼運行時持續執行
            {
                if (talkNPCID == -1) // 如果尚未找到目標NPC的ID
                {
                    talkNPCID = 顏色尋目標前往(targetColorCheck, targetNPCType); // 呼叫顏色尋目標函數尋找目標

                    if (talkNPCID == -1) // 如果顏色尋目標函數找不到目標
                    {
                        findTargetTimeoutCounter++; // 增加超時計數器
                        if (findTargetTimeoutCounter > maxFindTargetTimeout) // 檢查是否超出最大超時次數
                        {
                            Debug.WriteLine($"尋找 {targetNPCType} 目標超時，可能發生異常。檢查碼: {targetColorCheck}");
                            MainWindow.MainState = $"尋找 {targetNPCType} 目標超時"; // 更新主視窗狀態 (如果需要)
                            return false; // 返回 false 表示尋找目標失敗
                        }
                        Task.Delay(100).Wait(); // 短暫延遲，避免過度消耗 CPU
                        continue; // 找不到目標，繼續下一次迴圈尋找
                    }
                    findTargetTimeoutCounter = 0; // 找到目標後重置超時計數器
                }

                // 條件判斷：是否已鎖定目標NPC且處於可以對話和結束戰鬥的狀態
                bool canTalkToTargetNPC = (MainNob?.GetTargetIDINT() == talkNPCID && MainNob.對話與結束戰鬥);

                if (canTalkToTargetNPC) // 如果可以與目標NPC對話
                {
                    r_talkNPCID = talkNPCID;
                    return true; // 返回 true 表示成功與NPC對話
                }
                else // 如果還不能與目標NPC對話
                {
                    MainNob?.MoveToNPC(talkNPCID); // 移動到目標NPC
                }
                talkcheckMax++;
                if (talkcheckMax > 200)
                {
                    talkcheckMax = 0;
                    MainNob?.KeyPress(VKeys.KEY_ESCAPE);
                    IgnoredIDs.Add(talkNPCID);
                    talkNPCID = -1;
                    Debug.WriteLine($"{talkNPCID} 對話超時");
                }
                Task.Delay(50).Wait(); // 增加短暫延遲，避免迴圈過快 (可根據需求調整)
            }

            return false; // 如果迴圈因為 MainWindow.CodeRun 為 false 而結束，也返回 false
        }


    }

    public enum E_TargetColor
    {
        藍NPC,
        橘NPC,
        紅NPC,
    }

    public struct 座標
    {
        public 座標(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X;
        public int Y;
    }
}