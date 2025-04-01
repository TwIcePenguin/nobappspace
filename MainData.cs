using NOBApp.GoogleData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace NOBApp
{
    public class UData
    {
        public string KeyStr { get; set; } = string.Empty;
    }

    public class UseTime
    {
        public string FromName { get; set; } = string.Empty;
        public string StartTimer { get; set; } = string.Empty;
        public string EndTimer { get; set; } = string.Empty;
        public string ISEND { get; set; } = string.Empty;
        public string CheckC { get; set; } = string.Empty;
    }

    public class FPDATA
    {
        public string UserName { get; set; } = string.Empty;
        public string Acc { get; set; } = string.Empty;
        public string Pww { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string LoginTimer { get; set; } = string.Empty;
    }

    public class PNobUserData
    {
        public string? Acc { get; set; }
        public string? StartTimer { get; set; }
        public string? ISEND { get; set; }
        public string? CheckC { get; set; }
        public override string ToString()
        {
            return $"{Acc} {StartTimer} {ISEND} {CheckC}";
        }
    }

    public class BTData
    {
        public long UID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => FirstName + LastName;

        public BTData(long uid)
        {
            UID = uid;
        }
    }

    public class SkillData
    {
        public int 順序 { get; set; }
        public bool 啟用 { get; set; }
        public int 重複 { get; set; }
        public UseStatus 類型 { get; set; } //技能 - 0 奧義 - 1 流派 -2 物品 - 3  
        public int 技能位置 { get; set; } //第幾個技能
        public int 技能二段 { get; set; } //第二段施放選擇 EX:
        public int 延遲施放 { get; set; }
        public string? 施展對象1 { get; set; }
        public string? 施展對象2 { get; set; }
        public string? 施展對象3 { get; set; }
        public string? 施展對象4 { get; set; }
        public string? 施展對象5 { get; set; }
        public string? 施展對象6 { get; set; }
        public string? 施展對象7 { get; set; }
    }

    public enum UseStatus : int { 技能 = 12, 奧義 = 63, 流派 = 50, 特殊 = 62, 物品 = 43 };

    public class 隊伍技能紀錄
    {
        public List<隊員資料紀錄檔> 方案A { get; set; } = new List<隊員資料紀錄檔>();
        public List<隊員資料紀錄檔> 方案B { get; set; } = new List<隊員資料紀錄檔>();
        public List<隊員資料紀錄檔> 方案C { get; set; } = new List<隊員資料紀錄檔>();
    }
    public class 隊員資料紀錄檔
    {
        public bool 同步 { get; set; }
        public string 用名 { get; set; } = string.Empty;
        public bool 重複放 { get; set; }
        public bool 一次放 { get; set; }
        public int 延遲 { get; set; }
        public int 間隔 { get; set; }
        public int 技能段1 { get; set; }
        public int 技能段2 { get; set; }
        public int 技能段3 { get; set; }
        public string 施放A { get; set; } = string.Empty;
        public string 施放B { get; set; } = string.Empty;
        public string 施放C { get; set; } = string.Empty;
        public int 程式速度 { get; set; }
    }

    public class Setting
    {
        public bool 使用定位點 { get; set; }
        public int 定位點X { get; set; }
        public int 定位點Y { get; set; }
        public int 後退時間 { get; set; }
        public int 上覽線 { get; set; }

        public 隊伍技能紀錄 隊伍技能 { get; set; } = new 隊伍技能紀錄();
        public string UseSkillName { get; set; } = string.Empty;
        public string 上次使用的腳本 { get; set; } = string.Empty;
        public List<string> 組隊玩家技能 { get; set; } = new List<string>();
        public bool AllInTeam { get; set; }
        public int 自動結束X位置 { get; set; }
        public int 自動結束Y位置 { get; set; }
        public int 連續戰鬥 { get; set; }
        public int 選擇關卡 { get; set; }
        public int 選擇難度 { get; set; }
        public int 目標A { get; set; }
        public int 目標B { get; set; }
        public int 目標C { get; set; }
        public int 其他選項A { get; set; }
        public int 其他選項B { get; set; }
        public int 搜尋範圍 { get; set; }
        public int 線路 { get; set; }
        public bool Enter點怪 { get; set; }
    }
}
