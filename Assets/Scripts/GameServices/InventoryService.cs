using System;
using System.Collections.Generic;
using System.Linq;
using Character.Player;
using EnvironmentTools;
using Interfaces;
using Items;
using UnityEngine;
using Utils;

namespace GameServices
{
    public class InventoryService : Service
    {
        private readonly Dictionary<IContainerOwner, BackpackFrame> equippedFrames = new();
        private readonly Dictionary<string, ContainerItem> createdContainers = new();

        public event Action<ContainerItem, IContainerOwner> OnContainerRegistered;
        public event Action<ContainerItem, Item, IContainerOwner> OnItemRemoved;
        public event Action OnInventoryChanged;
        public event Action<BackpackFrame, IContainerOwner> OnBackpackFrameEquipped;
        public event Action<BackpackFrame> OnBackpackFrameUnequipped;
        
        public override void Initialize()
        {
            Logs.Log("Inventory service initialized.", "GameServices");
        }

        public BackpackFrame GetEquippedFrame(IContainerOwner owner)
        {
            return equippedFrames.GetValueOrDefault(owner);
        }

        public bool EquipBackpackFrame(BackpackFrame frame, IContainerOwner owner)
        {
            if (frame == null || owner == null) return false;
            if (!equippedFrames.TryAdd(owner, frame))
            {
                Debug.LogWarning($"Owner {owner.GetOwnerID()} already has a frame equipped!");
                return false;
            }

            OnBackpackFrameEquipped?.Invoke(frame, owner);
            OnInventoryChanged?.Invoke();
            Debug.Log("Equipped backpack frame.");
            return true;
        }

        public bool UnequipBackpackFrame(IContainerOwner owner)
        {
            if (owner == null) return false;
            if (!equippedFrames.Remove(owner, out var frame))
            {
                Debug.LogWarning($"Owner {owner.GetOwnerID()} doesn't have a frame equipped!");
                return false;
            }

            OnBackpackFrameUnequipped?.Invoke(frame);
            OnInventoryChanged?.Invoke();
            Debug.Log("Unequipped backpack frame.");
            return true;
        }

        public ContainerItem GetContainerItem(string containerID)
        {
            return createdContainers.GetValueOrDefault(containerID);
        }

        public List<ContainerItem> GetContainersOwnedBy(IContainerOwner owner)
        {
            switch (owner)
            {
                case PlayerInventory playerInventory:
                    var playerContainers = GetPlayerContainers(playerInventory);
                    if (playerContainers[0] == null) Debug.LogWarning("Failed to retrieve any player containers.");
                    return playerContainers;
                case LootableContainerHandler lootableContainer:
                    var container = lootableContainer.GetLootContainer();
                    if (container == null) Debug.LogWarning("Failed to retrieve lootable container.");
                    return new List<ContainerItem> { container };
                default:
                    Debug.LogWarning($"Failed to get containers owned by {owner.GetOwnerID()}");
                    break;
            }

            return null;
        }

        public bool IsItemOwnedBy(Item item, IContainerOwner owner)
        {
            var containers = GetContainersOwnedBy(owner);
            return containers.SelectMany(container => container.storage.GetAllItems())
                .Any(ownedItem => item == ownedItem);
        }

        public bool IsItemInPlayerInventory(Item item)
        {
            var player = GameManager.Instance.GetPlayer();
            if (player == null) return false;
            var playerInventory = player.GetComponent<PlayerInventory>();
            return IsItemOwnedBy(item, playerInventory);
        }

        public ContainerItem CreateContainer(ContainerDefinition definition, IContainerOwner owner)
        {
            var containerItem = new ContainerItem(definition);
            createdContainers[containerItem.containerID] = containerItem;
            OnContainerRegistered?.Invoke(containerItem, owner);
            // Debug.Log("Successfully created pockets container: " + containerItem.containerID);
            return containerItem;
        }

        public bool TransferItem(ContainerItem fromContainer, ContainerItem toContainer, Item item)
        {
            if (!RemoveItemFromContainer(fromContainer, item)) return false;
            if (toContainer.storage.TryAddItem(item))
            {
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Rollback on failure
            fromContainer.storage.TryAddItem(item);
            return false;
        }

        public bool TransferItem(ContainerItem fromContainer, ContainerItem toContainer,
            Item item, Vector2Int targetPosition)
        {
            if (!toContainer.storage.CanItemFitAtPosition(item, targetPosition)) return false;
            if (!RemoveItemFromContainer(fromContainer, item)) return false;
            if (toContainer.storage.TryAddItemAtPosition(item, targetPosition))
            {
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Rollback on failure
            fromContainer.storage.TryAddItem(item);
            return false;
        }

        // SIMPLIFIED: This now just queries the current state
        public List<ContainerItem> GetPlayerAccessibleContainers()
        {
            var player = GameManager.Instance.GetPlayer();
            if (player == null)
            {
                Debug.LogError("Player not found!");
                return null;
            }

            var playerInventory = player.GetComponent<PlayerInventory>();
            if (!playerInventory)
            {
                Debug.LogError("Player inventory component not found!");
                return null;
            }

            var containers = GetPlayerContainers(playerInventory);
            return containers;
        }
        
        private List<ContainerItem> GetPlayerContainers(PlayerInventory playerInventory)
        {
            List<ContainerItem> containers = new();

            if (!equippedFrames.TryGetValue(playerInventory, out var frame))
            {
                Debug.LogWarning("No frame equipped!");
                return null;
            }
            
            var attachedModules = frame.GetAttachedModules();
            containers.AddRange(attachedModules);
            Debug.Log("Player containers:");
            foreach (var container in containers)
            {
                Debug.Log($"Container: {container.containerID}");
            }
            Debug.Log($"Return containers of size {containers.Count}");
            return containers;
        }

        public bool AddItemToPlayerInventory(Item item)
        {
            var playerContainers = GetPlayerAccessibleContainers();
            foreach (var container in playerContainers)
            {
                if (!container.storage.TryAddItem(item)) continue;
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        public bool TryDestroyItem(Item item)
        {
            Debug.Log($"Destroying item {item.itemDef.name}");

            // Instead of searching through a registry, search through all containers
            // we know about (this is less efficient but more reliable)
            foreach (var container in createdContainers.Values)
            {
                if (item.ownerContainer != container.storage) continue;
                OnInventoryChanged?.Invoke();
                RemoveItemFromContainer(container, item);
                return true;
            }

            Debug.LogWarning($"Failed to destroy item {item.itemDef.name}. Owner container not found.");
            return false;
        }

        public bool RemoveItemFromContainer(ContainerItem container, Item item)
        {
            if (!container.storage.TryRemoveItem(item)) return false;
            var owner = FindContainerOwner(container);
            OnItemRemoved?.Invoke(container, item, owner);
            OnInventoryChanged?.Invoke();
            return true;
        }

        private IContainerOwner FindContainerOwner(ContainerItem container)
        {
            foreach (var (owner, frame) in equippedFrames)
            { if (frame.GetAttachedModules().Contains(container)) { return owner; } }
            return null;
        }
    }
}