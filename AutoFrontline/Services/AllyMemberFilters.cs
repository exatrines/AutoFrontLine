using ECommons.GameHelpers;

namespace AutoFrontline.Services;

internal static class AllyMemberFilters
{
    public static bool IsSelf(AllianceMemberSnapshot member)
    {
        if (Player.CID != 0 && member.ContentId == Player.CID)
            return true;

        return Player.Object != null
               && member.EntityId != 0
               && member.EntityId == Player.Object.EntityId;
    }
}
