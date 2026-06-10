#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    /// <summary>
    /// One-time cleanup helper for Unity 6 obsolete find API warnings.
    /// Keeps replacements narrow and source-text based so it can repair scripts not included in this patch zip.
    /// </summary>
    public static class ZedDeprecatedFindApiCleanupMenu
    {
        [MenuItem("Legend of Zed/Tools/Cleanup Deprecated Find APIs")]
        public static void CleanupDeprecatedFindApis()
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/LegendOfZed" });
            int scanned = 0;
            int changed = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".cs"))
                {
                    continue;
                }

                scanned++;

                string fullPath = Path.GetFullPath(path);
                string original = File.ReadAllText(fullPath);
                string updated = CleanupText(original);

                if (updated == original)
                {
                    continue;
                }

                File.WriteAllText(fullPath, updated);
                changed++;
                Debug.Log("Cleaned deprecated Unity find API usage in: " + path);
            }

            AssetDatabase.Refresh();

            Debug.Log("Deprecated find API cleanup complete. Scanned=" + scanned + ", Changed=" + changed + ".");
        }

        private static string CleanupText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            text = text.Replace("FindAnyObjectByType<", "FindAnyObjectByType<");

            text = Regex.Replace(
                text,
                @"(?<!Object\.)FindObjectsByType<([^>]+)>\((FindObjectsInactive\.(?:Include|Exclude)),\s*FindObjectsSortMode\.None\)",
                "FindObjectsByType<$1>($2)");

            text = Regex.Replace(
                text,
                @"Object\.FindObjectsByType<([^>]+)>\((FindObjectsInactive\.(?:Include|Exclude)),\s*FindObjectsSortMode\.None\)",
                "Object.FindObjectsByType<$1>($2)");

            return text;
        }
    }
}
#endif
