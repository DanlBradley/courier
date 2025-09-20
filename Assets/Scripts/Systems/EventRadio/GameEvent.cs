using System.Collections.Generic;
using UnityEngine;

namespace Systems.EventRadio
{
    [CreateAssetMenu(menuName = "GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<GameEventListener> eventListeners = new ();

        public void Raise(Component sender, object data)
        { foreach (var t in eventListeners) { t.OnEventRaised(sender, data); } }

        public void RegisterListener(GameEventListener listener)
        { if (!eventListeners.Contains(listener)) eventListeners.Add(listener); }

        public void DeregisterListener(GameEventListener listener)
        { if (eventListeners.Contains(listener)) eventListeners.Remove(listener); }
    }
}