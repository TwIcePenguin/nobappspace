using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NOBApp.MainWindow;

namespace NOBApp.Sports
{
    internal class 黑槍特搜 : BaseClass
    {
        public bool 自動鎖定 = false;
        public bool 自動射擊 = false;

        public Action UpdateUI = null;
        public List<long> 主要目標 = new() { 22266458, 22361637, 22277929, 22364345 };
        public List<string> 主要IDs = new() { };

        public Dictionary<long, string> 鎖定PC = new()
        {
            [22266458] = "豆渣",
            [22277929] = "漢堡",
            [22361637] = "垂垂",
            [22364345] = "銀12",
            [21645307] = "左久",
            [22251137] = "我帥",
            [22201478] = "艾利",

        };
        //public Dictionary<string, Dictionary<long, bool>> 鎖定PC = new()
        //{
        //    ["豆渣"] = { [22266458] = false },
        //    ["漢堡"] = { [22277929] = false },
        //    ["垂垂"] = { [22361637] = false },
        //    ["銀12"] = { [22364345] = false },
        //    ["左久"] = { [21645307] = false },
        //    ["我帥"] = { [22251137] = false },
        //    ["艾利"] = { [22201478] = false },
        //};
        /*
         22266458-豆渣
         22277929-川
         22361637-垂
         22364345-兩
         21645307-佐久
         22251137-我帥
         22201478-艾利
         */

        public override void 初始化()
        {

        }

        public override void 腳本運作()
        {
            Task.Delay(100).Wait();
            UpdateUI?.Invoke();
        }

    }
}
