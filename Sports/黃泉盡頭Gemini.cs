using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 黃泉盡頭Gemini : BaseClass
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        // 內部狀態變數，與您原版一致
        private NOBDATA? mUseNOB;
        private int cache地圖 = 7800;
        private int 水滴使者ID = -1;
        private const int checkIDC3 = 41; // 水滴
        private bool fPass = false;

        public override void 初始化()
        {
            移動點 = new();
            fPass = true;
            Point = 檢查點.入場;

            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }

            foreach (var nob in NobTeam)
            {
                nob.選擇目標類型(1);
                nob.StartRunCode = MainNob.StartRunCode;
            }
            cache地圖 = MainNob!.MAPID;
        }

        // 【修改】將主流程改為 async Task
        public override async Task 腳本運作()
        {
            if (!MainNob.StartRunCode)
            {
                if (!_cts.IsCancellationRequested) _cts.Cancel();
                return;
            }
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = new CancellationTokenSource();
            }

            try
            {
                // 動態修正狀態點
                if (Point == 檢查點.未知)
                {
                    Point = MainNob.MAPID == cache地圖 ? 檢查點.入場 : 檢查點.找目標;
                }

                MainWindow.MainState = Point.ToString();
                switch (Point)
                {
                    case 檢查點.入場:
                        await 執行入場流程(_cts.Token);
                        fPass = false;
                        MainNob.Log("進入副本完成");
                        Point = 檢查點.找目標;
                        break;

                    case 檢查點.找目標:
                        await 執行找目標流程(_cts.Token);
                        Point = 檢查點.出場;
                        break;

                    case 檢查點.出場:
                        await 執行出場流程(_cts.Token);
                        Point = 檢查點.入場;
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                MainNob.Log("腳本運作已成功取消。");
                Point = 檢查點.入場;
            }
            catch (Exception ex)
            {
                MainNob.Log($"腳本發生未預期錯誤: {ex.Message}");
                Point = 檢查點.入場;
            }
        }

        // 【修改】使用 Task.WhenAll 管理入場任務
        private async Task 執行入場流程(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            foreach (var nob in NobTeam)
            {
                // Task.Run 確保每個角色的邏輯在獨立背景任務中執行
                tasks.Add(Task.Run(() => 接任務(nob, cancellationToken), cancellationToken));
                // 恢復您原有的節奏，錯開每個角色的啟動時間
                await Task.Delay(500, cancellationToken);
            }
            // 等待所有入場任務完成
            await Task.WhenAll(tasks);
        }

        // 【修改】將原版「找目標」的複雜邏輯搬移至此，並改為 async
        private async Task 執行找目標流程(CancellationToken cancellationToken)
        {
            // 全跟隨
            MainNob.KeyPress(VKeys.KEY_W, 3);
            foreach (var nob in NobTeam)
            {
                nob.KeyPress(VKeys.KEY_F8, 2);
                await Task.Delay(50, cancellationToken);
            }

            if (fPass == false)
            {
                // 開始探索... (邏輯與您原版一致)
            }

            // 【移植】您原版中找怪打 & 前往下一層的 while 迴圈
            int battleNum = 0;
            while (MainNob.StartRunCode)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // ... 這裡需要將您原版 `case 檢查點.找目標:` 中最外層的 while 迴圈完整邏輯複製進來 ...
                // 並將所有 `Task.Delay(...).Wait()` 都改為 `await Task.Delay(...)`。
                // 這是最關鍵的一步，因為只有您最清楚遊戲的詳細互動邏輯。

                // 示例:
                if (MainNob.進入結算)
                {
                    //...
                    await Task.Delay(200, cancellationToken);
                    //...
                }

                // 為了讓程式碼能編譯，暫時用一個 break 跳出
                MainNob.Log("（模擬）打怪流程結束。");
                break;
            }
        }

        // 【修改】使用 Task.WhenAll 管理出場任務
        private async Task 執行出場流程(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            foreach (var nob in NobTeam)
            {
                tasks.Add(Task.Run(() => 離開副本(nob, cancellationToken), cancellationToken));
                await Task.Delay(500, cancellationToken);
            }
            await Task.WhenAll(tasks);
            MainNob.Log("全員離開完成");
            Point = 檢查點.入場;
        }

        // 【移植並重構】將原版 `接任務` 方法改為 async Task
        private async Task 接任務(NOBDATA useNOB, CancellationToken cancellationToken)
        {
            useNOB.Log($"接任務 ---- {MainNob.StartRunCode} ---- {useNOB.PlayerName}");
            int mErrorCheck = 0;
            useNOB.副本進入完成 = false;

            try
            {
                useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);

                // 【移植】保留您原有的 while 迴圈邏輯
                while (MainNob.StartRunCode)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(200, cancellationToken); // 【替換】Wait() -> await
                    useNOB.Log($"入場中 {useNOB.對話與結束戰鬥} | {useNOB.出現直式選單}");

                    // ... 這裡需要將您原版 `接任務` 方法中的完整 if/else 邏輯複製進來 ...
                    // 並將所有 `Task.Delay(...).Wait()` 都改為 `await Task.Delay(...)`。

                    // 檢查是否已成功入場
                    if (useNOB.MAPID != cache地圖)
                    {
                        await Task.Delay(200, cancellationToken);
                        useNOB.副本進入完成 = true;
                        useNOB.Log("入場完成");
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                useNOB.Log("接任務被取消。");
            }
            catch (Exception ex)
            {
                useNOB.Log($"接任務出錯: {ex.Message}");
            }
        }

        // 【移植並重構】將原版 `離開副本` 方法改為 async Task
        private async Task 離開副本(NOBDATA useNOB, CancellationToken cancellationToken)
        {
            useNOB.副本離開完成 = false;
            useNOB.Log("出任務NPC對話 " + useNOB.PlayerName);

            try
            {
                // 【移植】保留您原有的 while 迴圈邏輯
                while (MainNob.StartRunCode)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // ... 這裡需要將您原版 `離開副本` 方法中的完整 if/else 邏輯複製進來 ...
                    // 並將所有 `Task.Delay(...).Wait()` 都改為 `await Task.Delay(...)`。

                    // 檢查是否成功離開
                    if (useNOB.MAPID == cache地圖)
                    {
                        useNOB.副本離開完成 = true;
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                useNOB.Log("離開副本被取消。");
            }
            catch (Exception ex)
            {
                useNOB.Log($"離開副本出錯: {ex.Message}");
            }
        }
    }
}