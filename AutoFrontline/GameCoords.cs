using System.Globalization;
using System.Numerics;

namespace AutoFrontline;

internal static class GameCoords
{
    public static string FormatDisplay(Vector3 position) =>
        $"{FormatComponent(position.X)}, {FormatComponent(position.Y)}, {FormatComponent(position.Z)}";

    public static string FormatCommand(Vector3 position) =>
        $"{FormatComponent(position.X)} {FormatComponent(position.Y)} {FormatComponent(position.Z)}";

    private static string FormatComponent(float value) =>
        value.ToString("F2", CultureInfo.InvariantCulture);

    public static bool AreNear(Vector3 a, Vector3 b, float thresholdMeters)
    {
        var thresholdSq = thresholdMeters * thresholdMeters;
        return Vector3.DistanceSquared(a, b) < thresholdSq;
    }
}
