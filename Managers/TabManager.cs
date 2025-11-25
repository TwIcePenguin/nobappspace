using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;

namespace NOBApp.Managers
{
 /// <summary>
 /// 分頁(TabControl)管理：載入/建立/保存標籤頁狀態
 /// </summary>
 public class TabManager
 {
 private const string TAB_STATE_FILENAME = "TabState.json";
 private TabControlState _tabState = new();
 public TabControlState CurrentState => _tabState;

 public void Initialize(TabControl tabControl, List<NOBDATA> allNobWindows)
 {
 if (tabControl == null) return;
 tabControl.Items.Clear();
 LoadTabState();
 EnsureEightTabs();
 for (int i =0; i < _tabState.TabItems.Count; i++)
 {
 CreateTabItem(tabControl, _tabState.TabItems[i], i, allNobWindows);
 }
 if (tabControl.Items.Count >0)
 {
 tabControl.SelectedIndex = Math.Min(_tabState.ActiveTabIndex, tabControl.Items.Count -1);
 }
 }

 public void Save(TabControl tabControl)
 {
 try
 {
 _tabState.ActiveTabIndex = tabControl.SelectedIndex;
 _tabState.TabItems.Clear();
 foreach (TabItem tab in tabControl.Items)
 {
 if (tab.Content is NobMainCodePage page)
 {
 _tabState.TabItems.Add(new TabItemState
 {
 Header = tab.Header?.ToString() ?? string.Empty,
 PlayerName = page.MainNob?.PlayerName ?? string.Empty,
 IsVerified = page.MainNob != null && page.MainNob.驗證完成
 });
 }
 }
 EnsureEightTabs();
 File.WriteAllText(TAB_STATE_FILENAME, JsonSerializer.Serialize(_tabState));
 Debug.WriteLine("[TabManager] 保存標籤頁狀態成功");
 }
 catch (Exception ex)
 {
 Debug.WriteLine($"[TabManager] 保存標籤頁狀態失敗: {ex.Message}");
 }
 }

 private void LoadTabState()
 {
 try
 {
 if (File.Exists(TAB_STATE_FILENAME))
 {
 var json = File.ReadAllText(TAB_STATE_FILENAME);
 _tabState = JsonSerializer.Deserialize<TabControlState>(json) ?? new TabControlState();
 Debug.WriteLine("[TabManager] 載入標籤頁狀態成功");
 }
 else
 {
 _tabState = new TabControlState();
 }
 }
 catch (Exception ex)
 {
 Debug.WriteLine($"[TabManager] 載入標籤頁狀態失敗: {ex.Message}");
 _tabState = new TabControlState();
 }
 }

 private void EnsureEightTabs()
 {
 while (_tabState.TabItems.Count <8)
 {
 int idx = _tabState.TabItems.Count;
 _tabState.TabItems.Add(new TabItemState
 {
 Header = $"角色{idx}",
 PlayerName = string.Empty,
 IsVerified = false
 });
 }
 if (_tabState.TabItems.Count >8)
 {
 _tabState.TabItems = _tabState.TabItems.Take(8).ToList();
 }
 }

        private void CreateTabItem(TabControl tabControl, TabItemState state, int index, List<NOBDATA> allNobWindows)
        {
            var tabItem = new TabItem();

            // Create custom header with Button
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var headerText = new TextBlock { Text = state.Header, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            var focusButton = new Button 
            { 
                Content = "F", 
                Width = 20, 
                Height = 20, 
                Margin = new System.Windows.Thickness(5, 0, 0, 0),
                ToolTip = "Focus Window",
                FontSize = 10,
                Padding = new System.Windows.Thickness(0)
            };

            focusButton.Click += (s, e) => 
            {
                if (tabItem.Content is NobMainCodePage page)
                {
                    page.FocusUserWindows();
                }
            };

            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(focusButton);

            tabItem.Header = headerPanel;
            tabItem.MouseDoubleClick += OnTabFocus;

            var content = new NobMainCodePage { RootTabItem = tabItem, PageIndex = index };
            tabItem.Content = content;
            tabControl.Items.Add(tabItem);
            if (!string.IsNullOrEmpty(state.PlayerName) && state.IsVerified)
            {
                var nobData = allNobWindows.Find(n => n.PlayerName == state.PlayerName);
                if (nobData != null)
                {
                    content.AutoRestoreState = true;
                    content.PlayerToRestore = state.PlayerName;
                }
            }
        } private void OnTabFocus(object sender, MouseButtonEventArgs e)
 {
 if (sender is TabItem tab && tab.Content is NobMainCodePage page)
 {
 page.FocusUserWindows();
 }
 }
 }
}