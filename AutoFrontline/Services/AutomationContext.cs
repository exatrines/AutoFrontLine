using ECommons.DalamudServices;

namespace AutoFrontline.Services;

/// <summary>フロントライン自動化の共通実行条件。</summary>
internal static class AutomationContext
{
    public static bool IsAutomationActive => RequiredPlugins.IsAutomationActive;

    public static bool IsRecordScreenOpen => FrontlineLeaveAutomation.IsRecordScreenVisible;

    public static bool IsInFrontline => FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType);

    public static bool CanRunInFrontlineMatch =>
        IsAutomationActive && !IsRecordScreenOpen && IsInFrontline;
}
