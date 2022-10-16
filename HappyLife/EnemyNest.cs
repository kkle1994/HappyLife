using BehTree;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Adventure;
using GameData.Domains.Character;
using GameData.Domains.Global;
using GameData.Domains.Taiwu;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(AdventureDomain), "OfflineAddConqueredEnemyNestIncome")]
        public class OfflineAddConqueredEnemyNestIncomePatch
        {
            public static void Postfix(ref EnemyNestSiteExtraData enemyNestData, DataContext context)
            {
                if (GetBoolSettings("EnemyNestDefaultIncome"))
                {
                    if (enemyNestData.Tribute.Type == -1)
                    {
                        var minValue = 200;
                        var maxValue = 600;
                        var count = new Random().Next(minValue, maxValue)
                            + new Random().Next(minValue, maxValue)
                            + new Random().Next(minValue, maxValue)
                            + new Random().Next(minValue, maxValue)
                            + new Random().Next(minValue, maxValue);
                        var taiwu = DomainManager.Taiwu.GetTaiwu();
                        taiwu.ChangeResource(context, ResourceType.Authority, count);
                        DomainManager.World.GetInstantNotificationCollection().AddResourceIncreased(taiwu.GetId(), ResourceType.Authority, count);
                    }
                }
            }
        }
    }
}
