using System.Collections.Generic;
using System.Linq;
using Routing;
using UnityEngine;
using WorldGeneration;
using TerrainData = WorldGeneration.TerrainData;

namespace Tests
{
    /// <summary>
    /// Test visualizer for the world chunk generation system
    /// </summary>
    public class WorldChunkGeneratorTest : MonoBehaviour
    {
        [Header("World Config")] 
        [SerializeField] private WorldGenerationConfig worldConfig;
        
        [Header("Visualization Options")]
        [SerializeField] private bool showTerrainMaps;
        [SerializeField] private bool showRegionGrid = true;
        [SerializeField] private bool showPinMarkers = true;
        [SerializeField] private bool showRegionInfo = true;
        
        [Header("Route Testing")]
        [SerializeField] private Vector2Int routeOrigin = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int routeDestination = new Vector2Int(10, 10);
        
        // Core components
        private RoutePlanner routePlanner;
        private TerrainMapGenerator terrainGenerator;
        private WorldChunkGenerator chunkGenerator;
        private TerrainData terrainData;
        private List<WorldPin> worldPins;
        private RegionGrid regionGrid;
        private RouteGraph currentRoute;
        
        // Visualization
        private Texture2D regionGridTexture;
        private Texture2D temperatureTexture;
        private Texture2D altitudeTexture;
        private Texture2D moistureTexture;
        private Dictionary<RegionType, Color> regionColors;
        
        // UI state
        private Vector2 mouseWorldPos;
        private Region hoveredRegion;
        
        private void Start()
        {
            InitializeColors();
            GenerateWorld();
        }

        private void InitializeColors()
        {
            regionColors = new Dictionary<RegionType, Color>
            {
                { RegionType.Forest, new Color(0.1f, 0.5f, 0.1f) },
                { RegionType.Mountain, new Color(0.5f, 0.5f, 0.6f) },
                { RegionType.MountainPeak, new Color(0.9f, 0.9f, 1f) },
                { RegionType.Hills, new Color(0.6f, 0.5f, 0.3f) },
                { RegionType.Swamp, new Color(0.2f, 0.4f, 0.3f) },
                { RegionType.Marsh, new Color(0.3f, 0.5f, 0.4f) },
                { RegionType.Meadows, new Color(0.5f, 0.7f, 0.3f) },
                { RegionType.Desert, new Color(0.9f, 0.8f, 0.4f) },
                { RegionType.Jungle, new Color(0.0f, 0.3f, 0.0f) }
            };
        }

        private void GenerateWorld()
        {
            // Create generators
            worldPins = worldConfig.Pins();
            terrainGenerator = worldConfig.CreateGenerator();
            chunkGenerator = new WorldChunkGenerator(worldConfig.regionDefinitions, worldConfig.worldSizeInRegions);
            
            // Initialize terrain, grid, and route planner
            terrainData = terrainGenerator.GenerateTerrainData(worldPins);
            regionGrid = chunkGenerator.CreateRegionGrid(terrainData, worldPins);
            routePlanner = new RoutePlanner(regionGrid);
            
            // Create visualizations
            CreateRegionGridTexture();
            if (showTerrainMaps) CreateTerrainTextures();
            
            currentRoute = null;
            LogWorldStatistics();
        }
        
        private void GenerateRoute()
        {
            // Validate coordinates
            if (routeOrigin.x < 0 || routeOrigin.x >= worldConfig.worldSizeInRegions ||
                routeOrigin.y < 0 || routeOrigin.y >= worldConfig.worldSizeInRegions)
            {
                Debug.LogError(
                    $"Route origin {routeOrigin} is outside " +
                    $"world bounds (0,0) to " +
                    $"({worldConfig.worldSizeInRegions-1},{worldConfig.worldSizeInRegions-1})");
                return;
            }
            
            if (routeDestination.x < 0 || routeDestination.x >= worldConfig.worldSizeInRegions ||
                routeDestination.y < 0 || routeDestination.y >= worldConfig.worldSizeInRegions)
            {
                Debug.LogError(
                    $"Route destination {routeDestination} is outside " +
                    $"world bounds (0,0) to " +
                    $"({worldConfig.worldSizeInRegions-1},{worldConfig.worldSizeInRegions-1})");
                return;
            }
            
            // Get origin and destination regions
            Region originRegion = regionGrid.GetRegion(routeOrigin.x, routeOrigin.y);
            Region destinationRegion = regionGrid.GetRegion(routeDestination.x, routeDestination.y);
            
            if (originRegion == null)
            {
                Debug.LogError($"No region found at origin {routeOrigin}");
                return;
            }
            
            if (destinationRegion == null)
            {
                Debug.LogError($"No region found at destination {routeDestination}");
                return;
            }

            routePlanner.PlanRoute(routeOrigin, routeDestination);
            currentRoute = routePlanner.GetCurrentRoute();
            
            // Recreate the texture to show the route
            CreateRegionGridTexture();
            
            // Log route info
            Debug.Log($"Generated route: {currentRoute.GetRouteDescription()}");
            Debug.Log($"Route has {currentRoute.nodes.Count} total nodes:");
            // Log all nodes
            foreach (var node in currentRoute.nodes)
            {
                string nodeInfo = $"  {node.nodeType}: {node.region.regionType} at ({node.region.gridX}, {node.region.gridY})";
                if (node.connections.Count > 0)
                {
                    var directions = node.connections.Select(c => c.direction.ToString());
                    nodeInfo += $" → exits: {string.Join(", ", directions)}";
                }
                Debug.Log(nodeInfo);
            }
        }
        
