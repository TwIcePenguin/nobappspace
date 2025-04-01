using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    internal class 隨機打怪 : BaseClass
    {
        bool F5解無敵 = false;
        int mBCHCount = 0;
        public override void 初始化() { }

        Dictionary<int, int> cacheIGID = new();
        public override void 腳本運作()
        {
            if (MainNob != null)
            {
                if (MainNob.待機)
                {
                    Task.Delay(100).Wait();
                    var npcs = NobMainCodePage.GetFilteredNPCs(TargetTypes.NPC, 4, MainNob.CodeSetting.搜尋範圍);
                    if (npcs.Count > 0)
                    {
                        NobMainCodePage.AllNPCIDs = npcs.Select(x => x.ID)
                            .Where(id => !NobMainCodePage.TargetsID.Contains(id))
                            .Where(id => !NobMainCodePage.IgnoredIDs.Contains(id)).ToList();
                    }
                }

                if (NobMainCodePage.AllNPCIDs != null && MainNob.開打)
                {
                    if (MainNob.待機)
                    {
                        if (MainNob.F5解無敵 && F5解無敵 == false)
                        {
                            F5解無敵 = true;
                            MainNob.KeyPress(VKeys.KEY_F5);
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }

                        MainNob.Log($"目標數量 ==> {NobMainCodePage.AllNPCIDs.Count}");
                        if (NobMainCodePage.AllNPCIDs.Count == 0)
                        {
                            MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                        }
                        else
                        {
                            for (int i = 0; i < NobMainCodePage.AllNPCIDs.Count; i++)
                            {
                                if (MainNob.開打 == false || !MainNob.待機)
                                {
                                    break;
                                }

                                var id = (int)NobMainCodePage.AllNPCIDs[i];
                                MainNob.Log($"鎖定打怪 => {id}");
                                if (MainNob.CodeSetting.Enter點怪)
                                {
                                    MainNob.鎖定NPC(id);
                                    Task.Delay(200).Wait();
                                    MainNob.KeyPress(VKeys.KEY_ENTER, 3);
                                }
                                else
                                {
                                    for (int j = 0; j < 4; j++)
                                    {
                                        MainNob.MoveToNPC(id);
                                        Task.Delay(500).Wait();
                                    }
                                    MainNob.KeyPress(VKeys.KEY_Q, 1, 300);
                                }

                                if (cacheIGID.ContainsKey(id) == false)
                                {
                                    cacheIGID.Add(id, 1);
                                }
                                else
                                {
                                    cacheIGID[id] = cacheIGID[id] + 1;
                                }
                            }

                            MainNob.KeyPress(VKeys.KEY_S, 1, 1000);
                            MainNob.KeyPress(VKeys.KEY_D, 1, 1500);
                        }
                        List<int> delint = new();
                        foreach (var item in cacheIGID)
                        {
                            if (item.Value > 5)
                            {
                                delint.Add(item.Key);
                            }
                        }
                        for (int i = 0; i < delint.Count; i++)
                        {
                            cacheIGID.Remove(delint[i]);

                            if (NobMainCodePage.IgnoredIDs.Contains(delint[i]) == false)
                                NobMainCodePage.IgnoredIDs.Add(delint[i]);
                        }
                    }

                    if (MainNob.戰鬥中)
                    {
                        cacheIGID.Clear();
                        F5解無敵 = false;
                        mBCHCount = 0;
                    }

                    if (MainNob.對話與結束戰鬥)
                        mBCHCount++;

                    if (MainNob.IsUseAutoSkill == false)
                    {
                        if (mBCHCount > 2)
                        {
                            mBCHCount = 0;
                            MainNob.離開戰鬥B();
                        }
                    }
                }
            }
        }
    }
}
