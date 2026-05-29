using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>追跡対象との距離に応じたマウント／降下。</summary>
public static unsafe class TrackedPlayerSync
{
    private static uint? mountRouletteActionId;

    public static float LastDistanceToTracked { get; private set; }
    public static bool LastSelfInCombat { get; private set; }

    public static bool ShouldDeferMovement =>
        NeedsMount() && !Player.Mounted && !Player.Mounting && (IsWeaponDrawn() || !InCombat);

    private static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

    public static void Update()
    {
        LastSelfInCombat = InCombat;

        var tracked = FollowTargetService.TryGetTrackedGameObject();
        if (tracked == null)
            return;

        var distance = Vector3.Distance(Player.Object!.Position, tracked.Position);
        LastDistanceToTracked = distance;
        SyncMount(distance);
    }

    private static void SyncMount(float distanceToTracked)
    {
        if (distanceToTracked >= FrontlineConstants.MountDistanceMeters)
        {
            TryMount();
            return;
        }

        if (Player.Mounted && EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
            Chat.ExecuteCommand("/mount");
    }

    private static void TryMount()
    {
        if (Player.Mounted || Player.Mounting)
            return;

        if (IsWeaponDrawn() || InCombat)
        {
            if (EzThrottler.Throttle(FrontlineConstants.ThrottleSheathe, FrontlineConstants.SheatheThrottleMs))
                Chat.ExecuteCommand("/battlemode off");
            return;
        }

        if (!InCombat && EzThrottler.Throttle(FrontlineConstants.ThrottleMount, FrontlineConstants.MountThrottleMs))
            UseMountRoulette();
    }

    private static bool NeedsMount()
    {
        var tracked = FollowTargetService.TryGetTrackedGameObject();
        if (tracked == null || Player.Object == null)
            return false;

        return Vector3.Distance(Player.Object.Position, tracked.Position) >= FrontlineConstants.MountDistanceMeters;
    }

    private static bool IsWeaponDrawn() =>
        Player.Available
        && Player.Object?.Address != nint.Zero
        && ((Character*)Player.Object.Address)->IsWeaponDrawn;

    private static void UseMountRoulette()
    {
        var actionId = mountRouletteActionId
                       ??= ResolveMountRouletteActionId()
                       ?? FrontlineConstants.MountRouletteGeneralActionId;

        var actionManager = ActionManager.Instance();
        if (actionManager != null)
            actionManager->UseAction(ActionType.GeneralAction, actionId, 0xE0000000);

        var name = Svc.Data.GetExcelSheet<GeneralAction>().GetRowOrDefault(actionId)?.Name.ToString();
        if (!string.IsNullOrEmpty(name))
            Chat.ExecuteCommand($"/gaction \"{name}\"");
    }

    private static uint? ResolveMountRouletteActionId()
    {
        foreach (var row in Svc.Data.GetExcelSheet<GeneralAction>())
        {
            var name = row.Name.ToString();
            if (name is "Mount Roulette" or "マウントルーレット")
                return row.RowId;
        }

        return null;
    }
}
