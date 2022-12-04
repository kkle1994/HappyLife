using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Global;
using GameData.Domains.Merchant;
using GameData.Domains.Organization;
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
        //[HarmonyPatch(typeof(OrganizationDomain), "MeetGenderRestriction")]
        //public class MeetGenderRestrictionPatch
        //{
        //    public static void Postfix(bool __result)
        //    {
        //        if (GetBoolSettings("AllowAllMonkTypeInOrganization"))
        //        {
        //            __result = true;
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(Settlement), "GetOrganizationMemberPotentialSuccessorsInSet")]
        public class GetOrganizationMemberPotentialSuccessorsInSetPatch
        {
            public static bool Prefix(ref OrganizationInfo orgInfo, ref IEnumerable<int> charIds, ref List<int> result)
            {
                if (!GetBoolSettings("AllowAllGenderInOrganization"))
                    return true;
                result.Clear();
                int currMaxInfluencePower = -1;
                foreach (int relatedCharId in charIds)
                {
                    GameData.Domains.Character.Character relatedChar;
                    bool flag = !DomainManager.Character.TryGetElement_Objects(relatedCharId, out relatedChar);
                    if (!flag)
                    {
                        bool flag2 = relatedChar.GetKidnapperId() >= 0;
                        if (!flag2)
                        {
                            bool flag3 = relatedChar.IsActiveExternalRelationState(4) && !relatedChar.GetLocation().IsValid();
                            if (!flag3)
                            {
                                OrganizationInfo relatedCharOrgInfo = relatedChar.GetOrganizationInfo();
                                bool flag4 = orgInfo.SettlementId != relatedCharOrgInfo.SettlementId || orgInfo.Grade <= relatedCharOrgInfo.Grade || !relatedCharOrgInfo.Principal;
                                if (!flag4)
                                {
                                    SettlementCharacter settlementCharacter = DomainManager.Organization.GetSettlementCharacter(relatedCharId);
                                    short influencePower = settlementCharacter.GetInfluencePower();
                                    bool flag5 = (int)influencePower > currMaxInfluencePower;
                                    if (flag5)
                                    {
                                        result.Clear();
                                        currMaxInfluencePower = (int)influencePower;
                                        result.Add(relatedCharId);
                                    }
                                    else
                                    {
                                        bool flag6 = (int)influencePower == currMaxInfluencePower;
                                        if (flag6)
                                        {
                                            result.Add(relatedCharId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
