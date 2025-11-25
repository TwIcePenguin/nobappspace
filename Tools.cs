using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;

namespace NOBApp
{
    public static class Tools
    {
        #region API
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

        // 新增取得磁碟序號的 Win32 API 作為 WMI失敗的後援
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetVolumeInformation(
            string rootPathName,
            StringBuilder volumeNameBuffer,
            int volumeNameSize,
            out uint volumeSerialNumber,
            out uint maximumComponentLength,
            out uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            int nFileSystemNameSize);

        private static readonly Dictionary<string, Assembly> LoadDlls = new Dictionary<string, Assembly>();
        private static readonly Dictionary<string, object> Assemblies = new Dictionary<string, object>();

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

        // 使用 Enum 來區分帳號等級，方便後續擴充與驗證
        public enum AccountLevel
        {
            None = 0,
            Free = 1,
            VIP = 2,
            Sponsor = 3,  // 贊助者
            Special = 4,  // 特殊者
            Admin = 99
        }

        public static AccountLevel CurrentLevel = AccountLevel.None;

        // IsVIP 改為唯讀屬性，根據 CurrentLevel 判斷
        public static bool IsVIP 
        {
            get { return CurrentLevel >= AccountLevel.VIP; }
            set 
            { 
                // 為了相容舊程式碼的賦值操作，這裡做簡單的轉換
                if (value)
                {
                    if (CurrentLevel < AccountLevel.VIP) CurrentLevel = AccountLevel.VIP;
                }
                else
                {
                    CurrentLevel = AccountLevel.Free;
                }
            }
        }

        public static bool isBANACC = false;

