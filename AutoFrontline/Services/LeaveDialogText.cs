namespace AutoFrontline.Services;

/// <summary>フロントライン退出確認 SelectYesno の文言判定。</summary>
internal static class LeaveDialogText
{
    public static bool IsLeaveConfirmation(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Contains("フロントライン", StringComparison.Ordinal)
            && text.Contains("退出", StringComparison.Ordinal))
            return true;

        if (text.Contains("Frontline", StringComparison.OrdinalIgnoreCase)
            && text.Contains("leave", StringComparison.OrdinalIgnoreCase))
            return true;

        return text.Contains("このまま退出", StringComparison.Ordinal)
               || text.Contains("leave now", StringComparison.OrdinalIgnoreCase);
    }
}
