using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>敵プレイヤー（自分・PT・アライアンス以外）のうち最も近い対象をターゲットする。</summary>
internal static class ClosestEnemyPlayerTargeting
{
    public static IPlayerCharacter LastClosestEnemy { get; private set; }
    public static float LastClosestEnemyDistance { get; private set; }

    public static void Update()
    {
        if (!AutomationContext.CanRunInFrontlineMatch)
        {
            ClearDebugState();
            return;
        }

        if (!Player.Available || Player.Object == null)
        {
            ClearDebugState();
            return;
        }

        if (!EzThrottler.Throttle(
                FrontlineConstants.ThrottleEnemyTarget,
                FollowTargetService.IsHostileMode
                    ? ConfigIntervals.HostileModeRefreshMs
                    : ConfigIntervals.GroupMovementRefreshMs))
            return;

        var allies = AllianceMemberCache.GetMembers();
        if (!NearbyEnemyDetector.TryGetClosestEnemy(allies, out var enemy, out var distance))
        {
            ClearDebugState();
            return;
        }

        LastClosestEnemy = enemy;
        LastClosestEnemyDistance = distance;

        if (Svc.Targets.Target?.GameObjectId == enemy.GameObjectId)
            return;

        Svc.Targets.Target = enemy;
    }

    private static void ClearDebugState()
    {
        LastClosestEnemy = null;
        LastClosestEnemyDistance = 0;
    }
}
