using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>密集中心プレイヤーの追跡と移動先の算出。</summary>
public static class FollowTargetService
{
    private static ulong trackedContentId;
    private static long lastSelectionTick;
    private static Vector3? cachedTrackedPosition;

    // Debug 表示用（DebugTab）
    public static int LastAllianceMemberCount { get; private set; }
    public static string LastPickedMemberName { get; private set; } = string.Empty;
    public static Vector3? LastTrackedPlayerPosition { get; private set; }
    public static Vector3? LastMoveAnchorPosition { get; private set; }
    public static Vector3? LastMoveTarget { get; private set; }
    public static Vector3? CachedTrackedPlayerPosition => cachedTrackedPosition;
    public static bool TrackedPositionUnchanged { get; private set; }
    public static string LastDensestMemberName { get; private set; } = string.Empty;
    public static int LastDensestNeighborCount { get; private set; }

    public static void UpdateSelection()
    {
        var members = CollectMembers();
        if (Player.Object == null)
        {
            ClearTarget();
            return;
        }

        if (trackedContentId != 0 && !IsTrackedAlive(members))
            SelectNewTarget(members);
        else if (trackedContentId == 0 || ShouldReselectOnTimer())
            SelectNewTarget(members);
    }

    public static IGameObject TryGetTrackedGameObject()
    {
        if (trackedContentId == 0 || Player.Object == null)
            return null;

        var snapshot = CollectMembers().FirstOrDefault(m => m.ContentId == trackedContentId);
        if (snapshot.ContentId == 0)
            return null;

        if (snapshot.EntityId != 0)
        {
            var byEntity = Svc.Objects.SearchByEntityId(snapshot.EntityId);
            if (byEntity != null)
                return byEntity;
        }

        return Svc.Objects.OfType<IPlayerCharacter>()
            .FirstOrDefault(pc => pc.Name.ToString() == snapshot.Name);
    }

    public static Vector3? TryGetMoveTarget()
    {
        var members = CollectMembers();
        if (trackedContentId == 0)
            return null;

        var tracked = FindTracked(members);
        if (tracked == null || tracked.Value.IsDead)
        {
            SelectNewTarget(members);
            tracked = FindTracked(members);
        }

        if (tracked == null)
        {
            ClearMoveDebug();
            return null;
        }

        var position = tracked.Value.Position;
        LastTrackedPlayerPosition = position;

        if (cachedTrackedPosition is Vector3 cached && PositionsEqual(cached, position))
        {
            TrackedPositionUnchanged = true;
            return null;
        }

        TrackedPositionUnchanged = false;
        cachedTrackedPosition = position;
        LastMoveAnchorPosition = position;
        LastMoveTarget = RandomOffset(position);
        return LastMoveTarget;
    }

    private static List<AllianceMemberSnapshot> CollectMembers()
    {
        var members = AllianceMemberCollector.Collect();
        LastAllianceMemberCount = members.Count;
        return members;
    }

    private static void SelectNewTarget(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        lastSelectionTick = Environment.TickCount64;

        if (Player.Object == null)
        {
            ClearTarget();
            return;
        }

        var previousId = trackedContentId;
        var densest = FindDensestMember(members, excludeSelf: true, excludeContentId: previousId)
                      ?? (previousId != 0 ? FindDensestMember(members, excludeSelf: true) : null);

        if (densest == null)
        {
            ClearTarget();
            return;
        }

        var picked = densest.Value.Member;
        trackedContentId = picked.ContentId;
        LastPickedMemberName = picked.Name;
        LastDensestMemberName = picked.Name;
        LastDensestNeighborCount = densest.Value.NeighborCount;
        LastTrackedPlayerPosition = picked.Position;
        cachedTrackedPosition = picked.Position;
        TrackedPositionUnchanged = false;
    }

    private static void ClearTarget()
    {
        trackedContentId = 0;
        ClearMoveDebug();
        LastPickedMemberName = string.Empty;
    }

    private static void ClearMoveDebug()
    {
        LastTrackedPlayerPosition = null;
        LastMoveAnchorPosition = null;
        LastMoveTarget = null;
        cachedTrackedPosition = null;
        TrackedPositionUnchanged = false;
        LastDensestMemberName = string.Empty;
        LastDensestNeighborCount = 0;
    }

    private static bool ShouldReselectOnTimer()
    {
        var now = Environment.TickCount64;
        if (now - lastSelectionTick < ConfigIntervals.PlayerReselectMs)
            return false;

        lastSelectionTick = now;
        return true;
    }

    private static bool IsTrackedAlive(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        var tracked = FindTracked(members);
        return tracked != null && !tracked.Value.IsDead;
    }

    private static AllianceMemberSnapshot? FindTracked(IReadOnlyList<AllianceMemberSnapshot> members) =>
        members.FirstOrDefault(m => m.ContentId == trackedContentId);

    private static bool PositionsEqual(Vector3 a, Vector3 b) =>
        GameCoords.AreNear(a, b, FrontlineConstants.PositionUnchangedThresholdMeters);

    private static (AllianceMemberSnapshot Member, int NeighborCount)? FindDensestMember(
        IReadOnlyList<AllianceMemberSnapshot> members,
        bool excludeSelf = false,
        ulong excludeContentId = 0)
    {
        var alive = members
            .Where(m => !m.IsDead
                        && (!excludeSelf || m.ContentId != Player.CID)
                        && (excludeContentId == 0 || m.ContentId != excludeContentId))
            .ToList();

        if (alive.Count == 0)
            return null;

        var radius = FrontlineConstants.DensityRadiusMeters;
        var radiusSq = radius * radius;
        var counts = new List<(AllianceMemberSnapshot Member, int Count)>(alive.Count);

        foreach (var candidate in alive)
        {
            var count = alive.Count(m => Vector3.DistanceSquared(candidate.Position, m.Position) <= radiusSq);
            counts.Add((candidate, count));
        }

        var maxCount = counts.Max(c => c.Count);
        var ties = counts.Where(c => c.Count == maxCount).Select(c => c.Member).ToList();
        return (ties[Random.Shared.Next(ties.Count)], maxCount);
    }

    private static Vector3 RandomOffset(Vector3 anchor)
    {
        var angle = Random.Shared.NextSingle() * MathF.PI * 2f;
        var min = FrontlineConstants.MoveOffsetMinMeters;
        var max = FrontlineConstants.MoveOffsetMaxMeters;
        var dist = min + Random.Shared.NextSingle() * (max - min);
        return anchor + new Vector3(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
    }
}