        private void CreateRegionGridTexture()
        {
            int textureSize = worldConfig.worldSizeInRegions * 32;
            regionGridTexture = new Texture2D(textureSize, textureSize)
            { filterMode = FilterMode.Point };
        
            Color[] pixels = new Color[textureSize * textureSize];
            
            // Create lookup for route nodes
            HashSet<Vector2Int> routeRegions = new HashSet<Vector2Int>();
            HashSet<Vector2Int> branchRegions = new HashSet<Vector2Int>();
            
            if (currentRoute != null)
            {
                foreach (var node in currentRoute.nodes)
                {
                    Vector2Int pos = new Vector2Int(node.region.gridX, node.region.gridY);
                    routeRegions.Add(pos);
                    
                    if (node.nodeType == NodeType.Branch)
                    {
                        branchRegions.Add(pos);
                    }
                }
            }
            
            // Draw each region
            for (int rx = 0; rx < worldConfig.worldSizeInRegions; rx++)
            {
                for (int ry = 0; ry < worldConfig.worldSizeInRegions; ry++)
                {
                    var region = regionGrid.GetRegion(rx, ry);
                    if (region == null) continue;
                    
                    Color regionColor = regionColors[region.regionType];
                    
                    // Highlight different node types
                    bool isOnRoute = routeRegions.Contains(new Vector2Int(rx, ry));
                    bool isBranch = branchRegions.Contains(new Vector2Int(rx, ry));
                    bool isOrigin = currentRoute != null && rx == routeOrigin.x && ry == routeOrigin.y;
                    bool isDestination = currentRoute != null && rx == routeDestination.x && ry == routeDestination.y;
                    
                    if (isOrigin)
                    {
                        regionColor = Color.green; // Origin in bright green
                    }
                    else if (isDestination)
                    {
                        regionColor = Color.red; // Destination in bright red
                    }
                    else if (isBranch)
                    {
                        regionColor = Color.Lerp(regionColor, Color.magenta, 0.7f); // Branch nodes in magenta
                    }
                    else if (isOnRoute)
                    {
                        regionColor = Color.Lerp(regionColor, Color.yellow, 0.6f); // Main path in yellow
                    }
                    
                    // Fill region pixels
                    for (int px = 0; px < 32; px++)
                    {
                        for (int py = 0; py < 32; py++)
                        {
                            int x = rx * 32 + px;
                            int y = ry * 32 + py;
                            
                            // Draw border (thicker for route segments)
                            int borderWidth = (isOnRoute || isOrigin || isDestination) ? 2 : 1;
                            if (px < borderWidth || px >= 32 - borderWidth || py < borderWidth || py >= 32 - borderWidth)
                            {
                                pixels[y * textureSize + x] = Color.black;
                            }
                            else
                            {
                                pixels[y * textureSize + x] = regionColor;
                            }
                        }
                    }
                }
            }
            
            regionGridTexture.SetPixels(pixels);
            regionGridTexture.Apply();
    
            // Add connection lines after the base texture is created
            DrawConnectionLines();
            regionGridTexture.Apply(); // Apply the line changes
        }
        
        private void DrawConnectionLines()
        {
            if (currentRoute == null) return;
            
            // Draw lines for all connections
            foreach (var node in currentRoute.nodes)
            {
                Vector2Int fromPos = new Vector2Int(node.region.gridX, node.region.gridY);
                
                foreach (var connection in node.connections)
                {
                    Vector2Int toPos = new Vector2Int(connection.toNode.region.gridX, connection.toNode.region.gridY);
                    DrawConnectionLine(fromPos, toPos, connection);
                }
            }
        }
        
