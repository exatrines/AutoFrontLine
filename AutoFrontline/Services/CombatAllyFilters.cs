using System.Collections.Generic;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>自分・PT・アライアンス味方の ContentId 集合。</summary>
internal static class CombatAllyFilters
{
    public static HashSet<ulong> BuildContentIds(IReadOnlyList<AllianceMemberSnapshot> allies)
    {
        var ids = new HashSet<ulong>();
        if (Player.CID != 0)
            ids.Add(Player.CID);

        foreach (var member in allies)
        {
            if (member.ContentId != 0)
                ids.Add(member.ContentId);
        }

        return ids;
    }
}
