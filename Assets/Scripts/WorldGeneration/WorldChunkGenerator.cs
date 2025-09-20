using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace WorldGeneration
{
    public class WorldChunkGenerator
    {
        private readonly int worldSizeInRegions;
        private readonly int samplesPerRegion;
        private readonly RegionDefinition[] regionDefinitions;
        
        public WorldChunkGenerator(RegionDefinition[] regionDefs, int worldSize = 16, int samplesPerRegion = 16)
        {
            regionDefinitions = regionDefs;
            this.worldSizeInRegions = worldSize;
            this.samplesPerRegion = samplesPerRegion;
        }
        
        public RegionGrid CreateRegionGrid(TerrainData terrainData, List<WorldPin> worldPins)
        {
            var grid = new RegionGrid(worldSizeInRegions);
            int regionSize = terrainData.resolution / worldSizeInRegions;
            
            // Create each region
            for (int x = 0; x < worldSizeInRegions; x++)
            {
                for (int y = 0; y < worldSizeInRegions; y++)
                {
                    var region = new Region
                    {
                        gridX = x,
                        gridY = y,
                        worldBounds = new RectInt(
                            x * regionSize,
                            y * regionSize,
                            regionSize,
                            regionSize
                        )
                    };
                    
                    SampleTerrainForRegion(region, terrainData);
                    region.regionType = DetermineRegionType(region.sample);
                    region.regionDefinition = DetermineRegionDefinition(region.regionType);
                    AssociatePinWithRegion(region, worldPins, worldSizeInRegions);
                    
                    grid.SetRegion(x, y, region);
                }
            }
            return grid;
        }

        private RegionDefinition DetermineRegionDefinition(RegionType regionType)
        {
            List<RegionDefinition> possibleRegions = regionDefinitions.Where(
                regionDefinition => regionDefinition.RegionType == regionType
                ).ToList();

            return possibleRegions[Random.Range(0, possibleRegions.Count)];
        }

        private RegionType DetermineRegionType(TerrainSample sample)
        {
            // High altitude regions
            if (sample.altitude > 0.8f) return RegionType.MountainPeak;
            if (sample.altitude > 0.6f && sample.temperature < 0.4f) return RegionType.Mountain;
            if (sample.altitude > 0.6f) return RegionType.Hills;
            
            // Low altitude wet regions
            if (sample.altitude < 0.3f && sample.moisture > 0.7f) return RegionType.Swamp;
            if (sample.altitude < 0.4f && sample.moisture > 0.8f && sample.temperature > 0.6f) return RegionType.Marsh;
            
            // Forests
            if (sample.moisture > 0.5f && sample.temperature > 0.4f && sample.temperature < 0.7f) return RegionType.Forest;
            if (sample.moisture > 0.6f && sample.temperature > 0.7f) return RegionType.Jungle;
            
            // Dry regions
            if (sample.moisture < 0.3f && sample.temperature > 0.6f) return RegionType.Desert;
            
            // Default
            return RegionType.Meadows;
        }
        
        private void SampleTerrainForRegion(Region region, TerrainData terrainData)
        {
            float tempSum = 0, altSum = 0, moistSum = 0;
            int sampleCount = 0;
            
            // Sample at regular intervals within the region
            int stepSize = region.worldBounds.width / samplesPerRegion;
            if (stepSize < 1) stepSize = 1;
            
            for (int x = region.worldBounds.x; x < region.worldBounds.xMax; x += stepSize)
            {
                for (int y = region.worldBounds.y; y < region.worldBounds.yMax; y += stepSize)
                {
                    if (x >= terrainData.resolution || y >= terrainData.resolution) continue;
                    tempSum += terrainData.temperatureMap[x, y];
                    altSum += terrainData.altitudeMap[x, y];
                    moistSum += terrainData.moistureMap[x, y];
                    sampleCount++;
                }
            }
            
            // Store averaged values
            region.sample.temperature = tempSum / sampleCount;
            region.sample.altitude = altSum / sampleCount;
            region.sample.moisture = moistSum / sampleCount;
        }
        
        private static void AssociatePinWithRegion(Region region, List<WorldPin> pins, int gridSize)
        {
            foreach (var pin in pins)
            {
                int pinGridX = Mathf.FloorToInt(pin.position.x * gridSize);
                int pinGridY = Mathf.FloorToInt(pin.position.y * gridSize);
                if (pinGridX != region.gridX || pinGridY != region.gridY) continue;
                region.containedPin = pin;
                return;
            }
        }
    }

    

    
}