        private void DrawConnectionLine(Vector2Int from, Vector2Int to, RouteConnection connection)
        {
            if (regionGridTexture == null) return;
            
            int textureSize = regionGridTexture.width;
            int regionPixelSize = textureSize / worldConfig.worldSizeInRegions;
            
            // Calculate center points of regions in texture coordinates
            Vector2 fromCenter = new Vector2(
                (from.x + 0.5f) * regionPixelSize,
                (from.y + 0.5f) * regionPixelSize
            );
            
            Vector2 toCenter = new Vector2(
                (to.x + 0.5f) * regionPixelSize,
                (to.y + 0.5f) * regionPixelSize
            );
            
            // Choose line color based on connection type
            Color lineColor;
            if (connection.fromNode.nodeType == NodeType.Branch || connection.toNode.nodeType == NodeType.Branch)
            { lineColor = Color.cyan; }
            else
            { lineColor = Color.white; }

            DrawLineOnTexture(fromCenter, toCenter, lineColor, 2);
        }
        
        private void DrawLineOnTexture(Vector2 start, Vector2 end, Color color, int thickness = 1)
        {
            // Bresenham's line algorithm with thickness
            int x0 = Mathf.RoundToInt(start.x);
            int y0 = Mathf.RoundToInt(start.y);
            int x1 = Mathf.RoundToInt(end.x);
            int y1 = Mathf.RoundToInt(end.y);
            
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            int x = x0;
            int y = y0;
            
            while (true)
            {
                // Draw thick point
                DrawThickPoint(x, y, color, thickness);
                
                if (x == x1 && y == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }
        
        private void DrawThickPoint(int centerX, int centerY, Color color, int thickness)
        {
            int textureSize = regionGridTexture.width;
            int halfThickness = thickness / 2;
            
            for (int dx = -halfThickness; dx <= halfThickness; dx++)
            {
                for (int dy = -halfThickness; dy <= halfThickness; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    // Check bounds
                    if (x >= 0 && x < textureSize && y >= 0 && y < textureSize)
                    {
                        // Blend with existing color for semi-transparency
                        Color existingColor = regionGridTexture.GetPixel(x, y);
                        Color blendedColor = Color.Lerp(existingColor, color, 0.8f);
                        regionGridTexture.SetPixel(x, y, blendedColor);
                    }
                }
            }
        }
        
        private void CreateTerrainTextures()
        {
            // Temperature map (blue to red)
            temperatureTexture = CreateParameterTexture(terrainData.temperatureMap,
                new Color(0, 0, 1), new Color(1, 0, 0));
            
            // Altitude map (green to white)    
            altitudeTexture = CreateParameterTexture(terrainData.altitudeMap,
                new Color(0, 0.5f, 0), new Color(1, 1, 1));
            
            // Moisture map (tan to blue)
            moistureTexture = CreateParameterTexture(terrainData.moistureMap,
                new Color(0.8f, 0.8f, 0), new Color(0, 0, 0.8f));
        }
        
        private Texture2D CreateParameterTexture(float[,] map, Color lowColor, Color highColor)
        {
            var texture = new Texture2D(worldConfig.mapResolution, worldConfig.mapResolution);
            var pixels = new Color[worldConfig.mapResolution * worldConfig.mapResolution];
            
            for (int x = 0; x < worldConfig.mapResolution; x++)
            {
                for (int y = 0; y < worldConfig.mapResolution; y++)
                {
                    float value = map[x, y];
                    pixels[y * worldConfig.mapResolution + x] = Color.Lerp(lowColor, highColor, value);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private void LogWorldStatistics()
        {
            Dictionary<RegionType, int> typeCounts = new Dictionary<RegionType, int>();
            int regionsWithPins = 0;
            
            foreach (var region in regionGrid.GetAllRegions())
            {
                typeCounts.TryAdd(region.regionType, 0);
                typeCounts[region.regionType]++;
                
                if (region.containedPin != null) regionsWithPins++;
            }
            
            Debug.Log($"=== World Generation Complete ===");
            Debug.Log($"World size: {worldConfig.worldSizeInRegions}x{worldConfig.worldSizeInRegions} = " +
                      $"{worldConfig.worldSizeInRegions * worldConfig.worldSizeInRegions} regions");
            Debug.Log($"Regions with pins: {regionsWithPins}");
            
            Debug.Log("Region type distribution:");
            foreach (var kvp in typeCounts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value} regions");
            }
        }
        
        void OnGUI()
        {
            if (regionGrid == null) return;
            
            int mainSize = 512;
            int padding = 10;
            
            
            // Main region grid display
            if (showRegionGrid)
            {
                GUI.Label(new Rect(padding, padding, 300, 20), 
                    $"World Regions ({worldConfig.worldSizeInRegions}x{worldConfig.worldSizeInRegions})");
                
                Rect gridRect = new Rect(padding, padding + 25, mainSize, mainSize);
                GUI.DrawTexture(gridRect, regionGridTexture);
                
                // Track mouse position
                Event e = Event.current;
                if (gridRect.Contains(e.mousePosition))
                {
                    float relX = (e.mousePosition.x - gridRect.x) / gridRect.width;
                    float relY = (e.mousePosition.y - gridRect.y) / gridRect.height;
                    
                    int gridX = Mathf.FloorToInt(relX * worldConfig.worldSizeInRegions);
                    int gridY = Mathf.FloorToInt((1 - relY) * worldConfig.worldSizeInRegions); // Flip Y
                    
                    hoveredRegion = regionGrid.GetRegion(gridX, gridY);
                }
                
                // Draw pin markers
                if (showPinMarkers)
                {
                    foreach (var pin in worldPins)
                    {
                        float x = gridRect.x + pin.position.x * gridRect.width;
                        float y = gridRect.y + (1 - pin.position.y) * gridRect.height;
                        
                        GUI.color = Color.red;
                        GUI.Label(new Rect(x - 10, y - 10, 20, 20), "★");
                        GUI.color = Color.white;
                        GUI.Label(new Rect(x + 12, y - 10, 200, 20), pin.name);
                    }
                }
            }
            
            // Region info panel
            if (showRegionInfo && hoveredRegion != null)
            {
                int infoX = padding + mainSize + 20;
                int infoY = padding + 25;
                
                GUI.Box(new Rect(infoX, infoY, 250, 150), "Region Info");
                
                int labelY = infoY + 25;
                GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                    $"Position: ({hoveredRegion.gridX}, {hoveredRegion.gridY})");
                labelY += 20;
                
                GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                    $"Type: {hoveredRegion.regionType}");
                labelY += 20;
                
                GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                    $"Temp: {hoveredRegion.sample.temperature:F2}");
                labelY += 20;
                
                GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                    $"Alt: {hoveredRegion.sample.altitude:F2}");
                labelY += 20;
                
                GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                    $"Moist: {hoveredRegion.sample.moisture:F2}");
                labelY += 20;
                
                if (hoveredRegion.containedPin != null)
                {
                    GUI.Label(new Rect(infoX + 10, labelY, 230, 20), 
                        $"Pin: {string.Join(", ", hoveredRegion.containedPin.name)}");
                }
            }
            
