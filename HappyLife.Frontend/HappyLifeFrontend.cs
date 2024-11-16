using Config;
using GameData.Domains.Merchant;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;

namespace HappyLife.Frontend
{

    [PluginConfig("HappyLife", "kkle1994@outlook.com", "1.0")]
    public class HappyLifeFrontend : TaiwuRemakeHarmonyPlugin
    {
        public override void Initialize()
        {
            this.HarmonyInstance.PatchAll(typeof(GetCharmLevelTextPatch));
            this.HarmonyInstance.PatchAll(typeof(IsPageDisabledPatch));
            //this.HarmonyInstance.PatchAll(typeof(BuildingInitDataPatch));
            this.HarmonyInstance.PatchAll(typeof(UpdateHealthRecoverStatePatch));
            //this.HarmonyInstance.PatchAll(typeof(BindGlobalEventsPatch));
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

        //[HarmonyPatch(typeof(UI_MainMenu), "OnInit")]
        //public class BindGlobalEventsPatch
        //{
        //    public static string PatchFile = ".\\Mod\\HappyLife\\Datas\\BuildingDataPatch.csv";

        //    public static string CharacterTemplateIdsPatchFile = ".\\Mod\\HappyLife\\Datas\\CharacterTemplateIds.txt";
        //    public static void Postfix()
        //    {
        //        if (GetBoolSettings("EnableBuildResource"))
        //        {
        //            if (!File.Exists(PatchFile))
        //            {
        //                var newLines = new List<string>();
        //                newLines.Add("TemplateId,Name,Food,Wood,Metal,Jade,Fabric,Herb,Money,Authority");
        //                foreach (var item in BuildingBlock.Instance)
        //                {
        //                    var multiple = item.TemplateId >= 1 && item.TemplateId <= 20 ? 20 : 1;
        //                    newLines.Add($"{item.TemplateId}," +
        //                        $"{item.Name}," +
        //                        $"{item.BaseBuildCost[0] * multiple}," +
        //                        $"{item.BaseBuildCost[1] * multiple}," +
        //                        $"{item.BaseBuildCost[2] * multiple}," +
        //                        $"{item.BaseBuildCost[3] * multiple}," +
        //                        $"{item.BaseBuildCost[4] * multiple}," +
        //                        $"{item.BaseBuildCost[5] * multiple}," +
        //                        $"{item.BaseBuildCost[6] * multiple}," +
        //                        $"{item.BaseBuildCost[7] * multiple}"
        //                        );
        //                }
        //                Directory.CreateDirectory(Path.GetDirectoryName(PatchFile));
        //                File.WriteAllLines(PatchFile, newLines);
        //            }
        //            var lines = File.ReadAllLines(PatchFile);
        //            if (lines.Length != 0)
        //            {
        //                var headers = lines[0].Split(',');

        //                for (var lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        //                {
        //                    var line = lines[lineIndex];
        //                    var parts = line.Split(',');

        //                    var id = short.Parse(parts[0]);
        //                    BuildingBlockItem originalBlockItem = BuildingBlock.Instance[id];

        //                    var _dataArray = BuildingBlock.Instance.GetType().GetField("_dataArray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(BuildingBlock.Instance) as List<BuildingBlockItem>;
        //                    if (id >= 1 && id <= 20)
        //                    {
        //                        typeof(BuildingBlockItem).GetField(nameof(BuildingBlockItem.Type), BindingFlags.Instance | BindingFlags.Public).SetValue(originalBlockItem, EBuildingBlockType.Building);
        //                        typeof(BuildingBlockItem).GetField(nameof(BuildingBlockItem.Class), BindingFlags.Instance | BindingFlags.Public).SetValue(originalBlockItem, EBuildingBlockClass.Resource);
        //                        typeof(BuildingBlockItem).GetField(nameof(BuildingBlockItem.AddBuildCostPerLevel), BindingFlags.Instance | BindingFlags.Public).SetValue(originalBlockItem, (byte)50);
        //                    }
        //                    for (var resourceIndex = 0; resourceIndex < 8; resourceIndex++)
        //                    {
        //                        if(ushort.TryParse(parts[resourceIndex + 2], out var buildCost))
        //                            originalBlockItem.BaseBuildCost[resourceIndex] = buildCost;
        //                    }

        //                    // Add base workload
        //                    originalBlockItem.OperationTotalProgress[0] = 150;
        //                    originalBlockItem.OperationTotalProgress[1] = 50;
        //                    originalBlockItem.OperationTotalProgress[2] = 100;

        //                    if (GetBoolSettings("NoDependentBuilding"))
        //                    {
        //                        originalBlockItem.DependBuildings.RemoveAll(s => s > 20);
        //                    }

        //                    _dataArray[id] = originalBlockItem;
        //                }
        //            }
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(UI_BuildingOverview), "InitData")]
        //public class BuildingInitDataPatch
        //{
        //    public static void Postfix(UI_BuildingOverview __instance)
        //    {
        //        if (GetBoolSettings("EnableBuildResource"))
        //        {
        //            var _buildingMap = __instance.GetType().GetField("_buildingMap", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as Dictionary<EBuildingBlockClass, List<BuildingBlockItem>>;
        //            var _isHaveChickenKing = (bool)__instance.GetType().GetField("_isHaveChickenKing", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);



        //            BuildingBlock.Instance.Iterate(delegate (BuildingBlockItem item)
        //            {
        //                if (item.Class == EBuildingBlockClass.BornResource)
        //                {
        //                    if (!_buildingMap.TryGetValue(EBuildingBlockClass.Resource, out var value))
        //                    {
        //                        value = new List<BuildingBlockItem>();
        //                        _buildingMap.Add(EBuildingBlockClass.Resource, value);
        //                    }
        //                    // Skip useless buildings
        //                    if (item.TemplateId != 21 && item.TemplateId != 22 && item.TemplateId != 23)
        //                    {
        //                        value.Add(item);
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
                if (GetBoolSettings("ShowMaskCharm"))
                {
                    faceVisible = true;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(UI_Shop), nameof(UI_Shop.IsPageDisabled))]
        public class IsPageDisabledPatch
        {

            public static void Postfix(UI_Shop __instance, ref bool __result, int index)
            {
                if (index == 7)
                {
                    return;
                }
                var _merchantData = __instance.GetType().GetField("_merchantData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as MerchantData;
                var _merchantFavorability = (int)__instance.GetType().GetField("_merchantFavorability", BindingFlags.Static | BindingFlags.NonPublic).GetValue(__instance);
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
    }
}
