using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp
{
    public static class AddressData
    {
        static string FIXED_APPNAME_STR = "<nobolHD.bng>";
        static int offsetValA = 48;
        static int offsetValB = 4144;
        public static string Acc = "5B6A928".AddressAdd(-offsetValA); //5B69918
        public static string Pas = Acc.AddressAdd(21);
        public static string 角色名稱 = "4C4E220".AddressAdd(-offsetValA);
        public static string 地圖座標X = "5B6A5DC".AddressAdd(-offsetValA);
        public static string 地圖位置 = "4C4CC54".AddressAdd(-offsetValA);
        public static string 地圖座標H = 地圖座標X.AddressAdd(4);
        public static string 地圖座標Y = 地圖座標X.AddressAdd(8);
        public static string 快捷F8 = "5C347F2";  
        public static string 快捷F9 = 快捷F8.AddressAdd(222);  //差222
        public static string 快捷F10 = 快捷F9.AddressAdd(222);  //差222
        public static string 快捷F11 = 快捷F10.AddressAdd(222);  //差222
        public static string 快捷F12 = 快捷F11.AddressAdd(222);  //差222
        public static string 選擇項目 = "965560";
        public static string 選擇項目B = "965538";
        public static string 移動對象 = "96555C";
        public static string 開始移動到目標對象 = "965558";
        public static string 頻道認證A = "5C3FB1C";
        public static string 頻道認證B = "5C4011C";
        public static string 攝影機角度A = "5BB3CA4";    // 1 北 -1南 
        public static string 攝影機角度B = 攝影機角度A.AddressAdd(8); //1-西 -1 東
        //"A0 98" 戰鬥中
        //"F0 B8" 沒有任何視窗野外
        //"F0 F8" 開寶 出現對話框 戰鬥結束
        public static string 判別狀態A = "5C5C400";
        //0 沒有畫面 7-第一頁面 8-輔助 C-技能 2奧義 -僅判斷
        public static string 戰鬥可輸入判斷 = "5BA6744".AddressAdd(-offsetValA);
        public static string 戰鬥可輸入判斷II = "5BA67E0".AddressAdd(-offsetValA);
        public static string 戰鬥輔助 = "5BA67F0".AddressAdd(-offsetValA);  //0-正常化面 1-輔助畫面
        public static string 戰鬥輸入 = "5BA673C".AddressAdd(-offsetValA);

        //廢止
        public static string 戰鬥可輸隊員 = "5B8FD80"; //+4 7
        public static string 戰鬥輸入技能ID = "5B8FDF4"; //+4 7
        public static string 戰鬥輸入施放ID = "5B8FE00"; //+C 7
        public static string 戰鬥輸入選擇 = "5B8FDE8";  //0C 技能 2B 物品 3F 奧義 3E 特殊 32流派
        public static string 戰鬥技能編號起 = "5B9030C"; //+80 ~包含攻擊等等相關
        public static string 戰鬥列隊 = "5B92BAC";
        public static string 是否有觀察對象 = "4C4CA38"; //沒有的話是 FFFFFFFF\
        public static string 直選框 = "4C4E018".AddressAdd(-offsetValA);   //跳出選擇視窗[0~n選擇] ccTalkBox1
        public static string 製作Index = "4C4E171";   //製作完成會增加

        public static string 直選框文字 = "ADA944";      //確認選擇視窗文字 只會出現最後 ccTalkBox
        public static string 搜尋身邊NPCID起始 = "5C54EFC";
        public static string 視角 = $"B592A8"; //0-俯視 1-第一人稱
        public static string UI字型 = $"4C4C144"; //0~
        public static string 輸入數量視窗 = $"5BB35AC";

    }
}
