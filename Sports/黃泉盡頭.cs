using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NOBApp;

namespace NOBApp.Sports
{
	internal class 黃泉盡頭 : BaseClass
	{
		#region Fields & Constants

		// Renamed fields to follow C# conventions (camelCase for private fields)
		private int _outsideMapId = 0;
		private int _cachedMapId = 7800;
		private int _waterMessengerId = -1;
		private volatile bool _isNpcFound = false;
		private volatile bool _isScriptRunning = false;
		private int _clearCount = 0;
		private CancellationTokenSource _cts = new CancellationTokenSource();

		// Renamed constants to be more descriptive
		private const int ColorCheckWater = 41; // Previously CheckIDC3 (水滴)

		// Renamed Enum to be more descriptive
		public enum ScriptStage
		{
			Entrance = 0,   // 入場
			FindTarget = 1, // 找目標
			Exit = 2        // 出場
		}

		// Renamed property to match the new Enum
		public ScriptStage CurrentStage = ScriptStage.Entrance;

		#endregion

		/// <summary>
		/// 初始化腳本，檢查VIP權限，設定初始狀態和團隊配置
		/// </summary>
		public override void 初始化()
		{
			if (!Tools.IsVIP)
			{
				MessageBox.Show($"VIP 腳本暫不開放");
				MainNob!.StartRunCode = false;
				return;
			}

			移動點 = new();
			CurrentStage = ScriptStage.Entrance;

			if (_cts.IsCancellationRequested)
			{
				_cts.Dispose();
				_cts = new CancellationTokenSource();
			}

			foreach (var nob in NobTeam)
			{
				nob.選擇目標類型(1);
				nob.StartRunCode = MainNob!.StartRunCode;
			}

			MainNob!.KeyPress(VKeys.KEY_S);
			MainNob!.KeyPress(VKeys.KEY_W);
			_outsideMapId = MainNob!.MAPID;
			MainNob.Log($"外部地圖 ID: {_outsideMapId}"); // 記錄外部地圖ID以便後續判斷
		}

		/// <summary>
		/// 主要腳本運行邏輯，根據當前階段執行相應處理
		/// </summary>
		/// <returns>非同步任務</returns>
		public override async Task 腳本運作()
		{
			if (_isScriptRunning) return;

			if (!Tools.IsVIP)
			{
				MessageBox.Show($"VIP 腳本暫不開放");
				MainNob!.StartRunCode = false;
				return;
			}

			if (!MainNob!.StartRunCode)
			{
				foreach (var nob in NobTeam)
				{
					nob.StartRunCode = MainNob.StartRunCode;
				}
				if (!_cts.IsCancellationRequested) _cts.Cancel();
				return;
			}

			if (_cts.IsCancellationRequested)
			{
				_cts.Dispose();
				_cts = new CancellationTokenSource();
			}

			_isScriptRunning = true;
			try
			{
				MainWindow.MainState = CurrentStage.ToString();

				while (MainNob.StartRunCode)
				{
					_cts.Token.ThrowIfCancellationRequested();

					switch (CurrentStage)
					{
						case ScriptStage.Entrance:
							await HandleEntrance(_cts.Token);
							break;

						case ScriptStage.FindTarget:
							await HandleFindTarget(_cts.Token);
							break;

						case ScriptStage.Exit:
							await HandleExit(_cts.Token);
							break;

						default:
							MainWindow.MainState = "異常：未知階段";
							await Task.Delay(1000, _cts.Token);
							break;
					}

					await Task.Delay(200, _cts.Token);
				}
			}
			catch (OperationCanceledException)
			{
				MainNob.Log("腳本執行已取消。"); // 腳本執行被取消
			}
			catch (Exception ex)
			{
				MainNob.Log($"腳本錯誤：{ex.Message}"); // 腳本執行錯誤
			}
			finally
			{
				_isScriptRunning = false;
			}
		}

		#region Stage Handlers

