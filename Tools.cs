using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NOBApp
{
    public static class Tools
    {
        #region API
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        internal static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, ref UInt32 lpNumberOfBytesWritten);


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_RBUTTONDOWN = 0x0204;
        const uint WM_RBUTTONUP = 0x0205;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        #endregion

        private static readonly Dictionary<string, Assembly> LoadDlls = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, object> Assemblies = new Dictionary<string, object>();

        internal static void WriteBytes(this IntPtr hProcess, int address, byte[] value)
        {
            bool success;
            UInt32 nBytesRead = 0;
            success = WriteProcessMemory(hProcess, (IntPtr)address, value, (IntPtr)value.Length, ref nBytesRead);
        }

        internal static void WriteInt(this IntPtr hProcess, int address, int value)
        {
            bool success;
            byte[] buffer = BitConverter.GetBytes(value);
            UInt32 nBytesRead = 0;
            success = WriteProcessMemory(hProcess, (IntPtr)address, buffer, (IntPtr)4, ref nBytesRead);
        }

        public static string ReadStrII(this IntPtr hProcess, int address, byte[] buffer)
        {
            int bytesRead = 0;
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, ref bytesRead);
            string str = Encoding.Unicode.GetString(buffer);
            string deStr = str.SubStringByBytes(8);

            return deStr;
        }
        public static string ReadStr(this IntPtr hProcess, int address, byte[] buffer)
        {
            int bytesRead = 0;
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, ref bytesRead);
            return Encoding.Unicode.GetString(buffer).TrimEnd('\0');

            //  MainNob.Log("buffer : " + string.Join( " ", buffer) + " - " + buffer.Length);
            //return Encoding.Unicode.GetString(buffer,0,6);
        }
        public static string SubStringByBytes(byte[] source, int NumberOfBytes, string suffix = "")
        {
            if (source.Length == 0)
                return "";

            long tempLen = 0;
            StringBuilder sb = new StringBuilder();
            string str = Encoding.Unicode.GetString(source);
            foreach (var c in str)
            {
                Char[] _charArr = new Char[] { c };
                byte[] _charBytes = Encoding.Unicode.GetBytes(_charArr);
                if ((tempLen + source.Length) > NumberOfBytes)
                {
                    if (!string.IsNullOrWhiteSpace(suffix))
                        sb.Append(suffix);
                    break;
                }
                else
                {
                    tempLen += _charBytes.Length;
                    sb.Append(Encoding.Unicode.GetString(_charBytes));
                }
            }
            return sb.ToString();
        }

        public static async void M_RClick(this IntPtr hProcess, int x, int y)
        {
            PostMessage(hProcess, WM_RBUTTONDOWN, 0, x + (y << 16));
            await Task.Delay(50);
            PostMessage(hProcess, WM_RBUTTONUP, 0, x + (y << 16));
        }
        public static async void M_LClick(this IntPtr hProcess, int x, int y)
        {
            PostMessage(hProcess, WM_LBUTTONDOWN, 0, x + (y << 16));
            await Task.Delay(50);
            PostMessage(hProcess, WM_LBUTTONUP, 0, x + (y << 16));
        }

        public static string SubStringByBytes(this string source, int NumberOfBytes, System.Text.Encoding encoding, string suffix = "")
        {
            if (string.IsNullOrWhiteSpace(source) || source.Length == 0)
                return source;

            if (encoding.GetBytes(source).Length <= NumberOfBytes)
                return source;

            long tempLen = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var c in source)
            {
                Char[] _charArr = new Char[] { c };
                byte[] _charBytes = encoding.GetBytes(_charArr);
                if ((tempLen + _charBytes.Length) > NumberOfBytes)
                {
                    if (!string.IsNullOrWhiteSpace(suffix))
                        sb.Append(suffix);
                    break;
                }
                else
                {
                    tempLen += _charBytes.Length;
                    sb.Append(encoding.GetString(_charBytes));
                }
            }
            return sb.ToString();
        }
        public static string SubStringByBytes(this string source, int NumberOfBytes, string encoding = "UTF-8", string suffix = "")
        {
            return SubStringByBytes(source, NumberOfBytes, Encoding.GetEncoding(encoding), suffix);
        }

        public static int ReadInt(this IntPtr hProcess, int address, byte[] buffer)
        {
            int bytesRead = 0;
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static string ReadData(this IntPtr hProcess, int address, byte[] buffer)
        {
            int bytesRead = 0;
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, ref bytesRead);
            List<string> listStr = new();
            foreach (var item in buffer)
            {
                listStr.Add(item.ToString("X2"));
            }
            return string.Join(" ", listStr);
        }
        static uint repeatCount = 0;
        static uint scanCode = 0;
        static uint extended = 0;
        static uint context = 0;
        static uint previousState = 0;
        static uint transition = 0;
        static uint lParamDown;
        static uint lParamUp;
        public static void KeyPress(this IntPtr hProcess, VKeys keyCode, int ss = 100)
        {
            repeatCount = 0;
            scanCode = 0;
            extended = 0;
            context = 0;
            previousState = 0;
            transition = 0;

            lParamDown = repeatCount
                | (scanCode << 16)
                | (extended << 24)
                | (context << 29)
                | (previousState << 30)
                | (transition << 31);
            previousState = 1;
            transition = 1;
            lParamUp = repeatCount
                | (scanCode << 16)
                | (extended << 24)
                | (context << 29)
                | (previousState << 30)
                | (transition << 31);

            PostMessage(hProcess, WM_KEYDOWN, (int)keyCode, unchecked((int)lParamDown));
            //  MainNob.Log(keyCode + " - ss - 1 " + ss);
            Task.Delay(ss).Wait();
            //  MainNob.Log(keyCode + " - ss - 2 " + ss);
            PostMessage(hProcess, WM_KEYUP, (int)keyCode, unchecked((int)lParamUp)); //
                                                                                     //SendMessage(hProcess, WM_KEYDOWN, (IntPtr)0x41, (IntPtr)0); // A鍵
                                                                                     //  MainNob.Log("keyCode-->" + keyCode);
        }


        public static void KeyDown(this IntPtr hProcess, VKeys keyCode)
        {
            repeatCount = 0;
            scanCode = 0;
            extended = 0;
            context = 0;
            previousState = 0;
            transition = 0;

            lParamDown = repeatCount
            | (scanCode << 16)
            | (extended << 24)
            | (context << 29)
            | (previousState << 30)
            | (transition << 31);
            PostMessage(hProcess, WM_KEYDOWN, (int)keyCode, unchecked((int)lParamDown));

        }
        public static void KeyUp(this IntPtr hProcess, VKeys keyCode)
        {
            repeatCount = 0;
            scanCode = 0;
            extended = 0;
            context = 0;
            previousState = 0;
            transition = 0;
            previousState = 1;
            transition = 1;
            lParamUp = repeatCount
                | (scanCode << 16)
                | (extended << 24)
                | (context << 29)
                | (previousState << 30)
                | (transition << 31);

            PostMessage(hProcess, WM_KEYUP, (int)keyCode, unchecked((int)lParamUp));
        }

        public static string AddressAdd(this string str, int i)
        {
            return (int.Parse(str, NumberStyles.HexNumber) + i).ToString("X");
        }

        static public bool timeUpUpdate = false;
        public static async void GetWebsiteData(Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(uri);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                          Debug.WriteLine($"回傳訊息 -> \n{content}");
                        Authentication.讀取認證訊息Json(content);
                        // 處理回應內容
                    }
                }
                catch (Exception e)
                {
                      Debug.WriteLine("Message :{0} ", e.Message);
                }
            }
        }

        public static string GetSerialNumber()
        {
            try
            {
                return GetcomputerUUID();
            }
            catch
            {
                return "";
            }
        }

        public static string GetcomputerUUID()
        {
            var uuid = GetSmBIOSUUID();
            if (string.IsNullOrEmpty(uuid))
            {
                var cpuID = GetCPUID();
                var biosSerialNumber = GetBIOSSerialNumber();
                uuid = $"{cpuID}_{biosSerialNumber}";
            }
            return uuid;
        }

        public static string? GetCPUID()
        {
            var cmd = "wmic cpu get processorid";
            return ExecuteCMD(cmd, output =>
            {
                var cpuid = GetTextAfterSpecialText(output, "ProcessorId");
                return cpuid;
            });
        }

        public static string? GetSmBIOSUUID()
        {
            var cmd = "wmic csproduct get UUID";
            return ExecuteCMD(cmd, output =>
            {
                string? uuid = GetTextAfterSpecialText(output, "UUID");
                if (uuid == "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                    uuid = null;
                return uuid;
            });
        }

        public static string? GetBIOSSerialNumber()
        {
            var cmd = "wmic bios get serialnumber";
            return ExecuteCMD(cmd, output =>
            {
                var serialNumber = GetTextAfterSpecialText(output, "SerialNumber");
                return serialNumber;
            });
        }

        private static string? GetTextAfterSpecialText(string fullText, string specialText)
        {
            if (string.IsNullOrWhiteSpace(fullText) || string.IsNullOrWhiteSpace(specialText))
            {
                return null;
            }
            string? lastText = null;
            var idx = fullText.LastIndexOf(specialText);
            if (idx > 0)
            {
                lastText = fullText.Substring(idx + specialText.Length).Trim();
            }
            return lastText;
        }

        private static string? ExecuteCMD(string cmd, Func<string, string?> filterFunc)
        {
            //  MainNob.Log($"ExecuteCMD - CMD:{cmd}");
            using var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            //  MainNob.Log($"Start - CMD:{cmd}");
            process.StandardInput.WriteLine(cmd + " &exit");
            process.StandardInput.AutoFlush = true;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            //  MainNob.Log($"CMD:{cmd} output:{output.ToString()}");
            return filterFunc(output);
        }

        public static string? GetGamePadStr()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            return registryKey.GetValue("GamePadProductName")?.ToString();
        }

        public static List<string> InitResolution()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Window");
            string value = registryKey.GetValue("DeviceRequirement")?.ToString();
            List<string> res = new();
            var array = value?.Split(',');
            if (array == null)
            {
                res = new List<string>();
            }
            else if (array.Length > 9)
            {
                res.Add(array[5] + "," + array[6]);
            }
            res.Add("1024,576");
            res.Add("640,350");
            res.Add("854,480");
            res.Add("960,540");
            res.Add("1024,576");
            res.Add("680,480");
            res.Add("720,480");
            res.Add("720,576");
            res.Add("800,600");
            res.Add("1024,768");
            res.Add("1280,720");
            res.Add("1280,768");
            res.Add("1280,800");
            res.Add("1280,960");
            res.Add("1280,1024");
            res.Add("1600,900");
            res.Add("1600,1024");
            res.Add("1920,1080");
            return res;
        }

        public static void SetGamePad(string inputName)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            inputName = string.IsNullOrEmpty(inputName) ? "<none>" : inputName;

            registryKey.SetValue("GamePadInstanceGUID", inputName);
            registryKey.SetValue("GamePadProductGUID", inputName);
            registryKey.SetValue("GamePadProductName", inputName);

        }

        public static void SetResolution(string w, string h)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Window");
            string value = registryKey.GetValue("DeviceRequirement")?.ToString();
            var array = value?.Split(',');
            array![5] = array[8] = w;
            array[6] = array[9] = h;
            registryKey.SetValue("DeviceRequirement", string.Join(',', array));
        }

        public static void OpenNobMuit(int tryNum = 2, bool hasMusic = false)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? strPath = registryKey.GetValue("GameFolder")?.ToString();
            registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            string? oldMultiBootNum = registryKey.GetValue("MultiBootNum")?.ToString();
            registryKey.SetValue("MultiBootNum", 99);
            string dir = strPath?.Substring(0, strPath.IndexOf(':') + 1);
            string runFilePath = @$"{strPath}\nobolHD.bng";
            Debug.WriteLine(@$"runFilePath : {runFilePath}");

            if (File.Exists(runFilePath))
            {
                Process nob = new Process();
                if (hasMusic)
                {
                    nob.StartInfo.FileName = "cmd.exe";
                    nob.StartInfo.UseShellExecute = false;
                    nob.StartInfo.RedirectStandardOutput = true;
                    nob.StartInfo.RedirectStandardInput = true;
                    nob.StartInfo.CreateNoWindow = true;
                    nob.Start();
                    nob.StandardInput.WriteLine(dir);
                    nob.StandardInput.WriteLine($@"start {runFilePath}");
                }
                else
                {
                    nob.StartInfo.FileName = runFilePath;
                    nob.Start();
                }

                string path = @"./libs/handle.exe";
                if (!System.IO.File.Exists(path))
                {
                      Debug.WriteLine("Handle Error !!");
                    return;
                }

                Process tool = new Process();

                for (int i = 0; i < tryNum; i++)
                {
                    tool.StartInfo.FileName = path;
                    tool.StartInfo.Arguments = $"-a \"\\Sessions\\{i}\\BaseNamedObjects\\Nobunaga Online Game Mutex\"";
                    tool.StartInfo.UseShellExecute = false;
                    tool.StartInfo.RedirectStandardOutput = true;
                    tool.StartInfo.CreateNoWindow = true;

                    tool.Start();
                    while (!tool.StandardOutput.EndOfStream)
                    {
                        string s = tool.StandardOutput.ReadLine();
                        s = s?.Replace(" ", "") ?? string.Empty;
                        var index1 = s.IndexOf("Mutant");
                        if (index1 > 0)
                        {
                            var index2 = s.IndexOf(":", index1);
                            var ID = s.Substring(s.IndexOf("Mutant") + 6, index2 - index1 - 6);

                            Process tool2 = new();
                            tool2.StartInfo.FileName = path;
                            if (hasMusic)
                            {
                                Process[] processlist = Process.GetProcessesByName("nobolHD.bng");
                                foreach (var item in processlist)
                                {
                                    tool2.StartInfo.Arguments = $@"-p {item.Id} -c {ID} -y";
                                    tool2.StartInfo.UseShellExecute = false;
                                    tool2.StartInfo.RedirectStandardOutput = true;
                                    tool2.StartInfo.CreateNoWindow = true;
                                    tool2.Start();
                                    tool2.WaitForExit();
                                }
                            }
                            else
                            {
                                tool2.StartInfo.Arguments = $@"-p {nob.Id} -c {ID} -y";
                                tool2.StartInfo.UseShellExecute = false;
                                tool2.StartInfo.RedirectStandardOutput = true;
                                tool2.StartInfo.CreateNoWindow = true;
                                tool2.Start();
                                tool2.WaitForExit();
                            }
                        }
                    }
                }
            }

            registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            int mulitnum = 2;
            int.TryParse(oldMultiBootNum, out mulitnum);
            registryKey.SetValue("MultiBootNum", mulitnum);
        }

        public static void SetTimeUp()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? ppmitime = registryKey.GetValue("PPMITIME")?.ToString();
            registryKey.SetValue("PPMITIME", MainWindow.到期日.ToString());
        }

        public static void UpdateTimer(System.Windows.Controls.Label lb_time)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? ppmitime = registryKey.GetValue("PPMITIME")?.ToString();
            if (string.IsNullOrEmpty(ppmitime))
            {
                var time = DateTime.Now.AddDays(7);
                MainWindow.到期日 = time;
                registryKey.SetValue("PPMITIME", time.ToString());
                lb_time.Content = $"到期日:{time.ToString()}";
            }
            else
            {
                var tt = DateTime.Parse(ppmitime);
                MainWindow.到期日 = tt;
                lb_time.Content = $"到期日:{tt.ToString()}";
            }
        }
    }
}

