using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;
using WorldGeneration;
using TerrainData = WorldGeneration.TerrainData;

namespace GameServices
{
    /// <summary>
    /// OLD CODE::: Open-world implementation - OLD - Switched to "Route Ladder" system :::
    /// </summary>
    public class OpenWorldGenerationService : Service
    {
        [Header("World Configuration")]
        [SerializeField] private WorldGenerationConfig worldConfig;
        
        [Header("World Settings")]
        [SerializeField] private float regionWorldSize = 100f;
        [SerializeField] private int loadRadius = 2;
        [SerializeField] private Transform worldContainer;
        
        // Generated data
        private TerrainData terrainData;
        private RegionGrid regionGrid;
        private Dictionary<(int, int), GameObject> loadedRegions = new();
        
        // Runtime state
        private Transform playerTransform;
        private (int x, int y) lastPlayerGridPos = (-1, -1);
        
        public override void Initialize()
        {
            Logs.Log("Open world generation service initialized.", "GameServices");
        }
        
        public void GenerateWorld()
        {
            if (worldConfig == null) { Debug.LogError("No WorldGenerationConfig assigned!"); return; }
            
            Debug.Log("Starting world generation...");
            
            // Step 1: Generate terrain data
            var terrainGenerator = worldConfig.CreateGenerator();
            terrainData = terrainGenerator.GenerateTerrainData(worldConfig.Pins());
            
            // Step 2: Generate region grid
            var chunkGenerator = new WorldChunkGenerator(
                worldConfig.regionDefinitions,
                worldConfig.worldSizeInRegions,
                samplesPerRegion: 16
            );
            regionGrid = chunkGenerator.CreateRegionGrid(terrainData, worldConfig.Pins());
            
            // Step 3: Validate and assign region definitions
            ValidateRegionDefinitions();
            
            // Step 4: Setup world container
            if (!worldContainer)
            {
                GameObject containerGo = new GameObject("WorldRegions");
                worldContainer = containerGo.transform;
            }
            
            // Step 5: Get player reference and start loading regions
            playerTransform = GameManager.Instance.GetPlayer()?.transform;
            if (playerTransform != null) { UpdateLoadedRegions(); }
            else { Debug.LogWarning("No player found - regions will load when player is available"); }
            
            Debug.Log($"World generation complete! Generated {regionGrid.size}x{regionGrid.size} regions.");
        }
        
        private void Update()
        {
            if (regionGrid == null || playerTransform == null)
            {
                if (playerTransform == null) { playerTransform = GameManager.Instance.GetPlayer()?.transform; }
                return;
            }
            
            // Check player position every half second
            if (Time.frameCount % 30 != 0) return;
            var currentGridPos = GetGridPosition(playerTransform.position);
            if (currentGridPos == lastPlayerGridPos) return;
            lastPlayerGridPos = currentGridPos;
            UpdateLoadedRegions();
        }
        
        private void UpdateLoadedRegions()
        {
            var playerGridPos = GetGridPosition(playerTransform.position);
            
            // Determine which regions should be loaded
            HashSet<(int, int)> regionsToKeep = new();
            
            for (int x = -loadRadius; x <= loadRadius; x++)
            {
                for (int y = -loadRadius; y <= loadRadius; y++)
                {
                    int gridX = playerGridPos.x + x;
                    int gridY = playerGridPos.y + y;
                    if (IsValidGridPosition(gridX, gridY)) { regionsToKeep.Add((gridX, gridY)); }
                }
            }
            
            // Unload distant regions
            List<(int, int)> toUnload = (from kvp in loadedRegions 
                where !regionsToKeep.Contains(kvp.Key) select kvp.Key).ToList();

            foreach (var coords in toUnload) { UnloadRegion(coords); }
            
            // Load new regions
            foreach (var coords in regionsToKeep.Where(
                         coords => !loadedRegions.ContainsKey(coords))) { LoadRegion(coords); }
        }
        
