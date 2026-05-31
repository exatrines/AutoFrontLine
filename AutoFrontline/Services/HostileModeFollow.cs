using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

internal readonly record struct HostileFollowSnapshot(
    string EnemyName,
    IReadOnlyList<AllianceMemberSnapshot> AlliesNearEnemy);

/// <summary>敵対モード: 最寄り敵・味方列・ナビ位置の算出。</summary>
internal static class HostileModeFollow
{
    public static bool TryCreateSnapshot(
        IReadOnlyList<AllianceMemberSnapshot> members,
        out HostileFollowSnapshot snapshot)
    {
        snapshot = default;

        if (!NearbyEnemyDetector.TryGetClosestEnemy(
                members,
                FrontlineConstants.EnemyProximityFollowRadiusMeters,
                out var closestEnemy))
            return false;

        var allies = GetAlliesNearEnemy(closestEnemy.Position, members);
        if (allies.Count == 0)
            return false;

        snapshot = new HostileFollowSnapshot(closestEnemy.Name.ToString(), allies);
        return true;
    }

    public static AllianceMemberSnapshot GetTrackTarget(in HostileFollowSnapshot snapshot) =>
        snapshot.AlliesNearEnemy[0];

    public static Vector3 GetNavPosition(in HostileFollowSnapshot snapshot)
    {
        var allies = snapshot.AlliesNearEnemy;
        var first = allies[0].Position;
        var lastIndex = Math.Min(
            FrontlineConstants.EnemyProximityFrontlineAllyCount - 1,
            allies.Count - 1);
        var last = allies[lastIndex].Position;
        return Vector3.Lerp(first, last, C.HostileModePositionRatio);
    }

    private static List<AllianceMemberSnapshot> GetAlliesNearEnemy(
        Vector3 enemyPosition,
        IReadOnlyList<AllianceMemberSnapshot> members)
    {
        var maxDistanceSq = FrontlineConstants.EnemyProximityFollowRadiusMeters
                            * FrontlineConstants.EnemyProximityFollowRadiusMeters;

        return members
            .Where(m => !m.IsDead && !AllyMemberFilters.IsSelf(m))
            .Where(m => Vector3.DistanceSquared(enemyPosition, m.Position) <= maxDistanceSq)
            .OrderBy(m => Vector3.Distance(enemyPosition, m.Position))
            .Take(FrontlineConstants.EnemyProximityFrontlineAllyCount)
            .ToList();
    }
}
