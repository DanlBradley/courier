using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGeneration
{
    public class Region
    {
        public int gridX;
        public int gridY;
        public RectInt worldBounds;
        public TerrainSample sample;

        public RegionDefinition regionDefinition;
        public RegionType regionType;
        public WorldPin containedPin;
        
        public GameObject GetRegionPrefab()
        {
            return containedPin != null ? containedPin.uniquePinRegion : regionDefinition?.RegionObject;
        }

        public Sprite GetRegionSprite()
        {
            if (containedPin != null && containedPin.pinSprite != null) { return containedPin.pinSprite; }
            return regionDefinition.RegionSprite;
        }

        public string GetRegionInfo()
        {
            string line1 = "Position: " + gridX + ", " + gridY;
            string line2 = "Type: " + regionType.ToString();
            string line3 = "Landmark: " + (containedPin != null ? containedPin.name : "");
            return line1 + "\n" + line2 + "\n" + line3;
        }
    }
    
    public enum RegionType
    {
        Forest,
        Mountain,
        MountainPeak,
        Hills,
        Swamp,
        Marsh,
        Meadows,
        Desert,
        Jungle
    }
}