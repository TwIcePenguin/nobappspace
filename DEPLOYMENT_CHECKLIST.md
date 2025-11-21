# OnWebReg 改進 - 部署檢清單

## ✅ 部署前檢查

### 代碼審核
- [ ] WebRegistration.cs 編譯無誤
- [ ] MainData.cs 編譯無誤
- [ ] Authentication.cs 編譯無誤
- [ ] NOBMain.xaml.cs 編譯無誤
- [ ] NOBMain.xaml 設計器顯示正常
- [ ] 所有導入語句正確
- [ ] 沒有 using 衝突

### 功能驗證
- [ ] 驗證邏輯無誤（檢查 SendAuthenticationData 方法）
- [ ] 重試機制正常（MaxRetries = 3）
- [ ] 超時控制有效（TimeoutSeconds = 15）
- [ ] 時間記錄功能正常
- [ ] 重驗計算正確（7 天周期）
- [ ] 錯誤提示顯示正確
- [ ] UI 樣式應用正確

### Git 檢查
- [ ] 所有文件已保存
- [ ] 沒有未追蹤的文件（除了文檔）
- [ ] 變更內容清晰（查看 diff）
- [ ] 提交消息描述準確

---

## 🚀 部署步驟

### 1. 備份
```powershell
# 備份當前版本
Copy-Item -Path "NOBApp.csproj" -Destination "NOBApp.csproj.backup" -Force
Copy-Item -Path "WebRegistration.cs" -Destination "WebRegistration.cs.backup" -Force
Copy-Item -Path "Authentication.cs" -Destination "Authentication.cs.backup" -Force
```

### 2. 編譯
```powershell
# 清理舊版本
dotnet clean

# 編譯新版本
dotnet build

# 檢查編譯錯誤
# 如有錯誤，檢查 Build Output 窗口
```

### 3. 測試 (本地)
```powershell
# 啟動應用
# 進行基本驗證測試
# 確認 UI 改進生效
# 檢查 Debug 日志
```

### 4. 版本號更新
編輯 `AssemblyInfo.cs` 或 `VersionInfo.cs`：
```csharp
// 當前版本：1.0.0
// 新版本：1.1.0 (次要版本更新)

[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
```

### 5. 文檔更新
- [ ] 更新 README.md（新增改進說明）
- [ ] 更新 CHANGELOG.md（記錄版本改動）
- [ ] 確認所有文檔文件已添加

### 6. 發佈
```powershell
# 發佈到應用商店或分發點
# 通知用戶更新
# 監控使用反饋
```

---

## 📋 变更摘要 (用於 CHANGELOG)

```markdown
## [1.1.0] - 2025-11-20

### 新增
- 驗證重試機制 (最多 3 次，指數退避)
- 超時控制 (15 秒)
- 多 API 端點支持 (主 + 備用)
- 驗證時間自動記錄
- 下次驗證時間自動計算
- 驗證過期自動提醒
- 詳細的驗證狀態顯示
- 詳細的錯誤提示

### 改進
- 改進驗證 UI 界面 (綠色按鈕、藍色邊框)
- 改進驗證狀態反饋 (時間戳、進度動畫)
- 改進錯誤日志記錄
- 改進驗證流程可靠性

### 修復
- 修復大陸用戶驗證問題
- 修復驗證超時掛起問題
- 修復驗證失敗提示模糊問題

### 技術細節
- WebRegistration.cs: 添加 SendAuthenticationData 方法
- MainData.cs: PNobUserData 新增 LastAuthTime, NextReAuthTime
- Authentication.cs: 改進錯誤處理和時間計算
- NOBMain.xaml: 改進 UI 樣式
- NOBMain.xaml.cs: 改進驗證邏輯和反饋

### 已知問題
- 無

### 向後相容性
- ✅ 完全相容舊版本
- ✅ 舊版本 .nob 文件可正常讀取
```

---

## 🧪 部署後測試

### 1. 冒煙測試 (Smoke Test)
```
1. 啟動應用
2. 選擇角色
3. 點擊驗證
4. 確認驗證完成
5. 檢查到期日期
6. 檢查重驗時間信息
```

### 2. 區域測試
- [ ] **大陸用戶模擬**：
  - 臨時斷網，確認自動重試
  - 慢速網絡，確認 15 秒內完成
  - 檢查日志中的重試信息

- [ ] **台灣用戶模擬**：
  - 正常網絡驗證
  - 確認快速完成
  - 確認 UI 反饋正確

### 3. 邊界情況測試
- [ ] 多個角色同時驗證
- [ ] 驗證文件已存在
- [ ] 驗證文件損壞
- [ ] 網絡中斷恢復
- [ ] 認證碼錯誤

### 4. 性能測試
- [ ] 驗證平均耗時
- [ ] 內存消耗
- [ ] CPU 使用率
- [ ] 線程數量

---

## 📊 監控指標

### 部署後一周內監控
| 指標 | 目標 | 閾值 |
|-----|-----|------|
| 驗證成功率 | > 95% | < 90% 警報 |
| 平均驗證時間 | < 5s | > 10s 警報 |
| 重試率 | < 5% | > 10% 警報 |
| 用戶投訴 | 0 | > 2 警報 |

### 監控方法
```powershell
# 查看驗證日志
Get-Content "Debug_Output.log" | Select-String "驗證"

# 統計成功率
(Get-Content "Debug_Output.log" | Select-String "認證成功").Count / 
(Get-Content "Debug_Output.log" | Select-String "Web In").Count
```

---

## ⚠️ 問題處理

### 若驗證仍然失敗
1. 檢查 Debug Output 日志
2. 查看是否為網絡問題（ping 測試）
3. 驗證 API 端點是否正常
4. 檢查防火牆設置
5. 聯繫服務器管理員

### 若 UI 沒有改進
1. 清理 Bin/Obj 文件夾
2. 重新編譯
3. 重啟 Visual Studio
4. 檢查 XAML 是否正確保存

### 若時間計算錯誤
1. 檢查系統時間是否正確
2. 驗證 CalculateNextReAuthTime 方法
3. 檢查 .nob 文件是否正確保存

---

## 📞 應急聯繫

若部署出現問題：

| 問題類型 | 聯繫方式 | 優先級 |
|---------|---------|--------|
| 編譯錯誤 | 開發團隊 | 緊急 |
| 驗證失敗 | 服務器團隊 | 緊急 |
| UI 問題 | 設計團隊 | 重要 |
| 日志問題 | 技術支持 | 普通 |

---

## ✅ 部署完成檢清單

部署完成後確認以下事項：

- [ ] 應用成功編譯
- [ ] 所有單元測試通過
- [ ] 冒煙測試通過
- [ ] 區域測試通過
- [ ] 邊界情況測試通過
- [ ] 性能測試通過
- [ ] 文檔已更新
- [ ] 版本號已更新
- [ ] 用戶已通知
- [ ] 監控已啟用
- [ ] 備份已保存
- [ ] Git 提交已完成

---

## 📝 部署記錄

```
部署日期：_____________
部署人員：_____________
部署環境：_____________（開發/測試/生產）
版本號：_____________
編譯時間：_____________
測試結果：_____________
備註：_____________

簽名：_________________ 日期：_______
```

---

**版本**：1.1.0  
**檢查日期**：2025-11-20  
**狀態**：✅ 準備就緒