            // Route info panel
            if (currentRoute != null)
            {
                int routeInfoX = padding + mainSize + 20;
                int routeInfoY = padding + 200;
    
                GUI.Box(new Rect(routeInfoX, routeInfoY, 250, 140), "Current Route Graph");
    
                int labelY = routeInfoY + 25;
                GUI.Label(new Rect(routeInfoX + 10, labelY, 230, 20), 
                    $"Origin: ({routeOrigin.x}, {routeOrigin.y})");
                labelY += 20;
    
                GUI.Label(new Rect(routeInfoX + 10, labelY, 230, 20), 
                    $"Destination: ({routeDestination.x}, {routeDestination.y})");
                labelY += 20;
    
                GUI.Label(new Rect(routeInfoX + 10, labelY, 230, 20), 
                    $"Total Nodes: {currentRoute.nodes.Count}");
                labelY += 20;
    
                // Count different node types
                int mainPathNodes = currentRoute.nodes.Count(n => n.nodeType == NodeType.MainPath);
                int branchNodes = currentRoute.nodes.Count(n => n.nodeType == NodeType.Branch);
    
                GUI.Label(new Rect(routeInfoX + 10, labelY, 230, 20), 
                    $"Main Path: {mainPathNodes + 2}"); // +2 for origin/destination
                labelY += 20;
    
                GUI.Label(new Rect(routeInfoX + 10, labelY, 230, 20), 
                    $"Branches: {branchNodes}");
            }
            
