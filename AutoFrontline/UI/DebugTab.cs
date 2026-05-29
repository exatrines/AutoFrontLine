using System.Linq;
using System.Numerics;
using AutoFrontline.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameHelpers;

namespace AutoFrontline.UI;

public static class DebugTab
{
    public static void Draw()
    {
        DrawTerritorySection();
        DrawSelfSection();
        DrawTargetSection();
        DrawMovementSection();
        DrawStatusSection();
    }

    private static void DrawTerritorySection()
    {
        AflImGui.SectionHeader("Territory");

        var territoryId = Svc.ClientState.TerritoryType;
        var isFrontline = FrontlineFields.IsFrontline(territoryId);
        var fieldName = FrontlineFields.GetDisplayName(territoryId);
        if (string.IsNullOrEmpty(fieldName))
            fieldName = TerritoryName.GetTerritoryName(territoryId);

        var table = new DebugTable("##AflDbgTerritory");
        if (!table.Begin())
            return;

        table.Row("Field", $"[{territoryId}] {fieldName}",
            isFrontline ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed);
        table.Row("Alliance", $"{FollowTargetService.LastAllianceMemberCount}");

        if (!string.IsNullOrEmpty(AllianceMemberCollector.LastCollectionSource))
            table.Row("Source", AllianceMemberCollector.LastCollectionSource);

        table.End();
    }

    private static void DrawSelfSection()
    {
        AflImGui.SectionHeader("Self");

        var table = new DebugTable("##AflDbgSelf");
        if (!table.Begin())
            return;

        table.Row("Job", Player.Job.GetData().Name.ToString());
        table.Row("In combat", TrackedPlayerSync.LastSelfInCombat ? "Yes" : "No",
            TrackedPlayerSync.LastSelfInCombat ? ImGuiColors.DalamudRed : ImGuiColors.HealerGreen);
        table.Row("Mounted", Player.Mounted ? "Yes" : "No");
        table.End();
    }

    private static void DrawTargetSection()
    {
        AflImGui.SectionHeader("Target");

        var table = new DebugTable("##AflDbgTarget");
        if (!table.Begin())
            return;

        var tracked = FollowTargetService.LastPickedMemberName;
        table.Row("Tracked", string.IsNullOrEmpty(tracked) ? "—" : tracked);
        table.Row("Job", GetTrackedJobName());
        table.Row("Distance", $"{TrackedPlayerSync.LastDistanceToTracked:F1} m");

        if (!string.IsNullOrEmpty(FollowTargetService.LastDensestMemberName))
        {
            table.Row("Densest (50m)",
                $"{FollowTargetService.LastDensestMemberName} ({FollowTargetService.LastDensestNeighborCount})");
            table.Row("Densest job", TryGetJobDisplayNameByPlayerName(FollowTargetService.LastDensestMemberName));
        }

        table.End();
    }

    private static void DrawMovementSection()
    {
        AflImGui.SectionHeader("Movement");

        var table = new DebugTable("##AflDbgMove");
        if (!table.Begin())
            return;

        if (FollowTargetService.LastTrackedPlayerPosition is Vector3 trackedPos)
        {
            var posText = GameCoords.FormatDisplay(trackedPos);
            if (FollowTargetService.TrackedPositionUnchanged)
                posText += " (unchanged)";

            table.Row("Tracked pos.", posText,
                FollowTargetService.TrackedPositionUnchanged ? ImGuiColors.DalamudGrey : null);
        }

        if (FollowTargetService.CachedTrackedPlayerPosition is Vector3 cached
            && FollowTargetService.LastTrackedPlayerPosition is Vector3 live
            && !GameCoords.AreNear(cached, live, 0.1f))
        {
            table.Row("Cached pos.", GameCoords.FormatDisplay(cached));
        }

        if (FollowTargetService.LastMoveAnchorPosition is Vector3 anchor)
            table.Row("Move anchor", GameCoords.FormatDisplay(anchor));

        table.Row("Move target",
            FollowTargetService.LastMoveTarget is Vector3 move ? GameCoords.FormatDisplay(move) : "—");

        table.End();
    }

    private static void DrawStatusSection()
    {
        AflImGui.SectionHeader("Status");
        DrawStatusBullet("Movement", TrackedPlayerSync.ShouldDeferMovement, "Waiting to mount before move");
        DrawStatusBullet("Leave", FrontlineLeaveAutomation.PendingLeaveConfirm, "Waiting for leave confirmation");
    }

    private static void DrawStatusBullet(string label, bool active, string message)
    {
        ImGui.Bullet();
        ImGui.SameLine();
        if (active)
            ImGui.Text($"{label}: {message}");
        else
            ImGui.TextDisabled($"{label}: —");
    }

    private static string GetTrackedJobName()
    {
        if (FollowTargetService.TryGetTrackedGameObject() is IPlayerCharacter pc)
            return GetJobDisplayName((Job)pc.ClassJob.RowId);

        return TryGetJobDisplayNameByPlayerName(FollowTargetService.LastPickedMemberName);
    }

    private static string GetJobDisplayName(Job job) => job.GetData().Name.ToString();

    private static string TryGetJobDisplayNameByPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "—";

        var pc = Svc.Objects.OfType<IPlayerCharacter>()
            .FirstOrDefault(p => p.Name.ToString() == name);

        return pc == null ? "—" : GetJobDisplayName((Job)pc.ClassJob.RowId);
    }
}
