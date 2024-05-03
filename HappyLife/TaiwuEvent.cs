﻿using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Relation;
using GameData.Domains.TaiwuEvent;
using GameData.Domains.TaiwuEvent.DisplayEvent;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Domains.TaiwuEvent.EventOption;
using GameData.Domains.TaiwuEvent.MonthlyEventActions.CustomActions;
using GameData.Domains.World.MonthlyEvent;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace HappyLife
{
    public partial class HappyLife
    {
        //[HarmonyPatch(typeof(OptionConditionMatcher), "TeamMateLess")]
        //public class OnOption1AvailableCheckPatch
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("UnlimitedGroupMemberCount"))
        //            __result = true;
        //    }
        //}



        [HarmonyPatch(typeof(OptionConditionMatcher), "DynamicTeammateCountMax")]
        public class DynamicTeammateCountMaxPatch
        {
            public static void Postfix(ref bool __result)
            {
                if (GetBoolSettings("UnlimitedGroupMemberCount"))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(OptionConditionMatcher), "SwornBrotherOrSisterLess")]
        public class SwornBrotherOrSisterLessPatch
        {
            public static void Postfix(ref bool __result)
            {
                if (GetBoolSettings("UnlimitedSworn"))
                    __result = true;
            }
        }


        [HarmonyPatch(typeof(RelationType), nameof(RelationType.AllowAddingHusbandOrWifeRelation))]
        public class ApplyRelationBecomeHusbandOrWifePatch
        {
            public static void Postfix(ref bool __result, int charId, int relatedCharId)
            {
                if (GetBoolSettings("UnlimitedMarry") && (charId == DomainManager.Taiwu.GetTaiwuCharId() || relatedCharId == DomainManager.Taiwu.GetTaiwuCharId()))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(EventHelper), nameof(EventHelper.RoleHasAliveSpouse))]
        public class RoleHasAliveSpousePatch
        {
            public static void Postfix(ref bool __result, int charId)
            {

                if (GetBoolSettings("FightForMarryWithMarried") && (charId == DomainManager.Taiwu.GetTaiwuCharId()))
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(EventHelper), nameof(EventHelper.SelectTaiwuTeammateForMatchmaking))]
        public class RSelectTaiwuTeammateForMatchmakingPatch
        {
            public static bool Prefix(EventArgBox argBox, string saveKey, int charId)
            {

                if (GetBoolSettings("AllowSetUpAnyTeammate"))
                {
                    if (!DomainManager.Character.TryGetElement_Objects(charId, out var element))
                    {
                        throw new Exception($"can not find character of id {charId} to SelectTaiwuTeammateForMatchmaking");
                    }

                    EventSelectCharacterData eventSelectCharacterData = new EventSelectCharacterData();
                    eventSelectCharacterData.FilterList = new List<CharacterSelectFilter>();
                    CharacterSelectFilter item = default(CharacterSelectFilter);
                    item.SelectKey = saveKey;
                    item.FilterTemplateId = -1;
                    item.AvailableCharacters = default(CharacterSet);
                    foreach (int item2 in DomainManager.Taiwu.GetGroupCharIds().GetCollection())
                    {
                        if (item2 != charId && item2 != DomainManager.Taiwu.GetTaiwuCharId() && DomainManager.Character.TryGetElement_Objects(item2, out var element2))
                        {
                            item.AvailableCharacters.Add(item2);
                        }
                    }

                    eventSelectCharacterData.FilterList.Add(item);
                    argBox.Set("SelectCharacterData", eventSelectCharacterData);

                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MonthlyEventCollection), nameof(MonthlyEventCollection.AddAdviseExtendFavours))]
        public class AddAdviseExtendFavoursPatch
        {
            public static bool Prefix()
            {
                if (GetBoolSettings("BanEventSpendPrestige"))
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(MonthlyEventCollection), nameof(MonthlyEventCollection.AddAdviseMerchantFavor))]
        public class AddAdviseMerchantFavorPatch
        {
            public static bool Prefix()
            {
                if (GetBoolSettings("BanEventSpendPrestige"))
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.GetAliveSpouse))]
        public class GetAliveSpousePatch
        {
            public static bool Prefix(ref int __result)
            {
                if (!GetBoolSettings("MarryAdventrueIgnoreMarried"))
                    return true;

                var stack = new StackTrace();
                if (stack.GetFrames().Exist(f => f.GetMethod().Name == "MonthlyHandler"))
                {
                    __result = -1;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(MarriageTriggerAction), nameof(MarriageTriggerAction.MonthlyHandler))]
        public class MonthlyHandlerPatch
        {
            public static bool Prefix(ref MarriageTriggerAction __instance)
            {
                if (!GetBoolSettings("MarryAdventrueIgnoreMarried"))
                    return true;
                if (__instance.State == 1)
                {
                    var method = typeof(MarriageTriggerAction).GetMethod("CallParticipateCharacters", BindingFlags.Instance | BindingFlags.NonPublic);

                    method?.Invoke(__instance, new object[0]);
                    __instance.Activate();
                    __instance.State = 5;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.ChangeHappiness))]
        public class SetHappinessPatch
        {
            public static bool Prefix(ref Character __instance, ref int delta)
            {
                if (!GetBoolSettings("CancelMonthlyEventHappinessReduce"))
                    return true;
                var stack = new StackTrace();
                if (stack.GetFrames().Exist(f => f.GetMethod().Name == "AdvanceMonth"))
                {
                    if (delta < 0 && __instance.HasAdoredRelaltionWithTaiwu())
                    {
                        delta = 0;
                    }
                }
                return true;
            }
        }
    }
}
