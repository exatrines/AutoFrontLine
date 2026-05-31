using AutoFrontline.Services;

namespace AutoFrontline.UI;

internal static class MountPicker
{
    private static string searchFilter = string.Empty;

    public static void Draw()
    {
        var preview = MountCatalog.GetDisplayName(C.MountSelectionId);

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(AflImGui.DefaultSliderWidth);

        if (!ImGui.BeginCombo("Mount##AflMountCombo", preview, ImGuiComboFlags.HeightLarge))
            return;

        if (ImGui.IsWindowAppearing())
        {
            searchFilter = string.Empty;
            MountCatalog.InvalidateCache();
        }

        ImGuiEx.SetNextItemFullWidth();
        ImGui.InputTextWithHint("##AflMountSearch", "Search...", ref searchFilter, 128);

        foreach (var option in MountCatalog.GetOptions())
        {
            if (!MatchesFilter(option.DisplayName, searchFilter))
                continue;

            var selected = option.SelectionId == C.MountSelectionId;
            if (ImGui.Selectable(option.DisplayName, selected))
                C.MountSelectionId = option.SelectionId;

            if (ImGui.IsWindowAppearing() && selected)
                ImGui.SetScrollHereY();
        }

        ImGui.EndCombo();
    }

    private static bool MatchesFilter(string name, string filter) =>
        string.IsNullOrWhiteSpace(filter)
        || name.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);
}
