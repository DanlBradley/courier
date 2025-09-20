using System;
using System.Collections.Generic;
using System.Linq;
using Regions;
using UnityEngine;
using WorldGeneration;
using Random = UnityEngine.Random;

namespace Routing
{
    /// <summary>
    /// The route generator creates a node graph of regions based on a desired goal of reaching the destination.
    /// The main method, GenerateRoute, takes an origin, destination region, and some route preferences config
    /// and determines what regions the player must stop at on the route from the "RegionGrid", an object that contains
    /// NxM regions and is essentially the world map.
    /// </summary>
    public class RouteGenerator
    {
        private readonly RegionGrid regionGrid;
        private RoutePreferences routePreferences;
        public RouteGenerator(RegionGrid grid) { regionGrid = grid; }

        public RouteGraph GenerateRoute(Region origin, Region destination, RoutePreferences preferences = null)
        {
            preferences ??= new RoutePreferences();
            routePreferences = preferences;
            
            if (origin.containedPin == null) { Debug.LogWarning(
                $"Origin region at ({origin.gridX}, {origin.gridY}) doesn't contain a pin"); }
            
            if (origin.containedPin == null) { Debug.LogWarning(
                $"Destination region at ({destination.gridX}, {destination.gridY}) doesn't contain a pin"); }
            
            List<Vector2Int> mainPath = GenerateMainPath(
                new Vector2Int(origin.gridX, origin.gridY),
                new Vector2Int(destination.gridX, destination.gridY),
                preferences
            );
            
            List<RouteNode> nodes = CreateNodesFromPath(mainPath);
            AddBranches(nodes, preferences);
            CreateConnections(nodes);
            
            return new RouteGraph
            {
                nodes = nodes,
                originNode = nodes[0],
                destinationNode = nodes[^1]
            };
        }
        
        private List<Vector2Int> GenerateMainPath(Vector2Int origin, Vector2Int destination, RoutePreferences preferences)
        {
            List<Vector2Int> waypoints = new List<Vector2Int> { origin };
            
            Vector2Int pathVector = destination - origin;
            float totalDistance = pathVector.magnitude;
            
            if (totalDistance <= 1.5f)
            {
                waypoints.Add(destination);
                return waypoints;
            }
            
            int intermediateWaypoints = Mathf.Max(1, Mathf.FloorToInt(totalDistance / preferences.maxSegmentDistance));
            
            for (int i = 1; i <= intermediateWaypoints; i++)
            {
                float progress = (float)i / (intermediateWaypoints + 1);
                Vector2 idealPoint = Vector2.Lerp(origin, destination, progress);
                
                Vector2 deviation = Random.insideUnitCircle * preferences.routeDeviation;
                Vector2 actualPoint = idealPoint + deviation;
                
                Vector2Int gridPoint = new Vector2Int(
                    Mathf.Clamp(Mathf.RoundToInt(actualPoint.x), 0, regionGrid.size - 1),
                    Mathf.Clamp(Mathf.RoundToInt(actualPoint.y), 0, regionGrid.size - 1)
                );
                
                if (!waypoints.Contains(gridPoint))
                {
                    waypoints.Add(gridPoint);
                }
            }
            
            waypoints.Add(destination);
            return waypoints;
        }
        
        private List<RouteNode> CreateNodesFromPath(List<Vector2Int> path)
        {
            List<RouteNode> nodes = new List<RouteNode>();
            
            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int gridPos = path[i];
                Region region = regionGrid.GetRegion(gridPos.x, gridPos.y);
                
                if (region == null)
                {
                    Debug.LogWarning($"No region found at {gridPos}");
                    continue;
                }
                
                RouteNode node = new RouteNode
                {
                    region = region,
                    connections = new List<RouteConnection>(),
                    isOrigin = i == 0,
                    isDestination = i == path.Count - 1,
                    nodeType = i == 0 ? NodeType.Origin : (i == path.Count - 1 ? NodeType.Destination : NodeType.MainPath)
                };
                
                nodes.Add(node);
            }
            
            return nodes;
        }
        
        private void AddBranches(List<RouteNode> mainNodes, RoutePreferences preferences)
        {
            // Add branches to middle nodes (not origin or destination)
            for (int i = 1; i < mainNodes.Count - 1; i++)
            {
                RouteNode mainNode = mainNodes[i];
                
                // Chance to create a branch
                if (!(Random.Range(0f, 1f) < preferences.branchChance)) continue;
                RouteNode branchNode = CreateBranchNode(mainNode, mainNodes);
                if (branchNode != null) { mainNodes.Add(branchNode); }
            }
        }
        
