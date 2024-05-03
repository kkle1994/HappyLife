namespace HappyLife
{
    public partial class HappyLife
    {
        //[HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.GetResourceBlockGrowthChance))]
        //public class GetResourceBlockGrowthChancePatch
        //{
        //    public static void Postfix(ref sbyte __result)
        //    {
        //        if (GetBoolSettings("ResourceMustGrow"))
        //        {
        //            __result = __result == 0 ? (sbyte)0 : (sbyte)100;
        //        }
        //    }
        //}
    }
}
