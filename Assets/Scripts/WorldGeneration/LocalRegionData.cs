using UnityEngine;

namespace WorldGeneration
{
    public class LocalRegionData
    {
        public Region[,] regions;
        public int size;
        public Vector2Int playerLocalPosition; // Player's position within this local grid
        public Vector2Int worldOffset; // Offset from world grid (0,0) to local grid (0,0)
    
        public LocalRegionData(int localSize, Vector2Int playerWorldPos, Vector2Int offset)
        {
            size = localSize;
            regions = new Region[localSize, localSize];
            worldOffset = offset;
        
            // Calculate player's position within the local grid
            playerLocalPosition = playerWorldPos - offset;
        }
    
        public Region GetRegion(int localX, int localY)
        {
            if (localX < 0 || localX >= size || localY < 0 || localY >= size)
                return null;
            return regions[localX, localY];
        }
    
        public Vector2Int LocalToWorldCoords(int localX, int localY)
        {
            return new Vector2Int(localX + worldOffset.x, localY + worldOffset.y);
        }
    
        public Vector2Int WorldToLocalCoords(Vector2Int worldCoords)
        {
            return worldCoords - worldOffset;
        }
    }
}