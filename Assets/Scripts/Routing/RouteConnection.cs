namespace Routing
{
    /// <summary>
    /// A connection between two nodes in the RouteGraph
    /// </summary>
    public struct RouteConnection
    {
        public RouteNode fromNode;
        public RouteNode toNode;
        public ConnectionDirection direction;
    }
}