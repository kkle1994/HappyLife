using Config;
using Config.Common;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.CombatSkill;
using GameData.Domains.Global;
using GameData.Domains.Map;
using GameData.Domains.Organization;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using GameData.Utilities;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Utils;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.CanBuild))]
        public class CanBuildPatch
        {
            public static void Postfix(ref bool __result, short buildingTemplateId)
            {
                if (!GetBoolSettings("EnableBuildResource"))
                    return;
                BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[buildingTemplateId];
                if (buildingBlockItem.Class == EBuildingBlockClass.BornResource)
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(WorldDomain), "PreAdvanceMonth")]
        public class PreAdvanceMonthPatch
        {
            public unsafe static void Postfix(DataContext context)
            {
                if (GetBoolSettings("GrowAbilitiesWithLoopingNeigong"))
                {
                    var taiwu = DomainManager.Taiwu.GetTaiwu();
                    var neigong = DomainManager.Taiwu.GetTaiwu().GetLoopingNeigong();
                    var attributes = taiwu.GetBaseMainAttributes();
                    sbyte index = 0;
                    switch (neigong)
                    {
                        case 38:
                            index = 0;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                        case 85:
                            index = 1;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                        case 3:
                            index = 2;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                        case 64:
                            index = 3;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                        case 50:
                            index = 4;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                        case 42:
                            index = 5;
                            if (attributes.Items[index] < short.MaxValue)
                                taiwu.ChangeBaseMainAttribute(context, index, 1);
                            break;
                    }
                    //var template = DomainManager.CombatSkill.GetCharCombatSkills(taiwu.GetId())[neigong];
                    //for(short index = 0; index < 6; index++)
                    //{
                    //    if(template.GetCharPropertyBonus(index) > 0)
                    //        taiwu.ChangeBaseMainAttribute(context, (sbyte)index, 1);
                    //}
                    //var addProperties = CombatSkillDomain.EquipAddPropertyDict[neigong];
                    //var attributes = taiwu.GetBaseMainAttributes();
                    //if (addProperties != null)
                    //    for (sbyte index = 0; index < addProperties.Length; index++)
                    //    {
                    //        if (addProperties[index] > 0 && attributes.Items[index] < short.MaxValue)

                    //    }
                }
                if (GetBoolSettings("GrowQualificationsWithReadingBook"))
                {
                    var book = DomainManager.Taiwu.GetCurReadingBook();

                    var bookTemplate = SkillBook.Instance.GetItem(book.TemplateId);
                    if (bookTemplate == null)
                        return;
                    if (bookTemplate.Grade != 0)
                        return;
                    ECharacterPropertyReferencedType type = ECharacterPropertyReferencedType.Strength;
                    if (bookTemplate.CombatSkillType != -1)
                    {
                        type = (ECharacterPropertyReferencedType)(66 + bookTemplate.CombatSkillType);
                    }

                    if (bookTemplate.LifeSkillType != -1)
                    {
                        type = (ECharacterPropertyReferencedType)(34 + bookTemplate.LifeSkillType);
                    }
                    DomainManager.Taiwu.GetTaiwu().ModifyBasePropertyValue(context, type, 1);
                }
            }
        }

        //[HarmonyPatch(typeof(WorldDomain), "PreAdvanceMonth")]
        //public class PreAdvanceMonthPatch
        //{
        //    public unsafe static void Postfix(DataContext context)
        //    {
        //        if (GetIntSettings("ImproveSkillWithWorkPossibility") == 0)
        //            return;
        //        var workItems = DomainManager.Taiwu.GetType().GetField("_villagerWork", BindingFlags.Instance | BindingFlags.NonPublic)
        //            .GetValue(DomainManager.Taiwu) as Dictionary<int, VillagerWorkData>;

        //        var random = new Random();

        //        foreach (var workItem in workItems)
        //        {
        //            var workItemData = workItem.Value;
        //            var buildingTemplate = BuildingBlock.Instance[workItemData.BlockTemplateId];
        //            if (buildingTemplate.RequireLifeSkillType != -1)
        //            {
        //                if (random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
        //                {
        //                    var character =  DomainManager.Character.GetElement_Objects(workItemData.CharacterId);
        //                    var lifeSkills = character.GetBaseLifeSkillQualifications();

        //                    if (GetIntSettings("ImproveSkillWithWorkLimited") == 0 || lifeSkills.Items[buildingTemplate.RequireLifeSkillType] <= GetIntSettings("ImproveSkillWithWorkLimited"))
        //                    {
        //                        lifeSkills.Items[buildingTemplate.RequireLifeSkillType] = 120;
        //                        character.SetBaseLifeSkillQualifications(ref lifeSkills, context);
        //                    }
        //                }
        //            }
        //            if (buildingTemplate.RequireCombatSkillType != -1)
        //            {
        //                if (random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
        //                {
        //                    var character = DomainManager.Character.GetElement_Objects(workItemData.CharacterId);
        //                    var combatSkills = character.GetBaseCombatSkillQualifications();

        //                    var newData = new short[14];
        //                    for (var index = 0; index < 14; index++)
        //                    {
        //                        newData[index] = combatSkills.Items[index];
        //                    }

        //                    var skillId = random.Next(0, 14);
        //                    newData[skillId] = (short)(newData[skillId] + 1);
        //                    var newCombatSkill = new CombatSkillShorts(newData);
        //                    if (GetIntSettings("ImproveSkillWithWorkLimited") == 0 || newData[skillId] <= GetIntSettings("ImproveSkillWithWorkLimited"))
        //                    {
        //                        character.SetBaseCombatSkillQualifications(ref newCombatSkill, context);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(BuildingDomain), "ParallelUpdate")]
        public class ParallelUpdatePatch
        {
            public unsafe static bool Prefix(BuildingDomain __instance, DataContext context)
            {
                if (GetIntSettings("ImproveSkillWithWorkPossibility") == 0)
                    return true;

                var shopManagerDict = __instance.GetType().GetField("_shopManagerDict", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(__instance) as Dictionary<BuildingBlockKey, CharacterList>;
                var random = new Random();
                foreach (var dataPair in shopManagerDict)
                {
                    List<int> charList = dataPair.Value.GetCollection();
                    var configData = DomainManager.Building.GetBuildingBlockData(dataPair.Key);

                    foreach (var characterId in charList)
                    {
                        if (DomainManager.Character.TryGetElement_Objects(characterId, out GameData.Domains.Character.Character character))
                        {
                            if (BuildingBlock.Instance.Count <= configData.TemplateId || configData.TemplateId < 0)
                                continue;
                            var buildingTemplate = BuildingBlock.Instance[configData.TemplateId];
                            if (buildingTemplate.RequireLifeSkillType != -1)
                            {
                                if (random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
                                {
                                    var lifeSkills = character.GetBaseLifeSkillQualifications();


                                    if (lifeSkills.Items[buildingTemplate.RequireLifeSkillType] <= GetIntSettings("ImproveSkillWithWorkLimited"))
                                    {
                                        lifeSkills.Items[buildingTemplate.RequireLifeSkillType] += 1;
                                        character.SetBaseLifeSkillQualifications(ref lifeSkills, context);
                                    }
                                }
                            }
                            if (buildingTemplate.RequireCombatSkillType != -1)
                            {
                                if (random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
                                {
                                    var combatSkills = character.GetBaseCombatSkillQualifications();


                                    var skillId = random.Next(0, 14);
                                    if (combatSkills.Items[skillId] <= GetIntSettings("ImproveSkillWithWorkLimited"))
                                    {
                                        combatSkills.Items[skillId] += 1;
                                        character.SetBaseCombatSkillQualifications(ref combatSkills, context);
                                    }
                                }
                            }
                        }
                    }
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(OrganizationDomain), nameof(OrganizationDomain.GetCharacterTemplateId))]
        public class GetCharacterTemplateIdPatch
        {
            public unsafe static bool Prefix(ref sbyte gender, sbyte orgTemplateId)
            {
                if (GetIntSettings("WorldCreatePeopleSex") != 0)
                {
                    gender = GetIntSettings("WorldCreatePeopleSex") == 1 ? (sbyte)1 : (sbyte)0;
                }
                if (GetIntSettings("VillagerRecrultSex") != 0)
                {
                    var stack = new StackTrace();
                    if (stack.GetFrames().Exist(f => f.GetMethod().Name == "RecruitPeople"))
                        gender = GetIntSettings("VillagerRecrultSex") == 1 ? (sbyte)1 : (sbyte)0;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameData.Domains.Character.Character), "OfflineCreateIntelligentCharacter")]
        public class OfflineCreateIntelligentCharacterPatch
        {
            public static int MaxAvatarRetryTimes = 10;
            public static bool Prefix(ref GameData.Domains.Character.Character __instance, DataContext context, ref IntelligentCharacterCreationInfo info)
            {
                if (GetIntSettings("VillagerRecrultAppearanceLowerLimit") == 0)
                    return true;
                else
                {
                    var stack = new StackTrace();
                    if (stack.GetFrames().Exist(f => f.GetMethod().Name == "RecruitPeople"))
                        info.BaseAttraction = (short)(200 + GetIntSettings("VillagerRecrultAppearanceLowerLimit") * 100);
                    return true;
                }
            }

            public static void Postfix(ref GameData.Domains.Character.Character __instance, DataContext context, IntelligentCharacterCreationInfo info)
            {

                if (GetIntSettings("VillagerRecrultAppearanceLowerLimit") != 0)
                {
                    var stack = new StackTrace();
                    if (stack.GetFrames().Exist(f => f.GetMethod().Name == "RecruitPeople"))
                    {
                        var retryTimes = 0;
                        var characterItem = Config.Character.Instance[info.CharTemplateId];
                        while ((short)(200 + GetIntSettings("VillagerRecrultAppearanceLowerLimit") * 100) > __instance.GetAvatar().BaseCharm && retryTimes < MaxAvatarRetryTimes)
                        {
                            __instance.GetType().GetMethod("OfflineCreateAttractionAndAvatar", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[] { context, characterItem.PresetBodyType, info });
                            retryTimes++;
                        }
                    }
                }


                if (GetIntSettings("VillagerRecrultMorality") == 0)
                    return;
                else
                {
                    var stack = new StackTrace();
                    if (stack.GetFrames().Exist(f => f.GetMethod().Name == "RecruitPeople"))
                    {
                        __instance.GetType().GetField("_baseMorality", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (short)(200 * GetIntSettings("VillagerRecrultMorality") - 100 - 500));
                    }
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(WorldDomain), "OnLoadWorld")]
        public class BuildingPatch
        {
            public static string PatchFile = "..\\Mod\\HappyLife\\Datas\\BuildingDataPatch.csv";
            public static void Postfix()
            {
                //if (GetBoolSettings("EnableBuildResource"))
                //{
                //    var _dataArray = BuildingBlock.Instance.GetType().GetField("_dataArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BuildingBlock.Instance) as List<BuildingBlockItem>;
                //    for(var index = 0; index< _dataArray.Count; index++)
                //    {
                //        var buildItem = _dataArray[index];
                //        if (buildItem.Class == EBuildingBlockClass.BornResource)
                //        {
                //            for (var resourceIndex = 0; resourceIndex < 8; resourceIndex++)
                //            {
                //                buildItem.BaseBuildCost[resourceIndex] *= 20;
                //            }


                //            if (GetBoolSettings("NoDependentBuilding"))
                //            {
                //                buildItem.DependBuildings.RemoveAll(s => s > 20);
                //            }
                //        }
                //        _dataArray[index] = buildItem;
                //    }
                //}
                if (GetBoolSettings("RestoreAttainmentPerGrade"))
                {
                    GlobalConfig.Instance.AddAttainmentPerGrade = new sbyte[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 };
                }

                if (!GetBoolSettings("EnableBuildResource"))
                    return;
                if (File.Exists(PatchFile))
                {
                    var lines = File.ReadAllLines(PatchFile);

                    if (lines.Length != 0)
                    {

                        var headers = lines[0].Split(',');

                        for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
                        {
                            var line = lines[lineIndex];
                            var parts = line.Split(',');

                            var id = short.Parse(parts[0]);
                            BuildingBlockItem originalBlockItem = BuildingBlock.Instance[id];
                            if (id >= 1 && id <= 20)
                            {
                                //typeof(BuildingBlockItem).GetField(nameof(BuildingBlockItem.Class), BindingFlags.Instance | BindingFlags.Public).SetValue(originalBlockItem, EBuildingBlockClass.Resource);
                                typeof(BuildingBlockItem).GetField(nameof(BuildingBlockItem.AddBuildCostPerLevel), BindingFlags.Instance | BindingFlags.Public).SetValue(originalBlockItem, (byte)50);
                            }

                            var _dataArray = BuildingBlock.Instance.GetType().GetField("_dataArray", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(BuildingBlock.Instance) as List<BuildingBlockItem>;

                            for (var resourceIndex = 0; resourceIndex < 8; resourceIndex++)
                            {
                                originalBlockItem.BaseBuildCost[resourceIndex] = ushort.Parse(parts[resourceIndex + 2]);
                            }

                            // Add base workload
                            originalBlockItem.OperationTotalProgress[0] = 150;
                            originalBlockItem.OperationTotalProgress[1] = 50;
                            originalBlockItem.OperationTotalProgress[2] = 100;
                            if (GetBoolSettings("NoDependentBuilding"))
                            {
                                originalBlockItem.DependBuildings.RemoveAll(s => s > 20);
                            }
                            _dataArray[id] = originalBlockItem;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.CanUpgrade))]
        public class CanUpgradePatch
        {
            public unsafe static void Postfix(BuildingDomain __instance, BuildingBlockKey blockKey, ref bool __result)
            {
                if (GetIntSettings("EnableBuildResource") != 0)
                {
                    BuildingBlockData element_BuildingBlocks = __instance.GetElement_BuildingBlocks(blockKey);
                    BuildingBlockItem buildingBlockItem = BuildingBlock.Instance[element_BuildingBlocks.TemplateId];
                    if (element_BuildingBlocks.TemplateId >=1 && element_BuildingBlocks.TemplateId <= 20)
                    {
                        GameData.Domains.Character.Character taiwu = DomainManager.Taiwu.GetTaiwu();
                        for (sbyte b = 0; b < 8; b = (sbyte)(b + 1))
                        {
                            int num = buildingBlockItem.BaseBuildCost[b] * (100 + buildingBlockItem.AddBuildCostPerLevel * element_BuildingBlocks.Level) / 1000 * 10;
                            if (taiwu.GetResource(b) < num)
                            {
                                __result = false;
                            }
                        }
                    }
                    __result = true;
                }
            }
        }


        [HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.StartMakeItem))]
        public class StartMakeItemPatch
        {
            public static string PatchFile = "..\\Mod\\HappyLife\\Datas\\BuildingDataPatch.csv";
            public static void Postfix(ref MakeItemData __result)
            {
                if (GetBoolSettings("MakeItemImmediately"))
                {
                    __result.LeftTime = 0;
                }
            }
        }
    }
}
