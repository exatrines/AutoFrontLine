using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>集団移動モード: 半径内の味方数が最大のプレイヤーを選ぶ。</summary>
internal static class DensestMemberSelector
{
    public static (AllianceMemberSnapshot Member, int NeighborCount)? Find(
        IReadOnlyList<AllianceMemberSnapshot> members,
        bool excludeSelf = false,
        ulong excludeContentId = 0)
    {
        var alive = members
            .Where(m => !m.IsDead
                        && (!excludeSelf || !AllyMemberFilters.IsSelf(m))
                        && (excludeContentId == 0 || m.ContentId != excludeContentId))
            .ToList();

        if (alive.Count == 0)
            return null;

        var radiusSq = FrontlineConstants.DensityRadiusMeters * FrontlineConstants.DensityRadiusMeters;
        var counts = new List<(AllianceMemberSnapshot Member, int Count)>(alive.Count);

        foreach (var candidate in alive)
        {
            var count = alive.Count(m =>
                Vector3.DistanceSquared(candidate.Position, m.Position) <= radiusSq);
            counts.Add((candidate, count));
        }

        var maxCount = counts.Max(c => c.Count);
        var ties = counts.Where(c => c.Count == maxCount).Select(c => c.Member).ToList();
        return (ties[Random.Shared.Next(ties.Count)], maxCount);
    }
}
