using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

namespace AutoFrontline.Services;

/// <summary>非戦闘時はマウントを試行。近傍の味方が多数戦闘中なら降下。</summary>
public static unsafe class TrackedPlayerSync
{
    private static uint? mountRouletteActionId;

    public static float LastDistanceToTracked { get; private set; }
    public static bool LastSelfInCombat { get; private set; }
    public static int LastNearbyAlliesInCombat { get; private set; }

    public static bool ShouldDeferMovement =>
        !InCombat && !Player.Mounted && !Player.Mounting;

    private static bool InCombat => Svc.Condition[ConditionFlag.InCombat];

    public static void Update()
    {
        LastSelfInCombat = InCombat;
        SyncMount();

        var tracked = FollowTargetService.TryGetTrackedGameObject();
        if (tracked == null)
            return;

        LastDistanceToTracked = Vector3.Distance(Player.Object!.Position, tracked.Position);
    }

    private static void SyncMount()
    {
        LastNearbyAlliesInCombat = CountNearbyAlliesInCombat();

        if (Player.Mounted && LastNearbyAlliesInCombat >= FrontlineConstants.NearbyCombatDismountCount)
        {
            if (EzThrottler.Throttle(FrontlineConstants.ThrottleDismount, FrontlineConstants.DismountThrottleMs))
                Chat.ExecuteCommand("/mount");
            return;
        }

        if (InCombat)
            return;

        TryMount();
    }

    private static int CountNearbyAlliesInCombat()
    {
        if (!Player.Available || Player.Object == null)
            return 0;

        var selfPosition = Player.Object.Position;
        var radius = FrontlineConstants.NearbyCombatRadiusMeters;
        var count = 0;

        foreach (var member in AllianceMemberCollector.Collect())
        {
            if (member.IsDead || member.ContentId == Player.CID)
                continue;

            if (Vector3.Distance(selfPosition, member.Position) > radius)
                continue;

            if (IsMemberInCombat(member))
                count++;
        }

        return count;
    }

    private static bool IsMemberInCombat(AllianceMemberSnapshot member)
    {
        if (member.EntityId == 0)
            return false;

        var obj = Svc.Objects.SearchByEntityId(member.EntityId);
        if (obj is not ICharacter character || character.CurrentHp == 0)
            return false;

        return ((Character*)obj.Address)->InCombat;
    }

    private static void TryMount()
    {
        if (Player.Mounted || Player.Mounting)
            return;

        if (IsWeaponDrawn())
        {
            if (EzThrottler.Throttle(FrontlineConstants.ThrottleSheathe, FrontlineConstants.SheatheThrottleMs))
                Chat.ExecuteCommand("/battlemode off");
            return;
        }

        if (EzThrottler.Throttle(FrontlineConstants.ThrottleMount, FrontlineConstants.MountThrottleMs))
            UseMountRoulette();
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
