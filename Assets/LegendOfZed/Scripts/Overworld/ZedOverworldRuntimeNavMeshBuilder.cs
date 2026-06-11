using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LegendOfZed.Overworld
{
    [DisallowMultipleComponent]
    public class ZedOverworldRuntimeNavMeshBuilder : MonoBehaviour
    {
        public static bool HasSuccessfulRuntimeBuild { get; private set; }
        public static float LastSuccessfulBuildTime { get; private set; }
        public static int LastSuccessfulBuildFrame { get; private set; }

        [Header("Build")]
        public bool BuildOnStart = true;
        public Vector3 BoundsCenter = Vector3.zero;
        public Vector3 BoundsSize = new Vector3(230f, 24f, 230f);

        [Header("Source Collection")]
        public bool IncludeAllSceneMeshes = false;
        [Tooltip("Optional. Leave blank unless the project has this tag defined. Name matching is always used.")]
        public string WalkableTag = "";
        public LayerMask IncludedLayers = ~0;

        [Header("Prototype Safety")]
        [Tooltip("Use collider bounds boxes instead of source meshes. This avoids Read/Write warnings from imported Synty meshes.")]
        public bool UseColliderBoundsOnly = true;

        [Header("Debug")]
        public bool LogBuild = false;

        private NavMeshData _navMeshData;
        private NavMeshDataInstance _navMeshInstance;

        private readonly List<NavMeshBuildSource> _sources = new List<NavMeshBuildSource>();

        public bool HasBuiltValidNavMesh => _navMeshData != null && _navMeshInstance.valid;
        public int LastSourceCount => _sources.Count;

        private void Start()
        {
            if (BuildOnStart)
            {
                BuildNow();
            }
        }

        [ContextMenu("Build Now")]
        public void BuildNow()
        {
            RemoveExisting();

            _sources.Clear();
            CollectSources();

            if (_sources.Count == 0)
            {
                Debug.LogWarning("ZedOverworldRuntimeNavMeshBuilder found no NavMesh sources.", this);
                return;
            }

            Bounds bounds = new Bounds(BoundsCenter, BoundsSize);
            NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
            _navMeshData = NavMeshBuilder.BuildNavMeshData(settings, _sources, bounds, Vector3.zero, Quaternion.identity);

            if (_navMeshData == null)
            {
                Debug.LogWarning("ZedOverworldRuntimeNavMeshBuilder failed to build NavMeshData.", this);
                return;
            }

            _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);
            HasSuccessfulRuntimeBuild = true;
            LastSuccessfulBuildTime = Time.time;
            LastSuccessfulBuildFrame = Time.frameCount;

            if (LogBuild)
            {
                Debug.Log("Zed overworld runtime NavMesh built. Sources=" + _sources.Count + " Bounds=" + bounds, this);
            }
        }

        [ContextMenu("Remove Existing")]
        public void RemoveExisting()
        {
            if (_navMeshInstance.valid)
            {
                _navMeshInstance.Remove();
            }

            _navMeshData = null;
        }

        private void OnDestroy()
        {
            RemoveExisting();
        }

        private void CollectSources()
        {
            Collider[] colliders = FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.isTrigger)
                {
                    continue;
                }

                if (!IsLayerIncluded(collider.gameObject.layer))
                {
                    continue;
                }

                if (!IncludeAllSceneMeshes && !IsWalkableCandidate(collider.gameObject))
                {
                    continue;
                }

                AddColliderSource(collider);
            }
        }

        private bool IsLayerIncluded(int layer)
        {
            return (IncludedLayers.value & (1 << layer)) != 0;
        }

        private bool IsWalkableCandidate(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(WalkableTag) && SafeCompareTag(go, WalkableTag))
            {
                return true;
            }

            string name = go.name.ToLowerInvariant();
            return name.Contains("floor") ||
                   name.Contains("road") ||
                   name.Contains("sidewalk") ||
                   name.Contains("path") ||
                   name.Contains("park") ||
                   name.Contains("walkable");
        }

        private static bool SafeCompareTag(GameObject go, string tagName)
        {
            try
            {
                return go.CompareTag(tagName);
            }
            catch (UnityException)
            {
                return false;
            }
        }

        private void AddColliderSource(Collider collider)
        {
            if (UseColliderBoundsOnly)
            {
                AddBoundsBoxSource(collider.bounds);
                return;
            }

            if (collider is BoxCollider box)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    box.transform.TransformPoint(box.center),
                    box.transform.rotation,
                    Vector3.Scale(box.transform.lossyScale, box.size));

                NavMeshBuildSource source = new NavMeshBuildSource();
                source.shape = NavMeshBuildSourceShape.Box;
                source.transform = matrix;
                source.size = Vector3.one;
                source.area = 0;
                _sources.Add(source);
                return;
            }

            if (collider is SphereCollider sphere)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    sphere.transform.TransformPoint(sphere.center),
                    sphere.transform.rotation,
                    Vector3.one * sphere.radius * 2f);

                NavMeshBuildSource source = new NavMeshBuildSource();
                source.shape = NavMeshBuildSourceShape.Sphere;
                source.transform = matrix;
                source.size = Vector3.one;
                source.area = 0;
                _sources.Add(source);
                return;
            }

            MeshCollider meshCollider = collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
            {
                NavMeshBuildSource source = new NavMeshBuildSource();
                source.shape = NavMeshBuildSourceShape.Mesh;
                source.sourceObject = meshCollider.sharedMesh;
                source.transform = meshCollider.transform.localToWorldMatrix;
                source.area = 0;
                _sources.Add(source);
                return;
            }

            AddBoundsBoxSource(collider.bounds);
        }

        private void AddBoundsBoxSource(Bounds bounds)
        {
            if (bounds.size.x <= 0.001f || bounds.size.y <= 0.001f || bounds.size.z <= 0.001f)
            {
                return;
            }

            NavMeshBuildSource source = new NavMeshBuildSource();
            source.shape = NavMeshBuildSourceShape.Box;
            source.transform = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
            source.size = Vector3.one;
            source.area = 0;
            _sources.Add(source);
        }
    }
}
