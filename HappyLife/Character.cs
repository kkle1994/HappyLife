using BehTree;
using Config;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.ParallelModifications;
using GameData.Domains.Character.Relation;
using GameData.Domains.CombatSkill;
using GameData.Domains.Global;
using GameData.Domains.Map;
using GameData.Domains.TaiwuEvent.EventHelper;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static GameData.Domains.Character.Ai.AiHelper;
using Character = GameData.Domains.Character.Character;

namespace HappyLife
{
    public static class CharacterHelper
    {
        public static bool IsTaiwu(this Character character)
        {
            return character != null ? character.GetId() == DomainManager.Taiwu.GetTaiwuCharId() : false;
        }

        public static bool IsTaiwuVillagers(this Character character)
        {
            var villagersStatus = DomainManager.Taiwu.GetAllVillagersStatus();
            return character != null ? villagersStatus.Exists(v => v.CharacterId == character.GetId()) : false;
        }

        public static T GetValue<T>(this Character character, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return character != null ? (T)character.GetType().GetField(fieldName, flags).GetValue(character) : default(T);
        }

        public static void SetValue<T>(this Character character, string fieldName, T value, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            character.GetType().GetField(fieldName, flags).SetValue(character, value);
        }
    }

    public partial class HappyLife
    {
        public static bool HasRelation(int charater, int target, short relation)
        {
            if (DomainManager.Character.TryGetRelation(charater, target, out RelatedCharacter relationInside))
            {
                return relationInside.RelationType / relation % 2 == 1;
            }
            return false;
        }

        [HarmonyPatch(typeof(Character), "OfflineUpdateHealth")]
        public class OfflineUpdateHealthPatch
        {
            public static bool Prefix(Character __instance)
            {
                if (GetBoolSettings("VillagersNeverDie") && __instance.IsTaiwuVillagers())
                    return false;
                if (GetBoolSettings("TaiwuNeverDie") && __instance.IsTaiwu())
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "GetXiangshuInfectionDelta")]
        public class GetXiangshuInfectionDeltaPatch
        {
            public static void Postfix(ref sbyte __result)
            {
                var value = __result * (GetIntSettings("InfectedSpeedRate") / 100f);
                value = Math.Clamp(value, sbyte.MinValue, sbyte.MaxValue);
                __result = (sbyte)value;
            }
        }

        [HarmonyPatch(typeof(Character), "CanBeXiangshuInfected")]
        public class CanBeXiangshuInfectedPatch
        {
            public static void Postfix(Character __instance, ref bool __result)
            {
                if (GetBoolSettings("BanTaiwuVillagerInfected"))
                {
                    var villagersStatus = DomainManager.Taiwu.GetAllVillagersStatus();
                    if (villagersStatus.Exists(v => v.CharacterId == __instance.GetId()))
                    {
                        __result = false;
                    }
                }
                if(GetBoolSettings("BanTaiwuInfected"))
                {
                    if (__instance.IsTaiwu())
                        __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Character), "ChangeXiangshuInfection")]
        public class ChangeXiangshuInfectionPatch
        {
            public static bool Prefix(Character __instance, ref int delta)
            {
                if (GetBoolSettings("BanTaiwuInfected"))
                {
                    if (__instance.IsTaiwu())
                        delta = int.MinValue;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "ChangeResource")]
        public class ChangeResourcePatch
        {
            public static bool Prefix(Character __instance, sbyte resourceType, ref int delta)
            {
                var stacktrace = new StackTrace();
                var frames = stacktrace.GetFrames();

                var rate = GetIntSettings("AddResourceMultiple");
                if (frames.ToList().Exists(f => f.GetMethod().Name == "CalcVillagerWorkOnMap")
                    || frames.ToList().Exists(f => f.GetMethod().Name == "PreAdvanceMonth")
                    || !GetBoolSettings("ResourceMultipleOnlyForMonthlyIncreasing"))
                {
                    if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                    {
                        if (delta > 0 && resourceType >= 0 && resourceType <= 6)
                            delta *= rate;
                    }
                }
                else
                {
                    if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                    {
                        if (delta > 0 && resourceType >= 0 && resourceType <= 6)
                            delta *= rate;
                    }
                }

                if (__instance.GetResource(resourceType) + delta < 0)
                    return false;

                return true;
            }
        }

