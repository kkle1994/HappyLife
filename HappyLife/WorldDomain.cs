using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Global;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
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
    }
}
