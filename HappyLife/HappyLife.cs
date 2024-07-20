using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Combat;
using GameData.Domains.Information;
using GameData.Domains.LifeRecord.GeneralRecord;
using GameData.Domains.Mod;
using GameData.Domains.Organization;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TaiwuModdingLib.Core.Plugin;

namespace HappyLife
{
    [PluginConfig("HappyLife", "kkle1994@outlook.com", "1.0")]
    public partial class HappyLife : TaiwuRemakeHarmonyPlugin
    {
        public static bool IsAutoCollecting = false;
        public static List<RenderInfo> MonthlyInfos = new List<RenderInfo>();
        public static HappyLife? Instance;
        public static bool EventPatchLoaded = false;
        public override void Initialize()
        {
            base.Initialize();
            Instance = this;
        }

        #region Settings
        public static int GetIntSettings(string field)
        {
            var result = 0;
            foreach (var modId in ModDomain.GetLoadedModIds())
            {
                if (DomainManager.Mod.GetSetting(modId.ToString(), field, ref result))
                    return result;
            }
            return 1;
        }

        public static string GetStringSettings(string field)
        {
            var result = "";
            foreach (var modId in ModDomain.GetLoadedModIds())
            {
                if (DomainManager.Mod.GetSetting(modId.ToString(), field, ref result))
                    return result;
            }
            return string.Empty;
        }

        public static bool GetBoolSettings(string field)
        {
            var result = false;
            foreach (var modId in ModDomain.GetLoadedModIds())
            {
                if (DomainManager.Mod.GetSetting(modId.ToString(), field, ref result))
                    return result;
            }
            return false;
        }

        public static bool GetModPath(string field)
        {
            var result = false;
            foreach (var modId in ModDomain.GetLoadedModIds())
            {
                if (DomainManager.Mod.GetSetting(modId.ToString(), field, ref result))
                    return result;
            }
            return false;
        }

        #endregion

        [HarmonyPatch(typeof(Character), "GetRecoveryOfFlaw")]
        public class NoRecover1Patch
        {
            public static void Postfix(Character __instance, ref short __result)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId() && GetIntSettings("TaiwuRecoverPercent") != 100)
                    __result = (short)(__result * (GetIntSettings("TaiwuRecoverPercent") / 100f));
                if (__instance.GetId() != DomainManager.Taiwu.GetTaiwuCharId() && GetIntSettings("EnemyRecoverPercent") != 100)
                    __result = (short)(__result * (GetIntSettings("EnemyRecoverPercent") / 100f));
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), "CalcFavorabilityDelta")]
        public class CalcFavorabilityDeltaPatch
        {
            public static void Postfix(CharacterDomain __instance, int characterId, int relatedCharId, ref int __result)
            {
                if (characterId == DomainManager.Taiwu.GetTaiwuCharId() || relatedCharId == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    var trace = new StackTrace();
                    if (__result < 0 && GetBoolSettings("CancelMonthlyEventFavorReduce"))
                    {
                        if (trace.GetFrames().Exist(f => f.GetMethod()!.Name == "ApplyInitialChangesForTaiwu" || f.GetMethod()!.Name == "MakeSecretInformationBroadcast"))
                        {
                            __result = 0;
                            return;
                        }
                    }

                    var multiple = GetIntSettings("FavorabilityMultiple");
                    if (__result > 0)
                        __result *= multiple;
                }
            }
        }

        [HarmonyPatch(typeof(CombatDomain), "ReduceWeaponDurability")]
        public class NoWeaponDurablilityPatch
        {
            public static bool Prefix(ref int reduceValue)
            {
                var random = new Random();
                var rate = GetIntSettings("WeaponDurabilityRate");
                reduceValue = random.Next(1, 100) < rate ? 1 : 0;
                return true;
            }
        }

        [HarmonyPatch(typeof(OrganizationDomain), "GetApprovingRateUpperLimit")]
        public class GetMerchantFavorabilityBuilding
        {
            public static void Postfix(ref short __result)
            {
                if (GetBoolSettings("UnlimitApprovingRateUpperLimit"))
                {
                    __result = 1000;
                }
            }
        }

        [HarmonyPatch(typeof(WorldDomain), nameof(WorldDomain.GetCombatDifficulty))]
        public class GetCombatDifficultyPatch
        {
            public static void Postfix(ref byte __result)
            {
                if (GetIntSettings("CombatDiffcultyOverwrite") != 0)
                {
                    __result = (byte)(GetIntSettings("CombatDiffcultyOverwrite") - 1);
                }
            }
        }

        [HarmonyPatch(typeof(TaiwuDomain), "CalcBuildingSpaceLimit")]
        public class IncreaseBuildingMaxCount
        {
            public static void Postfix(ref int __result)
            {
                var increasement = GetIntSettings("IncreaseBuildingMaxCount");
                __result += increasement;
            }
        }

        [HarmonyPatch(typeof(TaiwuDomain), "GetCricketLuckPoint")]
        public class NoCricketPregnantPatch
        {
            public static void Postfix(ref int __result)
            {
                if (GetBoolSettings("BanCricketBorn"))
                    __result = 0;
            }
        }

        [HarmonyPatch(typeof(NormalInformationCollection), "SetUsedCount")]
        public class SetUsedCountPatch
        {
            public static bool Prefix(ref sbyte count)
            {
                if (GetBoolSettings("UnlimitedInformation"))
                    count = 0;
                return true;
            }
        }

    }
}
