using ConchShip.EventConfig.Taiwu;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Mod;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.World;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace HappyLifeEvent
{
    [PluginConfig("HappyLifeEvent", "kkle1994@outlook.com", "1.0")]
    public class HappyLifeEvent : TaiwuRemakeHarmonyPlugin
    {
        public static bool IsLoaded = false;
        public static HappyLifeEvent Instance;

        public override void Initialize()
        {
            this.HarmonyInstance.PatchAll(typeof(EventInjector));
            Instance = this;
        }

        #region Setting Readers
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
        #endregion

        [HarmonyPatch(typeof(WorldDomain), "OnLoadWorld")]
        public class EventInjector
        {
            static Harmony dynamicHarmony = new Harmony("HappyLifeEventInside");
            public static bool Prefix()
            {
                Harmony.UnpatchID("HappyLifeEventInside");
                dynamicHarmony.PatchAll(typeof(OnOption22VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption10VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption1VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption15VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption1SelectPatch1));
                dynamicHarmony.PatchAll(typeof(OnOption1SelectPatch2));
                dynamicHarmony.PatchAll(typeof(OnOption1SelectPatch3));
                return true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption11VisibleCheck")]
        public class OnOption10VisibleCheckPatch
        {
            public static void Postfix(ref bool __result, TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance)
            {
                Character character = __instance.ArgBox.GetCharacter("RoleTaiwu");
                Character character2 = __instance.ArgBox.GetCharacter("CharacterId");
                var relation = DomainManager.Character.GetRelation(character.GetId(), character2.GetId());
                var hasAdoredRelation = relation.RelationType / 16384 % 2 == 1;

                if (hasAdoredRelation)
                {
                    __result = false;
                }
                else if (GetBoolSettings("UnlimitedConfess"))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption16VisibleCheck")]
        public class OnOption15VisibleCheckPatch
        {
            public static void Postfix(ref bool __result, TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance)
            {
                Character character = __instance.ArgBox.GetCharacter("RoleTaiwu");
                Character character2 = __instance.ArgBox.GetCharacter("CharacterId");
                var relation = DomainManager.Character.GetRelation(character.GetId(), character2.GetId());
                var hasAdoredRelation = relation.RelationType / 16384 % 2 == 1;
                var dontHasMarriageBetween = relation.RelationType / 1024 % 2 == 1;

                if (hasAdoredRelation && !dontHasMarriageBetween && GetBoolSettings("UnlimitedMarry"))
                {
                    __result = true;
                }
            }
        }

        /// <summary>
        /// 允许任意做媒
        /// </summary>
        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption24VisibleCheck")]
        public class OnOption22VisibleCheckPatch
        {
            public static void Postfix(ref bool __result, TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance)
            {
                if (!EventHelper.CheckMainStoryLineProgress(8))
                {
                    return;
                }

                if (GetBoolSettings("AllowSetUpAnyTeammate"))
                {
                    __result = true;
                }
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_8948e6ae892c4999bca7a235926ad6d6), "OnOption1VisibleCheck")]
        public class OnOption1VisibleCheckPatch
        {
            public static void Postfix(ref bool __result)
            {
                if (GetBoolSettings("HideKillRuMoRenOption"))
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_3124b791cf0a4f7e80d8e93de8fe4de7), "OnOption1Select")]
        public class OnOption1SelectPatch1
        {
            public static bool Prefix(TaiwuEvent_3124b791cf0a4f7e80d8e93de8fe4de7 __instance, ref string __result)
            {
                if (GetBoolSettings("RemoveSupportCost"))
                {
                    Character character = __instance.ArgBox.GetCharacter("CharacterId");
                    sbyte roleGrade = EventHelper.GetRoleGrade(character);
                    int num = (1 + roleGrade) * (1 + roleGrade) * (1 + roleGrade) * 50;
                    Character character2 = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    if (character2.GetExp() > num)
                    {
                        __result = "20fd5e4a-add7-4b7e-ba3d-54195378a60e";
                    }
                    else
                        __result = "ad734ddc-f99e-4fbd-8cbc-555e6661c5b2";

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_0e9bec3fe20d4cb3acd88b2775bc9f73), "OnOption1Select")]
        public class OnOption1SelectPatch2
        {
            public static bool Prefix(TaiwuEvent_0e9bec3fe20d4cb3acd88b2775bc9f73 __instance, ref string __result)
            {
                if (GetBoolSettings("RemoveSupportCost"))
                {
                    Character character = __instance.ArgBox.GetCharacter("CharacterId");
                    sbyte roleGrade = EventHelper.GetRoleGrade(character);
                    int num = -(20 + roleGrade * 10) * 10;
                    short characterOrganizationAreaId = EventHelper.GetCharacterOrganizationAreaId(character);
                    if (EventHelper.GetAreaSpiritualDebt(characterOrganizationAreaId) >= -num)
                    {
                        __result = "52989a8c-3c4e-41cb-9d3d-0a6a1a285d13";
                    }
                    else
                        __result = "8dea7537-f565-4301-82c6-b30b105a1500";
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_0c35bf5e73ce4b029be5e580561183b2), "OnOption1Select")]
        public class OnOption1SelectPatch3
        {
            public static void Postfix(TaiwuEvent_0c35bf5e73ce4b029be5e580561183b2 __instance)
            {
                if (GetBoolSettings("SeverAndDivorce"))
                {
                    Character character = __instance.ArgBox.GetCharacter("CharacterId");
                    Character character2 = __instance.ArgBox.GetCharacter("RoleTaiwu");
                    EventHelper.ApplyRelationSeverHusbandOrWife(character2, character);
                }
            }
        }
    }
}
