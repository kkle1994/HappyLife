using GameData.Domains.World;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {

        [HarmonyPatch(typeof(WorldDomain), nameof(WorldDomain.AdvanceDaysInMonth))]
        public class AdvanceDaysInMonthPatch
        {
            public static bool Prefix(ref int days)
            {
                if (GetBoolSettings("LockDaysInMonth") && days != 30)
                    days = 0;
                return true;
            }
        }

        [HarmonyPatch(typeof(WorldDomain), nameof(WorldDomain.ConsumeActionPoint))]
        public class ConsumeActionPointPatch
        {
            public static bool Prefix(ref int actionPoints)
            {
                if (GetBoolSettings("LockDaysInMonth"))
                    actionPoints = 0;
                return true;
            }
        }
    }
}
