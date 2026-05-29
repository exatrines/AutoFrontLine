using ECommons.Automation.UIInput;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace AutoFrontline.Services;

/// <summary>コンテンツファインダー参加確認で Commence を押す。</summary>
public static unsafe class FrontlineDutyConfirmAutomation
{
    private static bool commencedForCurrentAddon;

    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        if (!GenericHelpers.TryGetAddonByName("ContentsFinderConfirm", out AtkUnitBase* addon))
        {
            commencedForCurrentAddon = false;
            return;
        }

        if (!addon->IsVisible || !GenericHelpers.IsAddonReady(addon))
        {
            commencedForCurrentAddon = false;
            return;
        }

        if (commencedForCurrentAddon)
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleContentsFinderConfirm, FrontlineConstants.ContentsFinderConfirmThrottleMs))
            return;

        if (TryCommence(addon))
            commencedForCurrentAddon = true;
    }

    private static bool TryCommence(AtkUnitBase* addon)
    {
        var confirm = (AddonContentsFinderConfirm*)addon;
        var button = confirm->CommenceButton;
        if (button == null)
            return false;

        if (!button->IsEnabled || !button->AtkResNode->IsVisible())
            return false;

        button->ClickAddonButton(addon);
        return true;
    }
}
