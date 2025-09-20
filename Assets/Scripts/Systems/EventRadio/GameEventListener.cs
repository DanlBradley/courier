using System;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.EventRadio
{
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent gameEvent;
        public ObjectUnityEvent response;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.DeregisterListener(this);
        }

        public void OnEventRaised(Component sender, object data)
        {
            response.Invoke(sender, data);
        }
    }
    
    [Serializable]
    public class ObjectUnityEvent : UnityEvent<Component, object> { }
}