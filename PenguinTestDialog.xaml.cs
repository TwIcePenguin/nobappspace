using System;
using System.Windows;

namespace NOBApp
{
 public partial class PenguinTestDialog : Window
 {
 public PenguinTestDialog()
 {
 InitializeComponent();
 }

 private async void Btn_Send_Click(object sender, RoutedEventArgs e)
 {
 Btn_Send.IsEnabled = false;
 TB_Result.Text = "請求中...";
 try
 {
 string acc = TB_Account.Text?.Trim() ?? string.Empty;
 string pwd = TB_Password.Text?.Trim() ?? string.Empty;
 if (string.IsNullOrEmpty(acc) || string.IsNullOrEmpty(pwd))
 {
 TB_Result.Text = "請輸入帳號與密碼";
 return;
 }

 string? encrypted = await WebRegistration.SendAuthenticationAndGetEncryptedAsync(acc, pwd);
 if (encrypted != null)
 {
 TB_Result.Text = encrypted;
 }
 else
 {
 TB_Result.Text = "取得失敗，請檢查網路或稍後再試";
 }
 }
 catch (Exception ex)
 {
 TB_Result.Text = "例外: " + ex.Message;
 }
 finally
 {
 Btn_Send.IsEnabled = true;
 }
 }

 private void Btn_Copy_Click(object sender, RoutedEventArgs e)
 {
 try
 {
 if (!string.IsNullOrEmpty(TB_Result.Text))
 {
 Clipboard.SetText(TB_Result.Text);
 }
 }
 catch { }
 }

 private void Btn_Close_Click(object sender, RoutedEventArgs e)
 {
 this.Close();
 }
 }
}