		/// <summary>
		/// 處理入場階段：鎖定NPC、設定地圖ID、執行任務報告和接受
		/// </summary>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task HandleEntrance(CancellationToken token)
		{
			int tryIn = 0;
			while (MainNob!.StartRunCode)
			{
				MainNob.鎖定NPC(MainNob.CodeSetting.目標A);
				await Task.Delay(100, token);
				if (MainNob.GetTargetIDINT() == MainNob.CodeSetting.目標A)
				{
					_outsideMapId = MainNob.MAPID;
					break;
				}

				tryIn++;
				if (tryIn > 5)
				{
					MainNob.Log($"入場階段卡在地圖：{MainNob.MAPID}"); // 入場階段卡在地圖內
					CurrentStage = ScriptStage.Exit;
					return;
				}
			}

			_cachedMapId = MainNob.MAPID;
			MainNob.Log($"入場開始，快取地圖：{_cachedMapId}"); // 入場開始，記錄快取地圖ID

			if (MainNob.MAPID == _outsideMapId) CurrentStage = ScriptStage.Entrance;
			else CurrentStage = ScriptStage.FindTarget;

			// Report tasks in parallel
			var tasks = new List<Task>();
			foreach (var nob in NobTeam)
			{
				tasks.Add(ReportTask(nob, token));
			}
			await Task.WhenAll(tasks);

			if (MainNob.MAPID != _cachedMapId)
			{
				_clearCount = 0;
				CurrentStage = ScriptStage.FindTarget;
				return;
			}

			await AcceptTask(MainNob, token);
			_waterMessengerId = -1;
		}

		/// <summary>
		/// 處理尋找目標階段：檢查是否在外部地圖，執行任務
		/// </summary>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task HandleFindTarget(CancellationToken token)
		{
			if (IsOutsideMap())
			{
				MainNob!.Log($"尋找目標階段：目前在外部地圖（{MainNob.MAPID}），返回入場"); // 尋找目標階段：目前在外部地圖，返回入場
				CurrentStage = ScriptStage.Entrance;
				return;
			}

			_isNpcFound = false;
			await ExecuteMission(MainNob!, token);
			MainNob!.Log($"任務完成 -> {CurrentStage}"); // 任務完成，記錄當前階段
		}

		/// <summary>
		/// 處理退出階段：檢查是否在外部地圖，尋找NPC並執行離開副本邏輯
		/// </summary>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task HandleExit(CancellationToken token)
		{
			if (IsOutsideMap())
			{
				MainNob!.Log($"退出階段：目前在外部地圖（{MainNob.MAPID}），返回入場"); // 退出階段：目前在外部地圖，返回入場
				CurrentStage = ScriptStage.Entrance;
				return;
			}

			MainNob!.Log($"開始退出"); // 退出開始
			_isNpcFound = false;

			// Find NPC first
			await FindSpecificNpc(MainNob, ColorCheckWater, token);

			var tasks = new List<Task>();
			foreach (var nob in NobTeam)
			{
				tasks.Add(LeaveDungeon(nob, token));
			}
			await Task.WhenAll(tasks);

			MainNob.Log($"所有成員已退出。"); // 所有成員已退出
			CurrentStage = ScriptStage.Entrance;
		}

		#endregion

		#region Core Logic Methods

