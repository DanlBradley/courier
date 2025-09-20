using System;
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    public class Door : MonoBehaviour, IInteractable
    {
        public bool isOpen = true;
        public Door connectedDoor;
        public Vector2 spawnOffset;

        public bool CanInteract(GameObject interactor)
        {
            if (connectedDoor.isOpen && isOpen && interactor.CompareTag("Player")) return true;
            Debug.LogWarning($"Cannot use door at {gameObject.name}. " +
                             $"Check that doors are open and interactor is a Player;");
            return false;
        }

        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        {
            interactor.transform.position = connectedDoor.TraversePosition();
            Debug.Log($"Entered/exited door to {gameObject.name}");
            onComplete?.Invoke();
        }

        public void CancelInteraction() { }
        public GameObject GetGameObject() { return gameObject; }
        private Vector3 TraversePosition()
        { return transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0); }
    }
}