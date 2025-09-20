using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Courier/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public new string name;
        public string description;
        public Sprite sprite;
        public Vector2Int size = new(1,1);
        
        [Header("Behaviors")]
        public List<ItemBehavior> behaviors = new();
    }

    public class Item
    {
        //No stacking and thus no item quantity for now.
        public ItemDefinition itemDef;
        public Container ownerContainer = null;
        private Dictionary<string, object> itemState = new();
        
        public Item(ItemDefinition itemDefinition)
        {
            if (itemDefinition is null)
            {
                throw new System.ArgumentNullException(nameof(itemDefinition));
            }
            if (itemDefinition.size.x <= 0 || itemDefinition.size.y <= 0)
            {
                throw new System.ArgumentException("Failed to create item - " +
                                                   "size must be 1x1 or larger and non-negative.");
            }
            
            itemDef = itemDefinition;
            foreach (var behavior in itemDef.behaviors) { behavior.InitializeItemState(this); }
        }

        public Vector2Int GetItemSize()
        {
            return itemDef.size;
        }
        
        public T GetState<T>(string key, T defaultValue = default)
        {
            return itemState.TryGetValue(key, out var value) && value is T typed 
                ? typed 
                : defaultValue;
        }
    
        public void SetState<T>(string key, T value)
        {
            itemState[key] = value;
        }
    
        public bool HasState(string key) => itemState.ContainsKey(key);
    }
    
    public class GridItem
    {
        private Item _item;
        private readonly GridPosition _gridPos;

        public GridItem(Item item, Vector2Int itemPos)
        {
            _item = item;
            _gridPos = new GridPosition(itemPos, item.GetItemSize());
        }

        public bool IsItemInPosition(Vector2Int pos) { return _gridPos.GetSpan().Any(t => t == pos); }

        public Vector2Int[] GetItemSpan() { return _gridPos.GetSpan(); }

        public Item GetItem() { return _item; }
        
        public Vector2Int GetRootPosition() { return _gridPos.GetPosition(); }
    }
    
    [Serializable]
    public class ItemSaveData
    {
        //TODO: Add an item definition registry, and also container definition registry etc.
        //TODO: I will probably want to do this for all objects that heavily utilize SOs.
        //TODO: Hopefully we can avoid doing this with dialogue... just some higher level dialogue state ref.?
        public string id;
        public int gridX, gridY; //root positions

        public ItemSaveData(GridItem gridItem)
        {
            id = gridItem.GetItem().itemDef.id;
            Vector2Int itemRootPosition = gridItem.GetRootPosition();
            gridX = itemRootPosition.x;
            gridY = itemRootPosition.y;
        }
    }
}
