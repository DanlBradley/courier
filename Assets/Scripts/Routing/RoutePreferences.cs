namespace Routing
{
    /// <summary>
    /// Preferences for route generation
    /// </summary>
    public class RoutePreferences
    {
        public float maxSegmentDistance = 4f;
        public float routeDeviation = 1f;
        public float branchChance = 0.5f;
        public float maxConnectionDistance = 5f; // Auto-connect nodes within this distance
        public int maxConnectionsPerNode = 4; // Prevent overly dense connections
    }
}