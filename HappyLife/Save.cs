using GameData.Domains.Global;
using HarmonyLib;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(GlobalDomain), "ShouldMakeBackup")]
        public class ShouldMakeBackupPatch
        {
            public static void Postfix(ref sbyte __result)
            {
                if (GetIntSettings("MaxSaveBackupCount") >= 3)
                    __result = (sbyte)GetIntSettings("MaxSaveBackupCount");
            }
        }
    }
}
