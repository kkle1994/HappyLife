using BehTree;
using GameData.Common;
using GameData.Common.SingleValueCollection;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Relation;
using GameData.Domains.Global;
using GameData.Domains.Map;
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
using System.Xml.Linq;

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
                if(GetIntSettings("TaiwuSpouseChildrenLimit") != -1 && (father.IsTaiwu() || mother.IsTaiwu()))
                {
                    var spouse = father.IsTaiwu() ?  mother : father;
                    var countOfChildren = DomainManager.Character.GetRelatedCharacters(spouse.GetId()).BloodChildren.GetCount();
                    if (countOfChildren >= GetIntSettings("TaiwuSpouseChildrenLimit"))
                        __result = false;
                }
                else if (GetIntSettings("NpcChildrenLimit") != -1 && !father.IsTaiwu() && !mother.IsTaiwu())
                {
                    if (GetIntSettings("NpcChildrenLimit") < DomainManager.Character.GetRelatedCharacters(father.GetId()).BloodChildren.GetCount() ||
                        GetIntSettings("NpcChildrenLimit") < DomainManager.Character.GetRelatedCharacters(mother.GetId()).BloodChildren.GetCount())
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

                if (!GetBoolSettings("AllowTaiwuHomoPregnant") && !GetBoolSettings("KeepVirgin"))
                    return true;

                var offlineAddFeatureMethod = typeof(Character).GetMethod("OfflineAddFeature", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (mother.GetGender() == father.GetGender() && !GetBoolSettings("AllowTaiwuHomoPregnant"))
                {
                    __result = false;
                    return false;
                }

                if (!GetBoolSettings("KeepVirgin"))
                {
                    offlineAddFeatureMethod.Invoke(mother, new object[] { (short)196, true, true });
                    offlineAddFeatureMethod.Invoke(father, new object[] { (short)196, true, true });
                }
                else
                {
                    offlineAddFeatureMethod.Invoke(mother, new object[] { (short)195, true, true });
                    offlineAddFeatureMethod.Invoke(father, new object[] { (short)195, true, true });
                }
                if (!PregnantState.CheckPregnant(random, father, mother, isRape))
                {
                    __result = false;
                    return false;
                }

                offlineAddFeatureMethod.Invoke(mother, new object[] { (short)197, true, true });
                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), "AddElement_PregnancyLockEndDates")]
        public class AddElement_PregnancyLockEndDatesPatch
        {
            public static bool Prefix(CharacterDomain __instance, int elementId, ref int value, DataContext context)
            {
                if (GetBoolSettings("NoTaiwuPregnancyLock") &&
                    (DomainManager.Taiwu.GetTaiwuCharId() == elementId || DomainManager.Character.GetAliveSpouse(elementId) == DomainManager.Taiwu.GetTaiwuCharId()))
                    value = 1;

                // Fix pregnant red code issue
                if (__instance.TryGetElement_PregnancyLockEndDates(elementId, out int outValue))
                {
                    return false;
                }
                return true;
            }
        }

        
        [HarmonyPatch(typeof(CharacterDomain), "GetElement_PregnantStates")]
        public class GetElement_PregnantStatesPatch
        {
            public static void Postfix(ref PregnantState __result)
            {
                if (GetBoolSettings("OnlyExistBloodParents") && (DomainManager.Taiwu.GetTaiwuCharId() == __result.FatherId))
                {
                    __result.CreateFatherRelation = true;
                }
            }
        }


        [HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.AddHusbandOrWifeRelations))]
        public class AddHusbandOrWifeRelationsPatch
        {
            public static bool Prefix(CharacterDomain __instance, DataContext context, int charId, int spouseCharId, int establishmentDate)
            {
                if (GetBoolSettings("BanAddStepChildren") && (DomainManager.Taiwu.GetTaiwuCharId() == charId || DomainManager.Taiwu.GetTaiwuCharId() == spouseCharId))
                {
                    __instance.AddRelation(context, charId, spouseCharId, 1024, establishmentDate);
                    return false;
                }
                return true;
            }
        }
        //[HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.GetAliveSpouse))]
        //public class GetAliveSpousePatch
        //{
        //    public static void Postfix(CharacterDomain __instance, ref int __result)
        //    {
        //        var spouseId = -1;
        //        var _relatedCharIds = __instance.GetType().GetField("_relatedCharIds", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(__instance) as Dictionary<int, RelatedCharacters>;
        //        if (!_relatedCharIds.TryGetValue(__result, out var value))
        //        {
        //            spouseId = -1;
        //        }
        //        else
        //        {
        //            HashSet<int> collection = value.HusbandsAndWives.GetCollection();
        //            foreach (int item in collection)
        //            {
        //                if (__instance.IsCharacterAlive(item))
        //                {
        //                    spouseId = item;
        //                }
        //            }
        //        }

        //        if (spouseId == DomainManager.Taiwu.GetTaiwuCharId())
        //        {
        //            var trace = new StackTrace();
        //            if (trace.GetFrames().Exist(f => f.GetMethod().Name == "ParallelCreateNewbornChildren"))
        //                __result = -1;
        //        }
        //    }
        //}


        //[HarmonyPatch(typeof(CharacterDomain), "GenerateTemplateId")]
        //public class GenerateTemplateIdPatch
        //{
        //    public static bool Prefix(CharacterDomain __instance, ref sbyte gender)
        //    {
        //        if (GetIntSettings("TaiwuChildGender") != 0)
        //        {
        //            var stack = new StackTrace();
        //            var frame = stack.GetFrames().ToList().Find(f => f.GetMethod().Name == "ParallelCreateNewbornChildren");
        //            if(frame != null)
        //            {

        //            }

        //        }
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(CharacterDomain), "ParallelCreateIntelligentCharacter")]
        public class ParallelCreateIntelligentCharacterPatch
        {
            private static (short baseAttraction, AvatarData avatar) GenerateMainChildAvatar(IRandomSource random, Character mother, Character father, DeadCharacter deadFather, sbyte mainChildGender)
            {
                short item = -1;
                AvatarData avatarData = null;
                AvatarData avatar = mother?.GetAvatar() ?? null;
                AvatarData avatarData2 = father?.GetAvatar() ?? deadFather?.Avatar ?? null;
                if (avatar != null || avatarData2 != null)
                {
                    avatarData = AvatarManager.Instance.GetRandomAvatar(random, mainChildGender, transgender: false, -1, avatarData2, avatar);
                    item = avatarData.GetBaseCharm();
                }

                return (item, avatarData);
            }
            public static bool Prefix(CharacterDomain __instance, DataContext context, ref IntelligentCharacterCreationInfo info, bool recordModification = true)
            {
                if (GetBoolSettings("OnlyExistBloodParents") && info.PregnantState != null && (DomainManager.Taiwu.GetTaiwuCharId() == info.PregnantState.FatherId))
                {
                    info.Father = DomainManager.Taiwu.GetTaiwu();
                    info.FatherCharId = info.PregnantState.FatherId;
                    info.PregnantState.CreateFatherRelation = true;
                }

                IRandomSource random = context.Random;
                if (GetIntSettings("TaiwuChildGender") != 0 &&
                    ((info.Father != null && info.Father.IsTaiwu()) || (info.Mother!=null && info.Mother.IsTaiwu())))
                {
                    if (info.CharTemplateId %2 == 1 && GetIntSettings("TaiwuChildGender") == 2)
                    {
                        (short baseAttraction, AvatarData avatar) tuple2 = GenerateMainChildAvatar(random, info.Mother, info.Father, info.DeadFather, 0);
                        IntelligentCharacterCreationInfo intelligentCharacterCreationInfo = new IntelligentCharacterCreationInfo(info.Location, info.OrgInfo, (short)(info.CharTemplateId - 1));
                        intelligentCharacterCreationInfo.GrowingSectGrade = info.GrowingSectGrade;
                        intelligentCharacterCreationInfo.MotherCharId = info.MotherCharId;
                        intelligentCharacterCreationInfo.Mother = info.Mother;
                        intelligentCharacterCreationInfo.PregnantState = info.PregnantState;
                        intelligentCharacterCreationInfo.FatherCharId = info.FatherCharId;
                        intelligentCharacterCreationInfo.Father = info.Father;
                        intelligentCharacterCreationInfo.DeadFather = info.DeadFather;
                        intelligentCharacterCreationInfo.MultipleBirthCount = info.MultipleBirthCount;
                        intelligentCharacterCreationInfo.Age = info.Age;
                        intelligentCharacterCreationInfo.BirthMonth = info.BirthMonth;
                        intelligentCharacterCreationInfo.BaseAttraction = info.BaseAttraction;
                        intelligentCharacterCreationInfo.Avatar = tuple2.avatar;
                        intelligentCharacterCreationInfo.ReincarnationCharId = info.ReincarnationCharId;
                        intelligentCharacterCreationInfo.DestinyType = info.DestinyType;
                        info = intelligentCharacterCreationInfo;
                    }
                    else if(info.CharTemplateId % 2 == 0 && GetIntSettings("TaiwuChildGender") == 1)
                    {
                        (short baseAttraction, AvatarData avatar) tuple2 = GenerateMainChildAvatar(random, info.Mother, info.Father, info.DeadFather, 1);
                        IntelligentCharacterCreationInfo intelligentCharacterCreationInfo = new IntelligentCharacterCreationInfo(info.Location, info.OrgInfo, (short)(info.CharTemplateId + 1));
                        intelligentCharacterCreationInfo.GrowingSectGrade = info.GrowingSectGrade;
                        intelligentCharacterCreationInfo.MotherCharId = info.MotherCharId;
                        intelligentCharacterCreationInfo.Mother = info.Mother;
                        intelligentCharacterCreationInfo.PregnantState = info.PregnantState;
                        intelligentCharacterCreationInfo.FatherCharId = info.FatherCharId;
                        intelligentCharacterCreationInfo.Father = info.Father;
                        intelligentCharacterCreationInfo.DeadFather = info.DeadFather;
                        intelligentCharacterCreationInfo.MultipleBirthCount = info.MultipleBirthCount;
                        intelligentCharacterCreationInfo.Age = info.Age;
                        intelligentCharacterCreationInfo.BirthMonth = info.BirthMonth;
                        intelligentCharacterCreationInfo.BaseAttraction = info.BaseAttraction;
                        intelligentCharacterCreationInfo.Avatar = tuple2.avatar; ;
                        intelligentCharacterCreationInfo.ReincarnationCharId = info.ReincarnationCharId;
                        intelligentCharacterCreationInfo.DestinyType = info.DestinyType;
                        info = intelligentCharacterCreationInfo;
                    }
                }
                else if (GetIntSettings("WorldChildGender") != 0 && !info.Father.IsTaiwu() && !info.Mother.IsTaiwu())
                {
                    if (info.CharTemplateId % 2 == 1 && GetIntSettings("WorldChildGender") == 2)
                    {
                        (short baseAttraction, AvatarData avatar) tuple2 = GenerateMainChildAvatar(random, info.Mother, info.Father, info.DeadFather, 0);
                        IntelligentCharacterCreationInfo intelligentCharacterCreationInfo = new IntelligentCharacterCreationInfo(info.Location, info.OrgInfo, (short)(info.CharTemplateId - 1));
                        intelligentCharacterCreationInfo.GrowingSectGrade = info.GrowingSectGrade;
                        intelligentCharacterCreationInfo.MotherCharId = info.MotherCharId;
                        intelligentCharacterCreationInfo.Mother = info.Mother;
                        intelligentCharacterCreationInfo.PregnantState = info.PregnantState;
                        intelligentCharacterCreationInfo.FatherCharId = info.FatherCharId;
                        intelligentCharacterCreationInfo.Father = info.Father;
                        intelligentCharacterCreationInfo.DeadFather = info.DeadFather;
                        intelligentCharacterCreationInfo.MultipleBirthCount = info.MultipleBirthCount;
                        intelligentCharacterCreationInfo.Age = info.Age;
                        intelligentCharacterCreationInfo.BirthMonth = info.BirthMonth;
                        intelligentCharacterCreationInfo.BaseAttraction = info.BaseAttraction;
                        intelligentCharacterCreationInfo.Avatar = tuple2.avatar;
                        intelligentCharacterCreationInfo.ReincarnationCharId = info.ReincarnationCharId;
                        intelligentCharacterCreationInfo.DestinyType = info.DestinyType;
                        info = intelligentCharacterCreationInfo;
                    }
                    else if (info.CharTemplateId % 2 == 0 && GetIntSettings("WorldChildGender") == 1)
                    {
                        (short baseAttraction, AvatarData avatar) tuple2 = GenerateMainChildAvatar(random, info.Mother, info.Father, info.DeadFather, 1);
                        IntelligentCharacterCreationInfo intelligentCharacterCreationInfo = new IntelligentCharacterCreationInfo(info.Location, info.OrgInfo, (short)(info.CharTemplateId + 1));
                        intelligentCharacterCreationInfo.GrowingSectGrade = info.GrowingSectGrade;
                        intelligentCharacterCreationInfo.MotherCharId = info.MotherCharId;
                        intelligentCharacterCreationInfo.Mother = info.Mother;
                        intelligentCharacterCreationInfo.PregnantState = info.PregnantState;
                        intelligentCharacterCreationInfo.FatherCharId = info.FatherCharId;
                        intelligentCharacterCreationInfo.Father = info.Father;
                        intelligentCharacterCreationInfo.DeadFather = info.DeadFather;
                        intelligentCharacterCreationInfo.MultipleBirthCount = info.MultipleBirthCount;
                        intelligentCharacterCreationInfo.Age = info.Age;
                        intelligentCharacterCreationInfo.BirthMonth = info.BirthMonth;
                        intelligentCharacterCreationInfo.BaseAttraction = info.BaseAttraction;
                        intelligentCharacterCreationInfo.Avatar = tuple2.avatar;
                        intelligentCharacterCreationInfo.ReincarnationCharId = info.ReincarnationCharId;
                        intelligentCharacterCreationInfo.DestinyType = info.DestinyType;
                        info = intelligentCharacterCreationInfo;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.CreateIntelligentCharacter))]
        public class CreateIntelligentCharacterPatch
        {
            public static void Postfix(CharacterDomain __instance,ref DataContext context, ref IntelligentCharacterCreationInfo info, ref Character __result)
            {
                if (GetBoolSettings("OnlyExistBloodParents") && info.PregnantState != null && info.PregnantState.FatherId == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    if (!__instance.GetRelatedCharacters(__result.GetId()).BloodParents.Contains(DomainManager.Taiwu.GetTaiwuCharId()))
                    {
                        __instance.AddBloodParentRelations(context, __result.GetId(), DomainManager.Taiwu.GetTaiwuCharId());
                    }
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
