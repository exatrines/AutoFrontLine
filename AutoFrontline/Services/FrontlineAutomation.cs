using System.Numerics;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>フレームごとの自動化オーケストレーション。</summary>
public static class FrontlineAutomation
{
    public static void Update()
    {
        RequiredPlugins.SyncEnabledState();

        if (Player.IsDead)
            return;

        FrontlineDutyConfirmAutomation.Update();
        FrontlineLeaveAutomation.Update();

        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (!RequiredPlugins.IsAutomationActive)
            return;

        FollowTargetService.UpdateSelection();
        TrackedPlayerSync.Update();

        if (TrackedPlayerSync.ShouldDeferMovement)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleMove, ConfigIntervals.FollowMs))
            return;

        if (FollowTargetService.TryGetMoveTarget() is not Vector3 target)
            return;

        MovementCommands.MoveTo(target);
    }
}
