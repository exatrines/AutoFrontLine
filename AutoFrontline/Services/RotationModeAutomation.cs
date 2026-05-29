using ECommons.DalamudServices;
using ECommons.GameHelpers;

namespace AutoFrontline.Services;

/// <summary>RSR が Off / Manual のとき Auto Big へ揃える。Auto Big 中は何もしない。</summary>
internal static class RotationModeAutomation
{
    public static void Update()
    {
        if (!RequiredPlugins.IsAutomationActive)
            return;

        if (!FrontlineFields.IsFrontline(Svc.ClientState.TerritoryType))
            return;

        if (!RequiredPlugins.IsLoaded(RequiredPlugins.RotationSolver.InternalName))
            return;

        var state = RotationSolverState.Get();

        if (state is RotationSolverOperatingState.AutoBig or RotationSolverOperatingState.Unknown)
            return;

        if (state is not (RotationSolverOperatingState.Off or RotationSolverOperatingState.Manual))
            return;

        if (!EzThrottler.Throttle(FrontlineConstants.ThrottleRotationAutoBig, FrontlineConstants.RotationAutoBigIntervalMs))
            return;

        Chat.ExecuteCommand("/rotation Auto Big");
    }
}
