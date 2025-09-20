using System.Collections.Generic;
using System.Linq;

namespace Routing
{
    /// <summary>
    /// A complete route graph between two destinations
    /// </summary>
    public class RouteGraph
    {
        public List<RouteNode> nodes;
        public RouteNode originNode;
        public RouteNode destinationNode;
        
        public string GetRouteDescription()
        {
            if (nodes == null || nodes.Count == 0) return "Invalid route";
            
            var mainPath = nodes.Where(n => n.nodeType != NodeType.Branch).ToList();
            var regionNames = mainPath.Select(n => n.region.regionType.ToString()).ToList();
            
            string originName = GetNodeName(originNode);
            string destName = GetNodeName(destinationNode);
            
            string description = $"{originName} → {string.Join(" → ", regionNames)} → {destName}";
            
            int branchCount = nodes.Count(n => n.nodeType == NodeType.Branch);
            if (branchCount > 0)
            {
                description += $" (+ {branchCount} branch{"es".Substring(branchCount == 1 ? 1 : 0)})";
            }
            
            return description;
        }
        
        private string GetNodeName(RouteNode node)
        {
            if (node.region.containedPin != null) { return node.region.containedPin.name; }
            return $"{node.region.regionType} ({node.region.gridX},{node.region.gridY})";
        }
    }
}