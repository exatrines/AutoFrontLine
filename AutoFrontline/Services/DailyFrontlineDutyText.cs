using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>コンテンツファインダー参加確認のデイリーフロントライン判定。</summary>
internal static unsafe class DailyFrontlineDutyText
{
    public static bool IsDailyFrontlineMatching(AtkUnitBase* addon)
    {
        if (addon == null)
            return false;

        var confirm = (AddonContentsFinderConfirm*)addon;
        var titleNode = confirm->AtkTextNode230;
        if (titleNode == null)
            return false;

        return MatchesDailyFrontlineTitle(titleNode->NodeText.GetText());
    }

    public static bool MatchesDailyFrontlineTitle(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        if (BilingualTextMatcher.ContainsAll(text, StringComparison.Ordinal, "デイリーチャレンジ", "フロントライン"))
            return true;

        return BilingualTextMatcher.ContainsAll(
            text,
            StringComparison.OrdinalIgnoreCase,
            "Daily Challenge",
            "Frontline");
    }
}
