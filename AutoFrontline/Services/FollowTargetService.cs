using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>追跡対象の選定と移動先の算出。</summary>
public static class FollowTargetService
{
    private static ulong trackedContentId;
    private static long lastSelectionTick;
    private static long lastNaviRebuildTick;
    private static Vector3? cachedTrackedPosition;
    private static FollowSelectionMode selectionMode;

    public static int LastAllianceMemberCount { get; private set; }
    public static string LastPickedMemberName { get; private set; } = string.Empty;
    public static Vector3? LastTrackedPlayerPosition { get; private set; }
    public static Vector3? LastMoveAnchorPosition { get; private set; }
    public static Vector3? LastMoveTarget { get; private set; }
    public static Vector3? CachedTrackedPlayerPosition => cachedTrackedPosition;
    public static bool TrackedPositionUnchanged { get; private set; }
    public static string LastDensestMemberName { get; private set; } = string.Empty;
    public static int LastDensestNeighborCount { get; private set; }
    public static string LastSelectionMode => selectionMode.ToDebugLabel();
    public static string LastProximityEnemyName { get; private set; } = string.Empty;
    public static bool IsHostileMode => selectionMode == FollowSelectionMode.Hostile;
    public static int MoveRefreshIntervalMs =>
        IsHostileMode ? ConfigIntervals.HostileModeRefreshMs : ConfigIntervals.GroupMovementRefreshMs;

    public static void UpdateSelection()
    {
        if (Player.Object == null)
        {
            ClearTarget();
            return;
        }

        var members = GetMembers();
        if (trackedContentId != 0 && !IsTrackedAlive(members))
            SelectNewTarget(members);
        else if (trackedContentId == 0 || ShouldReselectOnTimer())
            SelectNewTarget(members);
    }

    public static IGameObject TryGetTrackedGameObject()
    {
        if (trackedContentId == 0 || Player.Object == null)
            return null;

        if (PlayerObjectResolver.FindByContentId(trackedContentId) is { } byContentId)
            return byContentId;

        var snapshot = GetMembers().FirstOrDefault(m => m.ContentId == trackedContentId);
        if (snapshot.ContentId == 0)
            return null;

        return PlayerObjectResolver.ResolveFromSnapshot(snapshot);
    }

    public static bool TryGetMoveDestinationDistance(out float distanceMeters)
    {
        distanceMeters = 0;
        if (Player.Object == null)
            return false;

        Vector3? destination = LastMoveTarget;
        if (destination == null)
        {
            var tracked = TryGetTrackedGameObject();
            if (tracked == null)
                return false;

            destination = tracked.Position;
        }

        distanceMeters = Vector3.Distance(Player.Object.Position, destination.Value);
        return true;
    }

    public static Vector3? TryGetMoveTarget()
    {
        if (trackedContentId == 0)
            return null;

        if (!TryResolveAnchorPosition(out var anchor))
            return null;

        LastTrackedPlayerPosition = anchor;

        if (cachedTrackedPosition is Vector3 cached && GameCoords.AreNear(
                cached,
                anchor,
                FrontlineConstants.PositionUnchangedThresholdMeters))
        {
            if (Environment.TickCount64 - lastNaviRebuildTick < MoveRefreshIntervalMs)
            {
                TrackedPositionUnchanged = true;
                return null;
            }

            lastNaviRebuildTick = Environment.TickCount64;
            TrackedPositionUnchanged = false;
            if (LastMoveTarget is Vector3 existingTarget)
                return existingTarget;
        }

        TrackedPositionUnchanged = false;
        cachedTrackedPosition = anchor;
        LastMoveAnchorPosition = anchor;
        LastMoveTarget = FollowMovePlanner.CreateDestination(anchor, selectionMode);
        lastNaviRebuildTick = Environment.TickCount64;
        return LastMoveTarget;
    }

    private static bool TryResolveAnchorPosition(out Vector3 anchor)
    {
        anchor = default;

        var members = GetMembers();
        var tracked = FindTracked(members);
        if (tracked == null || tracked.Value.IsDead)
        {
            SelectNewTarget(members);
            tracked = FindTracked(members);
        }

        if (tracked == null)
        {
            ClearMoveDebug();
            return false;
        }

        if (selectionMode == FollowSelectionMode.Hostile
            && HostileModeFollow.TryCreateSnapshot(members, out var hostileSnapshot))
        {
            anchor = HostileModeFollow.GetNavPosition(hostileSnapshot);
            return true;
        }

        anchor = tracked.Value.Position;
        return true;
    }

    private static List<AllianceMemberSnapshot> GetMembers()
    {
        var members = AllianceMemberCache.GetMembers();
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

        if (HostileModeFollow.TryCreateSnapshot(members, out var hostileSnapshot))
        {
            var ally = HostileModeFollow.GetTrackTarget(hostileSnapshot);
            LastProximityEnemyName = hostileSnapshot.EnemyName;
            ApplyTarget(ally, FollowSelectionMode.Hostile, densestNeighborCount: 0);
            return;
        }

        LastProximityEnemyName = string.Empty;

        var previousId = trackedContentId;
        var densest = DensestMemberSelector.Find(members, excludeSelf: true, excludeContentId: previousId)
                      ?? (previousId != 0 ? DensestMemberSelector.Find(members, excludeSelf: true) : null);

        if (densest == null)
        {
            ClearTarget();
            return;
        }

        ApplyTarget(
            densest.Value.Member,
            FollowSelectionMode.GroupMovement,
            densestNeighborCount: densest.Value.NeighborCount);
    }

    private static void ApplyTarget(
        AllianceMemberSnapshot picked,
        FollowSelectionMode mode,
        int densestNeighborCount)
    {
        trackedContentId = picked.ContentId;
        LastPickedMemberName = picked.Name;
        selectionMode = mode;

        if (mode == FollowSelectionMode.GroupMovement)
        {
            LastDensestMemberName = picked.Name;
            LastDensestNeighborCount = densestNeighborCount;
        }
        else
        {
            LastDensestMemberName = string.Empty;
            LastDensestNeighborCount = 0;
        }

        LastTrackedPlayerPosition = picked.Position;
        cachedTrackedPosition = picked.Position;
        TrackedPositionUnchanged = false;
    }

    private static void ClearTarget()
    {
        trackedContentId = 0;
        selectionMode = FollowSelectionMode.None;
        ClearMoveDebug();
        LastPickedMemberName = string.Empty;
        LastProximityEnemyName = string.Empty;
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
        lastNaviRebuildTick = 0;
    }

    private static bool ShouldReselectOnTimer()
    {
        if (Environment.TickCount64 - lastSelectionTick < MoveRefreshIntervalMs)
            return false;

        lastSelectionTick = Environment.TickCount64;
        return true;
    }

    private static bool IsTrackedAlive(IReadOnlyList<AllianceMemberSnapshot> members)
    {
        var tracked = FindTracked(members);
        return tracked != null && !tracked.Value.IsDead;
    }

    private static AllianceMemberSnapshot? FindTracked(IReadOnlyList<AllianceMemberSnapshot> members) =>
        members.FirstOrDefault(m => m.ContentId == trackedContentId);
}
