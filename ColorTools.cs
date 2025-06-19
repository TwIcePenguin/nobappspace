using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NOBApp
{
    public static class ColorTools
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, CopyPixelOperation rop);

        public static Color GetColorAt(IntPtr hwnd, Point location)
        {
            Bitmap screenPixel = new Bitmap(100, 50, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(hwnd))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 800, 400, hSrcDC, 1, 1, (int)CopyPixelOperation.SourceCopy);

                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }

                var data = screenPixel.LockBits(
                new Rectangle(Point.Empty, screenPixel.Size),
                ImageLockMode.ReadWrite, screenPixel.PixelFormat);
                var pixelSize = data.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3; // only works with 32 or 24 pixel-size bitmap!
                var padding = data.Stride - (data.Width * pixelSize);
                var bytes = new byte[data.Height * data.Stride];

                // copy the bytes from bitmap to array
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                var index = 0;
                var builder = new StringBuilder();
                  Debug.WriteLine($"{data.Height} {data.Width}");
                for (var y = 0; y < data.Height; y++)
                {
                    for (var x = 0; x < data.Width; x++)
                    {
                        Color pixelColor = Color.FromArgb(
                            pixelSize == 3 ? 255 : bytes[index + 3], // A component if present
                            bytes[index + 2], // R component
                            bytes[index + 1], // G component
                            bytes[index]      // B component
                            );

                        builder
                            .Append($" X:{x} Y:{y} ")
                            .Append("  ")
                            .Append($"{pixelColor.R:X}{pixelColor.G:X}{pixelColor.B:X}")
                            .AppendLine();

                        index += pixelSize;
                    }

                    index += padding;
                }
                  Debug.WriteLine($"{builder}");
            }

            return Color.White;
        }

        public static int ConvertABGRtoARGB(int abgr)
        {
            // 提取 A、B、G、R 分量
            byte a = (byte)((abgr >> 24) & 0xFF);
            byte b = (byte)((abgr >> 16) & 0xFF);
            byte g = (byte)((abgr >> 8) & 0xFF);
            byte r = (byte)(abgr & 0xFF);

            // 將 A、R、G、B 分量組合成 ARGB 格式的整數值
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        // 避免重複轉換顏色，將 targetColorInt 設為靜態只轉換一次 (如果 colorStr 是固定的)
        private static int _cachedTargetColorInt = -1;
        private static string _cachedColorStr = "";

        public static int GetColorNum(IntPtr hwnd, Point posStart, Point WeHi, string colorStr)
        {
            int mapping = 0;
            int targetColorInt;// = ConvertABGRtoARGB(ColorTranslator.FromHtml("#" + colorStr).ToArgb());

            // 顏色快取：如果 colorStr 沒有改變，則使用快取的 targetColorInt，避免重複轉換
            if (colorStr != _cachedColorStr)
            {
                try
                {
                    targetColorInt = ConvertABGRtoARGB(ColorTranslator.FromHtml("#" + colorStr).ToArgb());
                    _cachedTargetColorInt = targetColorInt;
                    _cachedColorStr = colorStr;
                    //  MainNob.Log($"Import {targetColorInt:X8}"); // 保留調試輸出，但可以考慮條件編譯
                }
                catch (Exception e)
                {
                      Debug.WriteLine($"Color conversion error for colorStr: {colorStr}, Error: {e.Message}");
                    return 0; // 顏色轉換失敗，直接返回 0 或拋出異常，根據您的錯誤處理策略
                }
            }
            else
            {


                targetColorInt = _cachedTargetColorInt;
            }
            //  MainNob.Log($"Import {targetColorInt:X8}");

            using (Bitmap screenPixel = new Bitmap(WeHi.X, WeHi.Y, PixelFormat.Format32bppArgb))
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            using (Graphics gsrc = Graphics.FromHwnd(hwnd))
            {
                IntPtr hSrcDC = gsrc.GetHdc();
                IntPtr hDC = gdest.GetHdc();

                // 使用正確的來源和目標座標
                int retval = BitBlt(hDC, 0, 0, WeHi.X, WeHi.Y, hSrcDC, posStart.X, posStart.Y - 31, (int)CopyPixelOperation.SourceCopy);

                gdest.ReleaseHdc();
                gsrc.ReleaseHdc();

                // 保存捕捉到的圖像到文件中以便調試
                //screenPixel.Save("captured_cop_image.png", ImageFormat.Png);

                BitmapData data = null;
                try
                {
                    data = screenPixel.LockBits(
                        new Rectangle(Point.Empty, screenPixel.Size),
                        ImageLockMode.ReadOnly,
                        screenPixel.PixelFormat);

                    int pixelSize = 4;
                    int padding = data.Stride - (data.Width * pixelSize);
                    byte[] bytes = new byte[data.Height * data.Stride];

                    Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                    unsafe
                    {
                        fixed (byte* pBytes = bytes)
                        {
                            byte* pPixel = pBytes;

                            for (int y = 0; y < data.Height; y++)
                            {
                                for (int x = 0; x < data.Width; x++)
                                {
                                    // 將像素顏色轉換為 ARGB 格式
                                    int pixelColorInt = (pPixel[3] << 24) | (pPixel[2] << 16) | (pPixel[1] << 8) | pPixel[0];

                                    if (pixelColorInt == targetColorInt)
                                    {
                                        mapping++;
                                    }

                                    pPixel += pixelSize;
                                }
                                pPixel += padding;
                            }
                        }
                    }
                }
                finally
                {
                    if (data != null)
                        screenPixel.UnlockBits(data);
                }
            }

            return mapping;
        }
    }
}