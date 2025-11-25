using RegisterDmSoftConsoleApp.Configs;
using RegisterDmSoftConsoleApp.DmSoft;
using System;
using System.Diagnostics;
using System.Windows;

namespace NOBApp.Managers
{
 /// <summary>
 /// 管理大漠插件註冊與初始化
 /// </summary>
 public class DmSoftManager
 {
 public DmSoftCustomClassName? DmSoft { get; private set; }

 /// <summary>註冊並初始化大漠插件</summary>
 public void InitializeDmSoft()
 {
 try
 {
 var registerDmSoftDllResult = RegisterDmSoft.RegisterDmSoftDll();
 if (!registerDmSoftDllResult)
 {
 Debug.WriteLine("[WARN] 註冊大漠插件失敗 (SetDllPathA 返回0)，相關功能將停用");
 MessageBox.Show("無法載入大漠插件，部分自動操作功能將停用。\n請確認 libs目錄下 dm.dll 與 DmReg.dll 存在。", "插件載入失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
 }
 }
 catch (DllNotFoundException ex)
 {
 Debug.WriteLine("[ERROR] DllNotFound: " + ex.Message);
 MessageBox.Show("找不到大漠插件 DLL，請確認已放置於 libs目錄。", "初始化錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
 }
 catch (BadImageFormatException ex)
 {
 Debug.WriteLine("[ERROR] 架構不相容: " + ex.Message);
 MessageBox.Show("大漠插件與目前程式架構不相容。請確保使用 x86。", "初始化錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
 }
 catch (Exception ex)
 {
 Debug.WriteLine("[ERROR] 註冊大漠插件發生例外: " + ex);
 MessageBox.Show("載入大漠插件時發生未預期錯誤，功能可能受限。", "初始化錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
 }

 DmSoft = new DmSoftCustomClassName();
 if (DmSoft.IsAvailable)
 {
 try { DmSoft.SetPath(DmConfig.DmGlobalPath); }
 catch (Exception ex) { Debug.WriteLine("[ERROR] SetPath失敗: " + ex.Message); }
 }
 else
 {
 Debug.WriteLine("[INFO] dmSoft COM 未可用，略過 SetPath。 InitError=" + DmSoft.InitError);
 }
 }
 }
}