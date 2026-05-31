using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>移動コマンド発行可否（詠唱・マウント騎乗中は不可）。</summary>
internal static class PlayerMovementGate
{
    public static bool CanIssueVnavMoveTo =>
        !Player.IsCasting
        && !Player.Mounting
        && !Svc.Condition[ConditionFlag.Mounting]
        && !Svc.Condition[ConditionFlag.Mounting71];
}
