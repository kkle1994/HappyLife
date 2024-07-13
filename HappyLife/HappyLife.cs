using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using GameData;
using HarmonyLib;
using GameData.Domains;
using GameData.Domains.Character;
using HarmonyLib.Tools;
using TaiwuModdingLib.Core.Plugin;
using System.IO;
using GameData.Domains.Adventure;
using GameData.Domains.Taiwu;
using GameData.Domains.Combat;
using GameData.Common;
using GameData.Domains.Mod;
using System.Runtime;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using GameData.Domains.Building;
using GameData.Domains.Information;
using GameData.Domains.Map;
using GameData.Domains.World.Notification;
using GameData.Domains.LifeRecord.GeneralRecord;
using GameData.Domains.Character.Relation;
using GameData.Domains.Merchant;
using GameData.Domains.Organization;
using GameData.Domains.TaiwuEvent.EventManager;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.World;
using GameData.Serializer;
using System.Diagnostics;
using GameData.Utilities;

namespace HappyLife
{
    [PluginConfig("HappyLife", "kkle1994@outlook.com", "1.0")]
    public partial class HappyLife : TaiwuRemakeHarmonyPlugin
    {
        public static bool IsAutoCollecting = false;
        public static List<RenderInfo> MonthlyInfos = new List<RenderInfo>();
        public static HappyLife Instance;
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
            return null;
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

        [HarmonyPatch(typeof(WorldDomain), "OnLoadWorld")]
        public class EventOutput
        {
            public static bool Prefix()
            {
                //if (EventPatchLoaded)
                //    return true;
                //if (GetBoolSettings("UnlimitedConfess"))
                //{
                //    Harmony harmony = new Harmony("Taiwuhentai event");
                //    harmony.PatchAll(typeof(HappyLifeTest.OnOption9VisibleCheckPatch));
                //}


                //var fields = typeof(TaiwuEventDomain).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
                //var properties = typeof(TaiwuEventDomain).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
                //var method = DomainManager.TaiwuEvent.GetType().GetField("_managerArray", BindingFlags.Static & BindingFlags.NonPublic);
                //var managerArray = DomainManager.TaiwuEvent.GetType().GetField("_managerArray", BindingFlags.Static | BindingFlags.NonPublic).GetValue(DomainManager.TaiwuEvent) as EventManagerBase[];
                //foreach (var manager in managerArray)
                //{
                //    if (manager == null)
                //        continue;
                //    var eventDictionary = manager.GetType().GetField("_eventDictionary", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager) as Dictionary<string, TaiwuEvent>;


                //    var filename = $"EventData_{manager.GetType()}.csv";
                //    if (File.Exists(filename))
                //    {
                //        File.Delete(filename);
                //    }
                //    var lines = new List<string>();
                //    var headerLine = "Guid,OptionKey,Text,Package";
                //    lines.Add(headerLine);
                //    foreach (var item in eventDictionary.Values)
                //    {
                //        var line = $"{item.EventGuid},{item.EventConfig.EscOptionKey},{item.EventConfig.EventContent.Replace(",", "<:comma>").Replace("\n", "<:lineend>")},{item.EventConfig.Package.ToString()}";
                //        lines.Add(line);
                //    }
                //    File.WriteAllLines(filename, lines);
                //}

                return true;
            }
        }

        //[HarmonyPatch(typeof(RelationType), "AllowAddingRelation")]
        //public class AllowAddingRelationPatch
        //{
        //    public static void Postfix(int charId, ref bool __result)
        //    {
        //        if (GetBoolSettings("AllowAllRelation") && charId == DomainManager.Taiwu.GetTaiwuCharId())
        //            __result = true;
        //    }
        //}

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
                        if (trace.GetFrames().Exist(f => f.GetMethod().Name == "ApplyInitialChangesForTaiwu" || f.GetMethod().Name == "MakeSecretInformationBroadcast"))
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
                Random random = new Random();
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
            //static Dictionary<string, byte> DifficultyDictionary = new Dictionary<string, byte>()
            //{
            //    { "简单", 0 },
            //    { "正常", 1 },
            //    { "困难", 2 },
            //    { "极难", 3 },
            //};
            public static void Postfix(ref byte __result)
            {
                if (GetIntSettings("CombatDiffcultyOverwrite") != 0)
                {
                    __result = (byte)(GetIntSettings("CombatDiffcultyOverwrite") - 1);
                }
            }
        }

        //[HarmonyPatch(typeof(BuildingDomain), "CanBuild")]
        //public class UnlimitedDependBuilding
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("UnlimitedDependBuilding"))
        //            __result = true;
        //    }
        //}

        //[HarmonyPatch(typeof(BuildingDomain), "SerialUpdate")]
        //public class SerialUpdatePatch
        //{
        //    public static void Postfix(BuildingDomain __instance, DataContext context)
        //    {
        //        HappyLife.IsAutoCollecting = true;
        //        if (GetBoolSettings("QuickCollectBuildingEarn"))
        //            __instance.QuickCollectBuildingEarn(context);

        //        if (GetBoolSettings("QuickRecruitPeople"))
        //        {
        //            __instance.QuickRecruitPeople(context);
        //            var info = new RenderInfo(117, $"建筑已被自动重新安排人员。");
        //            MonthlyInfos.Add(info);
        //        }
        //        HappyLife.IsAutoCollecting = false;
        //    }
        //}

        //[HarmonyPatch(typeof(BuildingDomain), "AcceptBuildingBlockCollectEarning")]
        //public class AcceptBuildingBlockCollectEarningPatch
        //{
        //    public static void Postfix(BuildingDomain __instance, DataContext context, BuildingBlockKey key)
        //    {
        //        if (HappyLife.IsAutoCollecting)
        //        {
        //            var collection = DomainManager.World.GetMonthlyNotificationCollection();
        //            var data = __instance.GetBuildingBlockData(key);
        //            MapAreaData element_Areas = DomainManager.Map.GetElement_Areas(key.AreaId);
        //            int settlementIndex = DomainManager.Map.GetElement_Areas(key.AreaId).GetSettlementIndex(key.BlockId);
        //            short settlementId = element_Areas.SettlementInfos[settlementIndex].SettlementId;
        //            collection.AddBuildingIncome(settlementId, data.TemplateId);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(MonthlyNotificationCollection), "GetRenderInfos")]
        //public class MonthlyNotificationCollectionPatch
        //{
        //    public static void Postfix(List<RenderInfo> renderInfos)
        //    {
        //        renderInfos.AddRange(MonthlyInfos);
        //        MonthlyInfos.Clear();
        //    }
        //}
        //GetRenderInfos
        //QuickCollectBuildingEarn
        //QuickRecruitPeople

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
