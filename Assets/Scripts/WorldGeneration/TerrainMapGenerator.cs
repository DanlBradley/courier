using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace WorldGeneration
{
    /// <summary>
    /// Generates a set of perlin noise maps associated with the different parameters: Currently temperature,
    /// altitude, and moisture.
    /// </summary>
    public class TerrainMapGenerator
    {
        private readonly int mapResolution;
        private readonly float noiseScale;
        private readonly int octaves;
        
        // Cached noise offsets for consistency
        private readonly Vector2 temperatureOffset;
        private readonly Vector2 altitudeOffset;
        private readonly Vector2 moistureOffset;
        
        public TerrainMapGenerator(int resolution = 256, float scale = 5f, int seed = 12345, int perlinOctaves = 4)
        {
            mapResolution = resolution;
            noiseScale = scale;
            octaves = perlinOctaves;
            
            // Generate unique offsets for each parameter based on seed
            Random.InitState(seed);
            temperatureOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
            altitudeOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
            moistureOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        }
        
        public TerrainData GenerateTerrainData(List<WorldPin> pins)
        {
            var terrainData = new TerrainData(mapResolution)
            {
                temperatureMap = GenerateParameterMap(pins, TerrainParameter.Temperature, temperatureOffset),
                altitudeMap = GenerateParameterMap(pins, TerrainParameter.Altitude, altitudeOffset),
                moistureMap = GenerateParameterMap(pins, TerrainParameter.Moisture, moistureOffset)
            };

            return terrainData;
        }
        
        private float[,] GenerateParameterMap(List<WorldPin> pins, TerrainParameter parameter, Vector2 noiseOffset)
        {
            float[,] map = new float[mapResolution, mapResolution];
            
            // First pass: Generate base Perlin noise
            for (int x = 0; x < mapResolution; x++)
            {
                for (int y = 0; y < mapResolution; y++)
                {
                    float xCoord = (float)x / mapResolution * noiseScale + noiseOffset.x;
                    float yCoord = (float)y / mapResolution * noiseScale + noiseOffset.y;
                    
                    // Multi-octave Perlin noise for more interesting terrain
                    float noiseValue = 0f;
                    float amplitude = 1f;
                    float frequency = 1f;
                    float maxValue = 0f;
                    
                    for (int i = 0; i < octaves; i++)
                    {
                        noiseValue += Mathf.PerlinNoise(xCoord * frequency, yCoord * frequency) * amplitude;
                        maxValue += amplitude;
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }
                    
                    map[x, y] = noiseValue / maxValue; // Normalize to 0-1
                }
            }
            
            // Second pass: Apply pin influences
            for (int x = 0; x < mapResolution; x++)
            {
                for (int y = 0; y < mapResolution; y++)
                {
                    Vector2 worldPos = new Vector2((float)x / mapResolution, (float)y / mapResolution);
                    float totalInfluence = pins.Sum(pin => pin.GetInfluenceAtPosition(worldPos, parameter));
                    
                    // Sum influences from all pins

                    // Blend pin influence with base noise
                    // Use tanh to keep values in reasonable range
                    map[x, y] = Mathf.Clamp01((map[x, y] + totalInfluence * 0.5f));
                }
            }
            
            return map;
        }
    }
    
    /// <summary>
    /// Container for all terrain parameter maps
    /// </summary>
    public class TerrainData
    {
        public float[,] temperatureMap;
        public float[,] altitudeMap;
        public float[,] moistureMap;
        public readonly int resolution;
        
        public TerrainData(int resolution)
        {
            this.resolution = resolution;
            temperatureMap = new float[resolution, resolution];
            altitudeMap = new float[resolution, resolution];
            moistureMap = new float[resolution, resolution];
        }
        
        /// <summary>
        /// Sample terrain data at a normalized world position
        /// </summary>
        public TerrainSample SampleAt(Vector2 worldPos)
        {
            int x = Mathf.Clamp((int)(worldPos.x * resolution), 0, resolution - 1);
            int y = Mathf.Clamp((int)(worldPos.y * resolution), 0, resolution - 1);
            
            return new TerrainSample
            {
                temperature = temperatureMap[x, y],
                altitude = altitudeMap[x, y],
                moisture = moistureMap[x, y]
            };
        }
    }
    
    /// <summary>
    /// Terrain values at a specific position
    /// </summary>
    public struct TerrainSample
    {
        public float temperature;
        public float altitude;
        public float moisture;
        
        public override string ToString()
        { return $"Temp: {temperature:F2}, Alt: {altitude:F2}, Moist: {moisture:F2}"; }
    }
}