using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NOBApp
{
    public partial class NobMainCodePage
    {
        public static List<NPCData> allNPCs = new();
        public static int NpcCountToRead = 150;
        public static List<NPCData> filteredNPCs = new();
        //顯示 TargetView清單用
        public static List<long> AllNPCIDs = new();
        public static List<long> TargetsID = new();
        public static List<long> IgnoredIDs = new();   // 忽略的 IDs

        /// <summary>
        /// 取得過濾後的 NPC 清單，您可以指定類型、距離範圍和 ID 範圍。
        /// </summary>
        /// <param name="types">要包含的 NPC 類型列表</param>
        /// <param name="minDistance">最小距離</param>
        /// <param name="maxDistance">最大距離</param>
        /// <param name="minID">ID 最小值</param>
        /// <param name="maxID">ID 最大值</param>
        /// <returns>符合條件的 NPC 清單</returns>
        public static List<NPCData> GetFilteredNPCs(TargetTypes flags = TargetTypes.None, int minDistance = 0, int maxDistance = int.MaxValue, long? minID = null, long? maxID = null)
        {
            // 取得所有 NPC 資料
            allNPCs = GetAllNPCs();

            // 過濾出符合條件的 NPC
            var query = allNPCs.AsEnumerable();

            // 過濾類型
            if (flags != TargetTypes.None)
            {
                query = query.Where(npc => (npc.Type & flags) != 0);
            }

            // 過濾距離
            query = query.Where(npc => npc.Distance >= minDistance && npc.Distance <= maxDistance);

            // 過濾 ID
            if (minID.HasValue)
            {
                query = query.Where(npc => npc.ID >= minID.Value);
            }
            if (maxID.HasValue)
            {
                query = query.Where(npc => npc.ID <= maxID.Value);
            }

            // 取得結果
            var result = query.ToList();

            // 更新 TargetsID
            AllNPCIDs = result.Select(npc => npc.ID).ToList();

            return result;
        }

        /// <summary>
        /// 取得所有 NPC 資料。
        /// </summary>
        /// <returns>所有 NPC 資料的清單</returns>
        public static List<NPCData> GetAllNPCs()
        {
            List<NPCData> allNPCs = new List<NPCData>();

            if (MainNob == null)
            {
                Debug.WriteLine("MainNob 為 null，無法搜尋 NPC。");
                return allNPCs; // 如果 NOB 未鎖定，返回空列表
            }

            string startAddress = AddressData.搜尋身邊NPCID起始; // 使用已儲存的 AddressData

            // 1. 批次讀取記憶體: 一次讀取所有 NPC 資料
            int npcCountToRead = NpcCountToRead; // 可考慮使其動態化
            int bytesToRead = npcCountToRead * 12; // 每個 NPC 條目佔 12 位元組

            string dataStr = MainWindow.dmSoft?.ReadData(MainNob.Hwnd, "<nobolHD.bng> + " + startAddress, bytesToRead);

            if (string.IsNullOrEmpty(dataStr))
                return allNPCs; // 處理 dataStr 失敗

            byte[] npcDataBytes;
            try
            {
                npcDataBytes = HexStringToByteArray(dataStr); // 使用 HexStringToByteArray 函數
            }
            catch (FormatException ex)
            {
                Debug.WriteLine($"轉換 Hex 字串時發生錯誤: {ex.Message}");
                return allNPCs; // 處理 Hex 字串格式錯誤
            }

            if (npcDataBytes == null || npcDataBytes.Length != bytesToRead)
            {
                Debug.WriteLine("讀取 NPC 資料區塊時發生錯誤。 ReadData 返回 null 或不正確的大小。");
                return allNPCs; // 處理記憶體讀取失敗
            }

            // 2. 在記憶體中處理批次資料
            for (int i = 0; i < npcCountToRead; i++)
            {
                int offset = i * 12; // 每個 NPC 條目的偏移量

                long findID = BitConverter.ToInt32(npcDataBytes, offset); // 讀取 ID
                int chid = npcDataBytes[offset + 3];
                ushort dis = BitConverter.ToUInt16(npcDataBytes, offset + 4); // 讀取距離

                // 忽略 IgnoredIDs 名單中的 NPC
                if (IgnoredIDs.Any(npc => npc == findID) || allNPCs.Any(npc => npc.ID == findID))
                {
                    continue;
                }

                TargetTypes type = TargetTypes.None;
                switch (chid)
                {
                    case 1: type = TargetTypes.Player; break;
                    case 96: type = TargetTypes.NPC; break;
                    case 254: type = TargetTypes.TreasureBox; break;
                    default: break;
                }
                //  MainNob.Log($"NPC ID: {findID}, Type: {type}, Distance: {dis}");
                allNPCs.Add(new NPCData
                {
                    ID = findID,
                    Type = type,
                    Distance = dis
                });
            }

            return allNPCs;
        }

        public static List<int> GetAllNPCIDs()
        {
            allNPCs = GetAllNPCs();
            return allNPCs.Select(npc => (int)npc.ID).ToList();
        }

        /// <summary>
        /// 將十六進位字串轉換為位元組陣列。
        /// </summary>
        /// <param name="hex">十六進位字串</param>
        /// <returns>位元組陣列</returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0)
            {
                throw new FormatException("Hex 字串的長度必須為偶數。");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }

    [Flags]
    public enum TargetTypes
    {
        None = 0,
        Player = 1 << 0,
        NPC = 1 << 1,
        TreasureBox = 1 << 2
    }

    /// <summary>
    /// 取代原本的 int chid，改用旗標表示具體類型
    /// </summary>
    public class NPCData
    {
        public long ID { get; set; }
        public TargetTypes Type { get; set; }
        public ushort Distance { get; set; }

        public override string ToString()
        {
            return $"{ID}-{Type.ToString()}-{Distance}";
        }
    }
}
