using System.Collections.Generic;

namespace AutoFrontline;

public static class FrontlineFields
{
    private static readonly Dictionary<uint, string> Fields = new()
    {
        [376] = "外縁遺跡群（制圧戦）",
        [431] = "シールロック（争奪戦）",
        [554] = "フィールド・オブ・グローリー（砕氷戦）",
        [888] = "オンサル・ハカイル（終節戦）",
        [1313] = "ウォーコー・チーテ（演習戦）",
    };

    public static bool IsFrontline(uint territoryId) => Fields.ContainsKey(territoryId);

    public static string GetDisplayName(uint territoryId) =>
        Fields.TryGetValue(territoryId, out var name) ? name : string.Empty;
}