        //[HarmonyPatch(typeof(Character), "ChangeResources")]
        //public class ChangeResourcePatch
        //{
        //    public unsafe static bool Prefix(Character __instance, ref ResourceInts delta)
        //    {
        //        if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
        //        {
        //            var rate = GetIntSettings("AddResourceMultiple");
        //            if (rate == 1)
        //                return true;
        //            for (int i = 0; i < 8; i++)
        //            {
        //                delta.Items[i] *= rate;
        //            }
        //        }
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(Character), "GetConsummateLevel")]
        public class GetConsummateLevelPatch
        {
            public static void Postfix(ref sbyte __result)
            {
                if (GetBoolSettings("SameConsummateLevel"))
                    __result = (sbyte)GetIntSettings("SameConsummateLevelValue");
            }
        }

        [HarmonyPatch(typeof(Character), "OfflineIncreaseAge")]
        public class OfflineIncreaseAgePatch
        {
            public static bool Prefix(Character __instance, ref sbyte __state)
            {
                __state = -1;
                if (GetBoolSettings("ShopTaiwuAgeIncreasing") && __instance.IsTaiwu())
                    return false;
                if (GetIntSettings("ChildQuickGrowAge") != 0 && __instance.GetActualAge() <= GetIntSettings("ChildQuickGrowAge") && __instance.IsTaiwuVillagers()
                    && __instance.GetValue<sbyte>("_birthMonth") != DomainManager.World.GetCurrMonthInYear())
                {
                    __state = __instance.GetValue<sbyte>("_birthMonth");
                    __instance.SetValue<sbyte>("_birthMonth", DomainManager.World.GetCurrMonthInYear());
                }
                return true;
            }

            public static void Postfix(Character __instance, ref sbyte __state)
            {
                if (__state != -1)
                    __instance.SetValue<sbyte>("_birthMonth", __state);
            }
            //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, Character __instance)
            //{
            //    if (GetIntSettings("ChildQuickGrowAge") != 0 && __instance.GetActualAge() <= GetIntSettings("ChildQuickGrowAge") && __instance.IsTaiwuVillagers())
            //    {
            //        var rowIndex = -1;

            //        var codes = new List<CodeInstruction>(instructions);
            //        for (var i = 0; i < codes.Count; i++)
            //        {
            //            if (codes[i].opcode == OpCodes.Brfalse)
            //            {
            //                codes[rowIndex].opcode = OpCodes.Nop;
            //            }
            //        }

            //        return codes.AsEnumerable();
            //    }

            //    return instructions;
            //}
        }
        
