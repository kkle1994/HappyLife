using GameData.Domains.Taiwu;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {
        //[HarmonyPatch(typeof(TaiwuDomain), nameof(TaiwuDomain.calc))]
        //public class GetTaiwuGroupMaxCounthPatch
        //{
        //    public static void Postfix(ref int __result)
        //    {
        //        //if (GetBoolSettings("UnlimitedGroupMemberCount"))
        //        __result = 100;
        //    }
        //}

        [HarmonyPatch(typeof(TaiwuDomain), "CalcReferenceBookSlotUnlockStates")]
        public class CalcReferenceBookSlotUnlockStatesPatch
        {
            public static void Postfix(ref byte __result)
            {
                if (GetBoolSettings("UnlimitedReferenceBookSlot"))
                    __result = 7;
            }
        }
    }
}
