using System;
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    public class WaterSource : MonoBehaviour, IInteractable
    {
        public bool CanInteract(GameObject interactor) { return true; }
        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        { onComplete?.Invoke(); }
        public void CancelInteraction() { }
        public GameObject GetGameObject() { return gameObject; }
    }
}