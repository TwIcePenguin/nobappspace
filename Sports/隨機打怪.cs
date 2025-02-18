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
        public Action UpdateUI = null;
        bool 進戰鬥結束 = false;
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
                    Task.Delay(500).Wait();
                    UpdateUI?.Invoke();
                    Task.Delay(100).Wait();
                }

                if (MainWindow.目標IDs != null && MainWindow.開打)
                {
                    if (MainNob.待機)
                    {
                        if (MainWindow.F5解無敵 && F5解無敵 == false)
                        {
                            F5解無敵 = true;
                            MainNob.KeyPress(VKeys.KEY_F5);
                            Task.Delay(100).Wait();
                            MainNob.KeyPress(VKeys.KEY_ESCAPE);
                        }

                        Debug.WriteLine($"{MainWindow.目標IDs.Count}");
                        if (MainWindow.目標IDs.Count == 0)
                        {
                            MainNob.KeyPress(VKeys.KEY_Q, 1, 500);
                        }
                        else
                        {
                            for (int i = 0; i < MainWindow.目標IDs.Count; i++)
                            {
                                if (MainWindow.開打 == false || !MainNob.待機)
                                {
                                    break;
                                }

                                if (i >= MainWindow.限點數量)
                                {
                                    break;
                                }

                                var id = (int)MainWindow.目標IDs[i];
                                if (MainWindow.Enter點怪)
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

                            if (MainWindow.忽略名單IDs.Contains(delint[i]) == false)
                                MainWindow.忽略名單IDs.Add(delint[i]);
                        }
                    }

                    if (MainNob.戰鬥中)
                    {
                        cacheIGID.Clear();
                        F5解無敵 = false;
                        進戰鬥結束 = true;
                        mBCHCount = 0;
                    }

                    if (MainNob.對話與結束戰鬥)
                        mBCHCount++;

                    if (MainWindow.UseAutoSkill == false)
                    {
                        if (mBCHCount > 2)
                        {
                            mBCHCount = 0;
                            do
                            {
                                MainNob.KeyPress(VKeys.KEY_ESCAPE);
                                Task.Delay(100).Wait();
                                MainNob.KeyPress(VKeys.KEY_ENTER);
                                Task.Delay(100).Wait();
                                if (MainNob.待機)
                                    break;
                            }
                            while (MainWindow.CodeRun);
                        }
                    }
                }
            }
        }
    }
}
