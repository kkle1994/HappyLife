using GameData.Domains;
using GameData.Domains.Character;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(Character), "GetChangeOfQiDisorder")]
        public class GetChangeOfQiDisorderPatch
        {
            public static void Postfix(Character __instance, ref short __result)
            {
                if (GetBoolSettings("TaiwuValligerNoQiDisorder"))
                {
                    if (__instance.IsTaiwuVillagers())
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
