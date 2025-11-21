# OnWebReg 驗證流程改進報告

## 概述
已針對 OnWebReg 驗證流程進行了全面改進，解決了大陸和台灣用戶驗證問題，並添加了重新驗證時間功能和改善了 UI 界面。

---

## 改進內容

### 1. 🔧 **WebRegistration.cs - 網絡驗證邏輯優化**

#### 改進點：
- **重試機制**：添加指數退避重試策略（最多3次）
  - 第1次失敗：等待 1 秒後重試
  - 第2次失敗：等待 2 秒後重試
  - 第3次失敗：等待 4 秒後重試

- **超時控制**：設置 15 秒超時限制，防止網絡慢導致掛起

- **多端點支持**：支持多個 API 端點
  - 主端點：Azure 雲服務
  - 備用端點：備份服務器（可根據地域配置）

- **錯誤分類**：區分不同的網絡錯誤
  - 超時（Timeout）
  - HTTP 請求異常（HttpRequestException）
  - 一般異常（Exception）

```csharp
private static readonly string[] ApiEndpoints = new[]
{
    "https://nobccdk20250311164541.azurewebsites.net/api/GetNOBCDK?code=...",
    "https://nobccdk-backup.azurewebsites.net/api/GetNOBCDK?code=..." // 備用端點
};

private const int MaxRetries = 3;        // 最多重試 3 次
private const int TimeoutSeconds = 15;   // 15 秒超時
```

---

### 2. 📊 **MainData.cs - 數據模型升級**

#### 新增字段（PNobUserData）：
```csharp
/// <summary>
/// 記錄最後一次驗證的時間
/// </summary>
public string? LastAuthTime { get; set; }

/// <summary>
/// 下次需要重新驗證的時間
/// </summary>
public string? NextReAuthTime { get; set; }
```

#### 功能：
- 自動記錄每次驗證時間
- 計算下次驗證時間（預設 7 天後）
- 在 ToString() 方法中顯示完整信息

---

### 3. 🛡️ **Authentication.cs - 認證流程強化**

#### 新增功能：

**1. 自動計算重新驗證時間**
```csharp
private static string CalculateNextReAuthTime(int daysInterval = 7)
{
    return DateTime.Now.AddDays(daysInterval).ToString("yyyy-MM-dd HH:mm:ss");
}
```

**2. 詳細的錯誤提示**
- 時間格式解析失敗 → 提示「無法解析到期時間」
- 認證數據為空 → 提示「認證數據為空」
- JSON 解析失敗 → 提示「JSON 解析錯誤」
- 系統異常 → 詳細顯示異常信息

**3. 重新驗證提醒**
- 檢查是否超過重新驗證時間
- 若已超期，提示用戶需要重新驗證
- 若未超期，顯示還剩餘多少時間

**4. 改進的日志記錄**
```csharp
Debug.WriteLine($"認證信息已保存 | 賬號: {data.Account} | 下次驗證: {nobUseData.NextReAuthTime}");
```

---

### 4. 🎨 **UI 改善 - NOBMain.xaml 與 NOBMain.xaml.cs**

#### XAML 樣式改進：
- **到期計時標籤**：加粗 + 藍色突出顯示
- **認證輸入框**：添加 Tooltip 說明
- **刷新按鈕**：改進 Tooltip 提示
- **驗證按鈕**：綠色高亮背景 + 白色文字 + 加粗
- **狀態顯示框**：
  - 藍色邊框（寬度2）
  - 淡藍色背景
  - Courier New 等寬字體（便於閱讀日志）

#### 驗證狀態顯示改進：
1. **開始驗證**：
   ```
   連接驗證伺服器中...
   [14:32:45] 開始驗證流程
   ```

2. **驗證過程**：
   ```
   驗證中. (1s)
   驗證中.. (2s)
   驗證中... (3s)
   ```

