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
