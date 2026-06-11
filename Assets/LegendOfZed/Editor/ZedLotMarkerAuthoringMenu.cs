#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LegendOfZed.MapIntegration.EditorTools
{
    /// <summary>
    /// Stable replacement for the old versioned lot rotation/category menus.
    /// </summary>
    public static class ZedLotMarkerAuthoringMenu
    {
        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Corner")]
        public static void SetCategoryCorner() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Corner); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Small Shop")]
        public static void SetCategorySmallShop() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.SmallShop); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Apartment")]
        public static void SetCategoryApartment() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Apartment); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Office")]
        public static void SetCategoryOffice() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Office); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Generic Block")]
        public static void SetCategoryGenericBlock() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Filler); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Back Lot / Service")]
        public static void SetCategoryBackLot() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Alley); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Set Category/Any (Migration Only)")]
        public static void SetCategoryAny() { SetSelectedCategory(ZedAuthoredBuildingLotCategory.Any); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Set Facing North")]
        public static void SetFacingNorth() { SetSelectedFacing(Vector3.forward); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Set Facing East")]
        public static void SetFacingEast() { SetSelectedFacing(Vector3.right); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Set Facing South")]
        public static void SetFacingSouth() { SetSelectedFacing(Vector3.back); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Set Facing West")]
        public static void SetFacingWest() { SetSelectedFacing(Vector3.left); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Rotate 90 CW")]
        public static void Rotate90Clockwise() { RotateSelected(90f); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Facing/Rotate 90 CCW")]
        public static void Rotate90CounterClockwise() { RotateSelected(-90f); }

        [MenuItem("Legend of Zed/Map Authoring/Lots/Validate Open Scene Lot Markers")]
        public static void ValidateOpenSceneLots()
        {
            ZedAuthoredBuildingLot[] lots = Object.FindObjectsByType<ZedAuthoredBuildingLot>(FindObjectsInactive.Include);
            int any = 0;
            int corner = 0;
            int disabled = 0;
            int badCornerOverride = 0;

            for (int i = 0; i < lots.Length; i++)
            {
                ZedAuthoredBuildingLot lot = lots[i];
                if (lot == null)
                {
                    continue;
                }

                if (!lot.approvedForRuntimeSpawning || !lot.canSpawnBuilding)
                {
                    disabled++;
                }

                if (lot.buildingCategory == ZedAuthoredBuildingLotCategory.Any)
                {
                    any++;
                }

                if (lot.buildingCategory == ZedAuthoredBuildingLotCategory.Corner)
                {
                    corner++;

                    if (lot.overrideBuildingPrefab != null &&
                        lot.overrideBuildingPrefab.name.IndexOf("Corner", System.StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        badCornerOverride++;
                        Debug.LogWarning("Corner lot has non-corner overridePrefab: " + GetPath(lot.transform), lot);
                    }
                }
            }

            Debug.Log("Lot marker validation complete. Lots=" + lots.Length + ", Any=" + any + ", Corner=" + corner + ", Disabled=" + disabled + ", BadCornerOverrides=" + badCornerOverride + ".");
        }

        private static void SetSelectedCategory(ZedAuthoredBuildingLotCategory category)
        {
            int changed = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                ZedAuthoredBuildingLot lot = ResolveLot(go);
                if (lot == null)
                {
                    continue;
                }

                Undo.RecordObject(lot, "Set authored lot category");
                lot.buildingCategory = category;
                EditorUtility.SetDirty(lot);
                changed++;
            }

            Debug.Log("Set selected authored lot category. Category=" + category + ", Changed=" + changed + ".");
        }

        private static void SetSelectedFacing(Vector3 forward)
        {
            int changed = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                ZedAuthoredBuildingLot lot = ResolveLot(go);
                if (lot == null)
                {
                    continue;
                }

                Undo.RecordObject(lot.transform, "Set authored lot facing");
                lot.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                EditorUtility.SetDirty(lot.transform);
                changed++;
            }

            Debug.Log("Set selected authored lot facing. Direction=" + DirectionName(forward) + ", Changed=" + changed + ".");
        }

        private static void RotateSelected(float yawDelta)
        {
            int changed = 0;
            foreach (GameObject go in Selection.gameObjects)
            {
                ZedAuthoredBuildingLot lot = ResolveLot(go);
                if (lot == null)
                {
                    continue;
                }

                Undo.RecordObject(lot.transform, "Rotate authored lot");
                Vector3 euler = lot.transform.eulerAngles;
                lot.transform.rotation = Quaternion.Euler(0f, euler.y + yawDelta, 0f);
                EditorUtility.SetDirty(lot.transform);
                changed++;
            }

            Debug.Log("Rotated selected authored lots. YawDelta=" + yawDelta + ", Changed=" + changed + ".");
        }

        private static ZedAuthoredBuildingLot ResolveLot(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            ZedAuthoredBuildingLot lot = go.GetComponent<ZedAuthoredBuildingLot>();
            if (lot != null)
            {
                return lot;
            }

            lot = go.GetComponentInParent<ZedAuthoredBuildingLot>();
            if (lot != null)
            {
                return lot;
            }

            Debug.LogWarning("Selected object is not an authored lot marker or child of one: " + go.name, go);
            return null;
        }

        private static string DirectionName(Vector3 forward)
        {
            if (forward == Vector3.right) return "East";
            if (forward == Vector3.back) return "South";
            if (forward == Vector3.left) return "West";
            return "North";
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
            {
                return "";
            }

            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
#endif
