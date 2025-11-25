using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NOBApp.Managers
{
 /// <summary>
 /// 遊戲視窗管理：刷新、排列、批次開啟
 /// </summary>
 public class GameWindowManager
 {
 public List<NOBDATA> NobWindows { get; } = new();
 public int OrinX { get; private set; }
 public int OrinY { get; private set; }

 public void RefreshNOBID(ComboBox cbHid, ComboBox[] comboBoxes)
 {
 var processes = Process.GetProcesses();
 NobWindows.Clear();
 cbHid.Items.Clear();
 foreach (var p in processes)
 {
 if (p.ProcessName.Contains("nobolHD"))
 {
 var data = new NOBDATA(p);
 NobWindows.Add(data);
 cbHid.Items.Add(data.PlayerName);
 }
 }
 UIUpdate.RefreshNOBID_Sec(comboBoxes, NobWindows);
 }

 public void AutoArrangeWindows(string resolutionText)
 {
 if ((OrinX ==0 || OrinY ==0) && !string.IsNullOrEmpty(resolutionText))
 {
 var arr = resolutionText.Split(',');
 if (arr.Length ==2 && int.TryParse(arr[0], out int x) && int.TryParse(arr[1], out int y))
 {
 OrinX = x +16;
 OrinY = y +39;
 }
 }
 if (OrinX <=0 || OrinY <=0) return;
 for (int i =0; i < NobWindows.Count; i++)
 {
 NobWindows[i].MoveWindowTool(i);
 }
 }

 public async Task MultiOpenAsync(int count, string resolutionText, string gamePadName)
 {
 if (!string.IsNullOrEmpty(resolutionText))
 {
 var arr = resolutionText.Split(',');
 if (arr.Length ==2) Tools.SetResolution(arr[0], arr[1]);
 }
 Tools.SetGamePad(gamePadName);
 for (int i =0; i < count; i++)
 {
 await Task.Run(() => Tools.OpenNobMuit());
 }
 }
 }
}