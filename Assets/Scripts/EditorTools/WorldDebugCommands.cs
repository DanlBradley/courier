using System;
using GameServices;
using Interfaces;
using Regions;
using UnityEngine;

namespace EditorTools
{
    public class WorldDebugCommands : MonoBehaviour
    {
        [SerializeField] private Vector2Int debugDestinationLocation;
        [SerializeField] private Vector2Int debugReturnHomeLocation;
        private WorldManagerService worldManagerService;
        private void Start()
        {
            worldManagerService = ServiceLocator.GetService<WorldManagerService>();
            InitializeConsole();
        }

        private void InitializeConsole()
        {
            DebugConsole.RegisterCommand("route.new", GenerateRoute, 
                "Generate a new route", "route.new");
            DebugConsole.RegisterCommand("route.clear", ClearRoute, 
                "Clear the current route", "route.clear");
            DebugConsole.RegisterCommand("route.home", ReturnHome, 
                "Return to the village!", "route.home");
            DebugConsole.RegisterCommand("route.describe", DescribeRoute, 
                "Describe the route", "route.describe");
        }

        private string GenerateRoute(string[] args)
        {
            worldManagerService.GenerateRoute(debugDestinationLocation);
            return "Generated new route!";
        }

        private string ClearRoute(string[] args)
        {
            worldManagerService.ClearRoute();
            return "Cleared the current route.";
        }

        private string ReturnHome(string[] args)
        {
            worldManagerService.GenerateRoute(debugReturnHomeLocation);
            return "Generated new route!";
        }

        private string DescribeRoute(string[] args)
        {
            var exits = worldManagerService.GetCurrentRegion().
                GetRegionPrefab().GetComponentsInChildren<RegionExit>();
            var status = worldManagerService.GetRouteStatus();
            string line1 = $"Current region: " +
                           $"{status.currentRegion?.regionType} at " +
                           $"({status.currentRegion?.gridX}, {status.currentRegion?.gridY})";
            string line2 = "Available exits: ";
            foreach (var exit in exits) { if (exit.enabled) line2 += exit.direction + ", "; }
            
            string line3 = $"Route complete: {status.isComplete}";
            return line1 + '\n' + line2 + '\n' + line3;
        }
    }
}