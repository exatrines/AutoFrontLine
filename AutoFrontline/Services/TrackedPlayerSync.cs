using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>移動先が遠いときマウントを試行。近傍の敵プレイヤーがいるとき降下。</summary>
public static unsafe class TrackedPlayerSync
{
    public static float LastDistanceToTracked { get; private set; }
    public static float LastDistanceToMoveDestination { get; private set; }
    public static bool LastSelfInCombat { get; private set; }
    public static int LastNearbyEnemyCount { get; private set; }

    public static bool ShouldDeferMovement =>
        ShouldTryMountForMoveDistance() && !Player.Mounted && !Player.Mounting;

    private static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

    public static void Update()
    {
        LastSelfInCombat = InCombat;
        SyncMount();

        var tracked = FollowTargetService.TryGetTrackedGameObject();
        if (tracked == null)
            return;

        LastDistanceToTracked = Vector3.Distance(Player.Object!.Position, tracked.Position);
        if (FollowTargetService.TryGetMoveDestinationDistance(out var moveDistance))
            LastDistanceToMoveDestination = moveDistance;
    }

    private static bool ShouldTryMountForMoveDistance() =>
        FollowTargetService.TryGetMoveDestinationDistance(out var distance)
        && distance >= C.MountDistanceMeters;

    private static void SyncMount()
    {
        var allies = AllianceMemberCache.GetMembers();
        var hasNearbyEnemy = NearbyEnemyDetector.HasNearbyEnemy(allies, out var enemyCount);
        LastNearbyEnemyCount = enemyCount;

        if (Player.Mounted && hasNearbyEnemy)
        {
            if (EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
                Chat.ExecuteCommand("/mount");
            return;
        }

        if (!ShouldTryMountForMoveDistance())
            return;

        TryMount();
    }

    private static void TryMount()
    {
        if (Player.Mounted || Player.Mounting)
            return;

        if (EzThrottler.Throttle(FrontlineConstants.ThrottleMount, FrontlineConstants.MountThrottleMs))
            UseConfiguredMount();
    }

    private static void UseConfiguredMount()
    {
        if (C.MountSelectionId != MountCatalog.RouletteSelectionId
            && MountCatalog.IsMountOwned(C.MountSelectionId))
        {
            UseSpecificMount(C.MountSelectionId);
            return;
        }

        UseMountRoulette();
    }

    private static void UseMountRoulette()
    {
        var actionId = MountCatalog.ResolveRouletteActionId();
        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.GeneralAction, actionId, 0xE0000000);

        var name = Svc.Data.GetExcelSheet<GeneralAction>().GetRowOrDefault(actionId)?.Name.ToString();
        if (!string.IsNullOrEmpty(name))
            Chat.ExecuteCommand($"/gaction \"{name}\"");
    }

    private static void UseSpecificMount(uint mountRowId)
    {
        if (Svc.Data.GetExcelSheet<Mount>().GetRowOrDefault(mountRowId) is not { } mount)
            return;

        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.Mount, mountRowId, 0xE0000000);
    }
}
