using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using NOBApp.Sports;
using NOBApp.Memory;
using static NOBApp.NobMainCodePage;

namespace NOBApp
{
    public class NOBDATA : NOBBehavior
    {
        private MemoryReaderFacade? _memory;

        private bool TryEnsureMemoryReaderInitialized()
        {
            if (_memory is not null)
                return _memory.IsWin32Available;

            try
            {
                var baseAddr = Proc.MainModule?.BaseAddress ?? IntPtr.Zero;
                _memory = new MemoryReaderFacade(Proc.Id, (nuint)baseAddr.ToInt64());
                return _memory.IsWin32Available;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryEnsureMemoryReaderInitialized failed: {ex}");
                _memory = null;
                return false;
            }
        }

        private void EnsureMemoryReaderInitialized()
        {
            _ = TryEnsureMemoryReaderInitialized();
        }

        private CancellationTokenSource? _hfCts;
        private volatile string _stateARawCache = string.Empty;
        private volatile int _mapIdCache;
        private volatile int _posXCache;
        private volatile int _posYCache;
        private volatile int _posHCache;
        private volatile float _camXCache;
        private volatile float _camYCache;

        private void StartHighFrequencyCache()
        {
            _hfCts?.Cancel();
            _hfCts = new CancellationTokenSource();
            _ = Task.Run(() => HighFrequencyLoopAsync(_hfCts.Token));
        }

        private async Task HighFrequencyLoopAsync(CancellationToken token)
        {
            const int periodMs = 50;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!TryEnsureMemoryReaderInitialized())
                    {
                        await Task.Delay(500, token);
                        continue;
                    }

