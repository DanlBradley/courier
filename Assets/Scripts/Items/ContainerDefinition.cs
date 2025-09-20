using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "ContainerDefinition", menuName = "Courier/Items/Container Definition")]
    public class ContainerDefinition : ItemDefinition
    {
        [Header("Container Properties")]
        public Vector2Int containerSize = new(5, 4);
    }
    
    public class ContainerItem : Item
    {
        private string containerId = string.Empty;
        public string containerID 
        { 
            get 
            {
                if (string.IsNullOrEmpty(containerId)) containerId = itemDef.id;
                return containerId;
            }
        }
        
        private Container _storage;
        public virtual Container storage
        { 
            get 
            {
                if (_storage != null) return _storage;
                var containerDef = (ContainerDefinition)itemDef;
                _storage = new Container(containerDef.containerSize);
                return _storage;
            }
        }

        public ContainerItem(ContainerDefinition containerDefinition) : base(containerDefinition)
        {
            if (containerDefinition == null) throw new ArgumentException("ContainerDefinition cannot be null");
        }
        
        public ContainerDefinition GetContainerDefinition() { return (ContainerDefinition)itemDef; }
    }

    [Serializable]
    public class ContainerSaveData
    {
        public string id;
        public List<ItemSaveData> storageSaveData;
        
        public ContainerSaveData(ContainerItem containerItem)
        {
            id = containerItem.itemDef.id;
            var allGridItems = containerItem.storage.GetAllGridItems();
            storageSaveData = new List<ItemSaveData>();
            foreach (var gridItem in allGridItems)
            {
                ItemSaveData itemSaveData = new ItemSaveData(gridItem);
                storageSaveData.Add(itemSaveData);
            }
        }
    }
}