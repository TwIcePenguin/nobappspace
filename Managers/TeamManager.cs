using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;

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

            // Read current UI controls into 隊員智能功能組 before saving
            ReadSkillSettingsFromUI();

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

                // Debug: print what will be saved
                try
                {
                    Debug.WriteLine($"[SaveTeamSkills] Preparing to save {list.Count} team entries (route A/B/C selection).\nEntries:");
                    foreach (var u in list)
                    {
                        Debug.WriteLine($" - {u.用名}: 同步={u.同步}, 一次放={u.一次放}, 重複放={u.重複放}, 延遲={u.延遲}, 間隔={u.間隔}, 技能段s=({u.技能段1},{u.技能段2},{u.技能段3}), 施放=({u.施放A},{u.施放B},{u.施放C}), 程式速度={u.程式速度}");
                    }

                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    Debug.WriteLine($"[SaveTeamSkills] JSON preview:\n{json}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SaveTeamSkills] Failed to produce debug output: {ex.Message}");
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
                // After writing the loaded values into UI controls, read them back into the runtime list
                ReadSkillSettingsFromUI();
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

        // Read UI control values from the skill setting panel into the runtime 隊員智能功能組 list
        private void ReadSkillSettingsFromUI()
        {
            try
            {
                var skillSettingPage = _view.隊員額外功能頁面;
                if (skillSettingPage == null) return;

                // For each canvas slot in the UI, extract its index and apply contained controls to the corresponding member
                foreach (var child in skillSettingPage.Children)
                {
                    if (child is Canvas c)
                    {
                        // Expect canvas name contains _{num}
                        var name = c.Name ?? string.Empty;
                        int slot = -1;
                        var idxPos = name.LastIndexOf('_');
                        if (idxPos >= 0 && int.TryParse(name.Substring(idxPos + 1), out var parsed))
                        {
                            slot = parsed - 1; // UI uses 1-based numbering
                        }

                        if (slot < 0 || slot >= NobMainCodePage.隊員智能功能組.Count)
                            continue;

                        var member = NobMainCodePage.隊員智能功能組[slot];
                        foreach (var item in c.Children)
                        {
                            if (item is CheckBox cb)
                            {
                                if (cb.Name.Contains("重複"))
                                    member.重複放 = cb.IsChecked == true;
                                if (cb.Name.Contains("開場"))
                                    member.一次放 = cb.IsChecked == true;
                                if (cb.Name.Contains("同步"))
                                    member.同步 = cb.IsChecked == true;
                            }

                            if (item is TextBox tb)
                            {
                                if (tb.Name.Contains("延遲") && int.TryParse(tb.Text, out var vDelay))
                                    member.延遲 = vDelay;
                                if (tb.Name.Contains("間隔") && int.TryParse(tb.Text, out var vInterval))
                                    member.間隔 = vInterval;
                                if (tb.Name.Contains("技能段1") && int.TryParse(tb.Text, out var s1))
                                    member.技能段1 = s1;
                                if (tb.Name.Contains("技能段2") && int.TryParse(tb.Text, out var s2))
                                    member.技能段2 = s2;
                                if (tb.Name.Contains("技能段3") && int.TryParse(tb.Text, out var s3))
                                    member.技能段3 = s3;
                                if (tb.Name.Contains("施放A"))
                                    member.施放A = tb.Text ?? string.Empty;
                                if (tb.Name.Contains("施放B"))
                                    member.施放B = tb.Text ?? string.Empty;
                                if (tb.Name.Contains("施放C"))
                                    member.施放C = tb.Text ?? string.Empty;
                                if (tb.Name.Contains("程式速度") && int.TryParse(tb.Text, out var spd))
                                    member.程式速度 = spd;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReadSkillSettingsFromUI error: {ex.Message}");
            }
        }

        public void SaveTeamSkillSet() => SaveTeamSkills();

        public void LoadTeamSkillSet() => LoadTeamSkills();

        public void UpdateAutoSkillTeamMembers()
        {
            try
            {
                // Determine which route is active
                List<隊員資料紀錄檔> list = null;
                if (_view.A套路.IsChecked == true)
                    list = NobMainCodePage.m隊伍技能紀錄.方案A;
                else if (_view.B套路.IsChecked == true)
                    list = NobMainCodePage.m隊伍技能紀錄.方案B;
                else if (_view.C套路.IsChecked == true)
                    list = NobMainCodePage.m隊伍技能紀錄.方案C;

                if (list == null || list.Count == 0)
                    return;

                foreach (var set in list)
                {
                    if (string.IsNullOrEmpty(set.用名))
                        continue;

                    // find existing auto skill entry by player name
                    var member = NobMainCodePage.隊員智能功能組.Find(x => x.NOB != null && x.NOB.PlayerName == set.用名);
                    if (member != null)
                    {
                        member.同步 = set.同步;
                        member.一次放 = set.一次放;
                        member.重複放 = set.重複放;
                        member.延遲 = set.延遲;
                        member.間隔 = set.間隔;
                        member.技能段1 = set.技能段1;
                        member.技能段2 = set.技能段2;
                        member.技能段3 = set.技能段3;
                        member.施放A = set.施放A;
                        member.施放B = set.施放B;
                        member.施放C = set.施放C;
                        member.程式速度 = set.程式速度;

                        // Also sync values into the runtime NOBDATA AutoSkillSet so battle logic uses them
                        if (member.NOB != null)
                        {
                            member.NOB.AutoSkillSet.同步 = member.同步;
                            member.NOB.AutoSkillSet.一次放 = member.一次放;
                            member.NOB.AutoSkillSet.重複放 = member.重複放;
                            member.NOB.AutoSkillSet.延遲 = member.延遲;
                            member.NOB.AutoSkillSet.間隔 = member.間隔;
                            member.NOB.AutoSkillSet.技能段1 = member.技能段1;
                            member.NOB.AutoSkillSet.技能段2 = member.技能段2;
                            member.NOB.AutoSkillSet.技能段3 = member.技能段3;
                            member.NOB.AutoSkillSet.施放A = member.施放A;
                            member.NOB.AutoSkillSet.施放B = member.施放B;
                            member.NOB.AutoSkillSet.施放C = member.施放C;
                            member.NOB.AutoSkillSet.程式速度 = member.程式速度;

                            // Sync RoundConfigs
                            member.NOB.AutoSkillSet.RoundConfigs.Clear();
                            if (set.RoundConfigs != null)
                            {
                                foreach (var kvp in set.RoundConfigs)
                                {
                                    member.NOB.AutoSkillSet.RoundConfigs[kvp.Key] = new NobMainCodePage.RoundConfig
                                    {
                                        重複放 = kvp.Value.重複放,
                                        一次放 = kvp.Value.一次放,
                                        延遲 = kvp.Value.延遲,
                                        間隔 = kvp.Value.間隔,
                                        技能段1 = kvp.Value.技能段1,
                                        技能段2 = kvp.Value.技能段2,
                                        技能段3 = kvp.Value.技能段3,
                                        施放A = kvp.Value.施放A,
                                        施放B = kvp.Value.施放B,
                                        施放C = kvp.Value.施放C,
                                        程式速度 = kvp.Value.程式速度
                                    };
                                }
                            }
                        }
                    }
                    else
                    {
                        // try to find NOBDATA in AllNobWindowsList
                        var nob = MainWindow.AllNobWindowsList?.Find(n => n.PlayerName == set.用名);
                        if (nob != null)
                        {
                            var newMember = new NobMainCodePage.自動技能組();
                            newMember.NOB = nob;
                            newMember.同步 = set.同步;
                            newMember.一次放 = set.一次放;
                            newMember.重複放 = set.重複放;
                            newMember.延遲 = set.延遲;
                            newMember.間隔 = set.技能段3;
                            newMember.技能段1 = set.技能段1;
                            newMember.技能段2 = set.技能段2;
                            newMember.技能段3 = set.技能段3;
                            newMember.施放A = set.施放A;
                            newMember.施放B = set.施放B;
                            newMember.施放C = set.施放C;
                            newMember.程式速度 = set.程式速度;

                            // Sync RoundConfigs
                            if (set.RoundConfigs != null)
                            {
                                foreach (var kvp in set.RoundConfigs)
                                {
                                    newMember.RoundConfigs[kvp.Key] = new NobMainCodePage.RoundConfig
                                    {
                                        重複放 = kvp.Value.重複放,
                                        一次放 = kvp.Value.一次放,
                                        延遲 = kvp.Value.延遲,
                                        間隔 = kvp.Value.間隔,
                                        技能段1 = kvp.Value.技能段1,
                                        技能段2 = kvp.Value.技能段2,
                                        技能段3 = kvp.Value.技能段3,
                                        施放A = kvp.Value.施放A,
                                        施放B = kvp.Value.施放B,
                                        施放C = kvp.Value.施放C,
                                        程式速度 = kvp.Value.程式速度
                                    };
                                }
                            }

                            // apply to nob runtime AutoSkillSet as well
                            if (nob != null)
                            {
                                nob.AutoSkillSet.同步 = newMember.同步;
                                nob.AutoSkillSet.一次放 = newMember.一次放;
                                nob.AutoSkillSet.重複放 = newMember.重複放;
                                nob.AutoSkillSet.延遲 = newMember.延遲;
                                nob.AutoSkillSet.間隔 = newMember.間隔;
                                nob.AutoSkillSet.技能段1 = newMember.技能段1;
                                nob.AutoSkillSet.技能段2 = newMember.技能段2;
                                nob.AutoSkillSet.技能段3 = newMember.技能段3;
                                nob.AutoSkillSet.施放A = newMember.施放A;
                                nob.AutoSkillSet.施放B = newMember.施放B;
                                nob.AutoSkillSet.施放C = newMember.施放C;
                                nob.AutoSkillSet.程式速度 = newMember.程式速度;

                                // Sync RoundConfigs
                                nob.AutoSkillSet.RoundConfigs.Clear();
                                foreach (var kvp in newMember.RoundConfigs)
                                {
                                    nob.AutoSkillSet.RoundConfigs[kvp.Key] = kvp.Value;
                                }
                            }

                            NobMainCodePage.隊員智能功能組.Add(newMember);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateAutoSkillTeamMembers error: {ex.Message}");
            }
        }

        // New: initialize 隊員智能功能組 from current AllNobWindowsList
        public void InitializeTeamMembersFromAllNobs()
        {
            try
            {
                NobMainCodePage.隊員智能功能組.Clear();
                var all = MainWindow.AllNobWindowsList;
                if (all == null || all.Count == 0)
                    return;

                foreach (var nob in all)
                {
                    if (nob == null) continue;
                    var member = new NobMainCodePage.自動技能組()
                    {
                        NOB = nob,
                        同步 = false,
                        重複放 = false,
                        一次放 = false,
                        延遲 = 0,
                        間隔 = 0,
                        技能段1 = -1,
                        技能段2 = -1,
                        技能段3 = -1,
                        施放A = string.Empty,
                        施放B = string.Empty,
                        施放C = string.Empty,
                        程式速度 = 0
                    };

                    // Initialize the NOBDATA AutoSkillSet to match the member defaults
                    if (nob != null)
                    {
                        nob.AutoSkillSet.同步 = member.同步;
                        nob.AutoSkillSet.一次放 = member.一次放;
                        nob.AutoSkillSet.重複放 = member.重複放;
                        nob.AutoSkillSet.延遲 = member.延遲;
                        nob.AutoSkillSet.間隔 = member.間隔;
                        nob.AutoSkillSet.技能段1 = member.技能段1;
                        nob.AutoSkillSet.技能段2 = member.技能段2;
                        nob.AutoSkillSet.技能段3 = member.技能段3;
                        nob.AutoSkillSet.施放A = member.施放A;
                        nob.AutoSkillSet.施放B = member.施放B;
                        nob.AutoSkillSet.施放C = member.施放C;
                        nob.AutoSkillSet.程式速度 = member.程式速度;
                    }

                    NobMainCodePage.隊員智能功能組.Add(member);
                }

                Debug.WriteLine($"Initialized 隊員智能功能組 with {NobMainCodePage.隊員智能功能組.Count} entries.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"InitializeTeamMembersFromAllNobs error: {ex.Message}");
            }
        }
    }
}
