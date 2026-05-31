namespace AutoFrontline.Services;

/// <summary>フロントライン退出確認 SelectYesno の文言判定。</summary>
internal static class LeaveDialogText
{
    public static bool IsLeaveConfirmation(string text)
    {
        if (BilingualTextMatcher.IsNullOrWhiteSpace(text))
            return false;

        if (BilingualTextMatcher.ContainsAll(text, StringComparison.Ordinal, "フロントライン", "退出"))
            return true;

        if (BilingualTextMatcher.ContainsAll(text, StringComparison.OrdinalIgnoreCase, "Frontline", "leave"))
            return true;

        if (BilingualTextMatcher.ContainsAny(
                text,
                StringComparison.Ordinal,
                "このまま退出")
            || BilingualTextMatcher.ContainsAny(
                text,
                StringComparison.OrdinalIgnoreCase,
                "leave now"))
            return true;

        return BilingualTextMatcher.ContainsAny(
                   text,
                   StringComparison.Ordinal,
                   "コンテンツから退出")
               || BilingualTextMatcher.ContainsAny(
                   text,
                   StringComparison.OrdinalIgnoreCase,
                   "Abandon duty");
    }
}
