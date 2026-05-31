using Dalamud.Game.Text.SeStringHandling;
using ECommons.EzDTR;

namespace AutoFrontline;

internal static class PluginDtr
{
    public static void Init() =>
        _ = new EzDtr(GetText, PluginCommands.ToggleEnabled, title: "AutoFrontline");

    private static SeString GetText() =>
        $"AutoFrontline: {(C.Enabled ? "On" : "Off")}";
}
