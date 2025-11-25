using System;
using System.Diagnostics;
using System.Drawing;

namespace NOBApp
{
    public static class PerformanceTest
    {
        public static void TestGetColorCopNum(IntPtr hwnd, Point posStart, Point WeHi, string colorStr)
        {
            // 測量優化前的方法執行時間
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int resultBefore = ColorTools.GetColorNum(hwnd, posStart, WeHi, colorStr);
            stopwatch.Stop();
            long timeBefore = stopwatch.ElapsedMilliseconds;

            // 打印結果
              Debug.WriteLine($"優化前的執行時間: {timeBefore} 毫秒, 匹配的顏色數量: {resultBefore}");

            // 測量優化後的方法執行時間
            stopwatch.Restart();
            int resultAfter = ColorTools.GetColorNum(hwnd, posStart, WeHi, colorStr);
            stopwatch.Stop();
            long timeAfter = stopwatch.ElapsedMilliseconds;

            // 打印結果
              Debug.WriteLine($"優化後的執行時間: {timeAfter} 毫秒, 匹配的顏色數量: {resultAfter}");

            // 比較結果
              Debug.WriteLine($"性能改進: {(timeBefore - timeAfter) / (double)timeBefore * 100}%");
        }
    }
}
