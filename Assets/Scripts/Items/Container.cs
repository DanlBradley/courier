using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Items
{
    public class Container
    {
        //This class contains a list of grid items of size M x N in a grid of size O x P,
        //  where objects cannot overlap.
        
        //You should be able to query any grid position (x, y), and
        //  add an item to position. Additionally, do NOT add item if any of the other items are
        //  currently taking up that requested position (x, y).

        private Vector2Int containerSize; // The size of the container O x P
        private readonly List<GridItem> items; // The list of grid item objects size M x N
        private readonly HashSet<Vector2Int> occupiedPositions;

        public Container(Vector2Int containerSize)
        {
            this.containerSize = containerSize;
            items = new List<GridItem>();
            occupiedPositions = new HashSet<Vector2Int>();
        }
        
        public bool TryAddItem(Item item)
        {
            //Default grid search method: Try adding an item starting from the top left most position on the grid.
            //  If this position is occupied, continue searching, first the top most row left to right, down each row.
            //  If no spaces are available, do not add item and return false.
            //  If a space is found, return true.
            // Debug.Log($"Trying to add item {item.itemDef.name}");
            //Outer loop: columns
            for (int i = 0; i < containerSize.y; i++)
            {
                //Inner loop: rows
                for (int j = 0; j < containerSize.x; j++)
                {
                    var pos = new Vector2Int(j, i);
                    var itemCandidate = new GridItem(item, pos);
                    if (!CanAddItemCandidate(itemCandidate)) continue;
                    // Debug.Log($"Successfully ran TryAddItem for item {item.itemDef.name}");
                    AddItem(itemCandidate);
                    
                    return true;
                }
            }

            return false;
        }
        
        public bool TryAddItemAtPosition(Item item, Vector2Int pos)
        {
            GridItem itemCandidate = new GridItem(item, pos);
            if (!CanAddItemCandidate(itemCandidate)) return false;
            AddItem(itemCandidate);
            return true;
        }

        private void AddItem(GridItem itemToAdd)
        {
            items.Add(itemToAdd);
            itemToAdd.GetItem().ownerContainer = this;
            AddOccupiedPositions(itemToAdd.GetItemSpan());
        }
        
        public bool TryRemoveItem(Item itemToRemove)
        {
            var gridItemToRemove = items.FirstOrDefault(gridItem => gridItem.GetItem() == itemToRemove);
            if (gridItemToRemove == null) return false;

            items.Remove(gridItemToRemove);
            itemToRemove.ownerContainer = null;
            RemoveOccupiedPositions(gridItemToRemove.GetItemSpan());
            return true;
        }

        public bool TryRemoveItemAtPosition(Vector2Int pos)
        {
            var gridItemToRemove = items.FirstOrDefault(item => item.IsItemInPosition(pos));
            if (gridItemToRemove == null) return false;
            RemoveItem(gridItemToRemove);
            return true;
        }
        
        private void RemoveItem(GridItem itemToRemove)
        {
            items.Remove(itemToRemove);
            itemToRemove.GetItem().ownerContainer = null;
            RemoveOccupiedPositions(itemToRemove.GetItemSpan());
        }
        
        public bool CanItemFitAtPosition(Item item, Vector2Int pos)
        {
            GridItem itemCandidate = new GridItem(item, pos);
            return CanAddItemCandidate(itemCandidate);
        }
        
        public Item[] GetAllItems()
        { return items.Select(item => item.GetItem()).ToArray(); }
        
        public GridItem[] GetAllGridItems()
        { return items.ToArray(); }
        
        public GridItem GetGridItem(Item item)
        { return items.FirstOrDefault(gridItem => gridItem.GetItem() == item); }
        
        public Vector2Int GetSize()
        {
            return containerSize;
        }
        
        public Item GetItemAtPosition(Vector2Int pos)
        {
            return items.FirstOrDefault(item => item.IsItemInPosition(pos))?.GetItem();
        }
        
        private bool CanAddItemCandidate(GridItem item)
        {
            //Need a list of vector 2 integers - this will be the "span" of the item
            var span = item.GetItemSpan();
            foreach (var itemPos in span)
            {
                if (!IsPositionValid(itemPos)) { return false; }
                if (IsPositionOccupied(itemPos)) { return false; }
            }
            return true;
        }
        
        private bool IsPositionValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < containerSize.x && 
                   pos.y >= 0 && pos.y < containerSize.y;
        }
        private bool IsPositionOccupied(Vector2Int pos) { return occupiedPositions.Contains(pos); }
        
        private void AddOccupiedPositions(IEnumerable<Vector2Int> positions)
        {
            foreach (var pos in positions) { occupiedPositions.Add(pos); }
        }

        private void RemoveOccupiedPositions(IEnumerable<Vector2Int> positions)
        {
            foreach (var pos in positions) { occupiedPositions.Remove(pos); }
        }
    }
    
    public class GridPosition
    {
        private Vector2Int gridPosition;
        private Vector2Int size;
        private readonly Vector2Int[] span;
        
        public Vector2Int GetPosition() => gridPosition;

        public GridPosition(Vector2Int location, Vector2Int size)
        {
            gridPosition = location;
            this.size = size;
            span = GenerateSpan();
        }

        private Vector2Int[] GenerateSpan()
        {
            List<Vector2Int> tempSpan = new();
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    tempSpan.Add(new Vector2Int(gridPosition.x + i, gridPosition.y + j));
                }
            }

            return tempSpan.ToArray();
        }
        
        public Vector2Int[] GetSpan() => span;
    }
}