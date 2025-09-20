using System.Collections.Generic;
using Interfaces;
using Regions;
using Routing;
using UnityEngine;
using Utils;
using WorldGeneration;
using TerrainData = WorldGeneration.TerrainData;

namespace GameServices
{
    public class WorldManagerService: Service
    {
        [Header("World Config")] 
        [SerializeField] private WorldGenerationConfig worldConfig;
        [SerializeField] private GameObject loadedRegionContainer;
        
        private List<WorldPin> worldPins;
        private RoutePlanner routePlanner;
        private TerrainMapGenerator terrainGenerator;
        private WorldChunkGenerator chunkGenerator;
        private TerrainData terrainData;
        private RegionGrid regionGrid;
        private Region currentRegion;
        private GameObject regionObject;
        private RouteGraph currentRoute;
        private Transform playerTransform;

        private bool worldGenerated;
        // private Vector2Int currentPlayerGridPosition;

        public override void Initialize()
        {
            Logs.Log("World manager service initialized.", "GameServices");
        }

        /// <summary>
        /// This should run before any other script
        /// </summary>
        public void GenerateWorld()
        {
            // Create generators
            worldPins = worldConfig.Pins();
            terrainGenerator = worldConfig.CreateGenerator();
            chunkGenerator = worldConfig.CreateWorldChunkGenerator();
            
            // Initialize terrain, grid, and route planner
            terrainData = terrainGenerator.GenerateTerrainData(worldPins);
            regionGrid = chunkGenerator.CreateRegionGrid(terrainData, worldPins);
            ValidateRegionDefinitions();
            // OK - now instead of initializing the route planner - just connect neighbor regions whenever
            // you load a region
            routePlanner = new RoutePlanner(regionGrid);
            worldGenerated = true;
            
            //FOR NOW: Just put player at the "Home Region" - whatever that is. Like right in the middle.
            //Maybe useful to have a "find home region" world pin status?
            Region homeRegion = regionGrid.GetHomeRegion();
            if (homeRegion == null) { Debug.LogError("No home region found!"); return; }

            var regionVec = new Vector2Int(homeRegion.gridX, homeRegion.gridY);
            LoadRegion(regionVec);
            currentRoute = null;
        }

        private void LoadRegion(Vector2Int regionCoords, SpawnPoint.SpawnType spawnType = SpawnPoint.SpawnType.Default)
        {
            if (!worldGenerated) { Debug.LogError("World not generated yet - check WMS timing"); return; }
    
    
            if (regionGrid == null) { Debug.LogError("regionGrid is null!"); return; }
    
            currentRegion = regionGrid.GetRegion(regionCoords.x, regionCoords.y);
            if (currentRegion == null) { Debug.LogError($"No region found at {regionCoords}!"); return; }
    
    
            if (regionObject != null) { UnloadRegion(); }

            // Check for unique pin region first
            GameObject regionPrefab = currentRegion.GetRegionPrefab();
            if (regionPrefab == null)
            {
                Debug.LogWarning($"No valid region prefab for region {regionCoords.x}, {regionCoords.y}");
                return;
            }

            regionObject = Instantiate(regionPrefab, loadedRegionContainer.transform);
            NotifyRegionLoaded(regionObject, currentRegion);
            var regionManager = regionObject.GetComponent<RegionManager>();
    
            //Spawn player based on spawn location
            var player = GameManager.Instance.GetPlayer();
            playerTransform = player.transform;
            Vector3 spawnPosition = regionManager != null ? regionManager.GetSpawnPosition(spawnType) :
                regionObject.transform.position;
            
            // Use the movement controller's Teleport method for proper Rigidbody handling
            var movementController = player.GetComponent<Inputs.PlayerMovementController>();
            if (movementController != null)
            {
                movementController.Teleport(spawnPosition);
            }
            else
            {
                // Fallback for other movement systems
                Debug.LogWarning("movement controller not found!");
                playerTransform.position = spawnPosition;
            }
            
            ConfigureRegionExits();
        }

        private void UnloadRegion()
        {
            NotifyRegionUnloading(regionObject);
            Destroy(regionObject);
        }
        
        

        #region Implementations

        public Region GetCurrentRegion() { return currentRegion; }
        public RegionGrid GetRegionGrid() { return regionGrid; }
        
