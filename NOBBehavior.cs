using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
                //Log(i.ToString());
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


        // Windows API函數導入
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);


        // Windows API常量
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00080000;

        private bool _isClickThrough = false;

        /// <summary>
        /// 設置視窗是否允許鼠標點擊穿透
        /// </summary>
        /// <param name="isClickThrough">true表示點擊穿透，false表示正常接收點擊</param>
        public void SetClickThrough(bool isClickThrough)
        {
            if (_isClickThrough == isClickThrough)
                return;

            _isClickThrough = isClickThrough;

            // 獲取當前視窗樣式
            int extendedStyle = GetWindowLong(Hwnd, GWL_EXSTYLE);

            if (isClickThrough)
            {
                // 添加點擊穿透樣式
                SetWindowLong(Hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
                //SetLayeredWindowAttributes(MainNob.Hwnd, 0, 128, LWA_ALPHA); // 設置半透明效果，可調整
            }
            else
            {
                // 移除點擊穿透樣式，但保留分層視窗屬性
                SetWindowLong(Hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
            }
        }
    }

}
