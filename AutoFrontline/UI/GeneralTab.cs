namespace AutoFrontline.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Auto Frontline");
        DrawRequiredPlugins();

        ImGui.Spacing();
        DrawRecommendedJob();

        AflImGui.SectionHeader("Settings");
        AflImGui.DrawSettingsWhenReady(DrawSettings);
    }

    private static void DrawRequiredPlugins()
    {
        ImGui.TextWrapped("Required plugins:");
        ImGui.Indent();
        foreach (var plugin in RequiredPlugins.Enumerate())
            AflImGui.DrawPluginStatus(plugin);
        ImGui.Unindent();
    }

    private static void DrawRecommendedJob()
    {
        ImGui.TextWrapped("Recommended job:");
        ImGui.Indent();
        ImGui.TextWrapped("Ranged DPS");
        ImGui.Unindent();
    }

    private static void DrawSettings()
    {
        ImGui.Checkbox("Enable", ref C.Enabled);
        AflImGui.SliderSeconds("Follow interval (sec)", ref C.FollowIntervalSeconds, 1, 60);
        AflImGui.SliderSeconds("Player reselect interval (sec)", ref C.PlayerReselectIntervalSeconds, 1, 120);
    }
}
