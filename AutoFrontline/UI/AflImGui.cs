namespace AutoFrontline.UI;

internal static class AflImGui
{
    public const float DefaultSliderWidth = 200f;

    public static void SectionHeader(string title)
    {
        ImGui.Spacing();
        ImGui.TextDisabled(title);
        ImGui.Separator();
    }

    public static void DrawPluginStatus(RequiredPlugin plugin)
    {
        var loaded = RequiredPlugins.IsLoaded(plugin.InternalName);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGuiEx.Text(
            loaded ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed,
            loaded ? FontAwesomeIcon.Check.ToIconString() : FontAwesomeIcon.Times.ToIconString());
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.TextWrapped(plugin.DisplayName);
    }

    public static void SliderSeconds(string label, ref float seconds, float min, float max, float width = DefaultSliderWidth)
    {
        seconds = Math.Clamp(seconds, min, max);
        ImGui.SetNextItemWidth(width);
        ImGui.SliderFloat(label, ref seconds, min, max, "%.1f");
    }

    /// <summary>必須プラグイン未充足時に設定項目を無効化して描画する。</summary>
    public static void DrawSettingsWhenReady(Action drawSettings)
    {
        RequiredPlugins.SyncEnabledState();

        if (RequiredPlugins.AreAllLoaded)
        {
            drawSettings();
            return;
        }

        ImGui.BeginDisabled();
        drawSettings();
        ImGui.EndDisabled();
        ImGui.TextDisabled(RequiredPlugins.GetMissingPluginsMessage());
    }
}
