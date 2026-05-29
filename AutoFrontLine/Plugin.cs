using System.Numerics;
using AutoFrontLine.Services;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons.SimpleGui;

namespace AutoFrontLine;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Auto FrontLine";

    internal static Configuration C = null!;
    internal static Plugin P = null!;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this);

        C = EzConfig.Init<Configuration>();
        EzConfigGui.Init(UI.ConfigWindow.Draw, windowType: EzConfigGui.WindowType.Both);
        ConfigureConfigWindow();
        EzCmd.Add("/autoflontline", PluginCommands.Handle, "Open settings window.");
        EzCmd.Add("/autofrontline on", PluginCommands.Handle, "Enable.");
        EzCmd.Add("/autofrontline off", PluginCommands.Handle, "Disable.");
        EzCmd.Add("/autofrontline toggle", PluginCommands.Handle, "Toggle.");

        Svc.Framework.Update += OnFrameworkUpdate;
    }

    private static void ConfigureConfigWindow()
    {
        if (EzConfigGui.Window == null)
            return;

        EzConfigGui.Window.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    private static void OnFrameworkUpdate(object _) => FrontlineAutomation.Update();

    public void Dispose()
    {
        Svc.Framework.Update -= OnFrameworkUpdate;
        ECommonsMain.Dispose();
        P = null!;
        C = null!;
    }
}