public class HotKey
{
    //如果函數執行成功，返回值不為0。
    //如果函數執行失败，返回值為0。要得到擴展錯誤信息，調用GetLastError。
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(
        IntPtr hWnd,                //要定義熱鍵的窗口的句柄
        int id,                     //定義熱鍵ID（不能與其他ID重覆）           
        KeyModifiers fsModifiers,   //標示热键是否在按Alt、Ctrl、Shift、Windows等鍵時才會生效
        Keys vk                     //定義熱鍵的内容
        );

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(
        IntPtr hWnd,                //要取消熱鍵的窗口的句柄
        int id                      //要取消熱鍵的ID
        );

    //定義了輔助鍵的名稱（將數字轉變為字符以便記憶，也可去除此枚舉而直接使用數值）
    [Flags()]
    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        WindowsKey = 8
    }
}

[Serializable]
public enum VKeys
{
    KEY_0 = 0x30, //0 key 
    KEY_1 = 0x31, //1 key 
    KEY_2 = 0x32, //2 key 
    KEY_3 = 0x33, //3 key 
    KEY_4 = 0x34, //4 key 
    KEY_5 = 0x35, //5 key 
    KEY_6 = 0x36, //6 key 
    KEY_7 = 0x37, //7 key 
    KEY_8 = 0x38, //8 key 
    KEY_9 = 0x39, //9 key
    KEY_MINUS = 0xBD, // - key
    KEY_PLUS = 0xBB, // + key
    KEY_A = 0x41, //A key 
    KEY_B = 0x42, //B key 
    KEY_C = 0x43, //C key 
    KEY_D = 0x44, //D key 
    KEY_E = 0x45, //E key 
    KEY_F = 0x46, //F key 
    KEY_G = 0x47, //G key 
    KEY_H = 0x48, //H key 
    KEY_I = 0x49, //I key 
    KEY_J = 0x4A, //J key 
    KEY_K = 0x4B, //K key 
    KEY_L = 0x4C, //L key 
    KEY_M = 0x4D, //M key 
    KEY_N = 0x4E, //N key 
    KEY_O = 0x4F, //O key 
    KEY_P = 0x50, //P key 
    KEY_Q = 0x51, //Q key 
    KEY_R = 0x52, //R key 
    KEY_S = 0x53, //S key 
    KEY_T = 0x54, //T key 
    KEY_U = 0x55, //U key 
    KEY_V = 0x56, //V key 
    KEY_W = 0x57, //W key 
    KEY_X = 0x58, //X key 
    KEY_Y = 0x59, //Y key 
    KEY_Z = 0x5A, //Z key 
    KEY_LBUTTON = 0x01, //Left mouse button 
    KEY_RBUTTON = 0x02, //Right mouse button 
    KEY_CANCEL = 0x03, //Control-break processing 
    KEY_MBUTTON = 0x04, //Middle mouse button (three-button mouse) 
    KEY_BACK = 0x08, //BACKSPACE key 
    KEY_TAB = 0x09, //TAB key 
    KEY_CLEAR = 0x0C, //CLEAR key 
    KEY_ENTER = 0x0D, //ENTER key 
    KEY_SHIFT = 0x10, //SHIFT key 
    KEY_CONTROL = 0x11, //CTRL key 
    KEY_MENU = 0x12, //ALT key 
    KEY_PAUSE = 0x13, //PAUSE key 
    KEY_CAPITAL = 0x14, //CAPS LOCK key 
    KEY_ESCAPE = 0x1B, //ESC key 
    KEY_SPACE = 0x20, //SPACEBAR 
    KEY_PRIOR = 0x21, //PAGE UP key 
    KEY_NEXT = 0x22, //PAGE DOWN key 
    KEY_END = 0x23, //END key 
    KEY_HOME = 0x24, //HOME key 
    KEY_LEFT = 0x25, //LEFT ARROW key 
    KEY_UP = 0x26, //UP ARROW key 
    KEY_RIGHT = 0x27, //RIGHT ARROW key 
    KEY_DOWN = 0x28, //DOWN ARROW key 
    KEY_SELECT = 0x29, //SELECT key 
    KEY_PRINT = 0x2A, //PRINT key 
    KEY_EXECUTE = 0x2B, //EXECUTE key 
    KEY_SNAPSHOT = 0x2C, //PRINT SCREEN key 
    KEY_INSERT = 0x2D, //INS key 
    KEY_DELETE = 0x2E, //DEL key 
    KEY_HELP = 0x2F, //HELP key 
    KEY_NUMPAD0 = 0x60, //Numeric keypad 0 key 
    KEY_NUMPAD1 = 0x61, //Numeric keypad 1 key 
    KEY_NUMPAD2 = 0x62, //Numeric keypad 2 key 
    KEY_NUMPAD3 = 0x63, //Numeric keypad 3 key 
    KEY_NUMPAD4 = 0x64, //Numeric keypad 4 key 
    KEY_NUMPAD5 = 0x65, //Numeric keypad 5 key 
    KEY_NUMPAD6 = 0x66, //Numeric keypad 6 key 
    KEY_NUMPAD7 = 0x67, //Numeric keypad 7 key 
    KEY_NUMPAD8 = 0x68, //Numeric keypad 8 key 
    KEY_NUMPAD9 = 0x69, //Numeric keypad 9 key 
    KEY_SEPARATOR = 0x6C, //Separator key 
    KEY_SUBTRACT = 0x6D, //Subtract key 
    KEY_DECIMAL = 0x6E, //Decimal key 
    KEY_DIVIDE = 0x6F, //Divide key 
    KEY_F1 = 0x70, //F1 key 
    KEY_F2 = 0x71, //F2 key 
    KEY_F3 = 0x72, //F3 key 
    KEY_F4 = 0x73, //F4 key 
    KEY_F5 = 0x74, //F5 key 
    KEY_F6 = 0x75, //F6 key 
    KEY_F7 = 0x76, //F7 key 
    KEY_F8 = 0x77, //F8 key 
    KEY_F9 = 0x78, //F9 key 
    KEY_F10 = 0x79, //F10 key 
    KEY_F11 = 0x7A, //F11 key 
    KEY_F12 = 0x7B, //F12 key 
    KEY_SCROLL = 0x91, //SCROLL LOCK key 
    KEY_LSHIFT = 0xA0, //Left SHIFT key 
    KEY_RSHIFT = 0xA1, //Right SHIFT key 
    KEY_LCONTROL = 0xA2, //Left CONTROL key 
    KEY_RCONTROL = 0xA3, //Right CONTROL key 
    KEY_LMENU = 0xA4, //Left MENU key 
    KEY_RMENU = 0xA5, //Right MENU key 
    KEY_COMMA = 0xBC, //, key
    KEY_PERIOD = 0xBE, //. key
    KEY_PLAY = 0xFA, //Play key 
    KEY_ZOOM = 0xFB, //Zoom key 
    NULL = 0x0,
}