        public LocalRegionData GetLocalRegionData(int localRadius = 8)
        {
            if (!worldGenerated || currentRegion == null)
            { Debug.LogWarning("World not generated or no current region"); return null; }
    
            Vector2Int playerWorldPos = new Vector2Int(currentRegion.gridX, currentRegion.gridY);
    
            // Calculate the bounds of our local area
            int minX = Mathf.Max(0, playerWorldPos.x - localRadius);
            int minY = Mathf.Max(0, playerWorldPos.y - localRadius);
            int maxX = Mathf.Min(regionGrid.size - 1, playerWorldPos.x + localRadius);
            int maxY = Mathf.Min(regionGrid.size - 1, playerWorldPos.y + localRadius);
    
            // Adjust local size if we're near world edges
            int actualWidth = maxX - minX + 1;
            int actualHeight = maxY - minY + 1;
    
            Vector2Int worldOffset = new Vector2Int(minX, minY);
            LocalRegionData localData = new LocalRegionData(
                Mathf.Max(actualWidth, actualHeight), 
                playerWorldPos, worldOffset);
    
            // Fill the local region array
            for (int worldX = minX; worldX <= maxX; worldX++)
            {
                for (int worldY = minY; worldY <= maxY; worldY++)
                {
                    int localX = worldX - minX;
                    int localY = worldY - minY;
                    localData.regions[localX, localY] = regionGrid.GetRegion(worldX, worldY);
                }
            }
    
            return localData;
        }

        public RouteStatus GetRouteStatus() { return routePlanner.GetRouteStatus(); }

        public void GenerateRoute(Vector2Int routeDestinationLocation)
        {
            if (!worldGenerated) { Debug.LogError("World not generated yet - check WMS timing"); return; }
            if (!IsValidGridPosition(routeDestinationLocation.x, routeDestinationLocation.y))
            {
                Debug.LogError(
                    $"Route destination {routeDestinationLocation} is outside " +
                    $"world bounds (0,0) to " +
                    $"({worldConfig.worldSizeInRegions-1},{worldConfig.worldSizeInRegions-1})");
                return;
            }

            Region destinationRegion = regionGrid.GetRegion(routeDestinationLocation.x, routeDestinationLocation.y);
            
            if (destinationRegion == null)
            { Debug.LogError($"No region found at destination {routeDestinationLocation}"); return; }

            routePlanner.PlanRoute(new Vector2Int(currentRegion.gridX, currentRegion.gridY), 
                routeDestinationLocation);
            currentRoute = routePlanner.GetCurrentRoute();

            if (regionObject == null) return;
            ConfigureRegionExits();
        }

        public void ClearRoute()
        {
            //1. clear current route
            currentRoute = null;
            
            //2. Clear available exits
            var exits = regionObject.GetComponentsInChildren<RegionExit>();
            foreach (var exit in exits)
            {
                exit.AssignDestination(null);
            }
        }

        public void TransitionToRegion(Vector2Int destinationCoords, SpawnPoint.SpawnType entryDirection)
        {
            if (!worldGenerated) { Debug.LogError("World not generated yet - check WMS timing"); return; }
            if (!IsValidGridPosition(destinationCoords.x, destinationCoords.y))
            {
                Debug.LogError($"Invalid destination coordinates: {destinationCoords}");
                return;
            }

            // Load the new region
            LoadRegion(destinationCoords, entryDirection);
        }

        #endregion
        
        private void ConfigureRegionExits()
        {
            var coords = new Vector2Int(currentRegion.gridX, currentRegion.gridY);
            var exits = regionObject.GetComponentsInChildren<RegionExit>();
            
            //just find the neighbor exits always - unless they're outside the bounds of the map
            foreach (var exit in exits)
            {
                Region dest = exit.direction switch
                {
                    //get the destination node:
                    ConnectionDirection.North => regionGrid.GetRegion(currentRegion.gridX, currentRegion.gridY + 1),
                    ConnectionDirection.South => regionGrid.GetRegion(currentRegion.gridX, currentRegion.gridY - 1),
                    ConnectionDirection.East => regionGrid.GetRegion(currentRegion.gridX + 1, currentRegion.gridY),
                    ConnectionDirection.West => regionGrid.GetRegion(currentRegion.gridX - 1, currentRegion.gridY),
                    _ => null
                };
                exit.AssignDestination(dest);
            }
        }
        private bool IsValidGridPosition(int x, int y)
        {
            return x >= 0 && x < regionGrid.size && y >= 0 && y < regionGrid.size;
        }
        private void NotifyRegionLoaded(GameObject regionObj, Region regionData)
        {
            var regionComponents = regionObj.GetComponentsInChildren<IRegionAware>();
            foreach (var component in regionComponents)
            { component.OnRegionLoaded(regionData); }
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
                if (region.regionDefinition != null) continue;
                Debug.LogError($"Region at ({region.gridX}, {region.gridY}) " +
                               $"of type {region.regionType} has no definition!");
                missingDefinitions++;
            }
    
            if (missingDefinitions > 0)
            {
                Debug.LogError($"Found {missingDefinitions} regions with missing definitions. " +
                               $"Check your RegionDefinitions array!");
            }
        }
    }
}