                    _stateARawCache = ReadData(GetFullAddress(AddressData.判別狀態A), 2);
                    _mapIdCache = ReadInt(GetFullAddress(AddressData.地圖位置), 1);
                    _posXCache = ReadInt(GetFullAddress(AddressData.地圖座標X), 0);
                    _posYCache = ReadInt(GetFullAddress(AddressData.地圖座標Y), 0);
                    _posHCache = ReadInt(GetFullAddress(AddressData.地圖座標H), 0);
                    _camXCache = ReadFloat(GetFullAddress(AddressData.攝影機角度A));
                    _camYCache = ReadFloat(GetFullAddress(AddressData.攝影機角度B));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"HighFrequencyLoopAsync error: {ex}");
                    try { await Task.Delay(500, token); } catch { }
                    continue;
                }

                await Task.Delay(periodMs, token);
            }
        }
        private CancellationTokenSource? _infoCts;
        private string _cachedAccount = string.Empty;
        private string _cachedPassword = string.Empty;
        private string _cachedPlayerName = string.Empty;

        public NOBDATA(Process proc) : base(proc)
        {
            int attempts = 0;
            const int maxAttempts = 10;

            while (attempts < maxAttempts)
            {
                try
                {
                    if (GetWindowRect(Proc.MainWindowHandle, out RECT rect))
                    {
                        原視窗 = rect;
                        NowWidth = rect.Right - rect.Left;
                        NowHeight = rect.Bottom - rect.Top;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NOBDATA] GetWindowRect failed attempt {attempts}: {ex.Message}");
                }

                attempts++;
                if (attempts >= maxAttempts)
                {
                    Debug.WriteLine($"[NOBDATA] GetWindowRect failed after {maxAttempts} attempts, using default size");
                    NowWidth = 800;
                    NowHeight = 600;
                    break;
                }

                Task.Delay(100).Wait();
            }

            _infoCts?.Cancel();
            _infoCts = new CancellationTokenSource();
            _ = Task.Run(() => RefreshBasicInfoLoop(_infoCts.Token));

            LogID = "(loading)";

            StartHighFrequencyCache();
        }

        private async Task RefreshBasicInfoLoop(CancellationToken token)
        {
            try
            {
                int attempts = 0;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (!TryEnsureMemoryReaderInitialized())
                        {
                            attempts++;
                            if (attempts <= 3)
                                Debug.WriteLine($"[NOBDATA] RefreshBasicInfoLoop: memory reader not available, attempt {attempts}");
                            await Task.Delay(attempts < 3 ? 500 : 1000, token);
                            continue;
                        }

                        var accAddr = GetFullAddress(AddressData.Acc);
                        var nameAddr = GetFullAddress(AddressData.角色名稱);

                        var acc = NormalizeAccount(ReadString(accAddr, 0, 15));
                        var pas = NormalizeAccount(ReadString(GetFullAddress(AddressData.Pas), 0, 15));
                        // 角色名稱最多 6 個中文字
                        var name = SanitizeDisplay(ReadString(nameAddr, 1, 6));

                        if (!string.IsNullOrEmpty(acc)) _cachedAccount = acc;
                        if (!string.IsNullOrEmpty(pas)) _cachedPassword = pas;
                        if (!string.IsNullOrEmpty(name)) _cachedPlayerName = name;

                        if (!string.IsNullOrEmpty(_cachedAccount) && LogID == "(loading)")
                        {
                            LogID = _cachedAccount;
                        }

                        attempts++;

                        if (!string.IsNullOrEmpty(_cachedAccount) && !string.IsNullOrEmpty(_cachedPlayerName))
                        {
                            await Task.Delay(5000, token);
                        }
                        else
                        {
                            await Task.Delay(attempts < 3 ? 200 : 1000, token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[NOBDATA] RefreshBasicInfoLoop error: {ex.Message}");
                        await Task.Delay(1000, token);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NOBDATA] RefreshBasicInfoLoop fatal: {ex}");
            }
        }

        /// <summary>
        /// 取得應用程式畫面
        /// </summary>
        /// <param name="hWnd">程序</param>
        /// <param name="bounds">範圍</param>
        /// <returns></returns>
        #region DllImport
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion

        public Setting CodeSetting = new();
        public 自動技能組 AutoSkillSet = new();
        public int CurrentRound { get; set; } = 1;
        private bool _wasInBattle = false;
        public DateTime 到期日 = DateTime.Now.AddYears(99);
        public RECT 原視窗;
        public int NowHeight;
        public int NowWidth;
        public bool VIPSP = false;
        public bool 追蹤 = false;
        private static readonly Random _random = new Random();

        #region 記憶體讀取位置
        private const string BASE_ADDRESS = "<nobolHD.bng> +";
        private readonly Dictionary<string, string> _addressCache = new();
        private string GetFullAddress(string address) =>
            _addressCache.TryGetValue(address, out var fullAddress)
                ? fullAddress
                : _addressCache[address] = $"{BASE_ADDRESS}{address}";

        // 角色基本資訊: return cached first to avoid blocking UI
        public string Account
        {
            get
            {
                var acc = !string.IsNullOrEmpty(_cachedAccount) ? _cachedAccount : ReadString(GetFullAddress(AddressData.Acc), 0, 15);
                return NormalizeAccount(acc);
            }
        }
        public string Password => !string.IsNullOrEmpty(_cachedPassword) ? _cachedPassword : ReadString(GetFullAddress(AddressData.Pas), 0, 15);
        public string PlayerName
        {
            get
            {
                // 角色名稱最多 6 個中文字
                var name = !string.IsNullOrEmpty(_cachedPlayerName) ? _cachedPlayerName : ReadString(GetFullAddress(AddressData.角色名稱), 1, 6);
                return SanitizeDisplay(name);
            }
        }

        // 同步直接讀取（需要即時值時使用）
        public string ReadAccountNow() => NormalizeAccount(ReadString(GetFullAddress(AddressData.Acc), 0, 15));
        public string ReadPlayerNameNow() => SanitizeDisplay(ReadString(GetFullAddress(AddressData.角色名稱), 1, 6));

        // 位置和地圖資訊
        public int MAPID => _mapIdCache != 0 ? _mapIdCache : ReadInt(GetFullAddress(AddressData.地圖位置), 1);
        public int PosX => _posXCache != 0 ? _posXCache : ReadInt(GetFullAddress(AddressData.地圖座標X), 0);
        public int PosH => _posHCache != 0 ? _posHCache : ReadInt(GetFullAddress(AddressData.地圖座標H), 0);
        public int PosY => _posYCache != 0 ? _posYCache : ReadInt(GetFullAddress(AddressData.地圖座標Y), 0);
        public float CamX => _camXCache != 0 ? _camXCache : ReadFloat(GetFullAddress(AddressData.攝影機角度A));
        public float CamY => _camYCache != 0 ? _camYCache : ReadFloat(GetFullAddress(AddressData.攝影機角度B));

        // UI 狀態
        public string 取得最下面選項(int num = 4) => ReadString(GetFullAddress(AddressData.直選框文字), 1, num);
        public bool 任務選擇框 => IsInState(GameState.QuestSelect);
        public bool 對話與結束戰鬥 => IsInState(GameState.Dialog);
        public bool 待機 => IsInState(GameState.Idle);
        public bool 戰鬥中 => IsInState(GameState.InBattle);

        public int 戰鬥中判定 = -1;

        // 結算相關
        public bool 進入結算
        {
            get
            {
                if (戰鬥中判定 >= 0 && IsInState(GameState.Dialog))
                {
                    戰鬥中判定++;
                    Task.Delay(100).Wait();
                }
                return 戰鬥中判定 > 3;
            }
        }
        // 視角相關
        // 修改後
        public bool 第三人稱
        {
            get => ReadInt(GetFullAddress(AddressData.視角), 0) == 0;
            set
            {
                WriteInt(GetFullAddress(AddressData.視角), 0, value ? 0 : 1);
            }
        }
        public bool 輸入數量視窗 => ReadInt(GetFullAddress(AddressData.輸入數量視窗), 0) == 39 || ReadInt(GetFullAddress(AddressData.輸入數量視窗), 0) == 34;
        // 觀察與交互系統
        public string 觀察對象Str => ReadData(GetFullAddress(AddressData.是否有觀察對象), 2);
        public bool 有觀察對象 => !ReadData(GetFullAddress(AddressData.是否有觀察對象), 2).Contains("FF FF");
        public int 確認選單 => ReadInt(GetFullAddress(AddressData.直選框), 1);
        public int 製作Index => ReadInt(GetFullAddress(AddressData.製作Index), 1);
        public bool 出現左右選單 => ReadInt(GetFullAddress(AddressData.直選框), 0) == 2;
        public bool 出現直式選單 => ReadInt(GetFullAddress(AddressData.直選框), 0) == 1;
        public string StateA => StateARaw;
        public bool ResetPoint = false;


        public enum GameState
        {
            Unknown = 0,
            InBattle = 1,      // A0 98 - 戰鬥中
            Idle = 2,          // F0 B8 - 野外待機
            /// <summary>
            /// 對話框或戰鬥結束
            /// </summary>
            Dialog = 3,        // F0 F8 - 對話或戰鬥結束
            QuestSelect = 4    // E0 F0 - 任務選擇框
        }
        // 2. 修改 StateA 相關實現
        private string _rawStateA = string.Empty;
        private GameState _currentState = GameState.Unknown;
        private DateTime _lastStateCheck = DateTime.MinValue;
        private const int STATE_CACHE_MS = 50; // 狀態緩存 50 毫秒
        /// <summary>
        /// 獲取原始狀態字串
        /// </summary>
        public string StateARaw
        {
            get
            {
                //Log($"{DateTime.Now} Update Raw - {_rawStateA}");
                // 檢查是否需要更新狀態緩存
                if ((DateTime.Now - _lastStateCheck).TotalMilliseconds > STATE_CACHE_MS)
                {
                    _rawStateA = ReadData(GetFullAddress(AddressData.判別狀態A), 2);
                    UpdateGameState();
                    _lastStateCheck = DateTime.Now;
                }
                return _rawStateA;
            }
        }
        /// <summary>
        /// 獲取當前遊戲狀態
        /// </summary>
        public GameState CurrentState => _currentState;

        /// <summary>
        /// 檢查當前是否處於指定狀態
        /// </summary>
        public bool IsInState(GameState state)
        {
            if ((DateTime.Now - _lastStateCheck).TotalMilliseconds > STATE_CACHE_MS)
            {
                速度();
                _rawStateA = ReadData(GetFullAddress(AddressData.判別狀態A), 2);
                UpdateGameState();
                _lastStateCheck = DateTime.Now;
            }
            //Debug.WriteLine($"Update -> {StateARaw}");
            return _currentState == state;
        }

        public bool 場上超過10人()
        {
            string data = ReadData(GetFullAddress(AddressData.戰鬥人數判斷.AddressAdd(40)), 4);
            //Debug.WriteLine(data);
            return data.Contains("00 00 00 00") == false;
        }

        /// <summary>
        /// 更新遊戲狀態
        /// </summary>
        private void UpdateGameState()
        {
            _currentState = ParseState(_rawStateA);
            if (_currentState == GameState.InBattle)
            {
                戰鬥中判定 = 0;
            }
        }

        private static GameState ParseState(string raw)
        {
            if (raw.Contains("A0 98")) return GameState.InBattle;
            if (raw.Contains("F0 B8")) return GameState.Idle;
            if (raw.Contains("F0 F8")) return GameState.Dialog;
            if (raw.Contains("E0 F0")) return GameState.QuestSelect;
            return GameState.Unknown;
        }
		// 驗證功能
		public bool 驗證國家
		{
			get
			{
				// 需要檢查的位址陣列，結構為 [baseAddress, offset1, offset2...]
				string[][] addressPairs = new string[][]
				{
					new[] { AddressData.頻道認證A, "0", "192", "384" }
				};

				foreach (var basePair in addressPairs)
				{
					string baseAddr = basePair[0];
					for (int i = 1; i < basePair.Length; i++)
					{
						string offset = basePair[i];
						string addr;

						// 安全解析 offset，避免拋出異常
						if (offset == "0")
							addr = baseAddr;
						else if (int.TryParse(offset, out var offVal))
							addr = baseAddr.AddressAdd(offVal);
						else
							addr = baseAddr; // fallback

						// 使用更健壯的讀取與比對方法
						if (驗證國家字串包含("胖鵝科技", addr))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		private bool 驗證國家字串包含(string 搜尋字串, string address)
		{
			try
			{
				string fullAddr = GetFullAddress(address);
				int attempts = 3;
				for (int i = 0; i < attempts; i++)
				{
					try
					{
						var result = ReadString(fullAddr, 1, 16);
						Debug.WriteLine(result);
						if (!string.IsNullOrEmpty(result) && result.Contains(搜尋字串))
							return true;
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"ReadString exception @{fullAddr}: {ex.Message}");
					}
					Task.Delay(100).Wait();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"驗證字串包含失敗: {ex.Message}");
			}

			return false;
		}

		// 目標處理
		public int GetTargetIDINT()
		{
			EnsureMemoryReaderInitialized();
			if (_memory is { IsWin32Available: true })
			{
				try
				{
					return _memory.ReadInt32Async(GetFullAddress(AddressData.選擇項目)).GetAwaiter().GetResult();
				}
				catch
				{
					return 0;
				}
			}
			return 0;
		}

		public int GetTargetClass()
		{
			EnsureMemoryReaderInitialized();
			if (_memory is { IsWin32Available: true })
			{
				try
				{
					return _memory.ReadInt16Async(GetFullAddress(AddressData.選擇項目.AddressAdd(3))).GetAwaiter().GetResult();
				}
				catch
				{
					return 0;
				}
			}
			return 0;
		}

		// 基礎讀取方法
		private string ReadString(string address, int type, int length)
		{
			EnsureMemoryReaderInitialized();

			if (_memory is { IsWin32Available: true })
			{
				try
				{
					// type=0: ASCII (帳號/密碼) - 每字元 1 byte
					if (type == 0)
					{
						int byteLength = Math.Max(length, 1);
						var asciiResult = _memory.ReadStringAsync(address, byteLength, Encoding.ASCII).GetAwaiter().GetResult();
						return asciiResult?.Replace("\0", string.Empty).Trim() ?? string.Empty;
					}

					// type=1: Unicode UTF-16 LE (角色名稱等)
					// 遊戲使用 Unicode 編碼，每個字元 2 bytes
					int readLength = Math.Max(length, 1) * 2;
					var rawBytes = _memory.ReadBytes(address, readLength);
					
					if (rawBytes == null || rawBytes.Length == 0)
						return string.Empty;

					// 使用 Unicode (UTF-16 LE) 解碼
					string unicodeResult = Encoding.Unicode.GetString(rawBytes);
					
					// 找到 null 結束符並截斷
					int nullIdx = unicodeResult.IndexOf('\0');
					if (nullIdx >= 0)
						unicodeResult = unicodeResult.Substring(0, nullIdx);
					
					return unicodeResult.Trim();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ReadString failed: {ex.Message}");
				}
			}

			return string.Empty;
		}

		private int ReadInt(string address, int type)
		{
			EnsureMemoryReaderInitialized();

			if (_memory is { IsWin32Available: true })
			{
				try
				{
					return type switch
					{
						2 => _memory.ReadInt16Async(address).GetAwaiter().GetResult(),
						_ => _memory.ReadInt32Async(address).GetAwaiter().GetResult()
					};
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ReadInt failed: {ex.Message}");
				}
			}

			return 0;
		}

		private float ReadFloat(string address)
		{
			EnsureMemoryReaderInitialized();

			if (_memory is { IsWin32Available: true })
			{
				try
				{
					return _memory.ReadSingleAsync(address).GetAwaiter().GetResult();
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ReadFloat failed: {ex.Message}");
				}
			}

			return 0.0f;
		}

		private string ReadData(string address, int type)
		{
			EnsureMemoryReaderInitialized();

			if (_memory is { IsWin32Available: true })
			{
				try
				{
					int lengthBytes = Math.Max(type, 1);
					return _memory.ReadDataHex(address, lengthBytes);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"ReadData failed: {ex.Message}");
				}
			}

			return string.Empty;
		}

        public int ReadIntValue(string address, int type) => ReadInt(address, type);
        public float ReadFloatValue(string address) => ReadFloat(address);
        public bool WriteIntValue(string address, int type, int value) => WriteInt(address, type, value);
        public bool WriteStringValue(string address, int type, string value) => WriteString(address, type, value);
        public bool WriteHexValue(string address, string hex) => WriteBytesFromHex(address, hex);

		public string ReadDataHex(string address, int lengthBytes) => ReadData(address, lengthBytes);
		#endregion

		public bool 特殊者 = false;
		public bool 贊助者 = false;
		public bool 驗證完成 = false;
		public float 比例 = 1;

		public bool 副本進入完成 = false;
		public bool 副本離開完成 = false;
		public bool 副本回報完成 = false;

		public string NOBCDKEY = "";
		public BaseClass? RunCode;
		public List<BTData> MYTeamData = new List<BTData>();
		public List<BTData> EMTeamData = new List<BTData>();
		public List<long> SetSkillsID = new List<long>();
		public List<string> SKNames = new List<string>();
		public bool 離開戰鬥確認 = false;
		public bool 完成必須對話 = false;
		public bool 啟動自動輔助中 = false;
		public bool 準備完成 = false;
		public bool 自動追蹤隊長 = false;
		public bool 自動結束_A = false;
		public bool 自動結束_B = false;
		public bool 希望取得 = false;
		/// <summary>
		/// 判斷是否開始隨機地圖上打怪
		/// </summary>
		public bool 開打 = false;
		public bool F5解無敵 = false;
		//使用 Enter 點怪(舊式)
		public bool isUseEnter = false;
		int errorCheckCount = 0;
		string cacheStatus = "";
		public bool 已經放過一次 = false;
		public bool 放技能完成 = false;
		bool isBanAccount
		{
			get
			{
				if (Account == "" || Account.Contains("li365523") || Account.Contains("jilujilu") || Account.Contains("zhangkasim") ||
					Account.Contains("lz19860212") || Account.Contains("wx2002") || Account.Contains("yesterdayyk") ||
					Account.Contains("li365522") || Account.Contains("nbzhouyi") || Account.Contains("zhao17371892972") ||
					Account.Contains("yamufg") || Account.Contains("wcy20240805"))
				{
					return true;
				}

				return false;
			}
		}

		public void CloseGame()
		{
			_hfCts?.Cancel();
			_infoCts?.Cancel();
			Proc.Kill();
		}
		public void ClearBTData()
		{
			SetSkillsID.Clear();
			MYTeamData.Clear();
			EMTeamData.Clear();
			SKNames.Clear();
		}
		public void CodeUpdate()
		{
			if (RunCode == null)
				return;

			bool _init = false;
			while (StartRunCode && RunCode != null)
			{
				if (Tools.isBANACC || isBanAccount)
				{
					var dt = Random.Shared.Next(1000, 5000);
					Task.Delay(dt).Wait();
				}

				if (StartRunCode == false)
					break;

				if (!_init)
				{
					_init = true;
					RunCode.SetMainUser(this);
					RunCode.初始化();
					RunCode.顯示顏色提示();
				}

				RunCode.腳本運作();

               // 非VIP使用VIP腳本時，整體速度放慢
               int loopDelay = Tools.GetVipDelay(200, RunCode.需要VIP);
               if (loopDelay > 0)
               {
                   Task.Delay(loopDelay).Wait();
               }

                if (cacheStatus != StateARaw)
                {
                    cacheStatus = StateARaw;
                    errorCheckCount = 0;
                }
                else
                {
                    Task.Delay(50).Wait();
                    errorCheckCount++;
                    if (errorCheckCount > 10000)
                    {
                        errorCheckCount = 0;
                        StartRunCode = false;
                        string msg = $"{RunCode?.GetType().Name ?? "無腳本"} 狀態長時間沒有變化 需要請企鵝確認";
                        if (Tools.IsVIP)
                        {
                            _ = DiscordNotifier.SendNotificationAsync(PlayerName, msg);
                        }
                        System.Windows.MessageBox.Show($"{PlayerName} -> {msg}");
                        System.Windows.MessageBox.Show($"{PlayerName} -> {msg}");
                    }
                }
			}

		}

		public async Task BattleUpdate()
		{
			Log($"1 啟動自動輔助中");
			if (啟動自動輔助中)
				return;
			Log($"2 啟動自動輔助中");
			啟動自動輔助中 = true;

			// Debug: print current AutoSkillSet values to help trace configuration
			try
			{
				Debug.WriteLine($"[AutoSkillSet] {PlayerName} -> 同步={AutoSkillSet.同步}, 一次放={AutoSkillSet.一次放}, 重複放={AutoSkillSet.重複放}, 需選擇={AutoSkillSet.需選擇}, 搖屁股={AutoSkillSet.搖屁股}, 背景Enter={AutoSkillSet.背景Enter}, 延遲={AutoSkillSet.延遲}, 間隔={AutoSkillSet.間隔}, 技能段1={AutoSkillSet.技能段1}, 技能段2={AutoSkillSet.技能段2}, 技能段3={AutoSkillSet.技能段3}, 施放A={AutoSkillSet.施放A}, 施放B={AutoSkillSet.施放B}, 施放C={AutoSkillSet.施放C}, 程式速度={AutoSkillSet.程式速度}, 特殊運作={AutoSkillSet.特殊運作}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to log AutoSkillSet for {PlayerName}: {ex.Message}");
			}


			bool 希望完成 = false;
			已經放過一次 = false;
			放技能完成 = false;
			while (IsUseAutoSkill)
			{
				//Log($"IsUseAutoSkill->{IsUseAutoSkill}");

				if (AutoSkillSet.背景Enter)
				{
					KeyPressPP(VKeys.KEY_ENTER);
					Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
					continue;
				}

				bool isInBattle = 戰鬥中;
				if (isInBattle && !_wasInBattle)
				{
					CurrentRound = 1;
					// Debug.WriteLine($"Battle Start, Round {CurrentRound}");
				}
				_wasInBattle = isInBattle;

				if (isInBattle)
				{
					希望完成 = false;
					處理戰鬥流程();
				}
				#region 對話框出現 + 戰鬥結束
				if (進入結算)
				{
					//Debug.WriteLine($"進入結算");
					放技能完成 = false;
					已經放過一次 = false;
					if (希望取得 && 希望完成 == false)
					{
						希望完成 = true;
						Task.Delay(1000).Wait();
						KeyPress(VKeys.KEY_ENTER, 6, 300);
						Task.Delay(100).Wait();
					}
					if (自動結束_A)
					{
						離開戰鬥A();
					}
					if (自動結束_B)
					{
						離開戰鬥B();
					}

					if (追蹤)
					{
						KeyPressPP(VKeys.KEY_F8);
					}
				}
				#endregion
				if (對話與結束戰鬥)
				{
					#region 對話與結束戰鬥
					await Task.Delay(AutoSkillSet.程式速度);
					#endregion
				}

				Task.Delay(AutoSkillSet.程式速度 <= 0 ? 100 : AutoSkillSet.程式速度).Wait();
			}
			//Log($"啟動自動輔助中 關閉->{IsUseAutoSkill}");
			啟動自動輔助中 = false;
		}

		bool _IsRuning = false;
		public void 處理戰鬥流程(bool isSkipCheck = false)
		{
			int checkDo = 0;

			// Determine configuration for current round
			bool useRoundConfig = AutoSkillSet.RoundConfigs.TryGetValue(CurrentRound, out var roundConfig);

			bool config_重複放 = useRoundConfig ? roundConfig.重複放 : AutoSkillSet.重複放;
			bool config_一次放 = useRoundConfig ? roundConfig.一次放 : AutoSkillSet.一次放;
			int config_延遲 = useRoundConfig ? roundConfig.延遲 : AutoSkillSet.延遲;
			int config_技能段1 = useRoundConfig ? roundConfig.技能段1 : AutoSkillSet.技能段1;
			int config_技能段2 = useRoundConfig ? roundConfig.技能段2 : AutoSkillSet.技能段2;
			int config_技能段3 = useRoundConfig ? roundConfig.技能段3 : AutoSkillSet.技能段3;
			string config_施放A = useRoundConfig ? roundConfig.施放A : AutoSkillSet.施放A;
			string config_施放B = useRoundConfig ? roundConfig.施放B : AutoSkillSet.施放B;
			string config_施放C = useRoundConfig ? roundConfig.施放C : AutoSkillSet.施放C;
			int config_程式速度 = useRoundConfig ? roundConfig.程式速度 : AutoSkillSet.程式速度;

			//Log($"進入戰鬥中{_IsRuning} {IsUseAutoSkill} {config_一次放 || config_重複放}");
			#region 戰鬥中
			//目前選數量 
			var index = ReadInt(GetFullAddress(AddressData.戰鬥可輸入判斷), 2);
			//if (index > 0 && (MYTeamData == null || MYTeamData.Count == 0))
			//{
			//    BtDataUpdate();
			//}

			if (config_一次放 || config_重複放)
			{
				if (_IsRuning)
					return;

				//Log($"有指令 準備進入指令 - 1");
				_IsRuning = true;
				do
				{
					index = ReadInt(GetFullAddress(AddressData.戰鬥可輸入判斷), 2);
					string supDataCheck = ReadData(GetFullAddress(AddressData.戰鬥可輸入判斷II), 1);
					if (supDataCheck.Length > 0 && supDataCheck.Substring(supDataCheck.Length - 1).Contains("4"))
					{
						string newD = supDataCheck[0] + "0";
						WriteBytesFromHex(GetFullAddress(AddressData.戰鬥可輸入判斷II), newD);
					}
					//Log($"有指令 準備進入指令 - 2 -> Index {index} 放過依次 : {已經放過一次} SP:{AutoSkillSet.特殊運作} {config_技能段1} {config_技能段2}");
					if (index > 0)
					{
						if (已經放過一次)
							break;

						if (config_延遲 > 0)
						{
							Task.Delay(config_延遲).Wait();
						}
						if (AutoSkillSet.技能段1 > -1)
						{
							int setindex = ReadInt(GetFullAddress(AddressData.戰鬥輸入), 2);
							Log("setindex : " + setindex);

							Task.Delay(config_程式速度).Wait();
							if (setindex == 0)
							{
								直向選擇(config_技能段1, config_程式速度, isSkipCheck);
							}
							if (setindex == 1 && config_技能段2 >= 0)
							{
								直向選擇(config_技能段2, config_程式速度, isSkipCheck);
							}

							int setNum = -1;
							if (setindex == 2)
							{
								if (config_技能段3 >= 0)
									直向選擇(config_技能段3, config_程式速度, isSkipCheck);

								if (string.IsNullOrEmpty(config_施放A) == false)
								{
									setNum = check(MYTeamData, config_施放A);
									直向選擇(setNum == -1 ? 0 : setNum, config_程式速度, isSkipCheck);
								}
								else
								{
									KeyPress(VKeys.KEY_ENTER);
									KeyPress(VKeys.KEY_K);
								}
							}

							if (setindex == 3)
							{
								if (string.IsNullOrEmpty(config_施放A) == false)
								{
									setNum = check(MYTeamData, config_施放A);
									直向選擇(setNum == -1 ? 0 : setNum, config_程式速度, isSkipCheck);
								}

								if (string.IsNullOrEmpty(config_施放B) == false)
								{
									setNum = check(MYTeamData, config_施放B);
									直向選擇(setNum == -1 ? 0 : setNum, config_程式速度, isSkipCheck);
								}
								else
								{
									KeyPress(VKeys.KEY_ENTER);
									KeyPress(VKeys.KEY_K);
								}
							}

							if (setindex == 4)
							{
								if (string.IsNullOrEmpty(config_施放B) == false)
								{
									setNum = check(MYTeamData, config_施放B);
									直向選擇(setNum == -1 ? 1 : setNum, config_程式速度, isSkipCheck);
								}

								if (string.IsNullOrEmpty(config_施放C) == false)
								{
									setNum = check(MYTeamData, config_施放C);
									直向選擇(setNum == -1 ? 1 : setNum, config_程式速度, isSkipCheck);
								}
								else
								{
									KeyPress(VKeys.KEY_ENTER);
									KeyPress(VKeys.KEY_K);
								}
							}

							if (setindex == 5)
							{
								if (string.IsNullOrEmpty(config_施放C) == false)
								{
									setNum = check(MYTeamData, config_施放C);
									直向選擇(setNum == -1 ? 2 : setNum, config_程式速度, isSkipCheck);
								}
								else
								{
									KeyPress(VKeys.KEY_ENTER);
								 KeyPress(VKeys.KEY_K);
								}
							}

						}
						else
						{
							BT_Cmd();
							Task.Delay(100).Wait();
							if (AutoSkillSet.需選擇 || (index > 1 && (checkDo == 0 || checkDo > 5)))
							{
								Task.Delay(50).Wait();
								if (index > 1)
								{
									KeyPress(VKeys.KEY_ENTER, 2);
									if (checkDo > 7)
										checkDo = 0;
								}
							}
							checkDo++;
						}

						放技能完成 = true;
					}

					if (index <= 0)
					{
						checkDo = 0;
						if (放技能完成)
						{
							if (config_一次放)
								已經放過一次 = true;

							CurrentRound++;
						}
						break;
					}
					Task.Delay(config_程式速度).Wait();
				}
				while ((isSkipCheck || IsUseAutoSkill) && index > 0);
				_IsRuning = false;
			}

			#endregion 戰鬥中
		}
		// 處理戰鬥結束動作
		private void ProcessBattleEndActions()
		{
			if (自動結束_A)
			{
				離開戰鬥A();
			}

			if (自動結束_B)
			{
				離開戰鬥B();
			}
		}

		// 處理技能施放
		private async Task ProcessSkillExecution(long index, bool 已放過, bool 放完成)
		{
			do
			{
				// 讀取戰鬥狀態
				index = ReadInt(GetFullAddress(AddressData.戰鬥可輸入判斷), 2);

				// 檢查是否需要更新輔助數據
				ProcessSupplementaryData();

				// 判斷是否可以輸入
				if (index > 0 && !已放過)
				{
					// 處理技能延遲
					if (AutoSkillSet.延遲 > 0)
					{
						await Task.Delay(AutoSkillSet.延遲);
					}

					// 根據不同模式執行技能
					if (AutoSkillSet.特殊運作)
					{
						await ExecuteSpecialSkills();
					}
					else
					{
						await ExecuteStandardSkills();
					}

					放完成 = true;
				}

				// 如果無法輸入，或已完成後需退出
				if (index <= 0 || (放完成 && AutoSkillSet.一次放))
				{
					if (放完成 && AutoSkillSet.一次放)
						已放過 = true;
					break;
				}

				await Task.Delay(AutoSkillSet.程式速度);
			}
			while (IsUseAutoSkill && index > 0);
		}
		// 處理輔助數據
		private void ProcessSupplementaryData()
		{
			string supDataCheck = ReadData(GetFullAddress(AddressData.戰鬥可輸入判斷II), 1);
			if (supDataCheck.Length > 0 && supDataCheck[supDataCheck.Length - 1].ToString().Contains("4"))
			{
				string newD = supDataCheck.Length > 0 ? supDataCheck[0] + "0" : "00";
				WriteBytesFromHex(GetFullAddress(AddressData.戰鬥可輸入判斷II), newD);
			}
		}

		// 執行特殊技能
		private async Task ExecuteSpecialSkills()
		{
			int setindex = (int)ReadInt(GetFullAddress(AddressData.戰鬥輸入), 2);

			// 延遲以確保操作同步
			await Task.Delay(AutoSkillSet.程式速度);

			// 根據不同階段選擇技能
			switch (setindex)
			{
				case 0:
					直向選擇(AutoSkillSet.技能段1, AutoSkillSet.程式速度);
					break;
				case 1:
					if (AutoSkillSet.技能段2 >= 0)
						直向選擇(AutoSkillSet.技能段2, AutoSkillSet.程式速度);
					break;
				case 2:
					await HandleSkillStage(2);
					break;
				case 3:
					await HandleSkillStage(3);
					break;
				case 4:
					await HandleSkillStage(4);
					break;
				case 5:
					await HandleSkillStage(5);
					break;
			}
		}

		// 處理各階段技能
		private async Task HandleSkillStage(int stage)
		{
			switch (stage)
			{
				case 2:
					if (AutoSkillSet.技能段3 >= 0)
						直向選擇(AutoSkillSet.技能段3, AutoSkillSet.程式速度);

					await SelectTeamMember(AutoSkillSet.施放A, 0);
					break;

				case 3:
					await SelectTeamMember(AutoSkillSet.施放A, 0);

					if (!string.IsNullOrEmpty(AutoSkillSet.施放B))
					{
						await SelectTeamMember(AutoSkillSet.施放B, 0);
					}
					else
					{
						直向選擇(1, AutoSkillSet.程式速度);
						直向選擇(0, AutoSkillSet.程式速度);
					}
					break;

				case 4:
					await SelectTeamMember(AutoSkillSet.施放B, 1);

					if (!string.IsNullOrEmpty(AutoSkillSet.施放C))
					{
						await SelectTeamMember(AutoSkillSet.施放C, 1);
					}
					else
					{
						直向選擇(2, AutoSkillSet.程式速度);
						直向選擇(1, AutoSkillSet.程式速度);
					}
					break;

				case 5:
					await SelectTeamMember(AutoSkillSet.施放C, 2);
					break;
			}
		}

		// 選擇團隊成員
		private async Task SelectTeamMember(string memberName, int defaultIndex)
		{
			if (!string.IsNullOrEmpty(memberName))
			{
				int setNum = check(MYTeamData, memberName);
				直向選擇(setNum == -1 ? defaultIndex : setNum, AutoSkillSet.程式速度);
			}
			else
			{
				直向選擇(defaultIndex, AutoSkillSet.程式速度);
			}
		}

		// 執行標準技能
		private async Task ExecuteStandardSkills()
		{
			BT_Cmd();
			await Task.Delay(100);

			if (AutoSkillSet.需選擇)
			{
				await Task.Delay(100);
				KeyPress(VKeys.KEY_ENTER);
			}
		}

		public void BT_Cmd()
		{
			WriteInt(GetFullAddress(AddressData.戰鬥輸入), 1, 6);
		}

		public void 更改F8追隨() => WriteString(GetFullAddress(AddressData.快捷F8), 1, "／追蹤：％Ｌ");
		public void 更改字型(int i)
		{
			WriteInt(AddressData.UI字型, 0, i);
		}
		public void MoveToNPC(int npcID)
		{
			WriteInt(GetFullAddress(AddressData.選擇項目), 0, npcID);
			WriteInt(GetFullAddress(AddressData.選擇項目B), 0, npcID);
			WriteInt(GetFullAddress(AddressData.移動對象), 0, npcID);
			WriteInt(GetFullAddress(AddressData.開始移動到目標對象), 0, npcID);
		}

		public void MoveToNPC2(int npcID)
		{
			while (StartRunCode)
			{
				if (GetTargetIDINT() == npcID && 對話與結束戰鬥)
				{
					break;
				}
				else
				{
					WriteInt(GetFullAddress(AddressData.選擇項目), 0, npcID);
					WriteInt(GetFullAddress(AddressData.移動對象), 0, npcID);
					WriteInt(GetFullAddress(AddressData.選擇項目B), 0, npcID);
					WriteInt(GetFullAddress(AddressData.開始移動到目標對象), 0, npcID);
					Task.Delay(500).Wait();
				}
			}
		}

		public void 鎖定NPC(int npcID)
		{
			WriteInt(GetFullAddress(AddressData.選擇項目), 0, npcID);
			WriteInt(GetFullAddress(AddressData.選擇項目B), 0, npcID);
		}

		public void 生產到底()
		{
			int checkDone = 0;
			int checkIndex = 製作Index;
			KeyPress(VKeys.KEY_ENTER, 5);
			while (StartRunCode)
			{
				KeyPress(VKeys.KEY_ENTER, 10, 10);
				//Debug.WriteLine($"出現左右選單 -- {出現左右選單}");
				if (checkIndex != 製作Index)
				{
					checkIndex = 製作Index;
					checkDone = 0;
					continue;
				}

				checkDone++;
				Task.Delay(50).Wait();
				if (checkDone > 7)
				{
					KeyPress(VKeys.KEY_ESCAPE, 10);
					break;
				}
			}
		}

		public void 速度()
		{
			if (VIPSP)
			{
				WriteInt("[<nobolHD.bng>+B02CF4] + 26a", 0, unchecked((int)3081718408));
				WriteInt("[<nobolHD.bng>+AFC254] + 260", 0, unchecked((int)3081718408));
			}
		}

		//檢查是否有歸0

		const string SelectData = @"[<nobolHD.bng>+4C53FB0] + C4";
		public void 直向選擇ZC(int num, int delay = 300, bool passCheck = false)
		{
			int indexCheck = -1;

			while (StartRunCode)
			{
				indexCheck = ReadInt(SelectData, 0);
				if (indexCheck == 0)
				{
					WriteInt(SelectData, 0, num);
					Task.Delay(delay).Wait();
					if (passCheck)
						KeyPressPP(VKeys.KEY_ENTER);
					else
						KeyPress(VKeys.KEY_ENTER);

					break;
				}
				else
					KeyPress(VKeys.KEY_ESCAPE);

				Task.Delay(500).Wait();
			}
		}

		public void 直向選擇PP(int num, int delay = 300)
		{
			WriteInt(SelectData, 0, num);
			Task.Delay(delay).Wait();
			KeyPressPP(VKeys.KEY_ENTER);
		}

		public void 直向選擇(int num, int delay = 300, bool passCheck = false)
		{
			WriteInt(SelectData, 0, num);
			Task.Delay(delay).Wait();
			if (passCheck)
				KeyPressPP(VKeys.KEY_ENTER);
			else
				KeyPress(VKeys.KEY_ENTER);
		}

		const string addKEY = "4C53FA4";
		public int 精準移動Index => ReadInt($"[[<nobolHD.bng> + {addKEY}] + 164] +54", 0);
		public int 點移動 => ReadInt($"[[<nobolHD.bng> + {addKEY}] + 164] +54", 0);


		public void 準確目標移動(float x, float y, float z)
		{
			while (StartRunCode)
			{
				if (精準移動Index == 0 || 精準移動Index == 1)
				{
					var x1 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +58");
					var y1 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +5C");
					var z1 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +60");
					var y2 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +6C");
					var z2 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +70");
					var x2 = ReadFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +68");

					var ii = ReadInt($"[[<nobolHD.bng> + {addKEY}] + 164] +54", 0);
					float f = 0;
					if (x1 < 0 || x2 < 0 || y1 < 0 || y2 < 0 || z1 < 0 || z2 < 0 ||
						!float.TryParse(x1.ToString(), out f) ||
						!float.TryParse(y1.ToString(), out f) ||
						!float.TryParse(z1.ToString(), out f))
					{
						ML_Click((int)NowWidth / 2, NowHeight / 2);
						Debug.WriteLine($"出現問題 重新修正位置");
						continue;
					}
					Debug.WriteLine($"-- Read {x1} {y1} {z1} {x2} {y2} {z2} {ii}");

					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +58", x);
					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +5C", y);
					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +60", z);
					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +68", x);
					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +6C", y);
					WriteFloat($"[[<nobolHD.bng> + {addKEY}] + 164] +70", z);
					Debug.WriteLine("寫入完成");
					WriteInt($"[[<nobolHD.bng> + {addKEY}] + 164] +54", 0, 1);
					Debug.WriteLine("開始移動");
					break;
				}
				else
				{
					ML_Click((int)NowWidth / 2, NowHeight / 2);
				}
			}
		}

		public void 離開戰鬥A()
		{
			離開戰鬥確認 = false;
			do
			{
				if (戰鬥中) { break; }

				if (對話與結束戰鬥)
				{
					Task.Delay(100).Wait();
					KeyPress(VKeys.KEY_ESCAPE);
					Task.Delay(100).Wait();
					KeyPress(VKeys.KEY_ESCAPE);
					Task.Delay(100).Wait();
					KeyPress(VKeys.KEY_ENTER);
					Task.Delay(100).Wait();
				}
				if (待機)
				{
					戰鬥中判定 = -1;
					離開戰鬥確認 = true;
					break;
				}
			}
			while (StartRunCode || IsUseAutoSkill);
			離開戰鬥確認 = true;
		}

		public void 離開戰鬥B()
		{
			var width = 原視窗.Right - 原視窗.Left;
			var height = 原視窗.Bottom - 原視窗.Top;

			int inPosX = width / 2;
			int inPosY = (height / 2) - 100;
			離開戰鬥確認 = false;
			int checkDoneCount = 0;
			Task.Delay(50).Wait();
			do
			{
				if (戰鬥中) { break; }
				//  MainNob.Log($"結算中 : {checkDoneCount}");

				if (待機)
				{
					戰鬥中判定 = -1;
					離開戰鬥確認 = true;
					KeyPress(VKeys.KEY_ESCAPE, 3);
					break;
				}

				if (對話與結束戰鬥)
				{
					checkDoneCount = 0;
					int x = inPosX + _random.Next(-100, 100);
					int y = inPosY + _random.Next(-20, 80);
					MR_Click(x, y);
					Task.Delay(100).Wait();
				}
				else
				{
					checkDoneCount++;
					if (checkDoneCount > 5)
					{
						戰鬥中判定 = -1;
						離開戰鬥確認 = true;
						break;
					}
					Task.Delay(400).Wait();
				}
			} while (StartRunCode || IsUseAutoSkill);
		}

		public async Task 離開戰鬥C()
		{
			var width = 原視窗.Right - 原視窗.Left;
			var height = 原視窗.Bottom - 原視窗.Top;

			int inPosX = width / 2;
			int inPosY = (height / 2) - 100;
			離開戰鬥確認 = false;
			await Task.Delay(50);
			do
			{
				if (戰鬥中) { break; }

				if (待機)
				{
					戰鬥中判定 = -1;
					離開戰鬥確認 = true;
					KeyPressPP(VKeys.KEY_ESCAPE, 3);
					break;
				}

				if (對話與結束戰鬥)
				{
					int x = inPosX + _random.Next(-100, 100);
					int y = inPosY + _random.Next(-20, 80);
					MR_Click(x, y);
					Task.Delay(100).Wait();
				}
			} while (true);
		}


		public void 縮小(string str = "")
		{
			if (!string.IsNullOrEmpty(str))
			{
				var array = str.Split(',');
				if (array.Length > 1)
				{
					if (int.TryParse(array[0], out int width) && int.TryParse(array[1], out int height))
					{
						UpdateWindowSize(width, height, -0.1f);
					}
				}
			}
			else
			{
				var width = 原視窗.Right - 原視窗.Left;
				var height = 原視窗.Bottom - 原視窗.Top;
				UpdateWindowSize(width, height, -0.1f);
			}
		}
		private void UpdateWindowSize(int width, int height, float scaleChange)
		{
			比例 = Math.Max(比例 + scaleChange, 0.3f);
			float f = 比例;
			NowWidth = (int)((width + 16) * f);
			NowHeight = (int)((height + 39) * f);
			MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, NowWidth, NowHeight, true);
		}

		public void MoveWindowTool(int tlIndex)
		{
			int TopPos = tlIndex * 40;
			int LeftPos = tlIndex * 120;

			MoveWindow(Proc.MainWindowHandle, LeftPos, TopPos, MainWindow.OrinX, MainWindow.OrinY, true);
		}

		public void FoucsNobWindows()
		{
			if (Proc.MainWindowHandle != IntPtr.Zero)
			{
				GetWindowRect(Proc.MainWindowHandle, out RECT rect);
				var nowPos = rect;
				SetForegroundWindow(Proc.MainWindowHandle);
			}
		}

		public void FoucsNobApp(Process proc)
		{
			if (proc.MainWindowHandle != IntPtr.Zero)
			{
				GetWindowRect(Proc.MainWindowHandle, out RECT rect);
				var nowPos = rect;
				MoveWindow(proc.MainWindowHandle, nowPos.Left - 100, nowPos.Top, (int)MainWindow.Instance!.Width, (int)MainWindow.Instance.Height, true);
				SetForegroundWindow(proc.MainWindowHandle);
			}
		}

		public void 還原(string str = "")
		{
			比例 = 1;
			if (!string.IsNullOrEmpty(str))
			{
				var array = str.Split(',');
				if (array.Length > 1)
				{
					if (int.TryParse(array[0], out int width) && int.TryParse(array[1], out int height))
					{
						MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, width + 16, height + 39, true);
					}
				}
			}
			else
			{
				MoveWindow(Proc.MainWindowHandle, 原視窗.Left, 原視窗.Top, 原視窗.Right - 原視窗.Left, 原視窗.Bottom - 原視窗.Top, true);
			}
		}

		private bool WriteInt(string address, int type, int value)
		{
			EnsureMemoryReaderInitialized();
			if (_memory is not { IsWin32Available: true }) return false;
			try
			{
				if (type == 2)
				{
					_memory.WriteInt16Async(address, (short)value).GetAwaiter().GetResult();
				}
				else
				{
					_memory.WriteInt32Async(address, value).GetAwaiter().GetResult();
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"WriteInt failed @{address}: {ex.Message}");
				return false;
			}
		}

		private bool WriteFloat(string address, float value)
		{
			EnsureMemoryReaderInitialized();
			if (_memory is not { IsWin32Available: true }) return false;
			try
			{
				_memory.WriteSingleAsync(address, value).GetAwaiter().GetResult();
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"WriteFloat failed @{address}: {ex.Message}");
				return false;
			}
		}

		private bool WriteBytesFromHex(string address, string hex)
		{
			EnsureMemoryReaderInitialized();
			if (_memory is not { IsWin32Available: true }) return false;
			try
			{
				_memory.WriteDataHexAsync(address, hex).GetAwaiter().GetResult();
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"WriteBytes failed @{address}: {ex.Message}");
				return false;
			}
		}

		private bool WriteString(string address, int type, string value)
		{
			EnsureMemoryReaderInitialized();
			if (_memory is not { IsWin32Available: true }) return false;
			try
			{
				Encoding encoding = type switch
				{
					0 => Encoding.ASCII,
					1 => Encoding.Unicode,
					2 => Encoding.GetEncoding(950),
					_ => Encoding.UTF8
				};
				_memory.WriteStringAsync(address, value, encoding).GetAwaiter().GetResult();
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"WriteString failed @{address}: {ex.Message}");
				return false;
			}
		}

		private static byte[] ConvertHexToBytes(string hex)
		{
			hex = hex.Replace(" ", string.Empty).Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);
			if (hex.Length % 2 != 0)
			{
				hex = "0" + hex;
			}
			var bytes = new byte[hex.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			}
			return bytes;
		}

		public void 選擇目標類型(int num)
		{
			WriteInt(GetFullAddress(AddressData.B630A4), 0, num);
		}

		private static string PickBestString(params string[] candidates)
		{
			// 直接回傳第一個有效的候選字串
			foreach (var raw in candidates)
			{
				if (string.IsNullOrEmpty(raw)) continue;
				var trimmed = raw.Replace("\0", string.Empty).Trim();
				if (trimmed.Length > 0 && !trimmed.All(ch => ch == '?' || ch == '\uFFFD'))
				{
					return trimmed;
				}
			}
			return string.Empty;
		}
 
 		private static string SanitizeDisplay(string input)
 		{
 			if (string.IsNullOrWhiteSpace(input))
 				return string.Empty;
 
 			var cleaned = input.Replace("\0", string.Empty).Replace("�", string.Empty).Trim();
			
			// 移除結尾的亂碼字元
			while (cleaned.Length > 0 && (cleaned[cleaned.Length - 1] == '?' || cleaned[cleaned.Length - 1] == '\uFFFD'))
			{
				cleaned = cleaned.Substring(0, cleaned.Length - 1);
			}
			
 			if (cleaned.All(ch => ch == '?'))
 				return string.Empty;

 			return cleaned;
 		}

		private static string NormalizeAccount(string acc)
		{
			if (string.IsNullOrWhiteSpace(acc))
				return string.Empty;
			return acc.Trim().ToLowerInvariant();
		}
    }
}
