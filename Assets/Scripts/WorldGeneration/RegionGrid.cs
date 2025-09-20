using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WorldGeneration
{
    /// <summary>
    /// The world map as a square 2 dimensional grid of Region objects. All regions are size NxN as
    /// identified in the constructor.
    /// </summary>
    public class RegionGrid
    {
        private readonly Region[,] regions;
        public int size { get; }
        
        public RegionGrid(int size)
        {
            this.size = size;
            regions = new Region[size, size];
        }
        
        public void SetRegion(int x, int y, Region region) { if (IsValidCoordinate(x, y)) { regions[x, y] = region; } }

        public Region GetRegion(int x, int y)
        {
            return IsValidCoordinate(x, y) ? regions[x, y] : null;
        }
        private bool IsValidCoordinate(int x, int y) { return x >= 0 && x < size && y >= 0 && y < size; }

        public List<Region> GetAllRegions()
        {
            var list = new List<Region>();
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (regions[x, y] != null) { list.Add(regions[x, y]); }
                }
            }
            return list;
        }

        /// <summary>
        /// Simple method that looks for the first region which contains a WorldPin identified as being the "home pin"
        /// TODO: Alter this so it either finds a specific one, has some order, etc. whatever. Doesn't matter for now.
        /// </summary>
        /// <returns></returns>
        public Region GetHomeRegion()
        {
            foreach (var region in regions)
            {
                if (region.containedPin == null) continue;
                if (region.containedPin.homePin) { return region; }
            }
            Debug.LogError("Error - No home region found.");
            return null;
        }
    }
}