            // Terrain parameter maps (optional)
            if (showTerrainMaps)
            {
                int mapSize = 150;
                int mapY = padding + mainSize + 60;
                
                GUI.Label(new Rect(padding, mapY, 150, 20), "Temperature");
                GUI.DrawTexture(new Rect(padding, mapY + 20, mapSize, mapSize), temperatureTexture);
                
                GUI.Label(new Rect(padding + mapSize + 10, mapY, 150, 20), "Altitude");
                GUI.DrawTexture(new Rect(padding + mapSize + 10, mapY + 20, mapSize, mapSize), altitudeTexture);
                
                GUI.Label(new Rect(padding + (mapSize + 10) * 2, mapY, 150, 20), "Moisture");
                GUI.DrawTexture(new Rect(padding + (mapSize + 10) * 2, mapY + 20, mapSize, mapSize), moistureTexture);
            }
            
            // Control buttons
            int buttonY = Screen.height - 80;
            int buttonWidth = 120;
            int buttonSpacing = 130;
            
            if (GUI.Button(new Rect(padding, buttonY, 150, 30), "Regenerate World"))
            {
                worldConfig.worldSeed = Random.Range(0, 100000);
                GenerateWorld();
            }
            
            if (GUI.Button(new Rect(padding + 160, buttonY, 150, 30), "Generate Route"))
            {
                GenerateRoute();
            }
            
            // Clear route button
            if (currentRoute != null && GUI.Button(new Rect(padding + 320, buttonY, 150, 30), "Clear Route"))
            {
                currentRoute = null;
                CreateRegionGridTexture(); // Refresh to remove route highlighting
            }

            // Travel buttons - second row
            int travelButtonY = buttonY - 40;
            if (GUI.Button(new Rect(padding + 480, travelButtonY, buttonWidth, 30), "Travel North"))
            {
                if (routePlanner.TravelToExit(ConnectionDirection.North))
                {
                    var status = routePlanner.GetRouteStatus();
                    Debug.Log($"Moved North! Current region: {status.currentRegion?.regionType} at ({status.currentRegion?.gridX}, {status.currentRegion?.gridY})");
                    Debug.Log($"Route complete: {status.isComplete}");
                    PrintCurrentExits();
                }
                else { Debug.LogWarning("Cannot travel North from current position"); }
            }
            
            if (GUI.Button(new Rect(padding + 480 + buttonSpacing, travelButtonY, buttonWidth, 30), "Travel South"))
            {
                if (routePlanner.TravelToExit(ConnectionDirection.South))
                {
                    var status = routePlanner.GetRouteStatus();
                    Debug.Log($"Moved South! Current region: {status.currentRegion?.regionType} at ({status.currentRegion?.gridX}, {status.currentRegion?.gridY})");
                    Debug.Log($"Route complete: {status.isComplete}");
                    PrintCurrentExits();
                }
                else { Debug.LogWarning("Cannot travel South from current position"); }
            }
            
            if (GUI.Button(new Rect(padding + 480 + buttonSpacing * 2, travelButtonY, buttonWidth, 30), "Travel East"))
            {
                if (routePlanner.TravelToExit(ConnectionDirection.East))
                {
                    var status = routePlanner.GetRouteStatus();
                    Debug.Log($"Moved East! Current region: {status.currentRegion?.regionType} at ({status.currentRegion?.gridX}, {status.currentRegion?.gridY})");
                    Debug.Log($"Route complete: {status.isComplete}");
                    PrintCurrentExits();
                }
                else { Debug.LogWarning("Cannot travel East from current position"); }
            }
            
            if (GUI.Button(new Rect(padding + 480 + buttonSpacing * 3, travelButtonY, buttonWidth, 30), "Travel West"))
            {
                if (routePlanner.TravelToExit(ConnectionDirection.West))
                {
                    var status = routePlanner.GetRouteStatus();
                    Debug.Log($"Moved West! Current region: {status.currentRegion?.regionType} at ({status.currentRegion?.gridX}, {status.currentRegion?.gridY})");
                    Debug.Log($"Route complete: {status.isComplete}");
                    PrintCurrentExits();
                }
                else { Debug.LogWarning("Cannot travel West from current position"); }
            }
            
            // Route status display
            if (routePlanner == null) return;
            var curStatus = routePlanner.GetRouteStatus();
            if (!curStatus.hasActiveRoute) return;
            GUI.Label(new Rect(padding + 480, travelButtonY - 25, 400, 20), 
                $"Current: {curStatus.currentRegion?.regionType} " +
                $"({curStatus.currentRegion?.gridX},{curStatus.currentRegion?.gridY}) | " +
                $"Exits: {curStatus.availableExitCount}");
            
        }

        private void PrintCurrentExits()
        {
            var exits = routePlanner.GetAvailableExits();
            Debug.Log($"Available exits: {string.Join(", ", exits.Select(e => $"{e.direction}: {e.displayName}"))}");
        }
    }
}