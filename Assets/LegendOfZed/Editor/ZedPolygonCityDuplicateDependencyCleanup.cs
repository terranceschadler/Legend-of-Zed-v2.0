#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.Editor
{
    public static class ZedPolygonCityDuplicateDependencyCleanup
    {
        private const string MapGeneratorPrefabRoot = "Assets/LegendOfZed/MapGeneratorImport/Prefabs";
        private const string DuplicatePolygonCityRoot = "Assets/LegendOfZed/MapGeneratorImport/ThirdParty/PolygonCity";
        private const string CanonicalPolygonCityRoot = "Assets/Synty/PolygonCity";
        private const string ReportPath = "Assets/LegendOfZed/ReadMeDocs/Zed_v3_0_PolygonCity_DuplicateDependencyReport.md";

        [MenuItem("Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/1 Report Duplicate PolygonCity Dependencies")]
        public static void ReportDuplicateDependencies()
        {
            List<string> prefabPaths = FindMapGeneratorPrefabs();
            DependencyScanResult result = ScanDependencies(prefabPaths);
            WriteReport(result, null, false);

            Debug.Log($"PolygonCity duplicate dependency report complete. Prefabs scanned={prefabPaths.Count}, duplicate dependencies={result.DuplicateDependencyCount}. Report: {ReportPath}");
            EditorUtility.DisplayDialog(
                "PolygonCity Dependency Report",
                $"Scanned {prefabPaths.Count} map-generator prefab(s).\nDuplicate dependencies found: {result.DuplicateDependencyCount}\n\nReport written to:\n{ReportPath}",
                "OK");
        }

        [MenuItem("Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/2 Remap Map Generator Prefabs To Assets/Synty/PolygonCity")]
        public static void RemapDuplicateDependenciesToCanonicalSynty()
        {
            if (!AssetDatabase.IsValidFolder(MapGeneratorPrefabRoot))
            {
                Debug.LogWarning($"Map generator prefab folder not found: {MapGeneratorPrefabRoot}");
                return;
            }

            if (!AssetDatabase.IsValidFolder(DuplicatePolygonCityRoot))
            {
                Debug.Log("No duplicate PolygonCity folder found under MapGeneratorImport. Nothing to remap.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(CanonicalPolygonCityRoot))
            {
                Debug.LogWarning($"Canonical Synty PolygonCity folder not found: {CanonicalPolygonCityRoot}");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Remap PolygonCity Dependencies",
                    "This will edit ONLY prefab YAML files under:\n" + MapGeneratorPrefabRoot +
                    "\n\nIt will replace GUID references that point into:\n" + DuplicatePolygonCityRoot +
                    "\n\nwith matching assets from:\n" + CanonicalPolygonCityRoot +
                    "\n\nNo folders or assets will be deleted. Continue?",
                    "Remap",
                    "Cancel"))
            {
                return;
            }

            List<string> prefabPaths = FindMapGeneratorPrefabs();
            DependencyScanResult before = ScanDependencies(prefabPaths);
            RemapResult remap = BuildGuidRemap(before.AllDuplicateDependencies);

            int changedPrefabCount = 0;
            int totalReplacements = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string prefabPath in prefabPaths)
                {
                    int replacements = ReplaceGuidsInTextAsset(prefabPath, remap.GuidMap);
                    if (replacements <= 0)
                    {
                        continue;
                    }

                    changedPrefabCount++;
                    totalReplacements += replacements;
                    AssetDatabase.ImportAsset(prefabPath, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            DependencyScanResult after = ScanDependencies(prefabPaths);
            WriteReport(after, remap, true, before, changedPrefabCount, totalReplacements);

            Debug.Log($"PolygonCity dependency remap complete. Changed prefabs={changedPrefabCount}, GUID replacements={totalReplacements}, remaining duplicate dependencies={after.DuplicateDependencyCount}. Report: {ReportPath}");
            EditorUtility.DisplayDialog(
                "PolygonCity Remap Complete",
                $"Changed prefab files: {changedPrefabCount}\nGUID replacements: {totalReplacements}\nRemaining duplicate dependencies: {after.DuplicateDependencyCount}\n\nReport written to:\n{ReportPath}",
                "OK");
        }

        [MenuItem("Legend of Zed/Map Integration/PolygonCity Dependency Cleanup/3 Validate Canonical PolygonCity Dependencies")]
        public static void ValidateCanonicalDependencies()
        {
            List<string> prefabPaths = FindMapGeneratorPrefabs();
            DependencyScanResult result = ScanDependencies(prefabPaths);
            WriteReport(result, null, false);

            if (result.DuplicateDependencyCount == 0)
            {
                Debug.Log("Map generator prefabs are clean: no dependencies point to the duplicate MapGeneratorImport/ThirdParty/PolygonCity folder.");
                EditorUtility.DisplayDialog("PolygonCity Dependency Validation", "Clean. No duplicate PolygonCity dependencies found in map-generator prefabs.", "OK");
            }
            else
            {
                Debug.LogWarning($"Map generator prefabs still have {result.DuplicateDependencyCount} duplicate PolygonCity dependency reference(s). See {ReportPath}");
                EditorUtility.DisplayDialog("PolygonCity Dependency Validation", $"Still found {result.DuplicateDependencyCount} duplicate dependency reference(s).\n\nSee:\n{ReportPath}", "OK");
            }
        }

        private static List<string> FindMapGeneratorPrefabs()
        {
            List<string> prefabPaths = new List<string>();
            if (!AssetDatabase.IsValidFolder(MapGeneratorPrefabRoot))
            {
                return prefabPaths;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { MapGeneratorPrefabRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    prefabPaths.Add(path);
                }
            }

            prefabPaths.Sort(StringComparer.OrdinalIgnoreCase);
            return prefabPaths;
        }

        private static DependencyScanResult ScanDependencies(List<string> prefabPaths)
        {
            DependencyScanResult result = new DependencyScanResult();
            result.PrefabCount = prefabPaths.Count;

            foreach (string prefabPath in prefabPaths)
            {
                string[] dependencies = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (string dependency in dependencies)
                {
                    if (string.IsNullOrEmpty(dependency))
                    {
                        continue;
                    }

                    if (!dependency.StartsWith(DuplicatePolygonCityRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    result.Add(prefabPath, dependency);
                }
            }

            return result;
        }

        private static RemapResult BuildGuidRemap(IEnumerable<string> duplicateDependencyPaths)
        {
            RemapResult result = new RemapResult();

            foreach (string duplicatePath in duplicateDependencyPaths)
            {
                string relativePath = duplicatePath.Substring(DuplicatePolygonCityRoot.Length).TrimStart('/', '\\');
                string canonicalPath = CanonicalPolygonCityRoot + "/" + relativePath.Replace('\\', '/');

                string oldGuid = AssetDatabase.AssetPathToGUID(duplicatePath);
                string newGuid = AssetDatabase.AssetPathToGUID(canonicalPath);

                if (string.IsNullOrEmpty(oldGuid))
                {
                    result.MissingOldAssets.Add(duplicatePath);
                    continue;
                }

                if (string.IsNullOrEmpty(newGuid))
                {
                    result.MissingCanonicalAssets.Add(duplicatePath + " -> " + canonicalPath);
                    continue;
                }

                if (oldGuid == newGuid)
                {
                    continue;
                }

                if (!result.GuidMap.ContainsKey(oldGuid))
                {
                    result.GuidMap.Add(oldGuid, newGuid);
                    result.PathMap.Add(duplicatePath + " -> " + canonicalPath);
                }
            }

            return result;
        }

        private static int ReplaceGuidsInTextAsset(string assetPath, Dictionary<string, string> guidMap)
        {
            if (guidMap == null || guidMap.Count == 0)
            {
                return 0;
            }

            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), assetPath));
            if (!File.Exists(fullPath))
            {
                return 0;
            }

            string text = File.ReadAllText(fullPath);
            string original = text;
            int replacements = 0;

            foreach (KeyValuePair<string, string> pair in guidMap)
            {
                int count = CountOccurrences(text, pair.Key);
                if (count <= 0)
                {
                    continue;
                }

                text = text.Replace(pair.Key, pair.Value);
                replacements += count;
            }

            if (replacements > 0 && text != original)
            {
                File.WriteAllText(fullPath, text);
            }

            return replacements;
        }

        private static int CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static void WriteReport(DependencyScanResult scan, RemapResult remap, bool wasRemapRun, DependencyScanResult before = null, int changedPrefabCount = 0, int totalReplacements = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Zed v3.0 PolygonCity Duplicate Dependency Report");
            sb.AppendLine();
            sb.AppendLine("This report checks map-generator prefabs for dependencies that still point to the duplicate imported PolygonCity folder.");
            sb.AppendLine();
            sb.AppendLine("## Canonical decision");
            sb.AppendLine();
            sb.AppendLine("Canonical PolygonCity source:");
            sb.AppendLine();
            sb.AppendLine("`" + CanonicalPolygonCityRoot + "`");
            sb.AppendLine();
            sb.AppendLine("Duplicate import folder to remove only after validation is clean:");
            sb.AppendLine();
            sb.AppendLine("`" + DuplicatePolygonCityRoot + "`");
            sb.AppendLine();

            if (wasRemapRun)
            {
                sb.AppendLine("## Remap result");
                sb.AppendLine();
                sb.AppendLine("- Prefabs changed: " + changedPrefabCount);
                sb.AppendLine("- GUID replacements: " + totalReplacements);
                if (before != null)
                {
                    sb.AppendLine("- Duplicate dependencies before: " + before.DuplicateDependencyCount);
                }
                sb.AppendLine("- Duplicate dependencies after: " + scan.DuplicateDependencyCount);
                sb.AppendLine();
            }

            sb.AppendLine("## Current scan");
            sb.AppendLine();
            sb.AppendLine("- Prefabs scanned: " + scan.PrefabCount);
            sb.AppendLine("- Duplicate dependency references found: " + scan.DuplicateDependencyCount);
            sb.AppendLine();

            if (remap != null)
            {
                sb.AppendLine("## GUID remap pairs");
                sb.AppendLine();
                if (remap.PathMap.Count == 0)
                {
                    sb.AppendLine("No GUID remap pairs were created.");
                }
                else
                {
                    foreach (string line in remap.PathMap)
                    {
                        sb.AppendLine("- `" + line + "`");
                    }
                }
                sb.AppendLine();

                if (remap.MissingCanonicalAssets.Count > 0)
                {
                    sb.AppendLine("## Missing canonical matches");
                    sb.AppendLine();
                    sb.AppendLine("These duplicate dependencies did not have a matching asset at the same relative path under Assets/Synty/PolygonCity.");
                    sb.AppendLine();
                    foreach (string line in remap.MissingCanonicalAssets)
                    {
                        sb.AppendLine("- `" + line + "`");
                    }
                    sb.AppendLine();
                }
            }

            sb.AppendLine("## Prefabs with duplicate dependencies");
            sb.AppendLine();
            if (scan.PrefabToDependencies.Count == 0)
            {
                sb.AppendLine("Clean: no map-generator prefab dependencies point to the duplicate PolygonCity folder.");
            }
            else
            {
                foreach (KeyValuePair<string, SortedSet<string>> pair in scan.PrefabToDependencies)
                {
                    sb.AppendLine("### `" + pair.Key + "`");
                    foreach (string dependency in pair.Value)
                    {
                        sb.AppendLine("- `" + dependency + "`");
                    }
                    sb.AppendLine();
                }
            }

            string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ReportPath));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, sb.ToString());
            AssetDatabase.ImportAsset(ReportPath, ImportAssetOptions.ForceUpdate);
        }

        private sealed class DependencyScanResult
        {
            public int PrefabCount;
            public int DuplicateDependencyCount;
            public readonly SortedDictionary<string, SortedSet<string>> PrefabToDependencies = new SortedDictionary<string, SortedSet<string>>(StringComparer.OrdinalIgnoreCase);
            public readonly SortedSet<string> AllDuplicateDependencies = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            public void Add(string prefabPath, string dependencyPath)
            {
                DuplicateDependencyCount++;
                AllDuplicateDependencies.Add(dependencyPath);

                if (!PrefabToDependencies.TryGetValue(prefabPath, out SortedSet<string> list))
                {
                    list = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
                    PrefabToDependencies.Add(prefabPath, list);
                }

                list.Add(dependencyPath);
            }
        }

        private sealed class RemapResult
        {
            public readonly Dictionary<string, string> GuidMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            public readonly List<string> PathMap = new List<string>();
            public readonly List<string> MissingOldAssets = new List<string>();
            public readonly List<string> MissingCanonicalAssets = new List<string>();
        }
    }
}
#endif
