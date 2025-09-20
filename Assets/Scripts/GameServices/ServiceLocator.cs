using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameServices
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();
        private static bool _isInitialized;

        public static void Initialize() { _isInitialized = true; }
        public static void Reset() { Services.Clear(); _isInitialized = false; }

        public static void RegisterAndInitialize<T>(T service) where T : class
        {
            if (!_isInitialized) { Debug.LogError("ServiceLocator not initialized!"); return; }
            Services[typeof(T)] = service;
            if (service is Service svc) { svc.Initialize(); }
        }

        public static T GetService<T>() where T : class
        {
            if (!_isInitialized) { Debug.LogError("ServiceLocator not initialized!"); return null; }
            if (Services.TryGetValue(typeof(T), out object service)) { return (T)service; }
        
            Debug.LogError($"Service not found: {typeof(T).Name}"); return null;
        }

    }
}