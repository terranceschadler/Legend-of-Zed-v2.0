using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.LegacyMapGenerator
{
    public class ZedLegacyTileCamera : MonoBehaviour
    {
        public Transform targetTile;
        public int tileIndex = 0;
        public List<Transform> _camTargets = new List<Transform>();

        private Transform _myTransform;
        private Vector3 _targetCamPosition = Vector3.zero;
        private ZedLegacyRandomMapGenerator _mapGenerator;

        private void Awake()
        {
            _myTransform = transform;
            GameObject generatorObject = FindGameObjectWithTagSafe("MapGenerator");
            if (generatorObject != null)
            {
                _mapGenerator = generatorObject.GetComponent<ZedLegacyRandomMapGenerator>();
            }
        }

        private void Update()
        {
            if (targetTile == null && _mapGenerator != null && _mapGenerator.startingTile != null)
            {
                targetTile = _mapGenerator.startingTile.transform;
            }

            if (targetTile != null)
            {
                _targetCamPosition = new Vector3(targetTile.position.x, _myTransform.position.y, targetTile.position.z);
            }
        }

        private void LateUpdate()
        {
            if (targetTile != null && _myTransform.position != _targetCamPosition)
            {
                _myTransform.position = _targetCamPosition;
                _myTransform.LookAt(targetTile);
            }
        }

        public void PopulateCamTargetList()
        {
            _camTargets.Clear();
            GameObject[] children = FindGameObjectsWithTagSafe("RoomTile");
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && !_camTargets.Contains(children[i].transform))
                {
                    _camTargets.Add(children[i].transform);
                }
            }
        }

        public void CameraFocusOnTile(Transform tile)
        {
            targetTile = tile;
        }

        public void SelectNextTile()
        {
            if (_camTargets.Count == 0)
            {
                PopulateCamTargetList();
            }

            if (_camTargets.Count == 0)
            {
                return;
            }

            tileIndex++;
            if (tileIndex >= _camTargets.Count)
            {
                tileIndex = 0;
            }

            CameraFocusOnTile(_camTargets[tileIndex]);
        }

        public void SelectPreviousTile()
        {
            if (_camTargets.Count == 0)
            {
                PopulateCamTargetList();
            }

            if (_camTargets.Count == 0)
            {
                return;
            }

            tileIndex--;
            if (tileIndex < 0)
            {
                tileIndex = _camTargets.Count - 1;
            }

            CameraFocusOnTile(_camTargets[tileIndex]);
        }

        private static GameObject FindGameObjectWithTagSafe(string tagName)
        {
            try { return GameObject.FindGameObjectWithTag(tagName); }
            catch (UnityException) { return null; }
        }

        private static GameObject[] FindGameObjectsWithTagSafe(string tagName)
        {
            try { return GameObject.FindGameObjectsWithTag(tagName); }
            catch (UnityException) { return new GameObject[0]; }
        }
    }
}
