using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NOBApp.Managers
{
    public class TeamManager
    {
        private readonly NobMainCodePage _view;

        public TeamManager(NobMainCodePage view)
        {
            _view = view;
        }

        public void SyncAllFunctions(VKeys key)
        {
            if (NobMainCodePage.隊員智能功能組 != null && NobMainCodePage.隊員智能功能組.Count > 0)
            {
                for (int i = 0; i < NobMainCodePage.隊員智能功能組.Count; i++)
                {
                    var nb = NobMainCodePage.隊員智能功能組[i];
                    if (nb.同步)
                    {
                        nb.NOB.KeyPressPP(key);
                    }
                }
            }
        }

        public void LeaveBattle()
        {
            var r = MainWindow.GetResolutioSize();
            foreach (var user in NobMainCodePage.隊員智能功能組)
            {
                if (user != null && user.NOB != null && user.同步)
                {
                    if (user.NOB != null)
                    {
                        int inPosX = (int)r.X / 2;
                        int inPosY = (int)r.Y / 2 - 50;
                        user.NOB.MR_Click(inPosX + 16, inPosY);
                        Task.Delay(50).Wait();
                        user.NOB.MR_Click(inPosX - 100, inPosY);
                        Task.Delay(50).Wait();
                        user.NOB.MR_Click(inPosX - 100, inPosY + 100);
                    }
                }
            }
        }

        public void SaveTeamSkills()
        {
            var aRoute = _view.A套路;
            var bRoute = _view.B套路;
            var cRoute = _view.C套路;

            _view.更新自動使用技能隊員名單(); // Need to be public

            if (NobMainCodePage.隊員智能功能組 != null && NobMainCodePage.隊員智能功能組.Count > 0)
            {
                List<隊員資料紀錄檔> list = new();
                foreach (var item in NobMainCodePage.隊員智能功能組)
                {
                    if (item == null || item.NOB == null)
                        continue;

                    隊員資料紀錄檔 user = new();
                    user.用名 = item.NOB.PlayerName;
                    user.同步 = item.同步;
                    user.一次放 = item.一次放;
                    user.重複放 = item.重複放;
                    user.延遲 = item.延遲;
                    user.技能段1 = item.技能段1;
                    user.技能段2 = item.技能段2;
                    user.技能段3 = item.技能段3;
                    user.施放A = item.施放A;
                    user.施放B = item.施放B;
                    user.施放C = item.施放C;
                    user.間隔 = item.間隔;
                    user.程式速度 = item.程式速度;
                    list.Add(user);
                }

                if (aRoute.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成A");
                    NobMainCodePage.m隊伍技能紀錄.方案A = list;
                }
                if (bRoute.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成B");
                    NobMainCodePage.m隊伍技能紀錄.方案B = list;
                }
                if (cRoute.IsChecked == true)
                {
                    Debug.WriteLine("儲存完成C");
                    NobMainCodePage.m隊伍技能紀錄.方案C = list;
                }

                _view.SaveSetting(); // Need to be public
            }
        }

        public void LoadTeamSkills()
        {
            var aRoute = _view.A套路;
            var bRoute = _view.B套路;
            var cRoute = _view.C套路;
            var skillSettingPage = _view.隊員額外功能頁面;

            if (NobMainCodePage.m隊伍技能紀錄 != null)
            {
                List<隊員資料紀錄檔> list = new();
                if (aRoute.IsChecked == true)
                {
                    list = NobMainCodePage.m隊伍技能紀錄.方案A;
                    Debug.WriteLine("讀取A");
                }
                if (bRoute.IsChecked == true)
                {
                    list = NobMainCodePage.m隊伍技能紀錄.方案B;
                    Debug.WriteLine("讀取B");
                }
                if (cRoute.IsChecked == true)
                {
                    list = NobMainCodePage.m隊伍技能紀錄.方案C;
                    Debug.WriteLine("讀取C");
                }
                int no = 1;
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        if (!string.IsNullOrEmpty(item.用名))
                        {
                            WriteSkillSetting(no, item, skillSettingPage);
                            no = no + 1;
                        }
                    }
                }
                
                Debug.WriteLine("讀取完成");
                _view.更新自動使用技能隊員名單(); // Need to be public
            }
        }

        private void WriteSkillSetting(int num, 隊員資料紀錄檔 set, StackPanel skillSettingPage)
        {
            Debug.WriteLine($"讀取 {skillSettingPage.Children.Count}");
            foreach (var c1 in skillSettingPage.Children)
            {
                if (c1 is Canvas)
                {
                    Canvas c = (Canvas)c1;
                    if (c.Name.Contains($"_{num}"))
                    {
                        foreach (var item in c.Children)
                        {
                            if (item is CheckBox)
                            {
                                CheckBox cb = (CheckBox)item;
                                if (cb.Name.Contains("重複"))
                                    cb.IsChecked = set.重複放;
                                if (cb.Name.Contains("開場"))
                                    cb.IsChecked = set.一次放;
                                if (cb.Name.Contains("同步"))
                                    cb.IsChecked = set.同步;
                            }

                            if (item is TextBox)
                            {
                                TextBox tb = (TextBox)item;
                                if (tb.Name.Contains("延遲"))
                                    tb.Text = set.延遲.ToString();
                                if (tb.Name.Contains("間隔"))
                                    tb.Text = set.間隔.ToString();
                                if (tb.Name.Contains("技能段1"))
                                    tb.Text = set.技能段1 == -1 ? "" : set.技能段1.ToString();
                                if (tb.Name.Contains("技能段2"))
                                    tb.Text = set.技能段2 == -1 ? "" : set.技能段2.ToString();
                                if (tb.Name.Contains("技能段3"))
                                    tb.Text = set.技能段3 == -1 ? "" : set.技能段3.ToString();
                                if (tb.Name.Contains("施放A"))
                                    tb.Text = set.施放A;
                                if (tb.Name.Contains("施放B"))
                                    tb.Text = set.施放B;
                                if (tb.Name.Contains("施放C"))
                                    tb.Text = set.施放C;
                                if (tb.Name.Contains("程式速度"))
                                    tb.Text = set.程式速度.ToString();
                            }
                        }
                    }
                }
            }
        }

        public void SaveTeamSkillSet() => SaveTeamSkills();

        public void LoadTeamSkillSet() => LoadTeamSkills();

        public void UpdateAutoSkillTeamMembers()
        {
            // Placeholder for future enhancements; keeps API contract with NobMainCodePage.
        }
    }
}
