using System.Numerics;

namespace AutoFrontline.Services;

/// <summary>vnavmesh と Rotation Solver へのチャットコマンド送信。</summary>
internal static class MovementCommands
{
    public static void MoveTo(Vector3 target) =>
        Chat.ExecuteCommand($"/vnav moveto {GameCoords.FormatCommand(target)}");
}
