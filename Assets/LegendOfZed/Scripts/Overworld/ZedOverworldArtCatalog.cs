using System.Collections.Generic;
using UnityEngine;

namespace LegendOfZed.Overworld
{
    [CreateAssetMenu(fileName = "ZedOverworldArtCatalog", menuName = "Legend of Zed/Overworld Art Catalog")]
    public class ZedOverworldArtCatalog : ScriptableObject
    {
        [Header("Roads")]
        public List<GameObject> RoadStraightPrefabs = new List<GameObject>();
        public List<GameObject> RoadIntersectionPrefabs = new List<GameObject>();
        public List<GameObject> RoadCrossingPrefabs = new List<GameObject>();
        public List<GameObject> ManholePrefabs = new List<GameObject>();

        [Header("Sidewalks")]
        public List<GameObject> SidewalkStraightPrefabs = new List<GameObject>();
        public List<GameObject> SidewalkCornerPrefabs = new List<GameObject>();
        public List<GameObject> SidewalkEdgePrefabs = new List<GameObject>();

        [Header("Buildings")]
        public List<GameObject> BuildingPrefabs = new List<GameObject>();
        public List<GameObject> SpecialBuildingPrefabs = new List<GameObject>();
        public List<GameObject> DoorOrEntrancePrefabs = new List<GameObject>();
        public List<GameObject> SignPrefabs = new List<GameObject>();

        [Header("Parks")]
        public List<GameObject> GrassOrParkGroundPrefabs = new List<GameObject>();
        public List<GameObject> ParkPathPrefabs = new List<GameObject>();
        public List<GameObject> TreePrefabs = new List<GameObject>();
        public List<GameObject> BushPrefabs = new List<GameObject>();
        public List<GameObject> BenchPrefabs = new List<GameObject>();

        [Header("Street Props")]
        public List<GameObject> VehiclePrefabs = new List<GameObject>();
        public List<GameObject> PoliceVehiclePrefabs = new List<GameObject>();
        public List<GameObject> StreetLightPrefabs = new List<GameObject>();
        public List<GameObject> TrafficLightPrefabs = new List<GameObject>();
        public List<GameObject> HydrantPrefabs = new List<GameObject>();
        public List<GameObject> MailboxPrefabs = new List<GameObject>();
        public List<GameObject> ParkingMeterPrefabs = new List<GameObject>();
        public List<GameObject> PowerBoxPrefabs = new List<GameObject>();

        [Header("Trash / Debris")]
        public List<GameObject> TrashCanPrefabs = new List<GameObject>();
        public List<GameObject> DumpsterOrSkipPrefabs = new List<GameObject>();
        public List<GameObject> TrashHeapPrefabs = new List<GameObject>();
        public List<GameObject> PaperDebrisPrefabs = new List<GameObject>();

        [Header("Other Useful Props")]
        public List<GameObject> BarrierPrefabs = new List<GameObject>();
        public List<GameObject> FencePrefabs = new List<GameObject>();
        public List<GameObject> GenericPropPrefabs = new List<GameObject>();

        [Header("Audit Info")]
        public int TotalScannedPrefabs;
        public int TotalCatalogedPrefabs;
        public string LastAuditTime;
        [TextArea(3, 12)]
        public string Notes;

        public void Clear()
        {
            RoadStraightPrefabs.Clear();
            RoadIntersectionPrefabs.Clear();
            RoadCrossingPrefabs.Clear();
            ManholePrefabs.Clear();

            SidewalkStraightPrefabs.Clear();
            SidewalkCornerPrefabs.Clear();
            SidewalkEdgePrefabs.Clear();

            BuildingPrefabs.Clear();
            SpecialBuildingPrefabs.Clear();
            DoorOrEntrancePrefabs.Clear();
            SignPrefabs.Clear();

            GrassOrParkGroundPrefabs.Clear();
            ParkPathPrefabs.Clear();
            TreePrefabs.Clear();
            BushPrefabs.Clear();
            BenchPrefabs.Clear();

            VehiclePrefabs.Clear();
            PoliceVehiclePrefabs.Clear();
            StreetLightPrefabs.Clear();
            TrafficLightPrefabs.Clear();
            HydrantPrefabs.Clear();
            MailboxPrefabs.Clear();
            ParkingMeterPrefabs.Clear();
            PowerBoxPrefabs.Clear();

            TrashCanPrefabs.Clear();
            DumpsterOrSkipPrefabs.Clear();
            TrashHeapPrefabs.Clear();
            PaperDebrisPrefabs.Clear();

            BarrierPrefabs.Clear();
            FencePrefabs.Clear();
            GenericPropPrefabs.Clear();

            TotalScannedPrefabs = 0;
            TotalCatalogedPrefabs = 0;
            LastAuditTime = string.Empty;
            Notes = string.Empty;
        }
    }
}
