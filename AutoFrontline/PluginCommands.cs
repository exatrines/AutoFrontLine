using ECommons.Logging;

namespace AutoFrontline;

internal static class PluginCommands
{
    private const string Usage = "/autofrontline on|off|toggle - Enable or disable (no args: open settings)";

    public static void Handle(string command, string args)
    {
        var parts = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sub = parts.Length > 0 ? parts[0].ToLowerInvariant() : null;

        switch (sub)
        {
            case "on":
            case "enable":
                SetEnabled(true);
                return;
            case "off":
            case "disable":
                SetEnabled(false);
                return;
            case "toggle":
                SetEnabled(!C.Enabled);
                return;
            case null:
            case "":
                EzConfigGui.Open();
                return;
            default:
                DuoLog.Information(Usage);
                return;
        }
    }

    private static void SetEnabled(bool enabled)
    {
        if (enabled && !RequiredPlugins.AreAllLoaded)
        {
            DuoLog.Information(RequiredPlugins.GetMissingPluginsMessage());
            return;
        }

        if (C.Enabled == enabled)
        {
            DuoLog.Information($"Auto Frontline is already {(enabled ? "enabled" : "disabled")}.");
            return;
        }

        C.Enabled = enabled;
        EzConfig.Save();
        DuoLog.Information($"Auto Frontline {(enabled ? "enabled" : "disabled")}.");
    }
}
