using System;

namespace NOBApp
{
    /// <summary>
    /// 集中管理記憶體位址，使用輔助方法以十六進位字串做運算，方便維護
    /// </summary>
    public static class AddressData
    {
        // 固定應用程式前綴（在讀取時會與這個字串結合使用）
        public const string FIXED_APPNAME_STR = "<nobolHD.bng>";

        // 全域偏移量（以十進位表示），可在此統一調整
        private const int offsetValA = 27264;  // 原先使用的 +offsetValA
        private const int offsetValB = 38304;  // 备用
        private const int offsetValC = 36384;  // 备用

        // Helper: 由十六進位字串解析為 long
        private static long HexToLong(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return 0;
            return Convert.ToInt64(hex, 16);
        }

        // Helper: 將 long 轉為大寫十六進位字串（不含前綴）
        private static string LongToHex(long value) => value.ToString("X");

        // Helper: 在原始十六進位位址上加上指定的偏移（十進位），回傳新的十六進位字串
        private static string AddOffset(string baseHex, int add = 0) => LongToHex(HexToLong(baseHex) + add);

        // Helper: 針對需要套用 offsetValA 的情況
        private static string WithOffsetA(string baseHex, int extra = 0) => AddOffset(baseHex, offsetValA + extra);

        // Helper: 在已存在的位址字串上加上小量偏移（例如 +4, +8, +21 等）
        private static string AddDelta(string hexAddress, int delta) => AddOffset(hexAddress, delta);

        // ---------- 定義位址（以易維護方式建置） ----------

        // 角色帳號與密碼
        public static readonly string Acc = WithOffsetA("5B6A928");
        public static readonly string Pas = AddDelta(Acc, 21);
        public static readonly string 角色名稱 = WithOffsetA("4C4E220");

        // 位置與地圖資訊
        public static readonly string 地圖座標X = WithOffsetA("5B6A5DC");
        public static readonly string 地圖位置 = WithOffsetA("4C4CC54");
        public static readonly string 地圖座標H = AddDelta(地圖座標X, 4);
        public static readonly string 地圖座標Y = AddDelta(地圖座標X, 8);

        // 快捷鍵（示例：F8 起點，後續每個鍵位差值為 222）
        public static readonly string 快捷F8 = "5C3D5A2"; // 原始位置（未套用 offsetA）
        public static readonly string 快捷F9 = AddDelta(快捷F8, 222);
        public static readonly string 快捷F10 = AddDelta(快捷F9, 222);
        public static readonly string 快捷F11 = AddDelta(快捷F10, 222);
        public static readonly string 快捷F12 = AddDelta(快捷F11, 222);

        // 選擇/移動相關
        public static readonly string 選擇項目 = "96953C";
        public static readonly string 選擇項目B = "969560";
        public static readonly string 移動對象 = "969560";
        public static readonly string 開始移動到目標對象 = "969558";

        // 頻道/攝影機/狀態判定
        public static readonly string 頻道認證A = "5C48F5C";
        public static readonly string 攝影機角度A = "5BB3CA4";    // 1 北 -1南
        public static readonly string 攝影機角度B = AddDelta(攝影機角度A, 8); // 1-西 -1 東

        // 狀態判別與戰鬥相關
        public static readonly string 判別狀態A = "5C65240";
        public static readonly string 戰鬥可輸入判斷 = "5BAF214";
        public static readonly string 戰鬥人數判斷 = AddDelta(戰鬥可輸入判斷, 116);
        public static readonly string 戰鬥可輸入判斷II = "5BAF2C0";
        public static readonly string 戰鬥輔助 = "5BA67F0";  //0-正常化面 1-輔助畫面
        public static readonly string 戰鬥輸入 = "5BAF20C";

        // 戰鬥與列隊舊址（保留以相容舊程式碼）
        public static readonly string 戰鬥可輸隊員 = "5B8FD80"; //+4 7
        public static readonly string 戰鬥輸入技能ID = "5B8FDF4"; //+4 7
        public static readonly string 戰鬥輸入施放ID = "5B8FE00"; //+C 7
        public static readonly string 戰鬥輸入選擇 = "5B8FDE8";  //0C 技能 2B 物品 3F 奧義 3E 特殊 32流派
        public static readonly string 戰鬥技能編號起 = "5B9030C"; //+80 ~包含攻擊等等相關
        public static readonly string 戰鬥列隊 = "5B92BAC";

        // 其他 UI / 檢查用位址
        public static readonly string 是否有觀察對象 = "4C534BC"; //沒有的話是 FFFFFFFF\ 4C4CA38
        public static readonly string 直選框 = "4C54AB0";   //跳出選擇視窗[0~n選擇] ccTalkBox1
        public static readonly string 製作Index = "4C4E171";   //製作完成會增加

        public static readonly string 直選框文字 = "ADF184";      //確認選擇視窗文字 只會出現最後 ccTalkBox
        public static readonly string 搜尋身邊NPCID起始 = "5C5DD24";
        public static readonly string 視角 = "B592A8"; //0-俯視 1-第一人稱
        public static readonly string UI字型 = "4C4C144"; //0~
        public static readonly string 輸入數量視窗 = "5BB35AC";

        // 新增：以前程式碼中直接使用的其他位址，已集中管理
        public static readonly string SelectDataBase = "4C53FB0"; // 用於 SelectData 的 base
        public static readonly string AddKey = "4C53FA4"; // addKEY
        public static readonly string B630A4 = "B630A4"; // 用於 選擇目標類型
        public static readonly string B02CF4 = "B02CF4"; // 用於 速度() 寫入
        public static readonly string AFC254 = "AFC254"; // 用於 速度() 寫入

        // 如果未來需取得套用固定偏移後的字串（例如要跟 FIXED_APPNAME_STR 組合），可呼叫此方法
        public static string GetAddressWithAppPrefix(string hexAddress) => $"{FIXED_APPNAME_STR} + {hexAddress}";

        // 範例：取得 Acc 的完整組合字串
        public static string AccFull => GetAddressWithAppPrefix(Acc);
    }
}
