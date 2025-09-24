using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace NOBApp.Sports
{
    internal class 隨機打怪 : BaseClass
    {
        bool F5解無敵 = false;
        int mBCHCount = 0;
        int f5CheckCount = 0;
        public override void 初始化() { }

        Dictionary<int, int> cacheIGID = new();
        //List<long> cacheIGID = new List<long>();
        public override Task 腳本運作()
        {
            if (MainNob != null)
            {
                if (MainNob.待機)
                {
                    Task.Delay(50).Wait();
                    NobMainCodePage.GetFilteredNPCs(MainNob, TargetTypes.NPC, 2, MainNob.CodeSetting.搜尋範圍);
                    if (NobMainCodePage.AllNPCIDs.Count > 0)
                    {
                        NobMainCodePage.AllNPCIDs = NobMainCodePage.AllNPCIDs
                            .Where(id => !NobMainCodePage.TargetsID.Contains(id))
                            .Where(id => !NobMainCodePage.IgnoredIDs.Contains(id)).ToList();
                    }
                }

                if (MainNob.開打)
                {
                    if (MainNob.對話與結束戰鬥)
                    {
                        mBCHCount++;
                    }

                    if (MainNob.IsUseAutoSkill == false)
                    {
                        if (mBCHCount > 2)
                        {
                            mBCHCount = 0;
                            MainNob.離開戰鬥B();
                        }
                    }

                    if (MainNob.戰鬥中)
                    {
                        cacheIGID.Clear();
                        F5解無敵 = false;
                        mBCHCount = 0;
                        f5CheckCount = 0;
                        Task.Delay(50).Wait();
                        return base.腳本運作();
                    }

                    if (NobMainCodePage.AllNPCIDs == null || NobMainCodePage.AllNPCIDs.Count == 0)
                    {
                        MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                        return base.腳本運作();
                    }

                    if (MainNob.待機)
                    {
                        MainNob.Log($"目標數量 ==> {NobMainCodePage.AllNPCIDs.Count}");
                        for (int i = 0; i < NobMainCodePage.AllNPCIDs.Count; i++)
                        {
                            if (MainNob.開打 == false || !MainNob.待機)
                            {
                                break;
                            }

                            var id = (int)NobMainCodePage.AllNPCIDs[i];
                            MainNob.Log($"鎖定打怪 => {id} | {MainNob.isUseEnter}");
                            if (MainNob.isUseEnter || MainNob.CodeSetting.Enter點怪)
                            {
                                if (F5解無敵)
                                {
                                    f5CheckCount = f5CheckCount + 1;
                                }
                                if (MainNob.F5解無敵 && (F5解無敵 == false || f5CheckCount > 5))
                                {
                                    f5CheckCount = 0;
                                    F5解無敵 = true;
                                    MainNob.KeyPress(VKeys.KEY_F5, 2, 50);
                                    MainNob.KeyPress(VKeys.KEY_ESCAPE, 4, 30);
                                    Task.Delay(50).Wait();
                                }

                                MainNob.鎖定NPC(id);
                                Task.Delay(50).Wait();
                                MainNob.KeyPress(VKeys.KEY_ENTER, 4, 50);
                                Task.Delay(50).Wait();
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

                        if (MainNob.F5解無敵 == false)
                        {
                            MainNob.KeyPress(VKeys.KEY_S, 1, 1000);
                            MainNob.KeyPress(VKeys.KEY_D, 1, 1500);
                        }


                        //List<int> delint = new();
                        //foreach (var item in cacheIGID)
                        //{
                        //    if (item.Value > 5)
                        //    {
                        //        delint.Add(item.Key);
                        //    }
                        //}
                        //for (int i = 0; i < delint.Count; i++)
                        //{
                        //    cacheIGID.Remove(delint[i]);

                        //    if (NobMainCodePage.IgnoredIDs.Contains(delint[i]) == false)
                        //        NobMainCodePage.IgnoredIDs.Add(delint[i]);
                        //}
                    }

                }
            }

            return base.腳本運作();
        }
    }
}
