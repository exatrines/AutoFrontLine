namespace AutoFrontline.UI;

public static class SettingsTab
{
    public static void Draw()
    {
        AflImGui.DrawSettingsWhenReady(DrawSettings);
    }

    private static void DrawSettings()
    {
        ImGui.Checkbox("Enable", ref C.Enabled);
        AflImGui.SectionHeader("General");
        MountPicker.Draw();
        DrawDistanceSlider("Mount distance for target (m)", ref C.MountDistanceMeters);
        DrawDistanceSlider("Dismount distance for enemy (m)", ref C.DismountEnemyDistanceMeters);
        AflImGui.SliderSeconds("Group movement refresh (sec)", ref C.GroupMovementRefreshIntervalSeconds, 0.5f, 3.0f);
        AflImGui.SliderSeconds("Hostile mode refresh (sec)", ref C.HostileModeRefreshIntervalSeconds, 0.5f, 3.0f);
        DrawRatioSlider(
            "Hostile mode position",
            ref C.HostileModePositionRatio,
            "0 = front ally, 1 = rearmost ally");
    }

    private static void DrawDistanceSlider(string label, ref int meters)
    {
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderInt(label, ref meters, 0, 100);
    }

    private static void DrawRatioSlider(string label, ref float ratio, string hint)
    {
        ratio = Math.Clamp(ratio, 0f, 1f);
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);
        ImGui.SliderFloat(label, ref ratio, 0f, 1f, "%.2f");
        ImGui.TextDisabled(hint);
    }
}
