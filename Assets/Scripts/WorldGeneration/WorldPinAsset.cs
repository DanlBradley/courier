using UnityEngine;

namespace WorldGeneration
{
    [CreateAssetMenu(fileName = "WorldPinConfiguration", menuName = "Courier/World Generation/Pin Configuration")]
    public class WorldPinAsset : ScriptableObject
    {
        [SerializeField] private WorldPin pin;
        public WorldPin Pin => pin;
    }
}