        private RouteNode CreateBranchNode(RouteNode fromNode, List<RouteNode> mainNodes)
        {
            // Try to find a valid position for a branch
            Vector2Int fromPos = new Vector2Int(fromNode.region.gridX, fromNode.region.gridY);
            
            // Generate a branch position offset from the main path
            Vector2[] branchDirections = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            Vector2 branchDir = branchDirections[Random.Range(0, branchDirections.Length)];
            
            Vector2Int branchPos = fromPos + Vector2Int.RoundToInt(branchDir * Random.Range(2, 4));
            
            // Clamp to grid bounds
            branchPos.x = Mathf.Clamp(branchPos.x, 0, regionGrid.size - 1);
            branchPos.y = Mathf.Clamp(branchPos.y, 0, regionGrid.size - 1);
            
            // Make sure it's not too close to existing nodes
            foreach (var existingNode in mainNodes)
            {
                Vector2Int existingPos = new Vector2Int(existingNode.region.gridX, existingNode.region.gridY);
                if (Vector2Int.Distance(branchPos, existingPos) < 2f)
                {
                    return null; // Too close to existing node
                }
            }
            
            Region branchRegion = regionGrid.GetRegion(branchPos.x, branchPos.y);
            if (branchRegion == null) return null;
            
            return new RouteNode
            {
                region = branchRegion,
                connections = new List<RouteConnection>(),
                isOrigin = false,
                isDestination = false,
                nodeType = NodeType.Branch
            };
        }
        
        private void CreateConnections(List<RouteNode> nodes)
        {
            var mainNodes = nodes.Where(n => n.nodeType != NodeType.Branch).ToList();
            // var branchNodes = nodes.Where(n => n.nodeType == NodeType.Branch).ToList();
    
            for (int i = 0; i < mainNodes.Count - 1; i++)
            { CreateConnection(mainNodes[i], mainNodes[i + 1]); }
    
            CreateDenseConnections(nodes);
        }
        
        private void CreateDenseConnections(List<RouteNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                RouteNode fromNode = nodes[i];
                Vector2Int fromPos = new Vector2Int(fromNode.region.gridX, fromNode.region.gridY);
        
                // Skip if this node already has enough connections
                if (fromNode.connections.Count >= routePreferences.maxConnectionsPerNode)
                    continue;
            
                // Find all nodes within connection distance
                List<(RouteNode node, float distance)> nearbyNodes = new();
        
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (i == j) continue; // Skip self
            
                    RouteNode toNode = nodes[j];
                    Vector2Int toPos = new Vector2Int(toNode.region.gridX, toNode.region.gridY);
                    float distance = Vector2Int.Distance(fromPos, toPos);

                    if (!(distance <= routePreferences.maxConnectionDistance)) continue;
                    // Check if connection already exists
                    bool connectionExists = fromNode.connections.Any(c => c.toNode == toNode);
                    if (!connectionExists) { nearbyNodes.Add((toNode, distance)); }
                }
        
                // Sort by distance and connect to closest nodes
                nearbyNodes.Sort((a, b) => a.distance.CompareTo(b.distance));
        
                int connectionsToAdd = Mathf.Min(
                    nearbyNodes.Count, 
                    routePreferences.maxConnectionsPerNode - fromNode.connections.Count
                );
        
                for (int k = 0; k < connectionsToAdd; k++) {CreateConnection(fromNode, nearbyNodes[k].node); }
            }
        }
        
        private void CreateConnection(RouteNode fromNode, RouteNode toNode)
        {
            Vector2Int fromPos = new Vector2Int(fromNode.region.gridX, fromNode.region.gridY);
            Vector2Int toPos = new Vector2Int(toNode.region.gridX, toNode.region.gridY);
            
            ConnectionDirection direction = CalculateDirection(fromPos, toPos);
            
            RouteConnection connection = new RouteConnection
            {
                fromNode = fromNode,
                toNode = toNode,
                direction = direction
            };
            
            fromNode.connections.Add(connection);
        }
        
        private ConnectionDirection CalculateDirection(Vector2Int from, Vector2Int to)
        {
            Vector2Int diff = to - from;

            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
            { return diff.x > 0 ? ConnectionDirection.East : ConnectionDirection.West; }
            else { return diff.y > 0 ? ConnectionDirection.North : ConnectionDirection.South; }
        }
    }

    /// <summary>
    /// A node in the route graph
    /// </summary>
    public class RouteNode
    {
        public Region region;
        public List<RouteConnection> connections;
        public bool isOrigin;
        public bool isDestination;
        public NodeType nodeType;
    }
    
    public enum NodeType
    {
        Origin,
        Destination,
        MainPath,
        Branch
    }
    
    public enum ConnectionDirection
    {
        North,
        South,
        East,
        West
    }
    
    
}