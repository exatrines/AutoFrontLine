namespace AutoFrontline.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        AflImGui.SectionHeader("Auto Frontline");
        DrawRequiredPlugins();

        ImGui.Spacing();
        DrawRecommendedJob();
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
}
