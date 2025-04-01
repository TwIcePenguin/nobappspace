using System;
using System.Collections.Generic;

namespace NOBApp
{
    public partial class MainWindow
    {
        public static int Dis(int x1, int y1, int x2, int y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            //  MainNob.Log($"x1:{x1} y1:{y1} x2:{x2} y2:{y2}");
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

    }
}
