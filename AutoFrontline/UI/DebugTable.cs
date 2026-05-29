using System.Numerics;

namespace AutoFrontline.UI;

/// <summary>Debug タブ用のラベル／値テーブル。</summary>
internal sealed class DebugTable
{
    private readonly string _id;

    public DebugTable(string id) => _id = id;

    public bool Begin() => ImGuiEx.BeginDefaultTable(_id, ["~", "~"], drawHeader: false);

    public void Row(string label, string value, Vector4? valueColor = null)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGuiEx.TextV(label);
        ImGui.TableNextColumn();
        if (valueColor != null)
            ImGuiEx.TextV(valueColor, value);
        else
            ImGui.Text(value);
    }

    public void End() => ImGui.EndTable();
}