        /// <summary>
        /// 取得電腦的唯一識別碼
        /// </summary>
        public static string GetSerialNumber()
        {
            try
            {
                isBANACC = GetMachineGuid().Contains("88B40D5033EF62CD0D2452F61165D8912CEE0CB3103AC8074524F4CB545D3021");
                return GetMachineGuid();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"獲取機器識別碼出錯: {ex.Message}");
                // 退回到舊方法
                try
                {
                    return GetcomputerUUID();
                }
                catch
                {
                    return "";
                }
            }
        }
        /// <summary>
        /// 從 WMI 取得電腦的 UUID
        /// </summary>
        private static string GetcomputerUUID()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct"))
                {
                    foreach (var managementObject in searcher.Get().Cast<ManagementObject>())
                    {
                        if (managementObject["UUID"] != null)
                        {
                            return managementObject["UUID"].ToString().Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得電腦 UUID 失敗: {ex.Message}");
            }

            return string.Empty;
        }
        /// <summary>
        /// 產生高可靠性的機器唯一識別碼
        /// </summary>
        private static string GetMachineGuid()
        {
            // 收集多個硬體資訊來源
            var components = new List<string>();

            // 1. 主機板序號
            components.Add(GetWmiPropertyValue("Win32_BaseBoard", "SerialNumber"));

            // 2. BIOS 序號
            components.Add(GetWmiPropertyValue("Win32_BIOS", "SerialNumber"));

            // 3. 處理器 ID
            components.Add(GetWmiPropertyValue("Win32_Processor", "ProcessorId"));

            // 4. 硬碟序號
            components.Add(GetDiskVolumeSerialNumber());

            // 5. 網路識別碼
            components.Add(GetMacAddress());

            // 6. 作業系統序號
            components.Add(GetWmiPropertyValue("Win32_OperatingSystem", "SerialNumber"));

            // 7. 電腦名稱
            components.Add(Environment.MachineName);

            // 8. Windows Product ID (從註冊表)
            components.Add(GetWindowsProductId());

            // 9. 安裝日期
            components.Add(GetWmiPropertyValue("Win32_OperatingSystem", "InstallDate"));

            // 移除空值後合併成一個字串
            string fingerprint = string.Join(":", components.Where(s => !string.IsNullOrEmpty(s)));

            // 產生雜湊值
            using (var hasher = SHA256.Create())
            {
                var hashBytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        /// <summary>
        /// 從 WMI 取得特定屬性值
        /// </summary>
        private static string GetWmiPropertyValue(string className, string propertyName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}"))
                {
                    foreach (var managementObject in searcher.Get().Cast<ManagementObject>())
                    {
                        if (managementObject[propertyName] != null)
                        {
                            return managementObject[propertyName].ToString().Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得 WMI {className}.{propertyName} 失敗: {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得磁碟序號 (優先使用 WMI，失敗後使用 Win32 API)
        /// </summary>
        private static string GetDiskVolumeSerialNumber()
        {
            //1. WMI 嘗試
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory); // e.g. C:\
                string driveId = systemDrive.TrimEnd('\\'); // C:
                driveId = driveId.TrimEnd(':') + ":"; // 確保格式 C:
                using (var searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID='{driveId}'"))
                {
                    foreach (ManagementObject drive in searcher.Get())
                    {
                        var v = drive["VolumeSerialNumber"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(v))
                            return v.Trim();
                    }
                }
            }
            catch (ManagementException mex)
            {
                Debug.WriteLine($"取得磁碟序號失敗 (WMI): {mex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得磁碟序號失敗 (一般): {ex.Message}");
            }

            //2. Win32 API 後援
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory); // C:\
                uint serial;
                uint maxCompLen;
                uint fileSysFlags;
                var volName = new StringBuilder(261);
                var fsName = new StringBuilder(261);
                if (GetVolumeInformation(systemDrive, volName, volName.Capacity, out serial, out maxCompLen, out fileSysFlags, fsName, fsName.Capacity))
                {
                    return serial.ToString("X8");
                }
                else
                {
                    Debug.WriteLine("GetVolumeInformation失敗, Win32LastError: " + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得磁碟序號失敗 (API): {ex.Message}");
            }

            return string.Empty;
        }

        /// <summary>
        /// 取得第一個實體網卡的 MAC 地址
        /// </summary>
        private static string GetMacAddress()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback && nic.OperationalStatus == OperationalStatus.Up)
                    .Select(nic => nic.GetPhysicalAddress().ToString())
                    .FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得 MAC 地址失敗: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 從註冊表取得 Windows 產品 ID
        /// </summary>
        private static string GetWindowsProductId()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        return key.GetValue("ProductId")?.ToString() ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"取得 Windows 產品 ID 失敗: {ex.Message}");
            }

            return string.Empty;
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

        public static void OpenNobMuit()
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? strPath = registryKey.GetValue("GameFolder")?.ToString();
            registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            string? oldMultiBootNum = registryKey.GetValue("MultiBootNum")?.ToString();
            registryKey.SetValue("MultiBootNum", 99);
            string dir = strPath?.Substring(0, strPath.IndexOf(':') + 1);
            string runFilePath = @$"{strPath}\nobolHD.bng";
            Debug.WriteLine(@$"runFilePath : {runFilePath}");
            int tryOpenCount = 0;
            // 從註冊表加載之前成功的Session Index
            int? sessionIndex = LoadSessionIndexFromRegistry();

            if (File.Exists(runFilePath))
            {
                Process nob = new Process();

                nob.StartInfo.FileName = runFilePath;
                nob.Start();

                string path = @"./libs/handle.exe";
                if (!System.IO.File.Exists(path))
                {
                    Debug.WriteLine("Handle Error !!");
                    return;
                }

                // 如果有已知的成功Session Index，優先使用它
                if (sessionIndex.HasValue)
                {
                    Debug.WriteLine($"嘗試使用之前成功的Session Index: {sessionIndex.Value}");
                    Process tool = new Process();

                    tool.StartInfo.FileName = path;
                    tool.StartInfo.Arguments = $"-a \"\\Sessions\\{sessionIndex.Value}\\BaseNamedObjects\\Nobunaga Online Game Mutex\"";
                    tool.StartInfo.UseShellExecute = false;
                    tool.StartInfo.RedirectStandardOutput = true;
                    tool.StartInfo.CreateNoWindow = true;

                    bool foundMutex = false;
                    tool.Start();
                    while (!tool.StandardOutput.EndOfStream)
                    {
                        string s = tool.StandardOutput.ReadLine();
                        s = s?.Replace(" ", "") ?? string.Empty;
                        var index1 = s.IndexOf("Mutant");
                        if (index1 > 0)
                        {
                            foundMutex = true;
                            var index2 = s.IndexOf(":", index1);
                            var ID = s.Substring(s.IndexOf("Mutant") + 6, index2 - index1 - 6);

                            Process tool2 = new();
                            tool2.StartInfo.FileName = path;
                            tool2.StartInfo.Arguments = $@"-p {nob.Id} -c {ID} -y";
                            tool2.StartInfo.UseShellExecute = false;
                            tool2.StartInfo.RedirectStandardOutput = true;
                            tool2.StartInfo.CreateNoWindow = true;
                            tool2.Start();
                            tool2.WaitForExit();
                            Debug.WriteLine($"已處理成功的Session {sessionIndex.Value} 的Mutex: {ID}");
                        }
                    }

                    // 如果當前的Session Index仍然有效，就不需要嘗試其他的Sessions
                    if (foundMutex)
                    {
                        Debug.WriteLine("使用已知Session Index處理完成");
                    }
                    else
                    {
                        // 如果已知的Session Index不再有效，清除記錄並嘗試新的Sessions
                        Debug.WriteLine("已知的Session Index不再有效，嘗試新的Sessions");
                        ClearSessionIndexFromRegistry();
                        TryNewSessions(nob, path);
                    }
                }
                else
                {
                    // 如果沒有已知的Session Index，嘗試新的Sessions
                    Debug.WriteLine("沒有已知的Session Index，開始探索新的Sessions");
                    TryNewSessions(nob, path);
                }


                tryOpenCount++;
                if (tryOpenCount > 100)
                {
                    return;
                }
            }

            registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
            int mulitnum = 2;
            int.TryParse(oldMultiBootNum, out mulitnum);
            registryKey.SetValue("MultiBootNum", mulitnum);
        }
        private static void TryNewSessions(Process nob, string path)
        {
            Process tool = new Process();
            int? successfulSessionIndex = null;

            for (int i = 0; i < 10; i++)
            {
                tool.StartInfo.FileName = path;
                tool.StartInfo.Arguments = $"-a \"\\Sessions\\{i}\\BaseNamedObjects\\Nobunaga Online Game Mutex\"";
                tool.StartInfo.UseShellExecute = false;
                tool.StartInfo.RedirectStandardOutput = true;
                tool.StartInfo.CreateNoWindow = true;

                bool foundMutex = false;
                tool.Start();
                while (!tool.StandardOutput.EndOfStream)
                {
                    string s = tool.StandardOutput.ReadLine();
                    s = s?.Replace(" ", "") ?? string.Empty;
                    var index1 = s.IndexOf("Mutant");
                    if (index1 > 0)
                    {
                        foundMutex = true;
                        var index2 = s.IndexOf(":", index1);
                        var ID = s.Substring(s.IndexOf("Mutant") + 6, index2 - index1 - 6);

                        Process tool2 = new();
                        tool2.StartInfo.FileName = path;
                        tool2.StartInfo.Arguments = $@"-p {nob.Id} -c {ID} -y";
                        tool2.StartInfo.UseShellExecute = false;
                        tool2.StartInfo.RedirectStandardOutput = true;
                        tool2.StartInfo.CreateNoWindow = true;
                        tool2.Start();
                        tool2.WaitForExit();
                        Debug.WriteLine($"已處理新Session {i} 的Mutex: {ID}");
                    }
                }

                // 如果在該Session中找到了Mutex，記錄這個成功的Session Index
                if (foundMutex)
                {
                    Debug.WriteLine($"新Session {i} 處理成功");
                    successfulSessionIndex = i;
                    // 只記錄第一個成功的Session
                    break;
                }
            }

            // 儲存發現的成功Session Index
            if (successfulSessionIndex.HasValue)
            {
                SaveSessionIndexToRegistry(successfulSessionIndex.Value);
                Debug.WriteLine($"儲存新的成功Session Index: {successfulSessionIndex.Value}");
            }
        }
        /// <summary>
        /// 從註冊表載入Session Index
        /// </summary>
        public static int? LoadSessionIndexFromRegistry()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
                object value = registryKey.GetValue("SessionsIndex");

                if (value != null)
                {
                    string valueStr = value.ToString();
                    if (int.TryParse(valueStr, out int index))
                    {
                        return index;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"從註冊表載入Session Index失敗: {ex.Message}");
            }

            return null;
        }
        /// <summary>
        /// 將Session Index儲存到註冊表
        /// </summary>
        public static void SaveSessionIndexToRegistry(int sessionIndex)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
                registryKey.SetValue("SessionsIndex", sessionIndex.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"儲存Session Index到註冊表失敗: {ex.Message}");
            }
        }
        /// <summary>
        /// 清除註冊表中的Session Index
        /// </summary>
        public static void ClearSessionIndexFromRegistry()
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc\Setting");
                registryKey.DeleteValue("SessionsIndex", false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"清除註冊表中的Session Index失敗: {ex.Message}");
            }
        }
        public static void SetTimeUp(NOBDATA user)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? ppmitime = registryKey.GetValue("PPMITIME")?.ToString();
            registryKey.SetValue("PPMITIME", user.到期日.ToString());
        }

        public static void UpdateTimer(System.Windows.Controls.Label lb_time)
        {
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(@"Software\TecmoKoei\Nobunaga Online HD Tc");
            string? ppmitime = registryKey.GetValue("PPMITIME")?.ToString();
            if (string.IsNullOrEmpty(ppmitime))
            {
                var time = DateTime.Now.AddDays(7);
                registryKey.SetValue("PPMITIME", time.ToString());
                lb_time.Content = $"到期日:{time.ToString()}";
            }
            else
            {
                var tt = DateTime.Parse(ppmitime);
                //MainWindow.到期日 = tt;
                lb_time.Content = $"到期日:{tt.ToString()}";
            }
        }
        public static int Dis(int x1, int y1, int x2, int y2)
        {
            long dx = x2 - x1;
            long dy = y2 - y1;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 檢查網路連線是否可用
        /// </summary>
        /// <returns>如果網路連線可用返回 true，否則返回 false</returns>
        public static bool IsNetworkAvailable()
        {
            try
            {
                // 方法 1: 檢查是否有可用的網路介面
                if (!NetworkInterface.GetIsNetworkAvailable())
                    return false;

                // 方法 2: 檢查是否有活動的網路連線
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                          (ni.NetworkInterfaceType != NetworkInterfaceType.Loopback) &&
                          (ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel));

                if (!networkInterfaces.Any())
                    return false;

                // 方法 3: 嘗試 Ping 可靠的外部伺服器
                return PingHost("8.8.8.8");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"檢查網路連線時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 嘗試 Ping 指定的主機
        /// </summary>
        /// <param name="hostNameOrAddress">主機名稱或 IP 地址</param>
        /// <returns>如果能成功 Ping 通則返回 true，否則返回 false</returns>
        private static bool PingHost(string hostNameOrAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(hostNameOrAddress, 2000);
                    return reply != null && reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
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