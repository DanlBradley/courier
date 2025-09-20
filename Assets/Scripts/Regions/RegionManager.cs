using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using WorldGeneration;

namespace Regions
{
    public class RegionManager : MonoBehaviour, IRegionAware
    {
        private List<SpawnPoint> spawnPoints;
        private Region regionData;
    
        private void Awake()
        {
            spawnPoints = new List<SpawnPoint>(GetComponentsInChildren<SpawnPoint>());
            if (spawnPoints.Count == 0) { Debug.LogWarning($"No spawn points found in region {name}"); }
        }
    
        public Vector3 GetSpawnPosition(SpawnPoint.SpawnType type = SpawnPoint.SpawnType.Default)
        {
            var validSpawns = spawnPoints.Where(sp => sp.spawnType == type).ToList();
            return validSpawns.Count > 0 ? validSpawns[0].transform.position : transform.position;
        }
    

        public void OnRegionLoaded(Region data) { regionData = data; }

        public void OnRegionUnloading() { }
    }
}