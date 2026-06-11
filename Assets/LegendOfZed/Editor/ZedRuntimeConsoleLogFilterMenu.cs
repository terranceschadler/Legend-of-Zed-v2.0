#if UNITY_EDITOR
using LegendOfZed.Runtime;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.EditorTools
{
    public static class ZedRuntimeConsoleLogFilterMenu
    {
        [MenuItem("Legend Of Zed/Cleanup/Console Logs/Quiet Runtime Logs (Warnings And Errors Only)")]
        public static void UseQuietRuntimeLogs()
        {
            ZedRuntimeConsoleLogFilter.SaveAndApply(LogType.Warning);
            Debug.LogWarning("Legend Of Zed runtime console filter set to Warning. Debug.Log success spam will be hidden in Play Mode; warnings and errors still show.");
        }

        [MenuItem("Legend Of Zed/Cleanup/Console Logs/Verbose Runtime Logs (Show Debug.Log)")]
        public static void UseVerboseRuntimeLogs()
        {
            ZedRuntimeConsoleLogFilter.SaveAndApply(LogType.Log);
            Debug.Log("Legend Of Zed runtime console filter set to Log. Debug.Log output is visible again.");
        }
    }
}
#endif
