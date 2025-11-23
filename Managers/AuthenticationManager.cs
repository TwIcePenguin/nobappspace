using NOBApp.GoogleData;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NOBApp.Managers
{
    public class AuthenticationManager
    {
        private readonly NobMainCodePage _view;
        
   // 用於時間防作弊與減少網路請求
        private DateTime _lastNetworkTime = DateTime.MinValue;
        private long _lastTickCount = 0;
      private bool _isUpdatingTime = false; // 防止重入

   public AuthenticationManager(NobMainCodePage view)
  {
     _view = view;
 }

        public async Task HandleLockClick()
        {
 var lockBtn = _view.LockBtn;
       var cbHid = _view.CB_HID;
     var auth2Cb = _view.認證2CB;
            var authTBox = _view.認證TBox;
         var statusBox = _view.視窗狀態;
        var startCode = _view.StartCode;
            var controlGrid = _view.ControlGrid;
 var skillDataGrid = _view.SkillDataGird;
        var useSkillCb = _view.UseSkill_CB;
            var rootTabItem = _view.RootTabItem;
     var vipSp = _view.VIPSP;
  var igMouse = _view.IGMouse;
var reportBtn = _view.Btn_ReportIssue;
  var contactBtn = _view.Btn_ContactPurchase;
            var reportPanel = _view.FindName("回報系統視窗") as StackPanel;

      bool reset = lockBtn.Content.ToString()!.Contains("解除");
bool isPass = false;

#if DEBUG && false
    _view.腳本區.IsEnabled = _view.腳本展區.IsEnabled = _view.戰鬥輔助面.IsEnabled = true;
            _view.UpdateSelectMenu();
            return;
#endif

  if (lockBtn != null && controlGrid != null && cbHid != null &&
             cbHid.SelectedValue != null && MainWindow.AllNobWindowsList != null)
            {
          string idstr = cbHid.SelectedValue.ToString();
        if (!reset && !string.IsNullOrEmpty(idstr))
           {
 _view.MainNob = MainWindow.AllNobWindowsList?.Find(r => r.PlayerName == idstr);

         if (MainWindow.AllNobWindowsList == null || MainWindow.AllNobWindowsList.Count == 0)
         {
MessageBox.Show("請先刷新角色資料");
return;
              }

            Debug.WriteLine($"Web Reg {MainWindow.AllNobWindowsList.Count}");

 if (_view.MainNob != null)
        {
          statusBox.Clear();

       // 檢查本地驗證檔案
               bool localValid = false;
      string cdkFilePath = $@"{_view.MainNob.Account}_CDK.nob";
                 PNobUserData? nobUseData = null;

            if (File.Exists(cdkFilePath))
       {
         try
            {
    using (StreamReader reader = new(cdkFilePath))
  {
      string jsonString = reader.ReadToEnd();
             string dJson = Encoder.AesDecrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
  nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);
    }
           }
 catch (Exception ex)
  {
     Debug.WriteLine($"Local Auth Check Failed: {ex.Message}");
  }
   }

      // 獲取網路時間進行比對
    DateTime currentTime = await NetworkTime.GetNowAsync();
          _lastNetworkTime = currentTime;
    _lastTickCount = Environment.TickCount64;

           Debug.WriteLine($"Current Network Time: {currentTime}");

        if (nobUseData != null && DateTime.TryParse(nobUseData.StartTimer, out DateTime expireDate))
          {
// 如果尚未過期 (使用網路時間比對)
    if (expireDate > currentTime)
     {
      bool needReAuth = true;
         if (DateTime.TryParse(nobUseData.NextReAuthTime, out DateTime nextReAuth))
                 {
      if (nextReAuth > currentTime) needReAuth = false;
   }

    if (needReAuth)
         {
     if (MessageBox.Show("驗證期限已過，是否進行驗證？\n(選擇「否」將在7天後再次詢問)", "驗證提示", MessageBoxButton.YesNo) == MessageBoxResult.No)
     {
  nobUseData.NextReAuthTime = currentTime.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss");
  await Authentication.儲存認證訊息Async(_view.MainNob, nobUseData);
                localValid = true;
}
    }
        else
                  {
            localValid = true;
                    }
            }
      }

   if (localValid)
  {
       statusBox.AppendText($"使用本機驗證資料...\n");
                 Authentication.讀取認證訊息Name(_view.MainNob);
           }
       else
            {
           statusBox.AppendText("連接驗證伺服器中...\n");
     statusBox.AppendText($"[{DateTime.Now:HH:mm:ss}] 開始驗證流程\n");

        await Task.Run(() => WebRegistration.OnWebReg());
       statusBox.AppendText($"[{DateTime.Now:HH:mm:ss}] 驗證請求已發送\n");

      if (_view.MainNob != null)
               {
            if (!_view.MainNob.驗證完成)
      {
              if (auth2Cb.IsChecked == true)
    {
       if (string.IsNullOrEmpty(authTBox.Text))
 {
          statusBox.AppendText("正在連接 Google Sheet...\n");
       await GoogleSheet.GoogleSheetInitAsync();
    statusBox.AppendText("Google Sheet 連接成功，檢查贊助名單...\n");
         await GoogleSheet.CheckDonateAsync(_view.MainNob);
   statusBox.AppendText("贊助名單檢查完成\n");
            }
     else
         {
         statusBox.AppendText($"正在解析認證碼: {authTBox.Text}...\n");
         Authentication.讀取認證訊息Json(_view.MainNob, authTBox.Text);
     }
           }

       string baseLog = statusBox.Text;
    int checkCount = 0;
              while (true)
        {
  Debug.WriteLine($"MainNob 驗證 {_view.MainNob.驗證完成} Count {checkCount}");
   if (_view.MainNob.驗證完成)
     {
 statusBox.Text = baseLog + $"\n✓ 驗證完成! [{DateTime.Now:HH:mm:ss}]";

                if (_view.MainNob.到期日 != DateTime.MinValue)
     {
 DateTime nowTime = await NetworkTime.GetNowAsync();
     TimeSpan remainingTime = _view.MainNob.到期日 - nowTime;
      if (remainingTime.TotalDays > 0)
    {
     statusBox.AppendText($"\n到期日期: {_view.MainNob.到期日:yyyy-MM-dd}\n剩餘時間: {remainingTime.Days} 天\n");
   }
   else
      {
          statusBox.AppendText($"\n⚠ 認證已過期！\n");
    }
   }

      checkCount = 0;
               break;
       }
         else
     {
    checkCount++;
     int dotCount = (checkCount % 3) + 1;
    statusBox.Text = baseLog + $"\n驗證中{new string('.', dotCount)} ({checkCount * 0.4:F1}s)";
  statusBox.ScrollToEnd();
    }
               if (checkCount >= 60)
   {
  statusBox.Text = baseLog + "\n等待超時 請重新點選驗證";
         MessageBox.Show("無法連接驗證伺服器，請稍後再試。", "驗證失敗");
     return;
          }
 await Task.Delay(400);
  }
              }
    }
    }

        statusBox.AppendText("取得相關資料 比對中..\n");
    try
         {
       bool SPPass = _view.MainNob.特殊者 ? _view.MainNob.驗證國家 : _view.MainNob.贊助者;
      
    // 如果過期，強制允許通過但關閉VIP
  DateTime nowTime = await NetworkTime.GetNowAsync();
  if (_view.MainNob.到期日 < nowTime)
             {
         isPass = true;
          }
       else
     {
      if (_view.MainNob.特殊者 && !_view.MainNob.驗證國家)
           {
     MessageBox.Show("免費使用者 需要加入遊戲頻道 請聯繫企鵝 取得加入的方式 或著請認識的朋友提供");
         return;
           }
      isPass = SPPass;
        }

  _view.MainNob.驗證完成 = isPass;

        Debug.WriteLine($"MainNob 驗證 {isPass} {_view.MainNob.特殊者} {_view.MainNob.驗證國家}");
               }
      catch (Exception err)
          {
      statusBox.AppendText($"資料錯誤.. \n{err}\n");
           }

         Tools.SetTimeUp(_view.MainNob);
      statusBox.AppendText($"驗證完成.. 更新時間 -> {_view.MainNob.到期日}\n");
   
 DateTime checkTime = await NetworkTime.GetNowAsync();
                if (_view.MainNob.到期日 >= checkTime)
       {
         _view.到期計時.Content = $"到期時間: {_view.MainNob.到期日:yyyy-MM-dd} (有效)";
     _view.到期計時.Foreground = new SolidColorBrush(Colors.Black);
  }
       else
  {
        _view.到期計時.Content = $"到期時間: {_view.MainNob.到期日:yyyy-MM-dd} (過期)";
            _view.到期計時.Foreground = new SolidColorBrush(Colors.Red);
          }

        await UpdateRemainingDays(_view.到期計時);

             ShowReAuthTimeInfo(_view.MainNob, statusBox);

            igMouse.IsEnabled = true;
                if (_view.MainNob.到期日 >= checkTime)
      {
           Tools.isBANACC = false;
  Tools.IsVIP = true;
            vipSp.IsEnabled = true;
       if (_view.MainNob.特殊者 || _view.MainNob.贊助者)
            {
      vipSp.IsChecked = true;
      }
          }
  else
  {
          Tools.IsVIP = false;
      vipSp.IsEnabled = false;
  vipSp.IsChecked = false;
    }

      if (isPass)
   {
  _view.RestartTimer();
       statusBox.AppendText($"通過驗證..");
      _view.腳本區.IsEnabled = _view.腳本展區.IsEnabled = _view.戰鬥輔助面.IsEnabled = true;
    _view.UpdateSelectMenu();
         _view.LoadSetting();
            _view.讀取隊員技能組();

       MainWindow.Instance?.SaveTabState();

         // 更新認證UI
                authTBox.Text = _view.MainNob.NOBCDKEY;
       auth2Cb.IsChecked = true;

        // 啟用 VIP相關按鈕
     reportBtn.IsEnabled = Tools.IsVIP;
         contactBtn.IsEnabled = Tools.IsVIP;

 // 顯示回報系統視窗
     if (reportPanel != null)
       reportPanel.Visibility = Visibility.Visible;
            }
  else
          {
 statusBox.AppendText($"該帳號 驗證失敗.. 請聯繫企鵝處理");
         _view.MainNob.StartRunCode = false;
            _view.StopTimer();
  reportBtn.IsEnabled = false;
         contactBtn.IsEnabled = false;

       // 仍然顯示回報系統視窗，但禁用按鈕
  if (reportPanel != null)
           reportPanel.Visibility = Visibility.Visible;
            }
      }
        else
       {
 startCode.IsChecked = false;
      startCode.UpdateLayout();
           MessageBox.Show("選擇異常 不存在的角色資料 或著該角色被關閉請刷新後 請重新嘗試驗證");
   }
     }
      else if (reset)
     {
          // 重置時隱藏回報系統視窗
       if (reportPanel != null)
 reportPanel.Visibility = Visibility.Hidden;
       }

        // UI updates based on isPass
      _view.隊員額外功能頁面.IsEnabled = isPass;
    skillDataGrid.IsEnabled = isPass;
 useSkillCb.IsEnabled = isPass;
   controlGrid.IsEnabled = isPass;
 cbHid.IsEnabled = !isPass;
                if (isPass)
  {
     rootTabItem.Header = $"{_view.MainNob!.PlayerName}";
 }

    lockBtn.Content = isPass ? "解除" : "驗證";
       lockBtn.UpdateLayout();
            if (!isPass)
       {
        reportBtn.IsEnabled = false;
        contactBtn.IsEnabled = false;
                }
  }
            else
       {
     MessageBox.Show("請選擇角色 ，　如果清單沒有角色名稱，開啟遊戲登入選擇完角色後點［刷新］");

      lockBtn.Content = "驗證";
           cbHid.IsEnabled = true;
       cbHid.UpdateLayout();
         startCode.IsChecked = false;
            startCode.UpdateLayout();
           reportBtn.IsEnabled = false;
          contactBtn.IsEnabled = false;

       // 隱藏回報系統視窗（未登入狀態）
    if (reportPanel != null)
     reportPanel.Visibility = Visibility.Hidden;
         }
        }

        public void ShowReAuthTimeInfo(NOBDATA user, TextBox statusBox)
  {
      if (user == null) return;

      try
      {
   string cdkFilePath = $@"{user.Account}_CDK.nob";
       if (File.Exists(cdkFilePath))
    {
         using (StreamReader reader = new(cdkFilePath))
      {
      string jsonString = reader.ReadToEnd();
       string dJson = Encoder.AesDecrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
     PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);

     if (nobUseData != null && !string.IsNullOrEmpty(nobUseData.NextReAuthTime))
        {
      if (DateTime.TryParse(nobUseData.NextReAuthTime, out DateTime nextReAuthDate))
         {
              TimeSpan timeUntilReAuth = nextReAuthDate - DateTime.Now;
  if (timeUntilReAuth.TotalHours > 0)
    {
         statusBox.AppendText($"\n[重新驗證提示]\n");
      statusBox.AppendText($"上次驗證: {nobUseData.LastAuthTime}\n");
           statusBox.AppendText($"下次驗證: {nobUseData.NextReAuthTime}\n");
            statusBox.AppendText($"剩餘時間: {timeUntilReAuth.Days} 天 {timeUntilReAuth.Hours} 小時\n");
 }
    else
               {
           statusBox.AppendText($"\n⚠ 已需要重新驗證！\n");
  }
        }
            }
  }
              }
  }
       catch (Exception ex)
            {
           Debug.WriteLine($"讀取驗證情報錯誤: {ex.Message}");
        }
      }

      public async Task UpdateRemainingDays(Label remainingDaysLabel)
  {
    if (_isUpdatingTime) return;
         _isUpdatingTime = true;

        try
{
          if (_view.MainNob == null || _view.MainNob.到期日 == DateTime.MinValue)
    {
        remainingDaysLabel.Content = "剩餘天數: 未驗證";
     return;
            }

                long currentTick = Environment.TickCount64;
   long elapsedMs = currentTick - _lastTickCount;
    
           if (_lastTickCount == 0 || elapsedMs > 14400000)
      {
      try 
       {
   _lastNetworkTime = await NetworkTime.GetNowAsync();
           _lastTickCount = Environment.TickCount64;
       currentTick = _lastTickCount;
          elapsedMs = 0;
        }
          catch
  {
           if (_lastTickCount == 0)
    {
       _lastNetworkTime = DateTime.Now;
   _lastTickCount = currentTick;
           }
          }
     }

         DateTime nowTime = _lastNetworkTime.AddMilliseconds(elapsedMs);
          TimeSpan remaining = _view.MainNob.到期日 - nowTime;

      if (remaining.TotalSeconds > 0)
       {
       remainingDaysLabel.Content = $"剩餘天數: {remaining.Days} 天";
             remainingDaysLabel.Foreground = remaining.Days <= 7
         ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Color.FromRgb(255, 221, 0));
             }
        else
     {
            remainingDaysLabel.Content = "剩餘天數: 已過期";
      remainingDaysLabel.Foreground = new SolidColorBrush(Colors.Red);
        }
    }
    finally
            {
     _isUpdatingTime = false;
     }
        }

      public async Task CheckValidity(TextBox statusBox, Label expireLabel)
        {
        if (_view.MainNob == null)
            {
         MessageBox.Show("請先選擇並驗證一個角色", "提示");
       statusBox.Clear();
  statusBox.AppendText($"[{DateTime.Now:HH:mm:ss}] ⚠ 請先選擇角色\n");
         return;
        }

         statusBox.Clear();
            statusBox.AppendText($"[{DateTime.Now:HH:mm:ss}] 查詢賬號有效期中...\n");
   statusBox.ScrollToEnd();

            try
    {
       await GoogleSheet.GoogleSheetInitAsync();
         await GoogleSheet.CheckDonateAsync(_view.MainNob);
            }
   catch (Exception ex)
            {
    statusBox.AppendText($"⚠️ 無法連接驗證伺服器: {ex.Message}\n");
         statusBox.AppendText($"將顯示本地緩存信息...\n");
      statusBox.ScrollToEnd();
  }

     try
     {
       string cdkFilePath = $@"{_view.MainNob.Account}_CDK.nob";
          if (!File.Exists(cdkFilePath))
       {
     statusBox.AppendText($"❌ 找不到驗證文件\n");
         statusBox.AppendText($"賬號: {_view.MainNob.Account}\n");
   statusBox.AppendText($"請先進行驗證！\n");
     MessageBox.Show($"賬號 {_view.MainNob.Account} 沒有驗證記錄，請先驗證", "未驗證");
       return;
      }

 using (StreamReader reader = new(cdkFilePath))
                {
          string jsonString = await reader.ReadToEndAsync();
           string dJson = Encoder.AesDecrypt(jsonString, "CHECKNOBPENGUIN", "CHECKNOB");
 PNobUserData nobUseData = JsonSerializer.Deserialize<PNobUserData>(dJson);

        if (nobUseData != null && DateTime.TryParse(nobUseData.StartTimer, out DateTime expireDate))
            {
           DateTime nowTime = await NetworkTime.GetNowAsync();
      _lastNetworkTime = nowTime;
             _lastTickCount = Environment.TickCount64;

     TimeSpan remaining = expireDate - nowTime;
 
      statusBox.AppendText($"\n📋 賬號驗證信息\n");
  statusBox.AppendText($"━━━━━━━━━━━━━━━━━━\n");
             statusBox.AppendText($"帳號: {_view.MainNob.Account}\n");
                statusBox.AppendText($"角色: {_view.MainNob.PlayerName}\n");
            statusBox.AppendText($"\n⏰ 有效期信息\n");
      statusBox.AppendText($"到期時間: {expireDate:yyyy-MM-dd HH:mm:ss}\n");

                 if (remaining.TotalSeconds > 0)
 {
          statusBox.AppendText($"✅ 狀態: 有效\n");
             statusBox.AppendText($"剩餘時間: {remaining.Days} 天 {remaining.Hours} 小時 {remaining.Minutes} 分鐘\n");
       
    expireLabel.Content = $"到期時間: {expireDate:yyyy-MM-dd} (有效)";
           expireLabel.Foreground = new SolidColorBrush(Colors.White);
             
                 if (remaining.TotalDays <= 7)
     {
    statusBox.AppendText($"⚠️ 提醒: 即將過期，請提前續費\n");
       }
            }
 else
    {
      statusBox.AppendText($"❌ 狀態: 已過期\n");
            statusBox.AppendText($"過期時間: {Math.Abs(remaining.Days)} 天前\n");
     statusBox.AppendText($"請聯繫管理員續費\n");
           
      expireLabel.Content = $"到期時間: {expireDate:yyyy-MM-dd} (已過期)";
        expireLabel.Foreground = new SolidColorBrush(Colors.Red);
              }

              await UpdateRemainingDays(_view.到期計時);

 var vipSp = _view.VIPSP;
       if (remaining.TotalSeconds > 0)
         {
         vipSp.IsEnabled = true;
            bool isVip = false;
             if (nobUseData.CheckC != null && nobUseData.CheckC.Contains("1"))
              {
           isVip = true;
      }
           if (_view.MainNob.特殊者 || _view.MainNob.贊助者)
         {
          isVip = true;
           }

            if (isVip)
        {
         vipSp.IsChecked = true;
 statusBox.AppendText($"👑 VIP 權限: 已啟用\n");
           }
    }
        else
   {
          vipSp.IsEnabled = false;
      vipSp.IsChecked = false;
     }
        
         statusBox.ScrollToEnd();

   if (!string.IsNullOrEmpty(nobUseData.LastAuthTime))
               {
    statusBox.AppendText($"\n📅 驗證記錄\n");
 statusBox.AppendText($"上次驗證: {nobUseData.LastAuthTime}\n");
         }

       if (!string.IsNullOrEmpty(nobUseData.NextReAuthTime))
           {
            if (DateTime.TryParse(nobUseData.NextReAuthTime, out DateTime nextReAuth))
     {
 TimeSpan timeUntilReAuth = nextReAuth - nowTime;
       statusBox.AppendText($"下次驗證: {nobUseData.NextReAuthTime}\n");
              if (timeUntilReAuth.TotalSeconds < 0)
     {
   statusBox.AppendText($"⚠ 已需要重新驗證！\n");
          }
  }
    else
                {
           statusBox.AppendText($"下次驗證: {nobUseData.NextReAuthTime}\n");
          }
        }

                  statusBox.AppendText($"━━━━━━━━━━━━━━━━━━\n");
   statusBox.AppendText($"[{DateTime.Now:HH:mm:ss}] ✓ 查詢完成\n");
 }
           else
  {
       statusBox.AppendText($"❌ 驗證文件格式錯誤\n");
      statusBox.AppendText($"無法解析到期時間\n");
     }
         }
            }
            catch (Exception ex)
        {
     statusBox.AppendText($"❌ 出現錯誤: {ex.Message}\n");
     Debug.WriteLine($"檢查有效期錯誤: {ex}");
     }
        }
    }
}
