using GameServices;
using Interfaces;
using Items;
using UnityEngine;

namespace EnvironmentTools
{
    [RequireComponent(typeof(TimedInteraction))]
    public class ForageableHandler : MonoBehaviour, ITimedInteractionHandler
    {
        [SerializeField] private ItemDefinition itemToBeForaged;
        [SerializeField] private bool canBeForaged = true;
        
        public void OnInteractionComplete(GameObject interactor)
        {
            if (!canBeForaged) return;
            
            Debug.Log($"Foraged {gameObject.name}!");
            bool foraged = ServiceLocator.GetService<InventoryService>().AddItemToPlayerInventory(new Item(itemToBeForaged));
            if (!foraged) { Debug.LogWarning("Can't forage. No room in inventory."); return; }
            
            canBeForaged = false;
            var timedInteraction = GetComponent<TimedInteraction>();
            if (timedInteraction != null) { timedInteraction.SetCanInteract(false); }
            Destroy(gameObject);
        }

        public bool CanPerformInteraction(GameObject interactor) { return canBeForaged; }
    }
}
