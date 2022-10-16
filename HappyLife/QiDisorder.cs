using GameData.Domains;
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
        [HarmonyPatch(typeof(Character), "GetChangeOfQiDisorder")]
        public class GetChangeOfQiDisorderPatch
        {
            public static void Postfix(Character __instance, ref short __result)
            {
                if(GetBoolSettings("TaiwuValligerNoQiDisorder"))
                {
                    var villagersStatus = DomainManager.Taiwu.GetAllVillagersStatus();
                    if (villagersStatus.Exists(v => v.CharacterId == __instance.GetId()) && DomainManager.Taiwu.GetTaiwuCharId() != __instance.GetId())
                    {
                        __result = short.MinValue;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Character), nameof(Character.ChangeDisorderOfQi))]
        //public class ChangeDisorderOfQiPatch
        //{
        //    public static void Postfix(Character __instance, ref short delta)
        //    {
        //        if (GetBoolSettings("TaiwuValligerNoQiDisorder"))
        //        {
        //            if (__instance.IsTaiwuVillagers() && DomainManager.Taiwu.GetTaiwuCharId() != __instance.GetId())
        //            {
        //                delta = short.MinValue;
        //            }
        //        }
        //    }
        //}
    }
}