        [HarmonyPatch(typeof(Relation), nameof(Relation.GetStartRelationSuccessRate_SexRelationBaseRate))]
        public class SexRelationBaseRatePatch
        {
            public static bool Prefix(ref int __result, Character selfChar, Character targetChar, RelatedCharacter selfToTarget, RelatedCharacter targetToSelf)
            {
                if ((!selfChar.IsTaiwu() && !targetChar.IsTaiwu()) || !GetBoolSettings("FixConfessIssue"))
                    return true;
                int num = 100;
                sbyte gender = selfChar.GetGender();
                sbyte displayingGender = selfChar.GetDisplayingGender();
                sbyte gender2 = targetChar.GetGender();
                sbyte displayingGender2 = targetChar.GetDisplayingGender();

                if ((!selfChar.IsTaiwu() && !targetChar.IsTaiwu()))
                {
                    if (targetChar.GetBisexual())
                    {
                        if (gender2 != displayingGender)
                        {
                            num -= 1000;
                        }
                    }
                    else if (gender2 == displayingGender)
                    {
                        num -= 1000;
                    }
                }

                if (RelationType.HasRelation(targetToSelf.RelationType, 1024))
                {
                    num += 40;
                }

                if (RelationType.ContainDirectBloodRelations(targetToSelf.RelationType))
                {
                    num -= 30;
                }

                if (RelationType.ContainNonBloodFamilyRelations(targetToSelf.RelationType) || RelationType.HasRelation(selfToTarget.RelationType, 2048) || RelationType.HasRelation(selfToTarget.RelationType, 4096))
                {
                    num -= 10;
                }

                short currAge = selfChar.GetCurrAge();
                short currAge2 = targetChar.GetCurrAge();
                num = ((selfChar.GetGender() != targetChar.GetGender()) ? (num - ((selfChar.GetGender() != 1) ? ((currAge2 > currAge) ? Math.Clamp(3 * (currAge2 - currAge - 10), 0, 60) : Math.Clamp(3 * (currAge - currAge2), 0, 60)) : ((currAge > currAge2) ? Math.Clamp(3 * (currAge - currAge2 - 10), 0, 60) : Math.Clamp(3 * (currAge2 - currAge), 0, 60)))) : (num - Math.Clamp(3 * Math.Abs(currAge - currAge2), 0, 60)));
                sbyte favorabilityType = FavorabilityType.GetFavorabilityType(targetToSelf.Favorability);
                if (favorabilityType >= 3)
                {
                    num += favorabilityType * 60;
                }

                num += (selfChar.GetAttraction() - targetChar.GetAttraction());
                num += (selfChar.GetInteractionGrade() - targetChar.GetInteractionGrade());
                if (selfChar.GetDisplayingGender() == 0)
                {
                    num += 20;
                }

                if (selfChar.GetGender() == targetChar.GetGender() && selfChar.GetBisexual() && targetChar.GetBisexual())
                {
                    num += 20;
                }

                var taiwu = DomainManager.Taiwu.GetTaiwu();
                var hasAdoredWithTaiwu = HasRelation(targetChar.GetId(), taiwu.GetId(), 16384) || HasRelation(taiwu.GetId(), targetChar.GetId(), 16384);
                if (selfChar.GetId() != taiwu.GetId() && hasAdoredWithTaiwu)
                {
                    num -= 500;
                    if (GetBoolSettings("BanNTRTaiwu"))
                        num = int.MinValue;
                }

                // If self is taiwu, skip all the relationship debuff about couple and children
                if (!selfChar.IsTaiwu())
                {
                    RelatedCharacters relatedCharacters = DomainManager.Character.GetRelatedCharacters(targetChar.GetId());
                    HashSet<int> collection = relatedCharacters.GetCharacterSet(16384).GetCollection();
                    int id = selfChar.GetId();
                    foreach (int item in collection)
                    {
                        if (DomainManager.Character.IsCharacterAlive(item) && item != id)
                        {
                            num -= 20;
                        }
                    }


                    int num2 = 0;
                    RelatedCharacters relatedCharacters2 = DomainManager.Character.GetRelatedCharacters(selfChar.GetId());
                    num2 += relatedCharacters2.BloodChildren.GetCount() + relatedCharacters2.StepChildren.GetCount();
                    num2 += relatedCharacters.BloodChildren.GetCount() + relatedCharacters.StepChildren.GetCount();
                    num -= 10 * num2;

                    if (selfChar.GetMonkType() != 0)
                    {
                        num -= 80;
                    }

                    if (targetChar.GetMonkType() != 0)
                    {
                        num -= 80;
                    }

                    if (selfChar.GetFertility() <= 0)
                    {
                        num -= 30;
                    }

                    if (targetChar.GetFertility() <= 0)
                    {
                        num -= 30;
                    }
                }
                __result = num;
                return false;
            }
        }

