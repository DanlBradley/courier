using UnityEngine;

namespace WorldGeneration
{
    [CreateAssetMenu(fileName = "RegionDefinition", menuName = "Courier/World Generation/Region Definition")]
    public class RegionDefinition: ScriptableObject
    {
        [SerializeField] private GameObject regionObject;
        public GameObject RegionObject => regionObject;
        [SerializeField] private RegionType regionType;
        public RegionType RegionType => regionType;
        [SerializeField] private Sprite regionSprite;
        public Sprite RegionSprite => regionSprite;

        [Header("Region Parameters")]
        [SerializeField] [Range(-1,1)] private float minTemp     = -1;
        [SerializeField] [Range(-1,1)] private float maxTemp     =  1;
        [SerializeField] [Range(-1,1)] private float minAltitude = -1;
        [SerializeField] [Range(-1,1)] private float maxAltitude =  1;
        [SerializeField] [Range(-1,1)] private float minMoisture = -1;
        [SerializeField] [Range(-1,1)] private float maxMoisture =  1;

        public bool IsRegionValid(TerrainSample sample)
        {
            if (sample.temperature < minTemp || sample.temperature > maxTemp) return false;
            if (sample.altitude < minAltitude || sample.altitude > maxAltitude) return false;
            if (sample.moisture < minMoisture || sample.moisture > maxMoisture) return false;
            return true;
        }
    }
}