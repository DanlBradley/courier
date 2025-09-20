using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace EnvironmentTools
{
    /// <summary>
    /// A workbench where players can configure their backpack frame by adding/removing modules
    /// </summary>
    [RequireComponent(typeof(TimedInteraction))]
    public class BackpackWorkbenchHandler : MonoBehaviour, ITimedInteractionHandler
    {
        [Header("Module Storage")] [SerializeField]
        private ContainerDefinition moduleStorageDefinition;

        [SerializeField] private ModuleDefinition[] startingModules;

        private InventoryService inventoryService;
        private UIService uiService;
        private ContainerItem moduleStorage;

        public string WorkbenchID => $"Backpack_Workbench_{gameObject.name}";

        private void Start()
        {
            inventoryService = ServiceLocator.GetService<InventoryService>();
            uiService = ServiceLocator.GetService<UIService>();

            moduleStorage = new ContainerItem(moduleStorageDefinition);

            foreach (var moduleDef in startingModules)
            {
                if (moduleDef == null) continue;
                var module = moduleDef.CreateModuleFromDefinition();
                moduleStorage.storage.TryAddItem(module);
            }

            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction == null) return;
            timedInteraction.SetInteractionText("Set up workbench");
            timedInteraction.SetHoldDuration(2f);
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            var owner = interactor.GetComponent<IContainerOwner>();
            if (owner != null && inventoryService.GetEquippedFrame(owner) != null)
            {
                return true;
            }

            Debug.Log("No frame on player.");
            return false;
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            var owner = interactor.GetComponent<IContainerOwner>();
            var frame = inventoryService.GetEquippedFrame(owner);

            if (frame != null)
            {
                uiService.OpenBackpackConfiguration(frame, moduleStorage, this);
                Debug.Log("Successfully set up BackpackWorkbench workspace");
            }
            else
            {
                Debug.LogWarning("Frame not found when completing workbench interaction");
            }
        }
    }
}