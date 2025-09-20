using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGeneration
{
    /// <summary>
    /// World generation configuration
    /// </summary>
    [CreateAssetMenu(fileName = "WorldGenConfig", menuName = "Courier/World Generation/Generation Config")]
    public class WorldGenerationConfig : ScriptableObject
    {
        [Header("Map Settings")]
        public int mapResolution = 256;
        public float noiseScale = 5f;
        public int worldSeed = 12345;
        [Range(1, 8)] public int noiseOctaves = 4;

        [Header("Pin Configuration")] 
        [SerializeField] private List<WorldPinAsset> pinAssets;

        public List<WorldPin> Pins()
        { return pinAssets.Select(asset => asset.Pin).ToList(); }

        [Header("Region Parameters")] 
        [Range(4,64)]public int worldSizeInRegions = 16;
        public RegionDefinition[] regionDefinitions;

        public TerrainMapGenerator CreateGenerator()
        { return new TerrainMapGenerator(
            mapResolution, 
            noiseScale, 
            worldSeed,
            noiseOctaves); }

        public WorldChunkGenerator CreateWorldChunkGenerator()
        { return new WorldChunkGenerator(regionDefinitions, worldSizeInRegions); }
    }
}