using System.Numerics;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>フレームごとの自動化オーケストレーション。</summary>
public static class FrontlineAutomation
{
    public static void Update()
    {
        AllianceMemberCache.BeginFrame();
        RequiredPlugins.SyncEnabledState();
        RotationModeAutomation.Update();

        if (Player.IsDead)
            return;

        FrontlineDutyConfirmAutomation.Update();
        FrontlineLeaveAutomation.Update();

        if (FrontlineLeaveAutomation.IsRecordScreenVisible)
            return;

        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (!RequiredPlugins.IsAutomationActive)
            return;

        FollowTargetService.UpdateSelection();
        ClosestEnemyPlayerTargeting.Update();
        TrackedPlayerSync.Update();

        if (TrackedPlayerSync.ShouldDeferMovement)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleMove, FollowTargetService.MoveRefreshIntervalMs))
            return;

        if (!PlayerMovementGate.CanIssueVnavMoveTo)
            return;

        if (FollowTargetService.TryGetMoveTarget() is not Vector3 target)
            return;

        MovementCommands.MoveTo(target);
    }
}
