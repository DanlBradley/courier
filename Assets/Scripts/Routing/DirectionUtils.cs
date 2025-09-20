using System;
using Regions;

namespace Routing
{
    /// <summary>
    /// Simple class to swap between SpawnType to Direction enums
    /// Probably a better way to handle these two enums, but it's fine for now.
    /// 1. SpawnType: Identifies where the player came from (I.e. FromSouth), or another kind of spawn (like city)
    /// 2. ConnectionDirection: Identifies where a RouteConnection connects to. Each RouteConnection has a direction.
    /// </summary>
    public static class DirectionUtils
    {
        public static ConnectionDirection SpawnTypeToConnectionDirection(SpawnPoint.SpawnType spawnType)
        {
            return spawnType switch
            {
                SpawnPoint.SpawnType.NorthSide => ConnectionDirection.South,
                SpawnPoint.SpawnType.SouthSide => ConnectionDirection.North, 
                SpawnPoint.SpawnType.EastSide => ConnectionDirection.West,
                SpawnPoint.SpawnType.WestSide => ConnectionDirection.East,
                _ => throw new ArgumentException($"Cannot convert {spawnType} to ConnectionDirection")
            };
        }
    
        public static SpawnPoint.SpawnType ConnectionDirectionToSpawnType(ConnectionDirection direction)
        {
            return direction switch
            {
                ConnectionDirection.North => SpawnPoint.SpawnType.SouthSide,
                ConnectionDirection.South => SpawnPoint.SpawnType.NorthSide,
                ConnectionDirection.East => SpawnPoint.SpawnType.WestSide, 
                ConnectionDirection.West => SpawnPoint.SpawnType.EastSide,
                _ => throw new ArgumentException($"Cannot convert {direction} to SpawnType")
            };
        }
    }
}