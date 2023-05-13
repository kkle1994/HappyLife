using GameData.Common;
using GameData.Domains.Merchant;
using GameData.Domains;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.Domains.Building;

namespace HappyLife
{
    public partial class HappyLife
    {
        //[HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.GetResourceBlockGrowthChance))]
        //public class GetResourceBlockGrowthChancePatch
        //{
        //    public static void Postfix(ref sbyte __result)
        //    {
        //        if (GetBoolSettings("ResourceMustGrow"))
        //        {
        //            __result = __result == 0 ? (sbyte)0 : (sbyte)100;
        //        }
        //    }
        //}
    }
}
