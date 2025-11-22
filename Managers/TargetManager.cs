using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NOBApp.Managers
{
    public class TargetManager
    {
        private readonly NobMainCodePage _view;
        private bool _lastNPCListUpdateStatus = false;
        private DateTime _lastNPCUpdateTime = DateTime.MinValue;
        private const int MIN_NPC_UPDATE_INTERVAL_MS = 300;

        public TargetManager(NobMainCodePage view)
        {
            _view = view;
        }

        public void UpdateNPCTargetsIfNeeded()
        {
            var targetViewPage = _view.TargetViewPage;
            var listIgnore = _view.List_忽略名單;
            var listLock = _view.List_鎖定名單;
            var listCurrent = _view.List_目前名單;

            bool shouldUpdateNPCList = _view.MainNob!.待機 && _view.UpdateNPCDataUI &&
                                      targetViewPage.Visibility == Visibility.Visible;

            bool intervalElapsed = (DateTime.Now - _lastNPCUpdateTime).TotalMilliseconds >= MIN_NPC_UPDATE_INTERVAL_MS;

            if (shouldUpdateNPCList && (intervalElapsed || shouldUpdateNPCList != _lastNPCListUpdateStatus))
            {
                _lastNPCListUpdateStatus = shouldUpdateNPCList;
                _lastNPCUpdateTime = DateTime.Now;

                _view.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        lock (typeof(NobMainCodePage))
                        {
                            var tempIgnoredIDs = new List<long>(NobMainCodePage.IgnoredIDs);
                            var tempTargetsID = new List<long>(NobMainCodePage.TargetsID);
                            var tempAllNPCIDs = new List<long>(NobMainCodePage.AllNPCIDs);

                            listIgnore.ItemsSource = tempIgnoredIDs.Count > 0 ? tempIgnoredIDs : null;
                            listLock.ItemsSource = tempTargetsID.Count > 0 ? tempTargetsID : null;
                            listCurrent.ItemsSource = tempAllNPCIDs.Count > 0 ? tempAllNPCIDs : null;

                            if (tempAllNPCIDs.Count > 0) listCurrent.Items.Refresh();
                            if (tempTargetsID.Count > 0) listLock.Items.Refresh();
                            if (tempIgnoredIDs.Count > 0) listIgnore.Items.Refresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"更新NPC列表錯誤: {ex.Message}");
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        public void HandleTargetClick(object sender)
        {
            if (_view.MainNob == null)
                return;

            int tid = (int)_view.MainNob.GetTargetIDINT();

            Button clickedButton = sender as Button;
            if (clickedButton == null)
                return;

            string buttonName = clickedButton.Name;

            if (buttonName.Contains("TargetA"))
            {
                _view.MainNob.CodeSetting.目標A = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetB"))
            {
                _view.MainNob.CodeSetting.目標B = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetC"))
            {
                _view.MainNob.CodeSetting.目標C = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetD"))
            {
                _view.MainNob.CodeSetting.目標D = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetE"))
            {
                _view.MainNob.CodeSetting.目標E = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
            else if (buttonName.Contains("TargetF"))
            {
                _view.MainNob.CodeSetting.目標F = tid;
                clickedButton.Content = "鎖定:" + tid;
            }
        }

        public void AddLockTarget()
        {
            var listCurrent = _view.List_目前名單;
            var listLock = _view.List_鎖定名單;

            var id = _view.MainNob!.GetTargetIDINT();
            if (!id.Equals(4294967295) && id > 0)
            {
                if (!NobMainCodePage.TargetsID.Contains(id))
                {
                    NobMainCodePage.TargetsID.Add(id);
                }

                listCurrent.ItemsSource = NobMainCodePage.AllNPCIDs;
                listLock.ItemsSource = NobMainCodePage.TargetsID;
                listLock.Items.Refresh();
                listCurrent.Items.Refresh();
            }
        }

        public void ToggleList()
        {
            var listCurrent = _view.List_目前名單;
            var listLock = _view.List_鎖定名單;
            var listIgnore = _view.List_忽略名單;

            if (listCurrent == null || listLock == null || listIgnore == null)
                return;

            if (listCurrent != null && listCurrent.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(listCurrent.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(listCurrent.SelectedValue.ToString(), out int id))
                    {
                        if (NobMainCodePage.IgnoredIDs.Contains(id) == false)
                            NobMainCodePage.IgnoredIDs.Add(id);
                        if (NobMainCodePage.AllNPCIDs.Contains(id))
                            NobMainCodePage.AllNPCIDs.Remove(id);
                    }
                }
            }

            if (listIgnore != null && listIgnore.SelectedValue != null)
            {
                if (string.IsNullOrEmpty(listIgnore.SelectedValue.ToString()) == false)
                {
                    if (int.TryParse(listIgnore.SelectedValue.ToString(), out int id))
                    {
                        if (NobMainCodePage.IgnoredIDs.Contains(id))
                            NobMainCodePage.IgnoredIDs.Remove(id);
                        if (NobMainCodePage.AllNPCIDs.Contains(id) == false)
                            NobMainCodePage.AllNPCIDs.Add(id);
                    }
                }
            }

            listLock.ItemsSource = NobMainCodePage.TargetsID;
            listCurrent.ItemsSource = NobMainCodePage.AllNPCIDs;
            listIgnore.ItemsSource = NobMainCodePage.IgnoredIDs;
            listIgnore.Items.Refresh();
            listCurrent.Items.Refresh();
            listLock.Items.Refresh();
        }
    }
}