        [HarmonyPatch(typeof(Character), "SetFameActionRecords")]
        public class HasRelationPatch
        {
            public static List<short> FameActionNeedRemove = new List<short> {
                FameAction.DefKey.Immoral,
                FameAction.DefKey.FightChildren,
                FameAction.DefKey.MakeBadLovers,
                FameAction.DefKey.Unfaithful,
                FameAction.DefKey.Unethical
            };
            public static bool Prefix(ref Character __instance, ref List<FameActionRecord> fameActionRecords)
            {
                if (__instance.IsTaiwu() && GetBoolSettings("NoImmoralFameActions"))
                {
                    fameActionRecords.RemoveAll(r => FameActionNeedRemove.Contains(r.Id));
                    __instance.GetFameActionRecords().RemoveAll(r => FameActionNeedRemove.Contains(r.Id));
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Character), "GetStartOrEndRelationTarget")]
        public class GetStartOrEndRelationTargetPatch
        {
            public static bool Prefix(ref Character __instance, ref Character __result, IRandomSource random, short aiRelationsTemplateId, List<int> selectableChars, ref Personalities selfPersonalities)
            {
                var tripleStartRelationChance = __instance.GetType().GetMethod("TripleStartRelationChance", BindingFlags.Instance | BindingFlags.NonPublic);
                if ((aiRelationsTemplateId == 2 || aiRelationsTemplateId == 3 || aiRelationsTemplateId == 4 || aiRelationsTemplateId ==5) && !__instance.IsTaiwu() && GetBoolSettings("NoImmoralFameActions"))
                {
                    bool flag = selectableChars.Count == 0;
                    Character character;
                    if (flag)
                    {
                        character = null;
                    }
                    else
                    {
                        int multiplier = ((bool)tripleStartRelationChance.Invoke(__instance, new object[] { aiRelationsTemplateId })) ? 3 : 1;
                        AiRelationsItem relationCfg = AiHelper.Relation.GetAiRelationConfig(aiRelationsTemplateId);
                        bool flag2 = !AiHelper.Relation.CheckChangeRelationTypeChance(random, ref selfPersonalities, relationCfg.PersonalityType, multiplier);
                        if (flag2)
                        {
                            character = null;
                        }
                        else
                        {
                            int targetCharId = selectableChars.GetRandom(random);
                            Character targetChar = DomainManager.Character.GetElement_Objects(targetCharId);

                            var taiwu = DomainManager.Taiwu.GetTaiwu();
                            var targetHasAdoredWithTaiwu = HasRelation(targetChar.GetId(), taiwu.GetId(), 16384) || HasRelation(taiwu.GetId(), targetChar.GetId(), 16384) || HasRelation(taiwu.GetId(), targetChar.GetId(), 1024);
                            var selfHasAdoredWithTaiwu = targetCharId != taiwu.GetId() && HasRelation(__instance.GetId(), taiwu.GetId(), 16384) || HasRelation(taiwu.GetId(), __instance.GetId(), 16384) || HasRelation(taiwu.GetId(), __instance.GetId(), 1024);
                            if (targetHasAdoredWithTaiwu || selfHasAdoredWithTaiwu)
                                character = null;
                            else
                            {
                                RelatedCharacter relation = DomainManager.Character.GetRelation(__instance.GetId(), targetCharId);
                                sbyte sectFavorability = DomainManager.Organization.GetSectFavorability(__instance.GetOrganizationInfo().OrgTemplateId, targetChar.GetOrganizationInfo().OrgTemplateId);
                                int triggerRate = Relation.GetStartOrEndRelationChance(relationCfg, __instance, targetChar, relation.RelationType, sectFavorability, multiplier);
                                character = (random.CheckProb(triggerRate, 10000) ? targetChar : null);
                            }
                        }
                    }
                    __result = character;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CombatSkillDomain), nameof(CombatSkillDomain.GetCharCombatSkills))]
        public class GetCharCombatSkillsPatch
        {
            public static void Postfix(int charId, ref Dictionary<short, GameData.Domains.CombatSkill.CombatSkill> __result)
            {
                if (GetBoolSettings("FixCalcCombatPowerRedCode"))
                {
                    var stack = new StackTrace();
                    if (stack.GetFrames().Exist(f => f.GetMethod().Name == "CalcCombatPower"))
                    {
                        foreach (var skill in DomainManager.Character.GetElement_Objects(charId).GetEquippedCombatSkills())
                        {
                            if (!__result.ContainsKey(skill))
                            {
                                __result[skill] = __result.First().Value;
                            }
                        }
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(CharacterDomain), "HasRelation")]
        //public class HasRelationPatch
        //{
        //    public static void Postfix(ref bool __result, int charId, int relatedCharId, ushort targetRelationType)
        //    {
        //        if (charId == DomainManager.Taiwu.GetTaiwuCharId() || relatedCharId == DomainManager.Taiwu.GetTaiwuCharId())
        //        {
        //            if (targetRelationType == 512)
        //                __result = false;
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(RelationType), "AllowAddingHusbandOrWifeRelation")]
        //class RelationType_Patch
        //{
        //    static void Postfix(ref bool __result, int charId, int relatedCharId)
        //    {
        //        if (charId == DomainManager.Taiwu.GetTaiwuCharId() || relatedCharId == DomainManager.Taiwu.GetTaiwuCharId())
        //            __result = true;
        //    }
        //}

        //[HarmonyPatch(typeof(RelationType), "AllowAddingAdoredRelation")]
        //class AllowAddingAdoredRelationPatch
        //{
        //    static void Postfix(ref bool __result, int charId, int relatedCharId)
        //    {
        //        if (charId == DomainManager.Taiwu.GetTaiwuCharId() || relatedCharId == DomainManager.Taiwu.GetTaiwuCharId())
        //            __result = true;
        //    }
        //}
    }
}
    