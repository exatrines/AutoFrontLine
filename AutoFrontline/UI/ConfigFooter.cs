using System.Numerics;
using Dalamud.Interface.Utility;

namespace AutoFrontline.UI;

public static class ConfigFooter
{
    private static readonly Vector4 GitHubGray = ImGuiEx.Vector4FromRGB(0x6E6E6E);
    private static readonly Vector4 GitHubGrayHover = ImGuiEx.Vector4FromRGB(0x858585);
    private static readonly Vector4 GitHubGrayActive = ImGuiEx.Vector4FromRGB(0x5A5A5A);

    private static readonly Vector4 SupportPink = ImGuiEx.Vector4FromRGB(0xFF5E5B);
    private static readonly Vector4 SupportPinkHover = ImGuiEx.Vector4FromRGB(0xFF7A77);
    private static readonly Vector4 SupportPinkActive = ImGuiEx.Vector4FromRGB(0xE04E4B);

    public static float GetReservedHeight()
    {
        var style = ImGui.GetStyle();
        return ImGui.GetFrameHeight() + style.ItemSpacing.Y * 3 + 2f;
    }

    public static void Draw()
    {
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled($"v{Svc.PluginInterface.Manifest.AssemblyVersion}");
        DrawSupportButtons();
    }

    private static void DrawSupportButtons()
    {
        const string githubLabel = "GitHub";
        const string ofuseLabel = "OFUSE";
        const string kofiLabel = "Ko-fi";

        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var totalWidth = ImGuiHelpers.GetButtonSize(githubLabel).X
                         + spacing
                         + ImGuiHelpers.GetButtonSize(ofuseLabel).X
                         + spacing
                         + ImGuiHelpers.GetButtonSize(kofiLabel).X;

        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - totalWidth);

        DrawLinkButton(githubLabel, SupportLinks.GitHubUrl, GitHubGray, GitHubGrayHover, GitHubGrayActive);
        ImGui.SameLine();
        DrawLinkButton(ofuseLabel, SupportLinks.OfuseUrl, SupportPink, SupportPinkHover, SupportPinkActive);
        ImGui.SameLine();
        DrawLinkButton(kofiLabel, SupportLinks.KoFiUrl, SupportPink, SupportPinkHover, SupportPinkActive);
    }

    private static void DrawLinkButton(string label, string url, Vector4 normal, Vector4 hover, Vector4 active)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, normal.ToUint());
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hover.ToUint());
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, active.ToUint());
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFFFFF);

        if (ImGui.Button(label))
            GenericHelpers.ShellStart(url);

        if (ImGui.IsItemHovered())
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        ImGui.PopStyleColor(4);
    }
}
