using BepInEx.Logging;

namespace NoSquashOnLoad;

public static class Logging {
    internal static void Log(object payload, LogLevel level = LogLevel.Info)
    {
        // This doesn't error, Rider just hasn't caught up
        NoSquashOnLoad.Instance!.Logger.Log(level, payload);
    }
}