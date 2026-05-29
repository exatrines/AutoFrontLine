using System.Numerics;

namespace AutoFrontline.UI;

public static class ConfigWindow
{
    private static readonly Vector2 DefaultSize = new(600, 600);

    public static void Draw()
    {
        ImGui.SetNextWindowSize(DefaultSize, ImGuiCond.FirstUseEver);

        var footerHeight = ConfigFooter.GetReservedHeight();
        var bodyHeight = Math.Max(80f, ImGui.GetContentRegionAvail().Y - footerHeight);

        ImGui.BeginChild("##AflBody", new Vector2(0, bodyHeight));
        if (ImGui.BeginTabBar("AflTabs"))
        {
            DrawTab("General", GeneralTab.Draw, null);
            DrawTab("Debug", DebugTab.Draw, ImGuiColors.DalamudGrey);
            ImGui.EndTabBar();
        }

        ImGui.EndChild();

        ConfigFooter.Draw();
    }

    private static void DrawTab(string name, Action draw, Vector4? color)
    {
        if (color != null)
            ImGui.PushStyleColor(ImGuiCol.Text, color.Value);

        if (ImGui.BeginTabItem(name))
        {
            if (color != null)
                ImGui.PopStyleColor();

            var contentHeight = ImGui.GetContentRegionAvail().Y;
            ImGui.BeginChild(name + "child", new Vector2(0, contentHeight));
            try
            {
                draw();
            }
            catch (Exception e)
            {
                e.Log();
            }

            ImGui.EndChild();
            ImGui.EndTabItem();
        }
        else if (color != null)
        {
            ImGui.PopStyleColor();
        }
    }
}
