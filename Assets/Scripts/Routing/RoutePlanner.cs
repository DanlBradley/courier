using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WorldGeneration;

namespace Routing
{
    /// <summary>
    /// The Route planner is a tool on top of the base RouteGenerator class that is the underlying logic of how a
    /// player moves through the node graph data structure underpinning the route itself. The route planner manages
    /// both generating new routes and navigating within the current route and is used by the RouteService.
    /// </summary>
    public class RoutePlanner
    {
        private readonly RegionGrid regionGrid;
        private readonly RouteGenerator routeGenerator;
        
        private RouteGraph currentRoute;
        private RouteNode currentNode;
        
        public RoutePlanner(RegionGrid grid)
        {
            regionGrid = grid;
            routeGenerator = new RouteGenerator(grid);
        }
        
        public bool PlanRoute(Vector2Int originCoords, Vector2Int destinationCoords, RoutePreferences preferences = null)
        {
            // Validate coordinates
            if (!IsValidCoordinate(originCoords) || !IsValidCoordinate(destinationCoords))
            {
                Debug.LogError($"Invalid coordinates: origin {originCoords}, destination {destinationCoords}");
                return false;
            }
            
            // Get regions
            Region originRegion = regionGrid.GetRegion(originCoords.x, originCoords.y);
            Region destinationRegion = regionGrid.GetRegion(destinationCoords.x, destinationCoords.y);
            
            if (originRegion == null || destinationRegion == null)
            {
                Debug.LogError("One or both regions not found");
                return false;
            }
            
            // Generate route
            currentRoute = routeGenerator.GenerateRoute(originRegion, destinationRegion, preferences);
            currentNode = currentRoute.originNode;
            
            return true;
        }
        
        /// <summary>
        /// Get available exits from the current node
        /// </summary>
        public List<RouteExit> GetAvailableExits()
        {
            if (currentRoute == null || currentNode == null)
            {
                return new List<RouteExit>();
            }
            
            List<RouteExit> exits = new List<RouteExit>();
            
            // Group connections by direction (handle multiple connections in same direction)
            var connectionsByDirection = currentNode.connections
                .GroupBy(c => c.direction)
                .ToList();
            
            foreach (var directionGroup in connectionsByDirection)
            {
                var connections = directionGroup.ToList();
                
                if (connections.Count == 1)
                {
                    // Single connection in this direction
                    exits.Add(new RouteExit
                    {
                        direction = directionGroup.Key,
                        destinationNode = connections[0].toNode,
                        displayName = GetRegionDisplayName(connections[0].toNode.region),
                        exitIndex = 0
                    });
                }
                else
                {
                    // Multiple connections in same direction - create numbered exits
                    for (int i = 0; i < connections.Count; i++)
                    {
                        exits.Add(new RouteExit
                        {
                            direction = directionGroup.Key,
                            destinationNode = connections[i].toNode,
                            displayName = $"{GetRegionDisplayName(connections[i].toNode.region)} (Route {i + 1})",
                            exitIndex = i
                        });
                    }
                }
            }
            
            return exits;
        }
        
        /// <summary>
        /// Move to a specific exit from current node
        /// </summary>
        public bool TravelToExit(ConnectionDirection direction, int exitIndex = 0)
        {
            if (currentRoute == null || currentNode == null)
            {
                Debug.LogWarning("No active route or current node");
                return false;
            }
            
            // Find connections in the specified direction
            var connectionsInDirection = currentNode.connections
                .Where(c => c.direction == direction)
                .ToList();
            
            if (connectionsInDirection.Count == 0)
            {
                Debug.LogWarning($"No exits found in direction {direction}");
                return false;
            }
            
            if (exitIndex >= connectionsInDirection.Count)
            {
                Debug.LogWarning($"Exit index {exitIndex} out of range for direction {direction}");
                return false;
            }
            
            // Move to the target node
            RouteConnection selectedConnection = connectionsInDirection[exitIndex];
            currentNode = selectedConnection.toNode;
            Debug.Log($"Traveled to node {currentNode.nodeType} at " +
                      $"{currentNode.region.gridX},{currentNode.region.gridY}");
            
            return true;
        }
        
        /// <summary>
        /// Check if the route is completed (at destination)
        /// </summary>
        public bool IsRouteComplete()
        {
            return currentRoute != null && currentNode != null && currentNode.isDestination;
        }
        
        /// <summary>
        /// Get current route status information
        /// </summary>
        public RouteStatus GetRouteStatus()
        {
            if (currentRoute == null)
            {
                return new RouteStatus
                {
                    hasActiveRoute = false,
                    routeDescription = "No active route"
                };
            }
            
            return new RouteStatus
            {
                hasActiveRoute = true,
                routeDescription = currentRoute.GetRouteDescription(),
                currentRegion = currentNode?.region,
                isComplete = IsRouteComplete(),
                availableExitCount = GetAvailableExits().Count
            };
        }
        
        /// <summary>
        /// Clear the current route
        /// </summary>
        public void ClearRoute()
        {
            currentRoute = null;
            currentNode = null;
        }
        
        /// <summary>
        /// Get the current route graph (for visualization/debugging)
        /// </summary>
        public RouteGraph GetCurrentRoute()
        {
            return currentRoute;
        }
        
        /// <summary>
        /// Get the current node (for world loading)
        /// </summary>
        public RouteNode GetCurrentNode()
        {
            return currentNode;
        }
        
        // Private helper methods
        private bool IsValidCoordinate(Vector2Int coord)
        {
            return coord.x >= 0 && coord.x < regionGrid.size && 
                   coord.y >= 0 && coord.y < regionGrid.size;
        }
        
        private string GetRegionDisplayName(Region region)
        {
            return region.containedPin != null ? region.containedPin.name : $"{region.regionType}";
        }
    }
    
    /// <summary>
    /// Information about an available exit from current location
    /// </summary>
    public struct RouteExit
    {
        public ConnectionDirection direction;
        public RouteNode destinationNode;
        public string displayName;
        public int exitIndex; // For handling multiple exits in same direction
    }
    
    /// <summary>
    /// Current status of the route planner
    /// </summary>
    public struct RouteStatus
    {
        public bool hasActiveRoute;
        public string routeDescription;
        public Region currentRegion;
        public bool isComplete;
        public int availableExitCount;
    }
}