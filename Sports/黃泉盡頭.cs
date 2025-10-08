using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NOBApp.Sports
{
    internal class 黃泉盡頭 : BaseClass
    {
        NOBDATA? mUseNOB;
        public int SetClass = 0;
        public int 接任務NPCID = 0;
        int cache地圖 = 7800;
        int 外面地圖ID = 0;
        int 水滴使者ID = -1;
        int checkIDC1 = 15;     //絕
        int checkIDC2 = 23;     //朝比奈
        int checkIDC3 = 41;     //水滴
        private volatile bool _isNPCCheckDone = false;
        private volatile bool _isRunning = false;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public override void 初始化()
        {
            if (Tools.IsVIP == false)
            {
                MessageBox.Show($"VIP 腳本暫不開放");
                MainNob!.StartRunCode = false;
                return;
            }

            移動點 = new();
            //Point = 檢查點.找目標;
            //Point = 檢查點.出場;ㄋ
            Point = 檢查點.入場;

            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }

            for (int i = 0; i < NobTeam.Count; i++)
            {
                NobTeam[i].選擇目標類型(1);
                NobTeam[i].StartRunCode = MainNob!.StartRunCode;
            }

            MainNob!.KeyPress(VKeys.KEY_S);
            MainNob!.KeyPress(VKeys.KEY_W);
            外面地圖ID = MainNob!.MAPID;
            MainNob.Log($"外面地圖ID {外面地圖ID}");
        }

        public override async Task 腳本運作()
        {
            if (_isRunning)
            {
                return;
            }
            if (Tools.IsVIP == false)
            {
                MessageBox.Show($"VIP 腳本暫不開放");
                MainNob!.StartRunCode = false;
                return;
            }
            if (!MainNob!.StartRunCode)
            {
                for (int i = 0; i < NobTeam.Count; i++)
                {
                    NobTeam[i].StartRunCode = MainNob.StartRunCode;
                }

                if (!_cts.IsCancellationRequested) _cts.Cancel();
                return;
            }
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }

            var tasks = new List<Task>();
            if (MainNob != null)
            {
                _isRunning = true;
                try
                {
                    MainWindow.MainState = Point.ToString();
                    switch (Point)
                    {
                        case 檢查點.入場:
                            {
                                int tryIn = 0;
                                while (MainNob.StartRunCode)
                                {
                                    MainNob.鎖定NPC(MainNob!.CodeSetting.目標A);
                                    await Task.Delay(100);
                                    if (MainNob.GetTargetIDINT() == MainNob!.CodeSetting.目標A)
                                    {
                                        外面地圖ID = MainNob.MAPID;
                                        break;
                                    }

                                    tryIn++;
                                    if (tryIn > 5)
                                    {
                                        MainNob.Log($"入場的Point 在裡面尚未出來 {MainNob.MAPID}");
                                        Point = 檢查點.出場;
                                        return;
                                    }
                                }

                                tasks = new List<Task>();
                                cache地圖 = MainNob.MAPID;
                                MainNob.Log($"入場 Start {cache地圖}");
                                if (MainNob.MAPID == 外面地圖ID) { Point = 檢查點.入場; }
                                if (MainNob.MAPID != 外面地圖ID) { Point = 檢查點.找目標; }
                                foreach (var nob in NobTeam)
                                {
                                    tasks.Add(Task.Run(() => 回報任務(nob, _cts.Token), _cts.Token));
                                    await Task.Delay(500, _cts.Token);
                                }
                                await Task.WhenAll(tasks);
                                //
                                if (MainNob.MAPID != cache地圖)
                                {
                                    Point = 檢查點.找目標;
                                    return;
                                }
                                await Task.WhenAll(接任務(MainNob));

                                水滴使者ID = -1;
                            }
                            break;
                        case 檢查點.找目標:
                            if (Math.Abs(MainNob.MAPID - 外面地圖ID) < 10)
                            {
                                MainNob.Log($"找目標的Point 尚未進場 回去重新進場 {MainNob.MAPID}");
                                Point = 檢查點.入場;
                                return;
                            }
                            _isNPCCheckDone = false;
                            await Task.WhenAll(Task.Run(() => 進行任務(MainNob, _cts.Token), _cts.Token));
                            MainNob.Log($"出場 -> {Point}");
                            break;
                        case 檢查點.出場:

                            if (Math.Abs(MainNob.MAPID - 外面地圖ID) < 10)
                            {
                                MainNob.Log($"找目標的Point 尚未進場 回去重新進場 {MainNob.MAPID}");
                                Point = 檢查點.入場;
                                return;
                            }
                            MainNob.Log($"出場 Start");
                            _isNPCCheckDone = false;
                            await Task.WhenAll(Task.Run(() => 取得特定NPC(MainNob, checkIDC3, _cts.Token), _cts.Token));

                            tasks = new List<Task>();
                            foreach (var nob in NobTeam)
                            {
                                tasks.Add(Task.Run(() => 離開副本(nob, _cts.Token), _cts.Token));
                                await Task.Delay(500, _cts.Token);
                            }
                            await Task.WhenAll(tasks);

                            MainNob.Log($"全員離開完成");
                            Point = 檢查點.入場;
                            break;
                        case 檢查點.未知:
                        default:
                            MainWindow.MainState = "出現異常";
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    MainNob.Log("腳本運作已成功取消。");
                }
                catch (Exception ex)
                {
                    MainNob.Log($"腳本發生未預期錯誤: {ex.Message}");
                }
                finally
                {
                    // 無論成功、失敗或取消，最後一定要解鎖
                    _isRunning = false;
                }
            }

            return;
        }

        private async Task 進行任務(NOBDATA useNOB, CancellationToken cancellationToken)
        {

            {
                //全跟隨
                foreach (var nob in NobTeam)
                {
                    if (nob != null)
                    {
                        nob.KeyPressPP(VKeys.KEY_F8, 2);
                        await Task.Delay(100);
                    }
                }

                while (MainNob!.StartRunCode)
                {
                    if (_isNPCCheckDone && useNOB.GetTargetIDINT() != -1)
                    {
                        await Task.Delay(100);
                        if (MainNob.對話與結束戰鬥)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                MainNob.KeyPress(VKeys.KEY_J);
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                            }
                            await Task.Delay(100);
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 15);

                            bool 還有怪 = false;
                            for (int i = 0; i < 10; i++)
                            {
                                MainNob.KeyPressT(i < 5 ? VKeys.KEY_Q : VKeys.KEY_E, 400);
                                int tryCount = 0;
                                while (MainNob!.StartRunCode)
                                {
                                    MainNob!.KeyPress(VKeys.KEY_J);

                                    if (useNOB.GetTargetIDINT() != 水滴使者ID && useNOB.GetTargetIDINT() != -1)
                                    {
                                        還有怪 = true;
                                        break;
                                    }
                                    tryCount++;
                                    await Task.Delay(50);
                                    if (tryCount > 4) break;
                                }

                                if (還有怪)
                                {
                                    break;
                                }

                            }
                            if (還有怪)
                            {
                                MainNob.Log($"開場成功 開始探索");
                                foreach (var nob in NobTeam)
                                {
                                    if (nob != null)
                                    {
                                        nob.KeyPress(VKeys.KEY_F8, 2);
                                        Task.Delay(50).Wait();
                                    }
                                }
                                break;
                            }
                            else
                            {
                                MainNob.Log("尚未找到魔物開場失敗繼續");
                            }
                        }
                        else
                        {
                            MainNob.MoveToNPC(水滴使者ID);
                        }
                    }
                    else
                    {
                        await Task.WhenAll(Task.Run(() => 取得特定NPC(useNOB, checkIDC3, cancellationToken), cancellationToken));
                    }
                }
                MainNob.Log($"開始探索");
            }

            //
            bool 該樓完成 = false;
            int battleNum = 0;
            int allBattleDoneCheck = 0;
            int xyCheck = 0;
            while (MainNob!.StartRunCode)
            {
                int talkCheck = 0;
                int endCheck = 0;
                //找怪打
                {
                    int battleCheck = 0;
                    int battleCheck2 = 0;
                    int battleIn = 0;
                    int battleID = 0;
                    int 剛打完的怪物ID = 0;
                    int 卡點Check = 0;
                    bool emyCCheck = false;
                    int ccemyCheck = 0;

                    //判斷NPC
                    while (MainNob.StartRunCode)
                    {
                        if (_isNPCCheckDone)
                            break;
                        if (_isNPCCheckDone == false)
                        {
                            該樓完成 = false;
                            await Task.WhenAll(Task.Run(() => 取得特定NPC(MainNob, checkIDC3, cancellationToken), cancellationToken));
                        }
                        await Task.Delay(300, cancellationToken);
                    }

                    MainNob!.MoveToNPC(水滴使者ID);
                    MainNob!.KeyPressT(VKeys.KEY_C, 100);
                    Task.Delay(1000).Wait();
                    MainNob!.KeyPress(VKeys.KEY_ESCAPE, 10);

                    MainNob.Log($" {MainNob.MAPID} 找完水滴使者 開始處理怪物");
                    bool 戰鬥中PC命令 = false;
                    while (MainNob.StartRunCode)
                    {
                        int getID = MainNob.GetTargetIDINT();

                        if (MainNob.進入結算)
                        {
                            var tasks = new List<Task>();
                            foreach (var nob in NobTeam)
                            {
                                nob.Log("進入結算");
                                tasks.Add(Task.Run(nob.離開戰鬥C, _cts.Token));
                                await Task.Delay(500, _cts.Token);
                                nob.IsUseAutoSkill = false;
                                nob.放技能完成 = nob.已經放過一次 = false;
                                戰鬥中PC命令 = false;
                            }

                            await Task.WhenAll(tasks);

                            while (MainNob.StartRunCode)
                            {
                                bool alldone = true;
                                foreach (var item in NobTeam)
                                {
                                    await Task.Delay(50);
                                    if (item.待機 == false)
                                    {
                                        item.KeyPressPP(VKeys.KEY_ESCAPE, 3);
                                        alldone = false;
                                        continue;
                                    }
                                }
                                if (alldone)
                                    break;
                                await Task.Delay(200);
                            }

                            MainNob.戰鬥中判定 = -1;
                            battleIn = 2;
                            if (emyCCheck)
                                該樓完成 = emyCCheck;
                        }

                        if (MainNob.戰鬥中)
                        {
                            await Task.Delay(200);
                            if (ccemyCheck < 10)
                            {
                                ccemyCheck++;
                                emyCCheck = MainNob.場上超過10人();
                            }
                            if (ccemyCheck >= 3 && emyCCheck)
                            {
                                戰鬥中PC命令 = true;
                                foreach (var nob in NobTeam)
                                {
                                    nob.處理戰鬥流程(true);
                                }
                            }

                            MainNob.戰鬥中判定 = 0;
                            battleIn = 1;
                            continue;
                        }

                        if (battleCheck2 > 10)
                        {
                            endCheck = 0;
                            MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                        }

                        if (MainNob.待機)
                        {
                            if (battleIn == 1)
                            {
                                MainNob.Log("戰鬥中 進入 待機");
                                continue;
                            }

                            MainNob.Log($"GID {getID} BID {battleID} SKEID {剛打完的怪物ID} BIN {battleIn} 卡點C {卡點Check} BC {battleCheck} BNum {battleNum} EC {endCheck}");

                            if (emyCCheck || endCheck > 4)
                            {
                                MainNob.Log("完全沒有新目標 結束前往下層");
                                break;
                            }

                            if (battleIn == 2)
                            {
                                剛打完的怪物ID = battleID;
                                battleID = 0;
                                卡點Check = 0;
                                battleNum++;
                                battleCheck = 0;
                                battleIn = 0;
                                endCheck = 0;
                                emyCCheck = false;

                                foreach (var nob in NobTeam)
                                {
                                    if (nob != null)
                                    {
                                        nob.KeyPress(VKeys.KEY_F8, 2);
                                        await Task.Delay(50);
                                    }
                                }
                                MainNob.後退(800);
                            }

                            battleCheck2 = 0;
                            ccemyCheck = 0;
                            await Task.Delay(50);

                            if (battleNum > 2)
                            {
                                MainNob.Log($"戰鬥三場 完成目前任務");
                                該樓完成 = true;
                                endCheck = 7;
                                break;
                            }

                            if (getID != 剛打完的怪物ID && getID == battleID)
                            {
                                if (卡點Check % 5 == 0)
                                {
                                    foreach (var nob in NobTeam)
                                    {
                                        if (nob != null)
                                        {
                                            nob.KeyPress(VKeys.KEY_F8, 2);
                                            await Task.Delay(50);
                                        }
                                    }
                                }

                                await Task.Delay(100);
                                MainNob!.MoveToNPC(battleID);
                                await Task.Delay(800);
                                卡點Check++;
                                if (卡點Check > 9)
                                {
                                    卡點Check = 0;
                                    MainNob!.KeyPressT(VKeys.KEY_D, 1500);
                                    battleCheck++;
                                    continue;
                                }
                                continue;
                            }
                            else
                            {
                                int tryCount = 0;

                                //battleID = 0;
                                while (MainNob!.StartRunCode)
                                {
                                    MainNob!.KeyPress(VKeys.KEY_J);
                                    await Task.Delay(50);
                                    getID = MainNob.GetTargetIDINT();
                                    if (getID != 剛打完的怪物ID && getID != 水滴使者ID && getID != -1)
                                    {
                                        foreach (var nob in NobTeam)
                                        {
                                            if (nob != null)
                                            {
                                                nob.KeyPress(VKeys.KEY_F8, 2);
                                                await Task.Delay(50);
                                            }
                                        }
                                        MainNob.Log($"有目標對象 {battleID}");
                                        battleID = getID;
                                        allBattleDoneCheck = 0;
                                        endCheck = 0;
                                        battleCheck = 0;
                                        break;
                                    }
                                    tryCount++;
                                    await Task.Delay(50);

                                    if (tryCount > 4) break;
                                }
                                MainNob.Log($"搜尋對象 {battleID}");

                                if (battleID == 0)
                                {
                                    await Task.Delay(100);
                                    MainNob!.KeyPressT(endCheck % 2 == 0 ? VKeys.KEY_Q : VKeys.KEY_E, 600);
                                }
                                else
                                    continue;
                            }

                            battleCheck++;
                            if (battleCheck > 4)
                            {
                                await Task.Delay(100);
                                MainNob!.KeyPressT(VKeys.KEY_C, 100);
                                MainNob!.MoveToNPC(水滴使者ID);

                                endCheck++;
                                battleCheck = 0;
                                await Task.Delay(1000);
                                MainNob!.KeyPress(VKeys.KEY_ESCAPE, 5);
                                await Task.Delay(200);
                                MainNob!.KeyPress(VKeys.KEY_S, 2);
                                MainNob!.KeyPressT(VKeys.KEY_C, 100);
                                await Task.Delay(500);
                                MainNob!.KeyPressT(VKeys.KEY_E, 100);
                            }

                        }
                        if (MainNob.對話與結束戰鬥)
                        {
                            if (getID != -1 && battleIn == 0)
                            {
                                MainNob.KeyPress(VKeys.KEY_J);
                                //稍微等待 PC 過來
                                if (NobTeam.Count > 1)
                                    await Task.Delay(800);
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                battleCheck2++;
                                await Task.Delay(2500);
                            }
                            else
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                await Task.Delay(100);
                            }
                        }
                    }
                }
                //下層樓
                //if (false) 
                {
                    {
                        MainNob.Log($"往下一層樓");
                        xyCheck = 0;
                        talkCheck = 0;
                        while (MainNob.StartRunCode)
                        {
                            int getID = MainNob.GetTargetIDINT();
                            await Task.Delay(100);
                            MainNob.Log($"{getID} - {MainNob.MAPID} - {該樓完成} - {xyCheck} - 確認往下一層 {allBattleDoneCheck}");

                            if ((_isNPCCheckDone || 該樓完成) && xyCheck != 0)
                            {
                                await Task.Delay(200);
                                if (getID == -1 || getID != 水滴使者ID || MathF.Abs(xyCheck - MainNob.PosX) > 10)
                                {
                                    _isNPCCheckDone = false;
                                    該樓完成 = false;
                                    MainNob.Log($"到了下一層了");
                                    水滴使者ID = -1;
                                    allBattleDoneCheck = 0;
                                    battleNum = 0;
                                    await Task.Delay(3000);
                                    MainNob.Log($"NEXT--");
                                    break;
                                }
                                else
                                {
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 2);
                                    allBattleDoneCheck++;
                                    if (該樓完成 == false && allBattleDoneCheck % 6 == 5)
                                    {
                                        MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                                        bool 還有怪 = false;
                                        for (int i = 0; i < 10; i++)
                                        {
                                            MainNob.KeyPressT(i < 5 ? VKeys.KEY_Q : VKeys.KEY_E, 400);
                                            int tryCount = 0;
                                            while (MainNob!.StartRunCode)
                                            {
                                                MainNob!.KeyPress(VKeys.KEY_J);

                                                if (getID != 水滴使者ID && getID != -1)
                                                {
                                                    還有怪 = true;
                                                    break;
                                                }
                                                tryCount++;
                                                await Task.Delay(50);
                                                if (tryCount > 4) break;
                                            }

                                            if (還有怪)
                                            {
                                                MainNob.Log($"還有怪");
                                                foreach (var nob in NobTeam)
                                                {
                                                    if (nob != null)
                                                    {
                                                        nob.KeyPress(VKeys.KEY_F8, 2);
                                                        Task.Delay(50).Wait();
                                                    }
                                                }
                                                break;
                                            }
                                        }

                                    }

                                    if (allBattleDoneCheck > 40)
                                    {
                                        MainNob.Log($"準備離開");
                                        Point = 檢查點.出場;
                                        return;
                                    }
                                    continue;
                                }
                            }

                            if (MainNob.待機)
                            {
                                //判斷水滴使者ID
                                while (MainNob.StartRunCode && _isNPCCheckDone == false)
                                {
                                    if (_isNPCCheckDone)
                                        break;
                                    if (_isNPCCheckDone == false)
                                        await Task.WhenAll(Task.Run(() => 取得特定NPC(MainNob, checkIDC3, cancellationToken), cancellationToken));
                                    await Task.Delay(300, cancellationToken);
                                }

                                await Task.Delay(300);
                                MainNob!.MoveToNPC(水滴使者ID);
                                if (talkCheck % 5 == 0)
                                    MainNob!.KeyPressT(VKeys.KEY_C, 100);
                                talkCheck++;
                                if (talkCheck > 25)
                                {
                                    talkCheck = 0;
                                    MainNob!.後退(500);
                                    MainNob!.MoveToNPC(水滴使者ID);
                                    MainNob!.KeyPressT(VKeys.KEY_C, 100);
                                    MainNob!.MoveToNPC(水滴使者ID);
                                }
                            }

                            if (MainNob.對話與結束戰鬥)
                            {
                                if (MainNob.出現左右選單)
                                {
                                    MainNob.Log($"出現確認往下一層");
                                    xyCheck = MainNob.PosX;
                                    MainNob.KeyPress(VKeys.KEY_J);
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                    await Task.Delay(500);
                                    continue;
                                }
                                else
                                {
                                    allBattleDoneCheck++;
                                    MainNob.KeyPress(VKeys.KEY_ENTER);
                                    MainNob.Log($"allBattleDoneCheck {allBattleDoneCheck}");
                                }
                            }

                            if (allBattleDoneCheck > 10)
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
                                Point = 檢查點.出場;
                                break;
                            }
                        }
                    }

                    if (allBattleDoneCheck > 10)
                    {
                        Point = 檢查點.出場;
                        break;
                    }
                }
            }

            return;
        }

        private async Task 回報任務(NOBDATA useNOB, CancellationToken cancellationToken)
        {
            useNOB.Log("回報任務");
            try
            {
                useNOB.副本回報完成 = false;
                int mErrorCheck = 0;
                // 【移植】保留您原有的 while 迴圈邏輯
                while (MainNob!.StartRunCode)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(300);
                    if (MainNob.MAPID != cache地圖)
                    {
                        MainNob.Log("意外入場 準備開始");
                        MainNob!.副本進入完成 = true;
                        Point = 檢查點.找目標;
                        return;
                    }

                    if (useNOB.對話與結束戰鬥)
                    {
                        if (useNOB.出現直式選單)
                        {
                            if (useNOB.取得最下面選項().Contains("返回"))
                            {
                                mErrorCheck = 0;
                                useNOB.直向選擇PP(1);
                                await Task.Delay(300);
                                useNOB.KeyPressPP(VKeys.KEY_ENTER, 5, 100);
                                useNOB.KeyPressPP(VKeys.KEY_ESCAPE, 25, 200);
                                useNOB.副本回報完成 = true;
                                useNOB.Log("任務回報完成");
                                return;
                            }
                        }
                        else
                        {
                            useNOB.KeyPressPP(VKeys.KEY_ESCAPE);
                        }

                        mErrorCheck++;
                        if (mErrorCheck > 30)
                        {
                            MainNob.Log(" ErrorCheck ");
                            mErrorCheck = 0;
                            if (useNOB.出現左右選單)
                            {
                                Task.Delay(500).Wait();
                                useNOB.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                            }
                            else
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            }

                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            await Task.Delay(200);
                        }
                    }
                    else
                    {
                        if (useNOB.出現左右選單)
                        {
                            useNOB.Log($"異常狀態下出現 視窗");
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                            Task.Delay(300).Wait();
                        }

                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }

                }
            }
            catch (OperationCanceledException)
            {
                useNOB.Log("離開副本被取消。");
            }
            catch (Exception ex)
            {
                useNOB.Log($"回報出錯: {ex.Message}");
            }

            return;
        }
        private async Task 接任務(NOBDATA useNOB)
        {
            useNOB.Log($"接任務");

            int mErrorCheck = 0;
            if (useNOB != null)
            {
                useNOB!.副本進入完成 = false;
                useNOB!.Log("尋找NPC對話 " + useNOB?.PlayerName);
                useNOB!.KeyPress(VKeys.KEY_ESCAPE, 5);

                while (MainNob!.StartRunCode)
                {
                    await Task.Delay(200);
                    if (MainNob.MAPID != cache地圖)
                    {
                        MainNob.Log("意外入場 準備開始");
                        MainNob!.副本進入完成 = true;
                        Point = 檢查點.找目標;
                        return;
                    }

                    useNOB.Log($"入場中 {useNOB.對話與結束戰鬥} | {useNOB.出現直式選單}");
                    if (useNOB.對話與結束戰鬥)
                    {
                        if (useNOB.出現直式選單)
                        {
                            if (useNOB.取得最下面選項().Contains("返回"))
                            {
                                mErrorCheck = 0;
                                useNOB.直向選擇(1);
                                Task.Delay(300).Wait();
                                useNOB.KeyPress(VKeys.KEY_ENTER, 10);
                                //等待轉換地圖入場
                                int outCheck = 0;
                                while (MainNob!.StartRunCode)
                                {
                                    useNOB.KeyPress(VKeys.KEY_S);
                                    useNOB.KeyPress(VKeys.KEY_W);
                                    if (MainNob.MAPID != cache地圖)
                                    {
                                        MainNob.Log("入場完成 準備開始");
                                        MainNob!.副本進入完成 = true;
                                        Point = 檢查點.找目標;
                                        return;
                                    }
                                    await Task.Delay(400);
                                    outCheck++;
                                    if (outCheck > 10)
                                    {
                                        useNOB.Log("一直沒有變化地圖 準備重新進場");
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            useNOB.KeyPressPP(VKeys.KEY_ESCAPE);
                        }

                        mErrorCheck++;
                        if (mErrorCheck > 30)
                        {
                            MainNob.Log(" ErrorCheck ");
                            mErrorCheck = 0;
                            if (useNOB.出現左右選單)
                            {
                                Task.Delay(500).Wait();
                                useNOB.KeyPress(VKeys.KEY_J);
                                useNOB.KeyPress(VKeys.KEY_ENTER);
                            }
                            else
                            {
                                useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
                            }

                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            Task.Delay(200).Wait();
                        }
                    }
                    else
                    {
                        if (useNOB.出現左右選單)
                        {
                            Task.Delay(500).Wait();
                            useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
                            useNOB.KeyPress(VKeys.KEY_ENTER);
                        }
                        useNOB.MoveToNPC(MainNob!.CodeSetting.目標A);
                    }
                }
            }
        }
        private async Task 離開副本(NOBDATA useNOB, CancellationToken cancellationToken)
        {
            if (useNOB != null)
            {
                useNOB.副本離開完成 = false;
                useNOB.Log("出任務NPC對話 " + useNOB?.PlayerName);
                useNOB!.KeyPress(VKeys.KEY_ESCAPE, 10);

                int checkmapid = 0, checkDone = 0, errorL = 0;
                int cacheID = 水滴使者ID;
                while (MainNob!.StartRunCode)
                {
                    if (Math.Abs(useNOB.MAPID - 外面地圖ID) < 10)
                    {
                        useNOB.Log($"找目標的Point 尚未進場 回去重新進場{外面地圖ID} {useNOB.MAPID}");
                        return;
                    }

                    await Task.Delay(500);
                    if (_isNPCCheckDone && useNOB.GetTargetIDINT() != -1)
                    {
                        if (useNOB.對話與結束戰鬥)
                        {
                            await Task.Delay(200);

                            if (useNOB.出現左右選單)
                            {
                                checkmapid = useNOB.MAPID;

                                useNOB.Log($"確定離開 {checkDone} : {useNOB.MAPID} - {checkmapid}");
                                useNOB.KeyPressPP(VKeys.KEY_J);
                                await Task.Delay(100);
                                useNOB.KeyPressPP(VKeys.KEY_ENTER);
                                await Task.Delay(100);
                                while (MainNob!.StartRunCode)
                                {
                                    useNOB.KeyPressPP(VKeys.KEY_D);
                                    await Task.Delay(300);
                                    useNOB.KeyPressPP(VKeys.KEY_W);

                                    if (checkmapid != useNOB.MAPID)
                                    {
                                        useNOB.副本離開完成 = true;
                                        return;
                                    }

                                    checkDone++;
                                    if (checkDone > 20)
                                    {
                                        useNOB.Log($"離開異常 重新離開");
                                        checkDone = 0;
                                        useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);
                                        errorL++;
                                        break;
                                    }
                                }
                            }
                            else if (useNOB.取得最下面選項().Contains("返回"))
                            {
                                useNOB.直向選擇PP(errorL < 2 ? 1 : 2);
                            }
                            else
                                useNOB.KeyPress(VKeys.KEY_ESCAPE);
                        }
                        else
                        {
                            useNOB.MoveToNPC(cacheID);
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        await Task.WhenAll(Task.Run(() => 取得特定NPC(useNOB, checkIDC3, cancellationToken), cancellationToken));
                    }
                }
            }

            return;
        }

        async Task 取得特定NPC(NOBDATA useNOB, int colorNum, CancellationToken cancellationToken)
        {
            int tryGet = 0;
            int tryGetOutLine = 0;
            while (MainNob!.StartRunCode)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (水滴使者ID != -1)
                {
                    useNOB.鎖定NPC(水滴使者ID);
                    await Task.Delay(100);
                    if (useNOB.GetTargetIDINT() == 水滴使者ID)
                    {
                        _isNPCCheckDone = true;
                        //NPCID 沒變換 可以直接用
                        return;
                    }
                    else
                    {
                        水滴使者ID = -1;
                        useNOB!.KeyPress(VKeys.KEY_J);
                        await Task.Delay(100);
                    }
                }

                var c2 = ColorTools.GetColorNum(useNOB.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
                await Task.Delay(100);
                //水滴
                if (c2 == colorNum)
                {
                    _isNPCCheckDone = true;
                    水滴使者ID = useNOB.GetTargetIDINT();
                    return;
                }
                else
                {
                    useNOB!.KeyPress(VKeys.KEY_J);
                    tryGet++;
                }

                if (tryGet > 3)
                {
                    useNOB!.KeyPressT(VKeys.KEY_E, 300);
                    tryGet = 0;
                    tryGetOutLine++;
                }

                if (tryGetOutLine > 2)
                {
                    tryGetOutLine = 0;
                    MainNob!.鎖定NPC(MainNob!.CodeSetting.目標A);
                    await Task.Delay(100);
                    if (MainNob.GetTargetIDINT() == MainNob!.CodeSetting.目標A)
                    {
                        外面地圖ID = MainNob.MAPID;
                        Point = 檢查點.入場;
                        return;
                    }
                }

            }
        }
    }
}
