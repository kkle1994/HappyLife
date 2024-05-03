using GameData.Common;
using GameData.Domains;
using GameData.Domains.Extra;
using GameData.Domains.Merchant;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {
        //[HarmonyPatch(typeof(MerchantData), nameof(MerchantData.GenerateGoods), new Type[] { typeof(DataContext), typeof(sbyte) })]
        //public class GenerateGoodsPatch
        //{
        //    public static bool Prefix(ref DataContext context, ref sbyte level)
        //    {
        //        if (GetIntSettings("GoodLevelIncrease") != 0)
        //        {
        //            level += (sbyte)GetIntSettings("GoodLevelIncrease");
        //        }
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(MerchantDomain), nameof(MerchantDomain.GetMerchantData))]
        public class GetMerchantDataPatch
        {
            public static void Postfix(ref MerchantData __result, DataContext context)
            {
                if (GetBoolSettings("HighLevelShop"))
                {
                    for (sbyte pageIndice = 0; pageIndice < 7; pageIndice++)
                    {
                        if (__result.GetGoodsList(pageIndice).Items.Count == 0)
                        {
                            GameData.Domains.Character.Character character = ((__result.CharId >= 0) ? DomainManager.Character.GetElement_Objects(__result.CharId) : null);
                            __result.GenerateGoodsLevelList(context, pageIndice, character);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ExtraDomain), nameof(ExtraDomain.AddMerchantDebt))]
        public class AddMerchantDebtPatch
        {
            public static void Postfix(DataContext context)
            {
                if (GetBoolSettings("HighLevelShop"))
                {

                }
            }
        }
    }
}
