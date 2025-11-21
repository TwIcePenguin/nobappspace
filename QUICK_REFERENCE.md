# OnWebReg 改進 - 快速參考

## 🎯 改進摘要

| 項目 | 改進內容 |
|-----|---------|
| **重試機制** | 3 次自動重試，指數退避（1s, 2s, 4s） |
| **超時控制** | 15 秒超時，防止無限掛起 |
| **多端點** | 支持主服務器 + 備用服務器切換 |
| **驗證時間** | 自動記錄上次驗證時間 + 計算下次驗證時間 |
| **提醒機制** | 超期自動提醒，顯示剩餘時間 |
| **UI 優化** | 綠色驗證按鈕、藍色邊框、動畫進度 |
| **錯誤提示** | 詳細的錯誤分類，不再模糊 |

---

## 🔧 如何使用

### 基本驗證
```
1. 選擇角色 → CB_HID 下拉框
2. 點擊「驗證」按鈕 → LockBtn (綠色)
3. 等待驗證完成 → 檢查視窗狀態
4. 查看重驗時間 → 自動顯示在狀態框
```

### 配置備用服務器
```csharp
// 編輯文件：WebRegistration.cs (第 19-23 行)
private static readonly string[] ApiEndpoints = new[]
{
    "https://現有服務器...",
    "https://你的備用服務器..." // 改這一行
};
```

### 調整驗證周期
```csharp
// 編輯文件：Authentication.cs (第 11 行)
private static string CalculateNextReAuthTime(int daysInterval = 7)
// 改 7 為你想要的天數 (例如 30 = 30天)
```

---

## 💾 核心修改文件

### 1️⃣ WebRegistration.cs
**變更**：添加重試、超時、多端點支持
```csharp
// 新增：AsyncTask SendAuthenticationData() 方法
// 特性：3 次重試 + 15 秒超時 + CancellationToken
```

### 2️⃣ MainData.cs
**變更**：PNobUserData 新增時間字段
```csharp
public string? LastAuthTime { get; set; }    // 上次驗證
public string? NextReAuthTime { get; set; }  // 下次驗證
```

### 3️⃣ Authentication.cs
**變更**：強化錯誤提示和自動計算驗證時間
```csharp
// 新增：CalculateNextReAuthTime() 方法
// 改進：詳細的 MessageBox 提示
// 改進：超期自動提醒
```

### 4️⃣ NOBMain.xaml
**變更**：UI 樣式改進
```xml
<!-- 驗證按鈕：綠色高亮 -->
<Button ... Background="#FF66CC00" Foreground="White"/>

<!-- 狀態框：藍色邊框 -->
<TextBox ... BorderBrush="#FF0066CC" BorderThickness="2"/>
```

### 5️⃣ NOBMain.xaml.cs
**變更**：驗證邏輯和 UI 反饋改進
```csharp
// 改進：ShowReAuthTimeInfo() 方法
// 改進：驗證進度動畫和詳細消息
```

---

## 📊 驗證狀態顯示

### ✅ 成功
```
✓ 驗證完成! [14:33:12]
到期日期: 2025-12-31
剩餘時間: 340 天

[重新驗證提示]
上次驗證: 2025-11-20 14:33:12
下次驗證: 2025-11-27 14:33:12
剩餘時間: 7 天 0 小時
```

### ⏳ 進行中
```
驗證中. (1s)
驗證中.. (2s)
驗證中... (3s)
```

### ❌ 超期
```
等待超時 請重新點選驗證
```

---

## 🐛 故障排查

| 問題 | 原因 | 解決方案 |
|-----|-----|---------|
| 無法驗證 | 網絡中斷 | 檢查網絡連接，稍後重試 |
| 驗證超時 | 服務器慢 | 自動重試 3 次，檢查 Debug 日志 |
| 驗證失敗 | 帳號不匹配 | 確認帳號密碼正確 |
| 時間解析失敗 | 數據格式錯誤 | 聯繫管理員檢查認證碼 |
| 已超期需重驗 | 超過 7 天 | 點擊驗證重新認證 |

---

## 📝 日志查看

### 位置
Visual Studio → Debug → Windows → Output  
快捷鍵：`Ctrl + Alt + O`

### 查找重驗信息
```
Ctrl + F 搜索：
- "下次驗證" → 查看重驗時間
- "Attempt" → 查看重試次數
- "Timeout" → 查看超時情況
```

---

## 🎓 對比表

| 功能 | 改進前 | 改進後 |
|-----|-------|--------|
| **重試** | ❌ 無 | ✅ 3 次 |
| **超時** | ❌ 無限制 | ✅ 15 秒 |
| **端點** | ❌ 單一 | ✅ 多個 |
| **時間記錄** | ❌ 無 | ✅ 自動 |
| **提醒** | ❌ 無 | ✅ 自動 |
| **錯誤信息** | ⚠️ 模糊 | ✅ 詳細 |
| **UI** | ⚠️ 簡陋 | ✅ 優雅 |

---

## 🔗 相關文檔

- **詳細文檔**：`AUTH_IMPROVEMENTS.md`
- **測試指南**：`TESTING_GUIDE.md`
- **完成摘要**：`COMPLETION_SUMMARY.md`

---

## 📞 支援

若有問題，請提供：
1. 錯誤信息（Debug Output）
2. 發生時間
3. 使用環境（大陸/台灣）
4. 網絡環境（寬帶/移動）

---

**版本**：1.1.0  
**日期**：2025-11-20  
**狀態**：✅ 已完成
