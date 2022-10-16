using GameData.Domains;
using GameData.Domains.Global;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(GlobalDomain), "ShouldMakeBackup")]
        public class ShouldMakeBackupPatch
        {
            public static void Postfix(ref sbyte __result)
            {
                if(GetIntSettings("MaxSaveBackupCount") >= 3)
                    __result =(sbyte)GetIntSettings("MaxSaveBackupCount");
            }
        }
    }
}
