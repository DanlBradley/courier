using System;
using UnityEngine;
using Interfaces;
using GameServices;
using Routing;
using WorldGeneration;

namespace Regions
{
    public class RegionExit : MonoBehaviour, IInteractable
    {
        [Header("Transition Settings")] 
        [SerializeField] private ConnectionDirection exitDirection;
        private WorldManagerService worldManager;
        private Region destinationRegion;
        
        public ConnectionDirection direction => exitDirection;
        public void AssignDestination(Region region) { destinationRegion = region; }
        
        private void Start()
        {
            worldManager = ServiceLocator.GetService<WorldManagerService>();
            var col = GetComponent<Collider2D>();
            if (col != null) { col.isTrigger = true; }
        }
        
        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        {
            if (destinationRegion == null) 
            { Debug.LogError("Cannot transition: no valid destination assigned"); return; }
        
            var destinationCoords = new Vector2Int(
                destinationRegion.gridX,
                destinationRegion.gridY);
            SpawnPoint.SpawnType entryType = DirectionUtils.ConnectionDirectionToSpawnType(exitDirection);
            worldManager.TransitionToRegion(destinationCoords, entryType);
            onComplete?.Invoke();
        }
        
        public bool CanInteract(GameObject interactor)
        { return interactor.CompareTag("Player") && destinationRegion != null; }

        public void CancelInteraction() { throw new NotImplementedException(); }
        public GameObject GetGameObject() { return gameObject; }
    }
}