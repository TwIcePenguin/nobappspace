using System;
using System.Collections.Generic;

namespace NOBApp
{
    public partial class NobMainCodePage
    {
        public class 自動技能組
        {
            public NOBDATA? NOB;
            public bool 同步 { get; set; }
            public bool 重複放 { get; set; }
            public bool 一次放 { get; set; }
            public bool 需選擇 { get; set; }
            public bool 搖屁股 { get; set; }
            public bool 背景Enter { get; set; }
            public int 延遲 { get; set; }
            public int 間隔 { get; set; }
            public int 技能段1 { get; set; }
            public int 技能段2 { get; set; }
            public int 技能段3 { get; set; }
            public string 施放A { get; set; } = string.Empty;
            public string 施放B { get; set; } = string.Empty;
            public string 施放C { get; set; } = string.Empty;
            public int 程式速度 { get; set; }
            public bool 特殊運作 = false;
        }

        public static List<自動技能組> 隊員智能功能組 = new();

        public static int check(List<BTData>? t, string chID)
        {
            int index = -1;
            if (t != null)
            {
                index = t.FindIndex(0, tdata => { return tdata.FullName.Contains(chID); });
            }
            return index;
        }

        public static long checkL(List<BTData> t, string name)
        {
            var d = t.Find(tdata => { return tdata.FullName.Contains(name); });
            if (d == null)
                return 0;
            else
                return d.UID;
        }
    }
}