		/// <summary>
		/// 執行主要任務邏輯：跟隨、尋找NPC、處理戰鬥和樓層進度
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task ExecuteMission(NOBDATA useNOB, CancellationToken token)
		{
			// Initial Follow
			foreach (var nob in NobTeam)
			{
				if (nob != null)
				{
					nob.KeyPressPP(VKeys.KEY_F8, 2);
					await Task.Delay(100, token);
				}
			}

			// Initial NPC Check Loop
			while (MainNob!.StartRunCode)
			{
				token.ThrowIfCancellationRequested();

				if (_isNpcFound && useNOB.GetTargetIDINT() != -1)
				{
					await Task.Delay(100, token);
					if (MainNob.對話與結束戰鬥)
					{
						// Handle Dialog
						await HandleDialogConfirm(10, token);
						MainNob.KeyPress(VKeys.KEY_ESCAPE, 15);

						if (await CheckForMonsters(useNOB, token))
						{
							MainNob.Log($"發現怪物，開始探索。"); // 發現怪物，開始探索
							foreach (var nob in NobTeam)
							{
								if (nob != null)
								{
									nob.KeyPress(VKeys.KEY_F8, 2);
									await Task.Delay(50, token);
								}
							}
							break;
						}
						else
						{
							MainNob.Log("未發現怪物，重試..."); // 未發現怪物，重試
						}
					}
					else
					{
						MainNob.MoveToNPC(_waterMessengerId);
					}
				}
				else
				{
					await FindSpecificNpc(useNOB, ColorCheckWater, token);
				}
			}

			MainNob.Log($"開始探索"); // 探索開始

			bool floorComplete = false;
			int battleNum = 0;
			int allBattleDoneCheck = 0;

			while (MainNob.StartRunCode)
			{
				token.ThrowIfCancellationRequested();

				// Battle Loop Variables
				int battleCheck = 0;
				int battleCheck2 = 0;
				int battleIn = 0;
				int battleID = 0;
				int lastMonsterID = 0;
				int stuckCheck = 0;
				bool enemyCountCheck = false;
				int ccEnemyCheck = 0;
				int endCheck = 0;

				// Ensure NPC is found
				while (MainNob.StartRunCode)
				{
					if (_isNpcFound) break;
					floorComplete = false;
					await FindSpecificNpc(MainNob, ColorCheckWater, token);
					await Task.Delay(300, token);
				}

				MainNob.MoveToNPC(_waterMessengerId);
				MainNob.KeyPressT(VKeys.KEY_C, 100);
				await Task.Delay(1000, token);
				MainNob.KeyPress(VKeys.KEY_ESCAPE, 10);

				MainNob.Log($"地圖 {MainNob.MAPID} 已找到 NPC，處理怪物。"); // NPC找到，處理怪物

				while (MainNob.StartRunCode)
				{
					token.ThrowIfCancellationRequested();
					int getID = MainNob.GetTargetIDINT();

					if (MainNob.進入結算)
					{
						await HandleBattleSettlement(token);

						MainNob.戰鬥中判定 = -1;
						battleIn = 2;
						if (enemyCountCheck) floorComplete = enemyCountCheck;
					}

					if (MainNob.戰鬥中)
					{
						await Task.Delay(200, token);
						if (ccEnemyCheck < 10)
						{
							ccEnemyCheck++;
							enemyCountCheck = MainNob.場上超過10人();
						}
						if (ccEnemyCheck >= 3 && enemyCountCheck)
						{
							foreach (var nob in NobTeam) nob.處理戰鬥流程(true);
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
							MainNob.Log("戰鬥中 -> 待機"); // 戰鬥中 -> 待機
							continue;
						}

						if (enemyCountCheck || endCheck > 4)
						{
							MainNob.Log("無新目標，前往下一樓層。"); // 無新目標，前進到下一樓層
							break;
						}

						if (battleIn == 2)
						{
							lastMonsterID = battleID;
							battleID = 0;
							stuckCheck = 0;
							battleNum++;
							battleCheck = 0;
							battleIn = 0;
							endCheck = 0;
							enemyCountCheck = false;

							foreach (var nob in NobTeam)
							{
								if (nob != null)
								{
									nob.KeyPress(VKeys.KEY_F8, 2);
									await Task.Delay(50, token);
								}
							}
							MainNob.後退(800);
						}

						battleCheck2 = 0;
						ccEnemyCheck = 0;
						await Task.Delay(50, token);

						if (battleNum > 2)
						{
							MainNob.Log($"已完成3場戰鬥，該樓層任務完成。"); // 3場戰鬥完成，樓層任務完成
							floorComplete = true;
							endCheck = 7;
							break;
						}

						if (getID != lastMonsterID && getID == battleID)
						{
							if (stuckCheck % 5 == 0)
							{
								foreach (var nob in NobTeam)
								{
									if (nob != null)
									{
										nob.KeyPress(VKeys.KEY_F8, 2);
										await Task.Delay(50, token);
									}
								}
							}

							await Task.Delay(100, token);
							MainNob.MoveToNPC(battleID);
							await Task.Delay(800, token);
							stuckCheck++;
							if (stuckCheck > 9)
							{
								stuckCheck = 0;
								MainNob.KeyPressT(VKeys.KEY_D, 1500);
								battleCheck++;
								continue;
							}
							continue;
						}
						else
						{
							int tryCount = 0;
							while (MainNob.StartRunCode)
							{
								MainNob.KeyPress(VKeys.KEY_J);
								await Task.Delay(50, token);
								getID = MainNob.GetTargetIDINT();
								if (getID != lastMonsterID && getID != _waterMessengerId && getID != -1)
								{
									foreach (var nob in NobTeam)
									{
										if (nob != null)
										{
											nob.KeyPress(VKeys.KEY_F8, 2);
											await Task.Delay(50, token);
										}
									}
									MainNob.Log($"發現目標：{battleID}"); // 目標找到
									battleID = getID;
									allBattleDoneCheck = 0;
									endCheck = 0;
									battleCheck = 0;
									break;
								}
								tryCount++;
								await Task.Delay(50, token);
								if (tryCount > 4) break;
							}

							if (battleID == 0)
							{
								await Task.Delay(100, token);
								MainNob.KeyPressT(endCheck % 2 == 0 ? VKeys.KEY_Q : VKeys.KEY_E, 600);
							}
							else continue;
						}

						battleCheck++;
						if (battleCheck > 4)
						{
							await Task.Delay(100, token);
							MainNob.KeyPressT(VKeys.KEY_C, 100);
							MainNob.MoveToNPC(_waterMessengerId);

							endCheck++;
							battleCheck = 0;
							await Task.Delay(1000, token);
							MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
							await Task.Delay(200, token);
							MainNob.KeyPress(VKeys.KEY_S, 2);
							MainNob.KeyPressT(VKeys.KEY_C, 100);
							await Task.Delay(500, token);
							MainNob.KeyPressT(VKeys.KEY_E, 100);
						}
					}

					if (MainNob.對話與結束戰鬥)
					{
						if (getID != -1 && battleIn == 0)
						{
							MainNob.KeyPress(VKeys.KEY_J);
							if (NobTeam.Count > 1) await Task.Delay(800, token);
							MainNob.KeyPress(VKeys.KEY_ENTER);
							battleCheck2++;
							await Task.Delay(2500, token);
						}
						else
						{
							MainNob.KeyPress(VKeys.KEY_ESCAPE);
							await Task.Delay(100, token);
						}
					}
				}

				// Next Floor Logic
				MainNob.Log($"前往下一樓層"); // 前進到下一樓層
				int talkCheck = 0;
				int cachedPosX = -1, cachedMapId = -1;
				bool waitingForMapChange = false;

				while (MainNob.StartRunCode)
				{
					token.ThrowIfCancellationRequested();
					int getID = MainNob.GetTargetIDINT();
					await Task.Delay(100, token);

					if (waitingForMapChange)
					{
						await Task.Delay(300, token);
						if (cachedMapId != MainNob.MAPID || cachedPosX != MainNob.PosX || (getID != -1 && getID != _waterMessengerId))
						{
							_isNpcFound = false;
							floorComplete = false;
							MainNob.Log($"已抵達下一樓層。"); // 抵達下一樓層
							_waterMessengerId = -1;
							allBattleDoneCheck = 0;
							battleNum = 0;
							await Task.Delay(3000, token);
							break; // Break to outer loop to restart battle logic
						}
						else
						{
							MainNob.KeyPress(VKeys.KEY_ENTER, 2);
							if (_clearCount > 6) allBattleDoneCheck++;
							allBattleDoneCheck++;

							if (!floorComplete && allBattleDoneCheck % 6 == 5)
							{
								MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
								if (await CheckForMonsters(MainNob, token))
								{
									MainNob.Log($"仍有怪物存在。"); // 怪物剩餘
									foreach (var nob in NobTeam)
									{
										if (nob != null)
										{
											nob.KeyPress(VKeys.KEY_F8, 2);
											await Task.Delay(50, token);
										}
									}
									break;
								}
							}

							if (allBattleDoneCheck > 40)
							{
								MainNob.Log($"準備退出。"); // 準備退出
								CurrentStage = ScriptStage.Exit;
								return;
							}
							continue;
						}
					}

					if (MainNob.待機)
					{
						while (MainNob.StartRunCode && !_isNpcFound)
						{
							await FindSpecificNpc(MainNob, ColorCheckWater, token);
							await Task.Delay(300, token);
						}

						await Task.Delay(300, token);
						MainNob.MoveToNPC(_waterMessengerId);
						if (talkCheck % 5 == 0) MainNob.KeyPressT(VKeys.KEY_C, 100);
						talkCheck++;
						if (talkCheck > 25)
						{
							MainNob.Log($"重新接近 NPC {_waterMessengerId} "); // 重新接近NPC
							talkCheck = 0;
							MainNob.後退(500);
							MainNob.MoveToNPC(_waterMessengerId);
							MainNob.KeyPressT(VKeys.KEY_C, 100);
							MainNob.MoveToNPC(_waterMessengerId);
						}
					}

					if (MainNob.對話與結束戰鬥)
					{
						cachedPosX = MainNob.PosX;
						cachedMapId = MainNob.MAPID;
						if (MainNob.出現左右選單)
						{
							waitingForMapChange = true;
							_clearCount++;
							MainNob.Log($"確認下一樓層。"); // 確認下一樓層
							MainNob.KeyPress(VKeys.KEY_J);
							MainNob.KeyPress(VKeys.KEY_ENTER);
							await Task.Delay(1500, token);
							continue;
						}
						else
						{
							if (_clearCount > 6) allBattleDoneCheck++;
							allBattleDoneCheck++;
							MainNob.KeyPress(VKeys.KEY_ENTER);
							await Task.Delay(300, token);
						}
					}

					if (_clearCount > 7 || allBattleDoneCheck > 10)
					{
						MainNob.KeyPress(VKeys.KEY_ESCAPE, 5);
						CurrentStage = ScriptStage.Exit;
						return;
					}
				}
			}
		}

