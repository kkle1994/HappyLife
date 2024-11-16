using GameData.Common;
using GameData.Domains;
using GameData.Domains.Extra;
using GameData.Domains.Merchant;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {
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
    }
}
