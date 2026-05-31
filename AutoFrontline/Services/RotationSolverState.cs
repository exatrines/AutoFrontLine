using System.Reflection;

namespace AutoFrontline.Services;

/// <summary>Rotation Solver Reborn の稼働状態（DataCenter 参照）。</summary>
internal enum RotationSolverOperatingState
{
    Unknown,
    /// <summary>RSR Auto Off（DataCenter.State == false）。</summary>
    Off,
    Manual,
    AutoBig,
    AutoManual,
    AutoOther,
}

internal static class RotationSolverState
{
    private const string DataCenterTypeName = "RotationSolver.Basic.DataCenter";

    public static RotationSolverOperatingState Get()
    {
        if (!RequiredPlugins.IsLoaded(RequiredPlugins.RotationSolver.InternalName))
            return RotationSolverOperatingState.Unknown;

        if (FindType(DataCenterTypeName) is not { } dataCenterType)
            return RotationSolverOperatingState.Unknown;

        var state = ReadStaticBool(dataCenterType, "State");
        var isManual = ReadStaticBool(dataCenterType, "IsManual");

        if (!state)
            return RotationSolverOperatingState.Off;

        if (isManual)
            return RotationSolverOperatingState.Manual;

        if (IsTargetingType(dataCenterType, "Big"))
            return RotationSolverOperatingState.AutoBig;

        if (IsTargetingType(dataCenterType, "Manual"))
            return RotationSolverOperatingState.AutoManual;

        return RotationSolverOperatingState.AutoOther;
    }

    private static bool IsTargetingType(Type dataCenterType, string typeName)
    {
        var targetingProp = dataCenterType.GetProperty("TargetingType", BindingFlags.Public | BindingFlags.Static);
        if (targetingProp?.GetValue(null) is not { } targetingValue)
            return false;

        return targetingValue.ToString()?.Equals(typeName, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool ReadStaticBool(Type type, string propertyName)
    {
        var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null) is bool value && value;
    }

    private static Type FindType(string fullName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName, false);
            if (type != null)
                return type;
        }

        return null;
    }
}