        private void LoadRegion((int x, int y) gridCoords)
        {
            var region = regionGrid.GetRegion(gridCoords.x, gridCoords.y);
            if (region.regionDefinition.RegionObject == null)
            {
                Debug.LogWarning($"No valid region definition for region {gridCoords.x}, {gridCoords.y}");
                return;
            }
            
            // Calculate world position
            Vector3 worldPosition = new Vector3(
                gridCoords.x * regionWorldSize,
                gridCoords.y * regionWorldSize,
                0
            );
            
            // Instantiate the region prefab
            GameObject regionInstance = Instantiate(
                region.regionDefinition.RegionObject,
                worldPosition,
                Quaternion.identity,
                worldContainer
            );
            
            regionInstance.name = $"Region_{gridCoords.x}_{gridCoords.y}_{region.regionType}";
            loadedRegions[gridCoords] = regionInstance;
            StartCoroutine(RefreshShadowCasters(regionInstance));
            NotifyRegionLoaded(regionInstance, region);
        }
        
        private void UnloadRegion((int x, int y) gridCoords)
        {
            if (!loadedRegions.TryGetValue(gridCoords, out GameObject regionObj)) return;
            NotifyRegionUnloading(regionObj);
            Destroy(regionObj);
            loadedRegions.Remove(gridCoords);
        }
        
        private void NotifyRegionLoaded(GameObject regionObj, Region regionData)
        {
            var regionComponents = regionObj.GetComponentsInChildren<IRegionAware>();
            //TODO: Fix this here
            // foreach (var component in regionComponents)
            // { component.OnRegionLoaded(regionData, regionWorldSize); }
        }
        
        private static void NotifyRegionUnloading(GameObject regionObj)
        {
            var regionComponents = regionObj.GetComponentsInChildren<IRegionAware>();
            foreach (var component in regionComponents) { component.OnRegionUnloading(); }
        }
        
        private void ValidateRegionDefinitions()
        {
            int missingDefinitions = 0;
    
            foreach (var region in regionGrid.GetAllRegions())
            {
                if (region.regionDefinition == null)
                {
                    Debug.LogError($"Region at ({region.gridX}, {region.gridY}) of type {region.regionType} has no definition!");
                    missingDefinitions++;
                }
            }
    
            if (missingDefinitions > 0)
            {
                Debug.LogError($"Found {missingDefinitions} regions with missing definitions. " +
                               $"Check your RegionDefinitions array!");
            }
        }
        
        // Helper methods
        private (int x, int y) GetGridPosition(Vector3 worldPosition)
        {
            int gridX = Mathf.FloorToInt(worldPosition.x / regionWorldSize);
            int gridY = Mathf.FloorToInt(worldPosition.y / regionWorldSize);
            return (gridX, gridY);
        }
        
        private bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < regionGrid.size && y >= 0 && y < regionGrid.size;
        }
        
        //Silly method to force re-registering shadow casters to the 2D lighting system. Apparently it's a known bug.
        private IEnumerator RefreshShadowCasters(GameObject regionObj)
        {
            yield return null;
    
            // Find all ShadowCaster2D components (including on tilemaps)
            var shadowCasters = regionObj.GetComponentsInChildren<ShadowCaster2D>();
    
            foreach (var shadowCaster in shadowCasters)
            {
                shadowCaster.enabled = false;
                yield return null;
                shadowCaster.enabled = true;
            }
    
            Debug.Log($"Refreshed {shadowCasters.Length} shadow casters for region {regionObj.name}");
        }
        

        #region Interface Implementation

        public Region GetRegionAt(Vector3 worldPosition)
        {
            if (regionGrid == null) return null;
            var gridPos = GetGridPosition(worldPosition);
            return regionGrid.GetRegion(gridPos.x, gridPos.y);
        }
        
        public List<WorldPin> GetAllWorldPins()
        {
            return worldConfig.Pins() ?? new List<WorldPin>();
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // foreach (var kvp in loadedRegions.Where(
            //              kvp => kvp.Value != null)) { Destroy(kvp.Value); }
            loadedRegions.Clear();
        }
    }
    
    
}