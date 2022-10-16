using GameData.Common;
using GameData.Common.SingleValueCollection;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Relation;
using GameData.Domains.Global;
using GameData.Domains.SpecialEffect.CombatSkill.Emeipai.DefenseAndAssist;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(PregnantState), "CheckPregnant")]
        public class CheckPregnantPatch
        {
            public static void Postfix(ref bool __result, Character father, Character mother)
            {
                var _pregnantStates = DomainManager.Character.GetType().GetField("_pregnantStates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).
                    GetValue(DomainManager.Character) as Dictionary<int, PregnantState>;
                int value;
                if (father.GetGender() == mother.GetGender() && !GetBoolSettings("AllowTaiwuHomoPregnant"))
                {
                    return;
                }

                if (_pregnantStates.ContainsKey(mother.GetId())
                    || _pregnantStates.ContainsKey(father.GetId())
                    || DomainManager.Character.TryGetElement_PregnancyLockEndDates(mother.GetId(), out value)
                    || DomainManager.Character.TryGetElement_PregnancyLockEndDates(father.GetId(), out value))
                {
                    __result = false;
                    return;
                }


                if ((father.IsTaiwu() || mother.IsTaiwu()) && !mother.GetFeatureIds().Contains(197)
                    && !_pregnantStates.ContainsKey(mother.GetId())
                    && !DomainManager.Character.TryGetElement_PregnancyLockEndDates(mother.GetId(), out value) && GetBoolSettings("TaiwuMustPregnant"))
                {
                    __result = true;
                }

                if (GetIntSettings("TaiwuChildrenLimit") != -1 && (father.IsTaiwu() || mother.IsTaiwu()))
                {
                    var taiwu = father.IsTaiwu() ? father : mother;
                    var countOfChildren = DomainManager.Character.GetRelatedCharacters(taiwu.GetId()).BloodChildren.GetCount();
                    if (countOfChildren >= GetIntSettings("TaiwuChildrenLimit"))
                        __result = false;
                }
                else if (GetIntSettings("NpcChildrenLimit") != -1 && !father.IsTaiwu() && !mother.IsTaiwu())
                {
                    if (GetIntSettings("NpcChildrenLimit") >= DomainManager.Character.GetRelatedCharacters(father.GetId()).BloodChildren.GetCount() ||
                        GetIntSettings("NpcChildrenLimit") >= DomainManager.Character.GetRelatedCharacters(mother.GetId()).BloodChildren.GetCount())
                        __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Character), "OfflineMakeLove")]
        public class OfflineMakeLovePatch
        {
            public static bool Prefix(ref bool __result, IRandomSource random, Character father, Character mother, bool isRape)
            {
                if (!father.IsTaiwu() && !mother.IsTaiwu())
                    return true;
                var offlineAddFeatureMethod = typeof(Character).GetMethod("OfflineAddFeature", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (mother.GetGender() == father.GetGender() && !GetBoolSettings("AllowTaiwuHomoPregnant"))
                {
                    __result = false;
                    return false;
                }

                if (!GetBoolSettings("KeepVirgin"))
                {
                    offlineAddFeatureMethod.Invoke(mother, new object[] { (short)196, true });
                    offlineAddFeatureMethod.Invoke(father, new object[] { (short)196, true });
                }
                else
                {
                    offlineAddFeatureMethod.Invoke(mother, new object[] { (short)195, true });
                    offlineAddFeatureMethod.Invoke(father, new object[] { (short)195, true });
                }
                if (!PregnantState.CheckPregnant(random, father, mother, isRape))
                {
                    __result = false;
                    return false;
                }

                offlineAddFeatureMethod.Invoke(mother, new object[] { (short)197, true });
                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), "AddElement_PregnancyLockEndDates")]
        public class AddElement_PregnancyLockEndDatesPatch
        {
            public static bool Prefix(CharacterDomain __instance, int elementId, DataContext context)
            {
                // Fix pregnant red code issue
                if (__instance.TryGetElement_PregnancyLockEndDates(elementId, out int value))
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.GetAliveSpouse))]
        public class GetAliveSpousePatch
        {
            public static void Postfix(CharacterDomain __instance, ref int __result)
            {
                var spouseId = -1;
                var _relatedCharIds = __instance.GetType().GetField("_relatedCharIds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance) as Dictionary<int, RelatedCharacters>;
                if (!_relatedCharIds.TryGetValue(__result, out var value))
                {
                    spouseId = -1;
                }
                else
                {
                    HashSet<int> collection = value.HusbandsAndWives.GetCollection();
                    foreach (int item in collection)
                    {
                        if (__instance.IsCharacterAlive(item))
                        {
                            spouseId = item;
                        }
                    }
                }

                if (spouseId == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    var trace = new StackTrace();
                    if (trace.GetFrames().Exist(f => f.GetMethod().Name == "ParallelCreateNewbornChildren"))
                        __result = -1;
                }
            }
        }


        //[HarmonyPatch(typeof(CharacterDomain), "AddElement_PregnantStates")]
        //public class AddElement_PregnantStatesPatch
        //{
        //    public static bool Prefix(CharacterDomain __instance, int elementId)
        //    {
        //        //var _pregnantStates = __instance.GetType().GetField("_pregnantStates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        //        //    .GetValue(__instance) as Dictionary<int, PregnantState>;
        //        //if (_pregnantStates.ContainsKey(elementId))
        //        //{
        //        //    var info = _pregnantStates[elementId];
        //        //    // Fix pregnant red code issue
        //        //    if (info != null)
        //        //        return false;
        //        //}
        //        return true;
        //    }
        //}
    }
}
