using GameServices;
using UnityEngine;

namespace Utils
{
    public static class ComponentHelper
    {
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component == null) { component = obj.AddComponent<T>(); }
            return component;
        }
    }

    public static class StaticMethods
    {
        public static GameObject GetPlayer()
        {
            GameObject localPlayer = GameManager.Instance.GetPlayer();
            if (localPlayer != null) { Debug.Log("Player found: " + localPlayer.name); }
            else { Debug.LogError("Player not found!"); }
            return localPlayer;
        }
    }
}