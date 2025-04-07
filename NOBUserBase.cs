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
        public int Hwnd => (int)Proc.MainWindowHandle;
        public bool StartRunCode = false;
        public bool IsUseAutoSkill = false;
        public NOBUserBase(Process proc)
        {
            Proc = proc;
        }
    }
}