		/// <summary>
		/// 報告任務：處理任務報告邏輯
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task ReportTask(NOBDATA useNOB, CancellationToken token)
		{
			useNOB.Log("報告任務"); // 報告任務
			try
			{
				useNOB.副本回報完成 = false;
				int mErrorCheck = 0;

				while (MainNob!.StartRunCode)
				{
					token.ThrowIfCancellationRequested();
					await Task.Delay(300, token);

					if (MainNob.MAPID != _cachedMapId)
					{
						MainNob.Log("報告期間發現意外進入。"); // 報告期間意外進入
						MainNob.副本進入完成 = true;
						CurrentStage = ScriptStage.FindTarget;
						return;
					}

					if (useNOB.對話與結束戰鬥)
					{
						if (useNOB.出現直式選單)
						{
							if (useNOB.取得最下面選項().Contains("返回"))
							{
								useNOB.直向選擇PP(1);
								await Task.Delay(500, token);
								useNOB.KeyPressPP(VKeys.KEY_ENTER, 5, 100);
								useNOB.KeyPressPP(VKeys.KEY_ESCAPE, 40);
								useNOB.副本回報完成 = true;
								useNOB.Log("任務已回報。"); // 任務已回報
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
							MainNob.Log("錯誤檢查"); // 錯誤檢查
							mErrorCheck = 0;
							if (useNOB.出現左右選單)
							{
								await Task.Delay(500, token);
								useNOB.KeyPress(VKeys.KEY_J);
								useNOB.KeyPress(VKeys.KEY_ENTER);
							}
							else
							{
								useNOB.KeyPress(VKeys.KEY_ESCAPE, 20);
							}
							useNOB.KeyPress(VKeys.KEY_ESCAPE, 15);
							await Task.Delay(200, token);
						}
					}
					else
					{
						if (useNOB.出現左右選單)
						{
							useNOB.Log($"發現意外對話框。"); // 意外對話框
							useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);
							useNOB.KeyPress(VKeys.KEY_ENTER);
							await Task.Delay(300, token);
						}
						useNOB.MoveToNPC(MainNob.CodeSetting.目標A);
					}
				}
			}
			catch (OperationCanceledException)
			{
				useNOB.Log("回報已取消。"); // 報告取消
			}
			catch (Exception ex)
			{
				useNOB.Log($"回報錯誤：{ex.Message}"); // 報告錯誤
			}
		}

