using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp
{
    public class NOBUserBase
    {
        public Process Proc;
        public string LogID = "";
        public string 目前動作 = "";
        string cacheLog = string.Empty;
        public int Hwnd => (int)Proc.MainWindowHandle;
        public bool StartRunCode = false;
        public bool IsUseAutoSkill = false;
        public bool 腳本暫停出招 = false;
        public NOBUserBase(Process proc)
        {
            Proc = proc;
        }
        public void Log(string str)
        {
            if (cacheLog == str || cacheLog.Contains(str))
            {
                return;
            }

            目前動作 = cacheLog = str;
            Debug.WriteLine($"{LogID}->{str}");
        }
    }
}
