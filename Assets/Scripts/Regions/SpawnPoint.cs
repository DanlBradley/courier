using UnityEngine;

namespace Regions
{
    public class SpawnPoint : MonoBehaviour
    {
        public enum SpawnType { Default, NorthSide, SouthSide, EastSide, WestSide }
        public SpawnType spawnType = SpawnType.Default;
    }
}