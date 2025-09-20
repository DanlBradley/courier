using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;
using TerrainData = WorldGeneration.TerrainData;

namespace Tests
{
    /// <summary>
    /// Test component to demonstrate world generation with pins
    /// </summary>
    public class WorldGeneratorTest : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int mapResolution = 256;
        [SerializeField] private float noiseScale = 5f;
        [SerializeField] private int worldSeed = 12345;
        
        [Header("Debug Visualization")]
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool showDebugGUI = true;
        
        private TerrainMapGenerator generator;
        private TerrainData terrainData;
        private List<WorldPin> worldPins;
        
        // Visualization textures
        private Texture2D temperatureTexture;
        private Texture2D altitudeTexture;
        private Texture2D moistureTexture;
        private Texture2D compositeTexture;
        
        private void Start()
        {
            if (generateOnStart)
            {
                GenerateWorld();
            }
        }
        
        private void GenerateWorld()
        {
            generator = new TerrainMapGenerator(mapResolution, noiseScale, worldSeed);
            worldPins = CreateExamplePins();
            terrainData = generator.GenerateTerrainData(worldPins);
            CreateVisualizationTextures();
        }
        
        private List<WorldPin> CreateExamplePins()
        {
            var pins = new List<WorldPin>();
            
            // Central city
            var cityPin = new WorldPin("Home Village", new Vector2(0.8f, 0.2f), PinType.City);
            pins.Add(cityPin);
            
            // Mountain fortress to the north
            var fortressPin = new WorldPin("Mountain Fort", new Vector2(0.5f, 0.8f), PinType.Fortress);
            fortressPin.influenceRadius = 0.25f;
            fortressPin.altitudeInfluence = 0.8f;
            pins.Add(fortressPin);
            
            // Mystical site in the east
            var mysticalPin = new WorldPin("Wizard's Hut", new Vector2(0.7f, 0.3f), PinType.MysticalSite);
            mysticalPin.moistureInfluence = 0.1f;
            pins.Add(mysticalPin);
            
            // Swamp ruins in the southwest
            var ruinsPin = new WorldPin("Bramble", new Vector2(0.2f, 0.3f), PinType.Ruins);
            ruinsPin.altitudeInfluence = -0.5f; // Force low ground
            ruinsPin.moistureInfluence = 0.2f;  // Very wet
            pins.Add(ruinsPin);
            
            // Natural lake in the west
            var lakePin = new WorldPin("Lake", new Vector2(0.15f, 0.6f), PinType.NaturalFeature);
            lakePin.altitudeInfluence = -0.5f;
            lakePin.moistureInfluence = 1f;
            lakePin.temperatureInfluence = 0.1f;
            pins.Add(lakePin);
            
            return pins;
        }
        
        private void CreateVisualizationTextures()
        {
            temperatureTexture = CreateParameterTexture(terrainData.temperatureMap, 
                new Color(0, 0, 1), new Color(1, 0, 0)); // Blue to Red
                
            altitudeTexture = CreateParameterTexture(terrainData.altitudeMap,
                new Color(0, 0.5f, 0), new Color(1, 1, 1)); // Green to White
                
            moistureTexture = CreateParameterTexture(terrainData.moistureMap,
                new Color(0.8f, 0.8f, 0), new Color(0, 0, 0.8f)); // Tan to Blue
                
            compositeTexture = CreateCompositeTexture();
        }
        
        private Texture2D CreateParameterTexture(float[,] map, Color lowColor, Color highColor)
        {
            var texture = new Texture2D(mapResolution, mapResolution);
            var pixels = new Color[mapResolution * mapResolution];
            
            for (int x = 0; x < mapResolution; x++)
            {
                for (int y = 0; y < mapResolution; y++)
                {
                    float value = map[x, y];
                    pixels[y * mapResolution + x] = Color.Lerp(lowColor, highColor, value);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private Texture2D CreateCompositeTexture()
        {
            var texture = new Texture2D(mapResolution, mapResolution);
            var pixels = new Color[mapResolution * mapResolution];
            
            for (int x = 0; x < mapResolution; x++)
            {
                for (int y = 0; y < mapResolution; y++)
                {
                    float temp = terrainData.temperatureMap[x, y];
                    float alt = terrainData.altitudeMap[x, y];
                    float moist = terrainData.moistureMap[x, y];
                    
                    // Create biome-like colors based on parameters
                    Color biomeColor = GetBiomeColor(temp, alt, moist);
                    pixels[y * mapResolution + x] = biomeColor;
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private Color GetBiomeColor(float temp, float alt, float moist)
        {
            // Simple biome determination
            if (alt > 0.8f) return new Color(1, 1, 1); // Snow
            if (alt > 0.6f && temp < 0.4f) return new Color(0.5f, 0.5f, 0.6f); // Mountain
            if (moist > 0.7f && alt < 0.3f) return new Color(0.2f, 0.4f, 0.3f); // Swamp
            if (moist > 0.6f && temp > 0.5f) return new Color(0.1f, 0.5f, 0.1f); // Jungle
            if (moist < 0.3f && temp > 0.6f) return new Color(0.9f, 0.8f, 0.4f); // Desert
            if (moist > 0.4f) return new Color(0.2f, 0.7f, 0.2f); // Forest
            return new Color(0.7f, 0.8f, 0.3f); // Grassland
        }
        
        // Debug GUI for visualization
        void OnGUI()
        {
            if (!showDebugGUI || terrainData == null) return;
            
            int size = 200;
            int padding = 10;
            
            GUI.Label(new Rect(padding, padding, size, 20), "Temperature Map");
            GUI.DrawTexture(new Rect(padding, padding + 20, size, size), temperatureTexture);
            
            GUI.Label(new Rect(padding + size + 10, padding, size, 20), "Altitude Map");
            GUI.DrawTexture(new Rect(padding + size + 10, padding + 20, size, size), altitudeTexture);
            
            GUI.Label(new Rect(padding, padding + size + 40, size, 20), "Moisture Map");
            GUI.DrawTexture(new Rect(padding, padding + size + 60, size, size), moistureTexture);
            
            GUI.Label(new Rect(padding + size + 10, padding + size + 40, size, 20), "Composite Biomes");
            GUI.DrawTexture(new Rect(padding + size + 10, padding + size + 60, size, size), compositeTexture);
            
            // Draw pin markers
            foreach (var pin in worldPins)
            {
                Vector2 screenPos = new Vector2(
                    padding + size + 10 + pin.position.x * size,
                    padding + size + 60 + (1f - pin.position.y) * size
                );
                
                GUI.color = Color.red;
                GUI.Label(new Rect(screenPos.x - 5, screenPos.y - 5, 10, 10), "●");
                GUI.color = Color.white;
                GUI.Label(new Rect(screenPos.x + 10, screenPos.y - 10, 100, 20), pin.name);
            }
            
            // Regenerate button
            if (GUI.Button(new Rect(padding, Screen.height - 40, 150, 30), "Regenerate World"))
            {
                worldSeed = Random.Range(0, 100000);
                GenerateWorld();
            }
        }
    }
}