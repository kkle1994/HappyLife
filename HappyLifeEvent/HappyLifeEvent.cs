using ConchShip.EventConfig.Taiwu;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Mod;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.TaiwuEvent.EventOption;
using GameData.Domains.World;
using HarmonyLib;
using System;
using System.Reflection;
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
                //if (IsLoaded)
                //    return true;

                dynamicHarmony.UnpatchAll("HappyLifeEventInside");
                //dynamicHarmony.PatchAll(Assembly.GetAssembly(typeof(HappyLifeEvent)));
                dynamicHarmony.PatchAll(typeof(OnOption9VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption10VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption1VisibleCheckPatch));
                dynamicHarmony.PatchAll(typeof(OnOption18VisibleCheckPatch));
                //IsLoaded = true;
                //if(GetBoolSettings("UnlimitedMerchantFavorability"))
                //{
                //    for(var index = 0; index < GlobalConfig.Instance.MerchantFavorabilityUpperLimits.Length; index++)
                //    {
                //        GlobalConfig.Instance.MerchantFavorabilityUpperLimits[index] = 100;
                //    }
                //}
                return true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption9VisibleCheck")]
        public class OnOption9VisibleCheckPatch
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

        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption10VisibleCheck")]
        public class OnOption10VisibleCheckPatch
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

        [HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption18VisibleCheck")]
        public class OnOption18VisibleCheckPatch
        {
            public static void Postfix(ref bool __result, TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance)
            {
                if (!EventHelper.CheckMainStoryLineProgress(8))
                {
                    return;
                }

                if (GetBoolSettings("AllowSetUpAnyTeammate"))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(TaiwuEvent_8948e6ae892c4999bca7a235926ad6d6), "OnOption1VisibleCheck")]
        public class OnOption1VisibleCheckPatch
        {
            public static void Postfix(ref bool __result)
            {
                if(GetBoolSettings("HideKillRuMoRenOption"))
                    __result = false;
            }
        }

        //[HarmonyPatch(typeof(TaiwuEvent_d27c6a3dcf134a8480d6b4e0d2b26702), "OnOption1VisibleCheck")]
        //public class OnOption1VisibleCheckPatch
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("HideKillRuMoRenOption"))
        //            __result = false;
        //    }
        //}

        //[HarmonyPatch(typeof(TaiwuEvent_716f82a10c634614aace0498de80d5d0), "OnOption10VisibleCheck")]

        //[HarmonyPatch(typeof(TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b), "OnOption13AvailableCheck")]
        //public class OnOption13AvailableCheckPatch
        //{
        //    public static void Postfix(ref bool __result, TaiwuEvent_bad63f08115a45aa970cfa203dd85e2b __instance)
        //    {
        //        if (GetBoolSettings("UnlimitedSworn"))
        //        {
        //            __result = true;
        //        }
        //    }
        //}
    }
}
