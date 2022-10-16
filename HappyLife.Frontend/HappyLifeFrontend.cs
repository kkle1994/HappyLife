using GameData.Domains.Character.AvatarSystem;
using GameData.Domains.Character;
using HarmonyLib;
using System.Reflection;
using TaiwuModdingLib.Core.Plugin;
using TaiwuModdingLib.Core.Utils;
using UnityEngine;
using Config;
using GameData.Domains.Merchant;
using System.Collections.Generic;
using GameData.Domains.World;
using GameData.GameDataBridge;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace HappyLife.Frontend
{

    [PluginConfig("HappyLife", "kkle1994@outlook.com", "1.0")]
    public class HappyLifeFrontend : TaiwuRemakeHarmonyPlugin
    {
        public override void Initialize()
        {
            this.HarmonyInstance.PatchAll(typeof(GetFavorabilityPatch));
            this.HarmonyInstance.PatchAll(typeof(GetCharmLevelTextPatch));
            this.HarmonyInstance.PatchAll(typeof(IsPageDisabledPatch));
            this.HarmonyInstance.PatchAll(typeof(IsPageShowPatch));
            this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            this.HarmonyInstance.PatchAll(typeof(UpdateHealthRecoverStatePatch));
            this.HarmonyInstance.PatchAll(typeof(BindGlobalEventsPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
        }
        public static bool GetBoolSettings(string field)
        {
            var result = false;
            foreach (var modId in ModManager.EnabledMods)
            {
                if (ModManager.GetSetting(modId.ToString(), field, ref result))
                    return result;
            }
            return false;
        }

        //[HarmonyPatch(typeof(UI_BuildingArea), "BlockCanBuild")]
        //public class BlockCanBuildPatch
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("UnlimitedDependBuilding"))
        //            __result = true;
        //    }
        //}

        //[HarmonyPatch(typeof(UI_BuildingArea), "NearDependBuildings")]
        //public class NearDependBuildingsPatch
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("UnlimitedDependBuilding"))
        //            __result = true;
        //    }
        //}

        //[HarmonyPatch(typeof(UI_BuildingArea), "RenderCanBuild")]
        //public class RenderCanBuildPatch
        //{
        //    public static void Postfix(ref bool __result)
        //    {
        //        if (GetBoolSettings("UnlimitedDependBuilding"))
        //            __result = true;
        //    }
        //}

        [HarmonyPatch(typeof(UI_Shop), "GetFavorability")]
        public class GetFavorabilityPatch
        {
            public static void Postfix(ref int __result)
            {
                if(GetBoolSettings("UnlimitedMerchantFavorability"))
                    __result = 5000;
            }
        }

        [HarmonyPatch(typeof(UI_RecordSelect), "OnEnable")]
        public class BindGlobalEventsPatch
        {
            public static string PatchFile = ".\\Mod\\HappyLife\\Datas\\BuildingDataPatch.csv";

            public static string CharacterTemplateIdsPatchFile = ".\\Mod\\HappyLife\\Datas\\CharacterTemplateIds.txt";
            public static void Postfix()
            {
                //if (GetBoolSettings("EnableBuildResource"))
                //{
                //    var _dataArray = BuildingBlock.Instance.GetType().GetField("_dataArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BuildingBlock.Instance) as List<BuildingBlockItem>;
                //    for (var index = 0; index < _dataArray.Count; index++)
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
                if (GetBoolSettings("EnableBuildResource"))
                {
                    if (!File.Exists(PatchFile))
                    {
                        var newLines = new List<string>();
                        newLines.Add("TemplateId,Name,Food,Wood,Metal,Jade,Fabric,Herb,Money,Authority");
                        foreach (var item in BuildingBlock.Instance)
                        {
                            var multiple = item.TemplateId >= 1 && item.TemplateId <= 20 ? 20 : 1;
                            newLines.Add($"{item.TemplateId}," +
                                $"{item.Name}," +
                                $"{item.BaseBuildCost[0] * multiple}," +
                                $"{item.BaseBuildCost[1] * multiple}," +
                                $"{item.BaseBuildCost[2] * multiple}," +
                                $"{item.BaseBuildCost[3] * multiple}," +
                                $"{item.BaseBuildCost[4] * multiple}," +
                                $"{item.BaseBuildCost[5] * multiple}," +
                                $"{item.BaseBuildCost[6] * multiple}," +
                                $"{item.BaseBuildCost[7] * multiple}"
                                );
                        }
                        Directory.CreateDirectory(Path.GetDirectoryName(PatchFile));
                        File.WriteAllLines(PatchFile, newLines);
                    }
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

                            var _dataArray = BuildingBlock.Instance.GetType().GetField("_dataArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BuildingBlock.Instance) as List<BuildingBlockItem>;

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



                    //var output = "";
                    //var constants = typeof(Character.DefKey).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    //foreach (var constant in constants)
                    //{
                    //    var name = constant.Name;
                    //    var value = constant.GetValue(null);
                    //    if (Character.Instance[(short)value].CreatingType == 1)
                    //        output += $"{name}\r\n";
                    //}
                    //File.WriteAllText(CharacterTemplateIdsPatchFile, output);

                }
            }
        }

        [HarmonyPatch(typeof(UI_BuildingOverview), "InitData")]
        public class BuildingInitDataPatch
        {
            public static void Postfix(UI_BuildingOverview __instance)
            {
                if (GetBoolSettings("EnableBuildResource"))
                {
                    var _buildingMap = __instance.GetType().GetField("_buildingMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as Dictionary<EBuildingBlockClass, List<BuildingBlockItem>>;
                    var _isHaveChickenKing = (bool)__instance.GetType().GetField("_isHaveChickenKing", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);



                    BuildingBlock.Instance.Iterate(delegate (BuildingBlockItem item)
                    {
                        if (item.Class == EBuildingBlockClass.BornResource)
                        {
                            if (!_buildingMap.TryGetValue(EBuildingBlockClass.Resource, out var value))
                            {
                                value = new List<BuildingBlockItem>();
                                _buildingMap.Add(EBuildingBlockClass.Resource, value);
                            }
                            // Skip useless buildings
                            if (item.TemplateId != 21 && item.TemplateId != 22 && item.TemplateId != 23)
                            {
                                value.Add(item);
                            }
                        }

                        return true;
                    });
                }


            }
        }

        //[HarmonyPatch(typeof(UI_BuildingArea), "CanExecuteOperation")]
        //public class CanExecuteOperationPatch
        //{
        //    public static void Postfix(UI_BuildingOverview __instance)
        //    {
        //        if (GetBoolSettings("EnableBuildResource"))
        //        {

        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(WorldDomainHelper), "InitGameRes")]
        //public class InitGameResPatch
        //{
        //    public static void Postfix(BuildingBlock __instance)
        //    {
        //        if (GetBoolSettings("EnableBuildResource"))
        //        {
        //            __instance.Iterate(delegate (BuildingBlockItem item)
        //            {
        //                if (item.Class == EBuildingBlockClass.BornResource)
        //                {
        //                    for(var resourceIndex = 0; resourceIndex < 7; resourceIndex++)
        //                    {
        //                        item.BaseBuildCost[resourceIndex] *= 20;
        //                    }
        //                }

        //                return true;
        //            });
        //        }
        //    }
        //}


        [HarmonyPatch(typeof(CommonUtils), "GetCharmLevelText")]
        public class GetCharmLevelTextPatch
        {
            public static bool Prefix(ref bool isFixedCharacter, ref bool faceVisible)
            {
                Debug.Log("Get!");
                if (GetBoolSettings("ShowKidCharm"))
                {
                    isFixedCharacter = true;
                }
                if(GetBoolSettings("ShowMaskCharm"))
                {
                    faceVisible = true;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UI_Shop), nameof(UI_Shop.IsPageShow))]
        public class IsPageShowPatch
        {
            //public static void Prefix(UI_Shop __instance)
            //{
            //    typeof(UI_Shop).GetField("_merchantFavorability").SetValue(__instance, 5000);
            //}

            public static void Postfix(UI_Shop __instance, ref bool __result, int index)
            {
                if (index == 7)
                {
                    return;
                }
                if (GetBoolSettings("HighLevelShop"))
                {
                    var _merchantData = __instance.GetType().GetField("_merchantData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as MerchantData;

                    __result = _merchantData.GetGoodsList(index).Items.Count != 0;
                }

            }
        }

        [HarmonyPatch(typeof(UI_Shop), nameof(UI_Shop.IsPageDisabled))]
        public class IsPageDisabledPatch
        {
            //public static void Prefix(UI_Shop __instance)
            //{
            //    typeof(UI_Shop).GetField("_merchantFavorability").SetValue(__instance, 5000);
            //}

            public static void Postfix(UI_Shop __instance, ref bool __result, int index)
            {
                if (index == 7)
                {
                    return;
                }
                var _merchantData = __instance.GetType().GetField("_merchantData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as MerchantData;
                var _merchantFavorability = (int)__instance.GetType().GetField("_merchantFavorability", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                if (GetBoolSettings("HighLevelShop"))
                {
                    sbyte id = MerchantData.FindMerchantTemplateId(_merchantData.MerchantType, (sbyte)index);
                    MerchantItem merchantItem2 = Merchant.Instance[id];
                    __result = merchantItem2.FavorRequirement > _merchantFavorability;
                }

            }
        }

        [HarmonyPatch(typeof(UI_CharacterMenuInfo), "UpdateHealthRecoverState")]
        public class UpdateHealthRecoverStatePatch
        {
            public static void Postfix(UI_CharacterMenuInfo __instance)
            {
                if (GetBoolSettings("BabyKickable"))
                {
                    ((Refers)__instance).CGet<CButton>("KickOutBtn").interactable = true;
                    var btn = ((Refers)__instance).CGet<CButton>("KickOutBtn");
                    Refers component = ((Component)(object)btn).GetComponent<Refers>();
                    component.CGet<GameObject>("Label").SetActive(btn.interactable);
                    component.CGet<GameObject>("LabelDisable").SetActive(!btn.interactable);
                    component.CGet<CImage>("Icon").SetSprite(btn.interactable ? "charactermenu3_01_gn_icon_2_0" : "charactermenu3_01_gn_icon_2_1");
                }

            }
        }

        //[HarmonyPatch(typeof(UI_Shop), "OnNotifyGameData")]
        //public class UI_ShopOnInitPatch
        //{
        //    public static void Prefix(UI_Shop __instance)
        //    {
        //        Debug.Log("OnNotifyGameData!!");
        //        typeof(UI_Shop).GetField("_merchantFavorability").SetValue(__instance, 5000);
        //    }

        //    public static void Postfix(UI_Shop __instance)
        //    {
        //        Debug.Log("OnNotifyGameData!!!");
        //        typeof(UI_Shop).GetField("_merchantFavorability").SetValue(__instance, 5000);
        //    }
        //}


        //[HarmonyPatch(typeof(UI_BuildingArea), "PlaceBuildingInputHandle")] 
        //public class PlaceBuildingInputHandlePatch
        //{
        //    public static bool Prefix(ref UI_BuildingArea __instance)
        //    {
        //        if (GetBoolSettings("UnlimitedDependBuilding"))
        //        {
        //            var data = __instance.GetFieldValue("_buildingPlacementData") as UI_BuildingArea.BuildingPlacementData;
        //            data.CanBuild = true;
        //            //var confirmBuildMethod = typeof(UI_BuildingArea).GetMethod("ConfirmBuild");
        //            //confirmBuildMethod.Invoke(__instance, null);
        //            //__instance._isPlacingBuildingNow = false;
        //            //__instance.CancelPlaceBuilding(true);
        //        }
        //        return false;
        //    }
        //}
    }
}
