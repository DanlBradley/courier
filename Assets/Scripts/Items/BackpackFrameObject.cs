using System;
using System.Linq;
using GameServices;
using UnityEngine;
using Interfaces;
using UnityEngine.SceneManagement;

namespace Items
{
    public class BackpackFrameObject : MonoBehaviour, IInteractable, IContainerOwner, ISaveable
    {
        [SerializeField] private BackpackFrameDefinition frameDefinition;
        [SerializeField] private bool isEquipped;
        
        private BackpackFrame backpackFrame;
        private IContainerOwner currentOwner;

        private bool loadedFrame;

        private void Start()
        {
            if (loadedFrame) return;
            backpackFrame = new BackpackFrame(frameDefinition);
        }
        
        private void Interact(GameObject interactor)
        {
            var containerOwner = interactor.GetComponent<IContainerOwner>();
            if (containerOwner == null) return;
            
            if (!isEquipped) { EquipFrame(containerOwner); }
            else if (currentOwner == containerOwner) { UnequipFrame(); }
        }
        
        private void EquipFrame(IContainerOwner owner)
        {
            Debug.Log("Equipping frame...");
            var inventoryService = ServiceLocator.GetService<InventoryService>();
            if (!inventoryService.EquipBackpackFrame(backpackFrame, owner)) return;
            currentOwner = owner;
            isEquipped = true;
    
            var playerTransform = GameManager.Instance.GetPlayer().transform;
            transform.parent = playerTransform;
            transform.localPosition = Vector3.back * .5f;
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        private void UnequipFrame()
        {
            Debug.Log("Unequipping frame...");
            var inventoryService = ServiceLocator.GetService<InventoryService>();
            if (!inventoryService.UnequipBackpackFrame(currentOwner)) return;
            currentOwner = null;
            isEquipped = false;

            var playerTransform = GameManager.Instance.GetPlayer().transform;
            if (playerTransform == null) return;
            Vector3 dropPosition = playerTransform.position + playerTransform.forward * 2f;
            transform.parent = GameObject.Find("World").transform;
            transform.position = dropPosition;
            transform.rotation = Quaternion.identity;
        }
        
        public BackpackFrame GetBackpackFrame() => backpackFrame;
        
        public bool CanInteract(GameObject interactor) { return true; }

        public void StartInteraction(GameObject interactor, Action onComplete, Action onCancel)
        { Interact(interactor); onComplete?.Invoke(); }

        public void CancelInteraction() { }
        public GameObject GetGameObject() { return gameObject; }
        public string GetOwnerID() { return frameDefinition.id; }
        public string SaveID => $"{frameDefinition.id}_{SceneManager.GetActiveScene().name}";
        public object CaptureState()
        {
            Debug.Log("Capturing backpack frame state");
            string tempOwnerId = "";
            if (currentOwner != null)
            {
                tempOwnerId = currentOwner.GetOwnerID();
                Debug.Log("set owner ID to: " + tempOwnerId);
            }
            Debug.Log($"Saving {backpackFrame.definition.name} to owner --{tempOwnerId}--");
            return new BackpackSaveData(new BackpackFrameSaveData(backpackFrame), 
                tempOwnerId);
        }

        public void RestoreState(object saveData)
        {
            Debug.Log("Restoring frame state...");
            if (saveData is not BackpackSaveData savedBackpack)
            {
                Debug.LogError("Backpack not found! Save data is of shape: " + saveData.GetType());
                return;
            }
            var savedFrame = savedBackpack.savedBackpackData;
            
            var registry = GameManager.Instance.GetIdRegistry();
            
            //First re-create the frame!
            backpackFrame = new BackpackFrame(frameDefinition, false);
            //Then modules...
            foreach (var module in savedFrame.modules)
            {
                ModuleDefinition moduleItemDef = registry.GetItemDefinition(module.moduleContainer.id) as ModuleDefinition;
                if (moduleItemDef == null) Debug.LogError("Warning - failed to retrieve module as a module def");
                var moduleItem = new ModuleItem(moduleItemDef);
                //finally fill in the modules with new items
                foreach (var savedItem in module.moduleContainer.storageSaveData)
                {
                    moduleItem.storage.TryAddItemAtPosition(
                        new Item(registry.GetItemDefinition(savedItem.id)),
                        new Vector2Int(savedItem.gridX, savedItem.gridY));
                }
                backpackFrame.moduleGridItem.storage.TryAddItemAtPosition(
                    (moduleItem),
                    new Vector2Int(module.gridX, module.gridY));
            }
            //This assumes GetOwnerID just throws the GO name directly
            var playerGo = GameObject.Find(savedBackpack.ownerName);
            Debug.Log($"Successfully loaded backpack frame data. Equipping {savedBackpack.ownerName}'s backpack...");
            if (playerGo != null) Interact(playerGo);
            loadedFrame = true;
        }
    }

    [Serializable]
    public class BackpackSaveData
    {
        public BackpackFrameSaveData savedBackpackData;
        public string ownerName;
        
        public BackpackSaveData(BackpackFrameSaveData frame, string ownerGoName = "")
        {
            savedBackpackData = frame;
            ownerName = ownerGoName;
        }
    }
    
}