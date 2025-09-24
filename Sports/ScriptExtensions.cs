using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NOBApp.Sports
{
    /// <summary>
    /// 包含用於簡化腳本編寫的輔助函式 (Extension Methods)。
    /// </summary>
    public static class ScriptExtensions
    {
        /// <summary>
        /// 【簡化版】等待直到指定的玩家進入可以對話的選單狀態。
        /// </summary>
        /// <param name="nob">要操作的玩家物件。</param>
        /// <param name="timeoutMilliseconds">超時時間（毫秒）。</param>
        /// <param name="cancellationToken">取消權杖。</param>
        /// <returns>如果成功等到，返回 true；如果超時，則拋出 TimeoutException。</returns>
        public static async Task<bool> WaitForDialogueMenuAsync(this NOBDATA nob, int timeoutMilliseconds = 30000, CancellationToken cancellationToken = default)
        {
            string actionName = $"[{nob.PlayerName}] 等待NPC對話選單";

            bool success = await BaseClass.WaitUntilAsync(
                () => nob.對話與結束戰鬥 && (nob.出現直式選單 || nob.出現左右選單),
                timeoutMilliseconds,
                actionName: actionName,
                cancellationToken: cancellationToken);

            if (!success)
            {
                throw new TimeoutException($"{actionName} 超時。");
            }
            return true;
        }

        /// <summary>
        /// 【簡化版】等待直到指定玩家的地圖 ID 發生變化。
        /// </summary>
        /// <param name="nob">要操作的玩家物件。</param>
        /// <param name="originalMapId">原始的地圖 ID。</param>
        /// <param name="timeoutMilliseconds">超時時間（毫秒）。</param>
        /// <param name="cancellationToken">取消權杖。</param>
        /// <returns>如果成功等到，返回 true；如果超時，則拋出 TimeoutException。</returns>
        public static async Task<bool> WaitForMapChangeAsync(this NOBDATA nob, int originalMapId, int timeoutMilliseconds = 30000, CancellationToken cancellationToken = default)
        {
            string actionName = $"[{nob.PlayerName}] 等待地圖從 {originalMapId} 切換";

            bool success = await BaseClass.WaitUntilAsync(
                () => nob.MAPID != originalMapId,
                timeoutMilliseconds,
                actionName: actionName,
                cancellationToken: cancellationToken);

            if (!success)
            {
                throw new TimeoutException($"{actionName} 超時。");
            }
            return true;
        }

        /// <summary>
        /// 【簡化版】等待團隊所有成員都滿足特定條件。
        /// </summary>
        /// <param name="team">玩家隊伍。</param>
        /// <param name="condition">每個成員都需滿足的條件。</param>
        /// <param name="actionName">操作名稱。</param>
        /// <param name="timeoutMilliseconds">超時時間（毫秒）。</param>
        /// <param name="cancellationToken">取消權杖。</param>
        public static async Task<bool> WaitAllMembersAsync(this IEnumerable<NOBDATA> team, Func<NOBDATA, bool> condition, string actionName, int timeoutMilliseconds = 60000, CancellationToken cancellationToken = default)
        {
            bool success = await BaseClass.WaitUntilAsync(
               () => team.All(condition),
               timeoutMilliseconds,
               actionName: actionName,
               cancellationToken: cancellationToken);

            if (!success)
            {
                throw new TimeoutException($"{actionName} 超時。");
            }
            return true;
        }
    }
}