		/// <summary>
		/// 接受任務：處理任務接受邏輯
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task AcceptTask(NOBDATA useNOB, CancellationToken token)
		{
			useNOB.Log($"接受任務"); // 接受任務
			int mErrorCheck = 0;
			useNOB.副本進入完成 = false;
			useNOB.Log("尋找接受任務的 NPC: " + useNOB.PlayerName); // 尋找NPC接受任務
			useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);

			while (MainNob!.StartRunCode)
			{
				token.ThrowIfCancellationRequested();
				await Task.Delay(200, token);

				if (MainNob.MAPID != _cachedMapId)
				{
					MainNob.Log("接受期間發現意外進入。"); // 接受期間意外進入
					MainNob.副本進入完成 = true;
					CurrentStage = ScriptStage.FindTarget;
					return;
				}

				if (useNOB.對話與結束戰鬥)
				{
					if (useNOB.出現直式選單)
					{
						if (useNOB.取得最下面選項().Contains("返回"))
						{
							mErrorCheck = 0;
							useNOB.直向選擇(1);
							await Task.Delay(300, token);
							useNOB.KeyPress(VKeys.KEY_ENTER, 10);

							int outCheck = 0;
							int okCheck = 0;
							while (MainNob.StartRunCode)
							{
								useNOB.鎖定NPC(_waterMessengerId);
								await Task.Delay(200, token);
								useNOB.KeyPress(VKeys.KEY_S);
								useNOB.KeyPress(VKeys.KEY_W);

								if (MainNob.GetTargetIDINT() != _waterMessengerId) okCheck++;

								if (okCheck > 2)
								{
									MainNob.Log("已完成進入。"); // 進入完成
									MainNob.副本進入完成 = true;
									CurrentStage = ScriptStage.FindTarget;
									_clearCount = 0;
									return;
								}
								await Task.Delay(400, token);
								outCheck++;
								if (outCheck > 10)
								{
									useNOB.Log("地圖切換逾時，重試。"); // 地圖變更超時，重試
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
						MainNob.Log("錯誤檢查"); // 錯誤檢查
						mErrorCheck = 0;
						if (useNOB.出現左右選單)
						{
							await Task.Delay(500, token);
							useNOB.KeyPress(VKeys.KEY_J);
							useNOB.KeyPress(VKeys.KEY_ENTER);
						}
						else
						{
							useNOB.KeyPress(VKeys.KEY_ESCAPE, 10, 200);
						}
						useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
						await Task.Delay(200, token);
					}
				}
				else
				{
					if (useNOB.出現左右選單)
					{
						await Task.Delay(500, token);
						useNOB.KeyPress(VKeys.KEY_ESCAPE, 5);
						useNOB.KeyPress(VKeys.KEY_ENTER);
					}
					useNOB.MoveToNPC(MainNob.CodeSetting.目標A);
				}
			}
		}

		/// <summary>
		/// 離開副本：處理團隊成員離開副本邏輯
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task LeaveDungeon(NOBDATA useNOB, CancellationToken token)
		{
			useNOB.副本離開完成 = false;
			useNOB.Log("離開副本：" + useNOB.PlayerName); // 離開副本
			useNOB.KeyPress(VKeys.KEY_ESCAPE, 10);

			int checkDone = 0;
			int errorL = 0;
			int cacheID = _waterMessengerId;

			while (MainNob!.StartRunCode)
			{
				token.ThrowIfCancellationRequested();

				if (IsOutsideMap())
				{
					useNOB.Log($"已在外部地圖：{useNOB.MAPID}"); // 已在外部
					return;
				}

				await Task.Delay(500, token);

				if (_isNpcFound && useNOB.GetTargetIDINT() != -1)
				{
					if (useNOB.對話與結束戰鬥)
					{
						await Task.Delay(200, token);
						if (useNOB.出現左右選單)
						{
							int checkmapid = useNOB.MAPID;
							useNOB.Log($"確認退出..."); // 確認退出
							useNOB.KeyPressPP(VKeys.KEY_J);
							await Task.Delay(100, token);
							useNOB.KeyPressPP(VKeys.KEY_ENTER);
							await Task.Delay(100, token);

							while (MainNob.StartRunCode)
							{
								useNOB.KeyPressPP(VKeys.KEY_D);
								await Task.Delay(300, token);
								useNOB.KeyPressPP(VKeys.KEY_W);

								if (checkmapid != useNOB.MAPID)
								{
									useNOB.副本離開完成 = true;
									return;
								}

								checkDone++;
								if (checkDone > 20)
								{
									useNOB.Log($"退出失敗，重試。"); // 退出失敗，重試
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
						{
							useNOB.KeyPress(VKeys.KEY_ESCAPE);
						}
					}
					else
					{
						useNOB.MoveToNPC(cacheID);
						await Task.Delay(500, token);
					}
				}
				else
				{
					await FindSpecificNpc(useNOB, ColorCheckWater, token);
				}
			}
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// 檢查是否在外部地圖
		/// </summary>
		/// <returns>是否在外部地圖</returns>
		private bool IsOutsideMap()
		{
			return Math.Abs(MainNob!.MAPID - _outsideMapId) < 10;
		}

		/// <summary>
		/// 處理對話確認：按下確認鍵多次
		/// </summary>
		/// <param name="count">確認次數</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task HandleDialogConfirm(int count, CancellationToken token)
		{
			for (int i = 0; i < count; i++)
			{
				MainNob!.KeyPress(VKeys.KEY_J);
				MainNob.KeyPress(VKeys.KEY_ENTER);
			}
			await Task.Delay(100, token);
		}

		/// <summary>
		/// 檢查是否有怪物：掃描周圍尋找怪物
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="token">取消令牌</param>
		/// <returns>是否有怪物</returns>
		private async Task<bool> CheckForMonsters(NOBDATA useNOB, CancellationToken token)
		{
			bool hasMonsters = false;
			for (int i = 0; i < 10; i++)
			{
				MainNob!.KeyPressT(i < 5 ? VKeys.KEY_Q : VKeys.KEY_E, 400);
				int tryCount = 0;
				while (MainNob.StartRunCode)
				{
					MainNob.KeyPress(VKeys.KEY_J);
					int targetId = useNOB.GetTargetIDINT();
					if (targetId != _waterMessengerId && targetId != -1)
					{
						hasMonsters = true;
						break;
					}
					tryCount++;
					await Task.Delay(50, token);
					if (tryCount > 4) break;
				}
				if (hasMonsters) break;
			}
			return hasMonsters;
		}

		/// <summary>
		/// 處理戰鬥結算：等待所有成員離開戰鬥狀態
		/// </summary>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task HandleBattleSettlement(CancellationToken token)
		{
			var tasks = new List<Task>();
			foreach (var nob in NobTeam)
			{
				nob.Log("結算階段"); // 結算階段
				tasks.Add(nob.離開戰鬥C());
			}
			await Task.WhenAll(tasks);

			foreach (var nob in NobTeam)
			{
				nob.IsUseAutoSkill = false;
				nob.放技能完成 = false;
				nob.已經放過一次 = false;
			}

			while (MainNob!.StartRunCode)
			{
				bool allIdle = true;
				foreach (var item in NobTeam)
				{
					if (!item.待機)
					{
						item.KeyPressPP(VKeys.KEY_ESCAPE, 3);
						allIdle = false;
					}
				}
				if (allIdle) break;
				await Task.Delay(200, token);
			}
		}

		/// <summary>
		/// 尋找特定NPC：根據顏色檢查尋找NPC
		/// </summary>
		/// <param name="useNOB">使用的NOB數據</param>
		/// <param name="colorNum">顏色編號</param>
		/// <param name="token">取消令牌</param>
		/// <returns>非同步任務</returns>
		private async Task FindSpecificNpc(NOBDATA useNOB, int colorNum, CancellationToken token)
		{
			int tryGet = 0;
			int tryGetOutLine = 0;

			while (MainNob!.StartRunCode)
			{
				token.ThrowIfCancellationRequested();

				if (_waterMessengerId != -1)
				{
					useNOB.鎖定NPC(_waterMessengerId);
					await Task.Delay(100, token);
					if (useNOB.GetTargetIDINT() == _waterMessengerId)
					{
						_isNpcFound = true;
						return;
					}
					else
					{
						_waterMessengerId = -1;
						useNOB.KeyPress(VKeys.KEY_J);
						await Task.Delay(100, token);
					}
				}

				var c2 = ColorTools.GetColorNum(useNOB.Proc.MainWindowHandle, new System.Drawing.Point(900, 70), new System.Drawing.Point(100, 70), "F6F67A");
				await Task.Delay(100, token);

				if (c2 == colorNum)
				{
					_isNpcFound = true;
					_waterMessengerId = useNOB.GetTargetIDINT();
					return;
				}
				else
				{
					useNOB.KeyPress(VKeys.KEY_J);
					tryGet++;
				}

				if (tryGet > 3)
				{
					useNOB.KeyPressT(VKeys.KEY_E, 300);
					tryGet = 0;
					tryGetOutLine++;
				}

				if (tryGetOutLine > 2)
				{
					tryGetOutLine = 0;
					MainNob.鎖定NPC(MainNob.CodeSetting.目標A);
					await Task.Delay(100, token);
					if (MainNob.GetTargetIDINT() == MainNob.CodeSetting.目標A)
					{
						_outsideMapId = MainNob.MAPID;
						CurrentStage = ScriptStage.Entrance;
						return;
					}
				}
			}
		}

		#endregion
	}
}
