using System.Collections.Generic;
using System.Windows.Controls;

namespace NOBApp
{
    public class UIUpdate
    {
        public static void RefreshNOBID_Sec(ComboBox[] comboBoxes, List<NOBDATA> nobList)
        {
            foreach (var cb in comboBoxes)
            {
                cb.Items.Clear();
                foreach (var item in nobList)
                {
                    cb.Items.Add(item.PlayerName);
                }
                cb.Items.Add("");
            }
        }
    }
}