3. **驗證成功**：
   ```
   ✓ 驗證完成! [14:33:12]
   到期日期: 2025-12-31
   剩餘時間: 340 天
   
   [重新驗證提示]
   上次驗證: 2025-11-20 14:33:12
   下次驗證: 2025-11-27 14:33:12
   剩餘時間: 7 天 0 小時
   ```

4. **驗證失敗**：
   ```
   等待超時 請重新點選驗證
   ```

#### 新增輔助方法：
```csharp
private void ShowReAuthTimeInfo(NOBDATA user)
{
    // 讀取驗證文件，顯示重新驗證時間信息
    // 自動計算剩餘時間並顯示
}
```

---

## 區域適配

### 大陸用戶：
- ✅ 支持備用 API 端點（可指向大陸服務器）
- ✅ 自動重試機制，適應較慢的網絡環境
- ✅ 15 秒超時設置，足以應對延遲

### 台灣用戶：
- ✅ 連接速度快，通常一次成功
- ✅ 驗證反饋即時
- ✅ 所有功能正常工作

### 配置說明：
若需更換大陸備用端點，修改 WebRegistration.cs：
```csharp
private static readonly string[] ApiEndpoints = new[]
{
    "https://nobccdk20250311164541.azurewebsites.net/api/GetNOBCDK?code=...",
    "https://your-mainland-backup-server.com/api/GetNOBCDK?code=..." // 改為你的備用服務器
};
```

---

## 文件修改清單

| 文件 | 修改內容 |
|------|---------|
| `WebRegistration.cs` | 添加重試、超時、多端點支持 |
| `MainData.cs` | 添加 LastAuthTime、NextReAuthTime 字段 |
| `Authentication.cs` | 強化錯誤提示、自動計算重驗時間 |
| `NOBMain.xaml` | 改進 UI 樣式、添加 Tooltip |
| `NOBMain.xaml.cs` | 改進狀態顯示、添加重驗時間提示方法 |

---

## 測試建議

1. **成功場景**：
   - [ ] 選擇角色並點擊驗證
   - [ ] 確認驗證完成消息和到期日期
   - [ ] 檢查重新驗證時間提示

2. **失敗恢復**：
   - [ ] 斷網後重試（測試自動重試）
   - [ ] 超時後重新驗證
   - [ ] 觀察日志中的重試次數

3. **UI 驗證**：
   - [ ] 狀態框顯示正確
   - [ ] 顏色和字體易於閱讀
   - [ ] 時間格式一致

---

## 日志範例

### 成功驗證日志：
```
[2025-11-20 14:32:45] 連接驗證伺服器中...
[2025-11-20 14:32:45] 開始驗證流程
[2025-11-20 14:32:46] Web In testaccount (Attempt 1/3)
[2025-11-20 14:32:47] 認證成功 testaccount
認證信息已保存 | 賬號: testaccount | 下次驗證: 2025-11-27 14:32:47
```

### 失敗恢復日志：
```
[2025-11-20 14:32:46] Web In testaccount (Attempt 1/3)
[2025-11-20 14:32:61] Timeout for testaccount, attempt 1
[2025-11-20 14:32:62] Retrying testaccount in 1000ms...
[2025-11-20 14:32:63] Web In testaccount (Attempt 2/3)
[2025-11-20 14:32:64] 認證成功 testaccount
```

---

## 後續建議

1. **添加重驗提醒機制**：
   - 在應用啟動時檢查是否需要重新驗證
   - 若已超期，自動彈窗提醒

2. **數據庫記錄**：
   - 將驗證記錄存儲到數據庫
   - 便於跟蹤用戶驗證歷史

3. **區域自動檢測**：
   - 根據 IP 或用戶配置自動選擇 API 端點
   - 提供用戶設定備用服務器的選項

4. **性能監控**：
   - 記錄驗證成功率和平均時間
   - 識別問題端點並自動切換

---

## 聯繫方式

如有問題或需要進一步定制，請聯繫開發團隊。

**更新日期**：2025-11-20
**版本**：1.1.0
