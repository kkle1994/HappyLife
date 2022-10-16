using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Global;
using GameData.Domains.Organization;
using GameData.Domains.Taiwu;
using GameData.Domains.World;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                if (GetIntSettings("ImproveSkillWithWorkPossibility") == 0)
                    return;
                var workItems = DomainManager.Building.GetType().GetField("_villagerWork", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(DomainManager.Building) as Dictionary<int, VillagerWorkData>;

                var random = new Random();

                foreach (var workItem in workItems)
                {
                    var workItemData = workItem.Value;
                    var buildingTemplate = BuildingBlock.Instance[workItemData.BlockTemplateId];
                    if(buildingTemplate.RequireLifeSkillType != -1)
                    {
                        if(random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
                        {
                            var character = DomainManager.Character.GetElement_Objects(workItemData.CharacterId);
                            var lifeSkills = character.GetBaseLifeSkillQualifications();
                            if (GetIntSettings("ImproveSkillWithWorkLimited") == 0 || lifeSkills.Items[buildingTemplate.RequireLifeSkillType] <= GetIntSettings("ImproveSkillWithWorkLimited"))
                            {
                                lifeSkills.Items[buildingTemplate.RequireLifeSkillType]++;
                                character.SetBaseLifeSkillQualifications(ref lifeSkills, context);
                            }
                        }
                    }
                    if (buildingTemplate.RequireCombatSkillType != -1)
                    {
                        if (random.Next(0, 100) < GetIntSettings("ImproveSkillWithWorkPossibility"))
                        {
                            var character = DomainManager.Character.GetElement_Objects(workItemData.CharacterId);
                            var combatSkills = character.GetBaseCombatSkillQualifications();
                            var skillId = random.Next(0, 13);
                            combatSkills.Items[skillId]++;
                            if (GetIntSettings("ImproveSkillWithWorkLimited") == 0 || combatSkills.Items[skillId] <= GetIntSettings("ImproveSkillWithWorkLimited"))
                            {
                                character.SetBaseCombatSkillQualifications(ref combatSkills, context);
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(OrganizationDomain), nameof(OrganizationDomain.GetCharacterTemplateId))]
        public class GetCharacterTemplateIdPatch
        {
            public unsafe static bool Prefix(ref sbyte gender)
            {
                if (GetIntSettings("VillagerRecrultSex") == 0)
                    return true;
                else
                {
                    gender = GetIntSettings("VillagerRecrultSex") == 1 ? (sbyte)1 : (sbyte)0;
                    return true;
                    //var constants = typeof(Character.DefKey).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    //foreach (var constant in constants)
                    //{
                    //    var name = constant.Name;
                    //    var value = constant.GetValue(null);
                    //    if(name == GetStringSettings("VillagerRecrultTemplate") && Character.Instance[(short)value].CreatingType == 1)
                    //    {
                    //        __result = (short)value;
                    //    }
                    //}
                }
                //foreach(var organization in Organization.Instance)
                //{
                //    foreach(var templateId in organization.CharTemplateIds)
                //    {
                //        var template = Character.Instance[templateId];
                //    }
                //}
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
    }
}
