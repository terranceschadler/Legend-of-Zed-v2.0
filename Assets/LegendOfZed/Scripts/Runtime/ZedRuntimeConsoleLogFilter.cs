using UnityEngine;

namespace LegendOfZed.Runtime
{
    /// <summary>
    /// Keeps Play Mode console output focused on actionable messages.
    /// Default runtime mode hides Debug.Log spam while still allowing warnings, errors, asserts, and exceptions.
    /// </summary>
    public static class ZedRuntimeConsoleLogFilter
    {
        public const string MinimumLogTypePlayerPrefsKey = "LegendOfZed.RuntimeConsole.MinimumLogType";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedRuntimeFilter()
        {
            LogType minimumLogType = (LogType)PlayerPrefs.GetInt(MinimumLogTypePlayerPrefsKey, (int)LogType.Warning);
            Apply(minimumLogType);
        }

        public static void Apply(LogType minimumLogType)
        {
            Debug.unityLogger.filterLogType = minimumLogType;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        }

        public static void SaveAndApply(LogType minimumLogType)
        {
            PlayerPrefs.SetInt(MinimumLogTypePlayerPrefsKey, (int)minimumLogType);
            PlayerPrefs.Save();
            Apply(minimumLogType);
        }
    }
}
