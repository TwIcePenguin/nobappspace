using NOBApp.Sports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NOBApp.Managers
{
    public class ScriptManager
    {
        private readonly NobMainCodePage _view;
        public Dictionary<string, Action> MenuMapping { get; private set; } = new Dictionary<string, Action>();

        public ScriptManager(NobMainCodePage view)
        {
            _view = view;
        }

        private readonly HashSet<string> _vipScripts = new();

        private sealed class VipBrushConverter : IValueConverter
        {
            private readonly HashSet<string> _vip;
            public VipBrushConverter(HashSet<string> vip) => _vip = vip;

            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var name = value?.ToString();
                return name != null && _vip.Contains(name);
            }

            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
        }

        public void UpdateSelectMenu()
        {
            _vipScripts.Clear();
            var selectMenu = _view.SelectMenu;
            var btnTargetA = _view.Btn_TargetA;
            var btnTargetB = _view.Btn_TargetB;
            var btnTargetC = _view.Btn_TargetC;
            var btnTargetD = _view.Btn_TargetD;
            var btnTargetE = _view.Btn_TargetE;
            var smenu2 = _view.SMENU2;
            var smenu1 = _view.SMENU1;
            var commonGrid = _view.通用欄;
            var skillSettingPage = _view.武技設定頁面;
            var cbAllIn = _view.CB_AllIn;
            var tbSelectLevel = _view.TB_選擇關卡;
            var tbSelectDiff = _view.TB_選擇難度;
            var tbSetCNum = _view.TB_SetCNum;
            var targetViewPage = _view.TargetViewPage;
            var cbAutoLockPc = _view.CB自動鎖定PC;
            var cbLockAfterBlackGun = _view.CB鎖定後自動黑槍;
            var listLockList = _view.List_鎖定名單;
            var listCurrentList = _view.List_目前名單;
            var otherOptionA = _view.其他選項A;
            var otherOptionB = _view.其他選項B;
            var cbPoint = _view.CB_定位點;
            var backTime = _view.後退時間;

            selectMenu.Items.Clear();
            var sportsTypes = Assembly.GetExecutingAssembly()
                                        .GetTypes()
                                        .Where(t => t.IsClass && t.Namespace == "NOBApp.Sports" &&
                                                    t.Name != "ScriptExtensions" &&
                                                    t.Name != "BaseClass" && !t.Name.Contains("<"))
                                        .ToList();

            foreach (var t in sportsTypes)
            {
                try
                {
                    if (Activator.CreateInstance(t) is BaseClass bc && bc.需要VIP)
                    {
                        _vipScripts.Add(t.Name);
                    }
                }
                catch { }
            }

            var sportsClasses = sportsTypes.Select(t => t.Name).ToList();

            foreach (var className in sportsClasses)
            {
                selectMenu.Items.Add(className);
            }

            MenuMapping = new Dictionary<string, Action>
            {
                { "黃泉盡頭", () => { _view.useMenu = new 黃泉盡頭(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; }  },
               
                { "刷熊本城", () => { _view.useMenu = new 刷熊本城(); btnTargetA.Content = "入場NPC"; btnTargetA.Visibility = Visibility.Visible; } },
                { "四聖青龍", () => { _view.useMenu = new 四聖青龍(); btnTargetA.Content = "老頭"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_酒井", () => { _view.useMenu = new 討伐2025_酒井(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_今川氏真", () => { _view.useMenu = new 討伐2025_今川氏真(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_謎之怪", () => { _view.useMenu = new 討伐2025_謎之怪(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_後藤", () => { _view.useMenu = new 討伐2025_後藤(); btnTargetA.Content = "安土NPC"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_朝比奈", () => { _view.useMenu = new 討伐2025_朝比奈(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_百地", () => { _view.useMenu = new 討伐2025_百地(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_井伊", () => { _view.useMenu = new 討伐2025_井伊(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_松", () => { _view.useMenu = new 討伐2025_松(); btnTargetA.Content = "水滴"; btnTargetA.Visibility = Visibility.Visible; } },
                { "討伐2025_白石", () => { _view.useMenu = new 討伐2025_白石(); btnTargetA.Content = "安土奉行"; btnTargetA.Visibility = Visibility.Visible; } },
                { "幽靈船全刷", () => { _view.useMenu = new 幽靈船全刷(); btnTargetA.Content = "九鬼"; smenu2.Visibility = btnTargetA.Visibility = Visibility.Visible; } },

                { "夢幻城", () => { _view.useMenu = new 夢幻城(); _view.useMenu.多人同時執行 = true; } },
                { "採集輔助", () => { _view.useMenu = new 採集輔助(); } },
                { "生產輔助", () => { _view.useMenu = new 生產輔助(); cbPoint.Visibility = btnTargetA.Visibility = Visibility.Visible; } },
                { "生產破魔", () => { _view.useMenu = new 生產破魔(); btnTargetA.Visibility = btnTargetB.Visibility = btnTargetC.Visibility = btnTargetD.Visibility = btnTargetE.Visibility = Visibility.Visible; } },
                { "生產剛破", () => { _view.useMenu = new 生產剛破(); btnTargetA.Visibility = btnTargetB.Visibility = btnTargetC.Visibility = btnTargetD.Visibility = Visibility.Visible; } },

                { "戰場製炮", () => {
                    _view.useMenu = new 戰場製炮();
                    btnTargetA.Content = "目付";
                    btnTargetB.Content = "砲基座";
                    btnTargetC.Content = "生砲道具";
                    _view.useMenu.多人同時執行 = true;
                    cbPoint.Visibility = backTime.Visibility = btnTargetC.Visibility
                    = btnTargetB.Visibility = btnTargetA.Visibility = Visibility.Visible; } },
                { "冥宮", () => { _view.useMenu = new 冥宮(); commonGrid.Visibility = Visibility.Visible;  } },
                { "鬼島", () => { _view.useMenu = new 鬼島();
                    _view.UpdateNPCDataUI = true; btnTargetA.Content = "村長-補符"; otherOptionB.Visibility = targetViewPage.Visibility = Visibility.Visible; cbAutoLockPc.Visibility = cbLockAfterBlackGun.Visibility = listLockList.Visibility = Visibility.Hidden; btnTargetA.Visibility = Visibility.Visible;
                    otherOptionA.ToolTip = "幾場後 找村長補符";
                    otherOptionA.Text = "80"; otherOptionB.Text = "0"; } },
                { "上覽打錢", () => { _view.useMenu = new 上覽打錢(); btnTargetA.Content = "目標大黑天"; btnTargetC.Content = "倉庫"; btnTargetC.Visibility = btnTargetB.Visibility = btnTargetA.Visibility = smenu1.Visibility = smenu2.Visibility = Visibility.Visible; } },
                
                { "地下町天地", () => { _view.useMenu = new 地下町天地(); skillSettingPage.Visibility = cbAllIn.Visibility = tbSelectLevel.Visibility = btnTargetC.Visibility = tbSelectDiff.Visibility = tbSetCNum.Visibility = Visibility.Visible; } },
                { "超級打怪", () => { _view.useMenu = new 超級打怪(); btnTargetA.Content = "鎖定目標"; btnTargetA.Visibility = Visibility.Visible; } },
                { "隨機打怪", () => { _view.useMenu = new 隨機打怪(); _view.UpdateNPCDataUI = true; cbAutoLockPc.Visibility = listCurrentList.Visibility = targetViewPage.Visibility = Visibility.Visible; cbLockAfterBlackGun.Visibility = Visibility.Hidden; } }
            };

            if (Tools.IsVIP == false)
            {
                selectMenu.Items.Remove("黃泉盡頭");
                selectMenu.Items.Remove("黃泉盡頭");
                selectMenu.Items.Remove("生產破魔");
                selectMenu.Items.Remove("刷熊本城");
                selectMenu.Items.Remove("四聖青龍");
            }

#if DEBUG == false
            selectMenu.Items.Remove("生產剛破");
#endif
            selectMenu.UpdateLayout();
        }

        public void HandleSelectionChanged(string selectedItem)
        {
            var startCode = _view.StartCode;
            var comboBoxes = _view.comboBoxes;

            if (string.IsNullOrEmpty(selectedItem))
            {
                startCode.IsEnabled = false;
                startCode.UpdateLayout();
                return;
            }
            startCode.IsChecked = false;
            startCode.IsEnabled = !string.IsNullOrEmpty(selectedItem);
            _view.useMenu = null;
            _view.UIStatus_Default(); // Need to be public
            UIUpdate.RefreshNOBID_Sec(comboBoxes, MainWindow.AllNobWindowsList);
            if (MenuMapping.ContainsKey(selectedItem))
            {
                MenuMapping[selectedItem].Invoke();
            }
            else
            {
                startCode.IsEnabled = false;
            }

            startCode.UpdateLayout();
        }

        public void SaveSetting(string playerName)
        {
            if (string.IsNullOrEmpty(playerName)) return;
            try
            {
                // Read current UI values back into CodeSetting before saving
                ReadSettingFromUI();

                var setting = _view.MainNob?.CodeSetting;
                if (setting == null) return;

                // Ensure team skill records are included before saving
                setting.隊伍技能 = NobMainCodePage.m隊伍技能紀錄 ?? new 隊伍技能紀錄();

                setting.上次使用的腳本 = _view.SelectMenu.Text;

                string settingFile = $@"{playerName}_Setting.json";
                string jsonString = JsonSerializer.Serialize(setting, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingFile, jsonString);
                Debug.WriteLine($"Saved settings for {playerName} to {settingFile}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void LoadSetting(string playerName)
        {
            if (string.IsNullOrEmpty(playerName)) return;
            try
            {
                string settingFile = $@"{playerName}_Setting.json";
                if (File.Exists(settingFile))
                {
                    string jsonString = File.ReadAllText(settingFile);
                    var loadedSetting = JsonSerializer.Deserialize<Setting>(jsonString);
                    if (loadedSetting != null)
                    {
                        _view.MainNob.CodeSetting = loadedSetting;

                        // Restore team skill records into the shared runtime store
                        NobMainCodePage.m隊伍技能紀錄 = loadedSetting.隊伍技能 ?? new 隊伍技能紀錄();

                        Debug.WriteLine($"Loaded settings for {playerName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
            }
        }

        public void SettingLoadToUI()
        {
            if (_view.MainNob == null) return;

            var setting = _view.MainNob.CodeSetting;
            _view.SelectMenu.Text = setting.上次使用的腳本;
            _view.CB_定位點.IsChecked = setting.使用定位點;
            _view.後退時間.Text = setting.後退時間.ToString();
            _view.延遲係數.Text = setting.延遲係數.ToString();
            _view.CB_AllIn.IsChecked = setting.AllInTeam;
            _view.TB_SetCNum.Text = setting.連續戰鬥.ToString();
            _view.TB_選擇關卡.Text = setting.選擇關卡.ToString();
            _view.TB_選擇難度.Text = setting.選擇難度.ToString();
            // If the compact per-script textbox SMENU1 is visible, mirror the value there too
            try
            {
                if (_view.SMENU1.Visibility == Visibility.Visible)
                {
                    _view.SMENU1.Text = setting.選擇難度.ToString();
                }
            }
            catch { }
            _view.TBX搜尋範圍.Text = setting.搜尋範圍.ToString();
            _view.其他選項A.Text = setting.其他選項A.ToString();
            _view.其他選項B.Text = setting.其他選項B.ToString();
            _view.腳本Point.Text = setting.MPoint.ToString();
            _view.E點怪.IsChecked = setting.Enter點怪;
            _view.FNPCID.Text = setting.目標A.ToString();
            _view.SMENU2.Text = setting.線路.ToString();

            // Update Target Buttons Content if they have values
            if (setting.目標A != 0) _view.Btn_TargetA.Content = "鎖定:" + setting.目標A;
            if (setting.目標B != 0) _view.Btn_TargetB.Content = "鎖定:" + setting.目標B;
            if (setting.目標C != 0) _view.Btn_TargetC.Content = "鎖定:" + setting.目標C;
            if (setting.目標D != 0) _view.Btn_TargetD.Content = "鎖定:" + setting.目標D;
            if (setting.目標E != 0) _view.Btn_TargetE.Content = "鎖定:" + setting.目標E;
            if (setting.目標F != 0) _view.Btn_TargetF.Content = "鎖定:" + setting.目標F;
        }

        public void ReadSettingFromUI()
        {
            if (_view.MainNob == null) return;
            var setting = _view.MainNob.CodeSetting;

            setting.上次使用的腳本 = _view.SelectMenu.Text;
            setting.使用定位點 = _view.CB_定位點.IsChecked ?? false;
            
            if (int.TryParse(_view.後退時間.Text, out int backTime)) setting.後退時間 = backTime;
            if (int.TryParse(_view.延遲係數.Text, out int delay)) setting.延遲係數 = delay;
            
            setting.AllInTeam = _view.CB_AllIn.IsChecked ?? false;
            
            if (int.TryParse(_view.TB_SetCNum.Text, out int cNum)) setting.連續戰鬥 = cNum;
            if (int.TryParse(_view.TB_選擇關卡.Text, out int level)) setting.選擇關卡 = level;
            if (int.TryParse(_view.TB_選擇難度.Text, out int diff)) setting.選擇難度 = diff;
            // Prefer SMENU1 when visible (user may input difficulty there)
            try
            {
                if (_view.SMENU1.Visibility == Visibility.Visible && int.TryParse(_view.SMENU1.Text, out int smDiff))
                {
                    setting.選擇難度 = smDiff;
                }
            }
            catch { }
            if (int.TryParse(_view.TBX搜尋範圍.Text, out int range)) setting.搜尋範圍 = range;
            if (int.TryParse(_view.其他選項A.Text, out int optA)) setting.其他選項A = optA;
            if (int.TryParse(_view.其他選項B.Text, out int optB)) setting.其他選項B = optB;
            if (int.TryParse(_view.腳本Point.Text, out int mPoint)) setting.MPoint = mPoint;
            
            setting.Enter點怪 = _view.E點怪.IsChecked ?? false;
            
            if (int.TryParse(_view.FNPCID.Text, out int targetA)) setting.目標A = targetA;
            if (int.TryParse(_view.SMENU2.Text, out int line)) setting.線路 = line;
        }
    }
}
