using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp
{
    public class NOBBehavior : NOBUserBase
    {
        public NOBBehavior(Process proc) : base(proc)
        { }

        public void 前進(int time)
        {
            KeyPressT(VKeys.KEY_W, time);
        }

        public void 後退(int time)
        {
            KeyPressT(VKeys.KEY_S, time);
        }

        public void KeyPress(VKeys keyCode, int loopNum = 1, int delay = 100)
        {
            for (int i = 0; i < loopNum; i++)
            {
                if (StartRunCode == false && IsUseAutoSkill == false)
                    break;
#if DEBUG
                //Debug.WriteLine($"i : {i}");
#endif
                Proc.MainWindowHandle.KeyPress(keyCode);
                if (loopNum > 1)
                    Task.Delay(delay).Wait();
            }
        }
        public void KeyPressPP(VKeys keyCode, int loopNum = 1, int delay = 100)
        {
            for (int i = 0; i < loopNum; i++)
            {
                Proc.MainWindowHandle.KeyPress(keyCode);
                if (loopNum > 1)
                    Task.Delay(delay).Wait();
            }
        }
        public void KeyPressT(VKeys keyCode, int ss)
        {
            Proc.MainWindowHandle.KeyPress(keyCode, ss);
        }
        public void MR_Click(int x, int y)
        {
            Proc.MainWindowHandle.M_RClick(x, y - 31);
        }
        public void ML_Click(int x, int y, int num = 1, int delay = 50)
        {
            for (int i = 0; i < num; i++)
            {
                Proc.MainWindowHandle.M_LClick(x, y - 31);
                Task.Delay(delay).Wait();
            }
        }
        public void KeyDown(VKeys keyCode)
        {
            Proc.MainWindowHandle.KeyDown(keyCode);
        }
        public void KeyUp(VKeys keyCode)
        {
            Proc.MainWindowHandle.KeyUp(keyCode);
        }
    }

}
