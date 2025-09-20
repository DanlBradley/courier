using UnityEngine;
using System;
using EditorTools;

namespace WorldGeneration
{
    /// <summary>
    /// Represents a designer-placed pin that influences world generation
    /// </summary>
    [Serializable]
    public class WorldPin
    {
        public string name;
        [Vector2Field] public Vector2 position; // Position on 2D world map (0-1 normalized)
        public PinType pinType;
        public Sprite pinSprite;
        
        [Range(-1f, 1f)] public float temperatureInfluence;
        [Range(-1f, 1f)] public float altitudeInfluence;
        [Range(-1f, 1f)] public float moistureInfluence;
        
        [Range(0.1f, 1f)] public float influenceRadius = 0.3f;
        [Range(0.5f, 4f)] public float influenceFalloff = 2f;
        public bool homePin;
        public GameObject uniquePinRegion;
        
        public WorldPin(string name, Vector2 position, PinType type)
        {
            this.name = name;
            this.position = position;
            this.pinType = type;
        }
        
        public float GetInfluenceAtPosition(Vector2 worldPos, TerrainParameter parameter)
        {
            float distance = Vector2.Distance(position, worldPos);
            if (distance > influenceRadius) return 0f;
            
            // Calculate falloff
            float normalizedDistance = distance / influenceRadius;
            float influence = 1f - Mathf.Pow(normalizedDistance, influenceFalloff);
            if (influence <= 0f) return 0f;
            
            // Apply parameter-specific influence
            return parameter switch
            {
                TerrainParameter.Temperature => influence * temperatureInfluence,
                TerrainParameter.Altitude => influence * altitudeInfluence,
                TerrainParameter.Moisture => influence * moistureInfluence,
                _ => 0f
            };
        }
    }
    
    public enum PinType
    {
        City,
        Fortress,
        MysticalSite,
        NaturalFeature,
        Ruins
    }
    
    public enum TerrainParameter
    {
        Temperature,
        Altitude,
        Moisture
    }
}