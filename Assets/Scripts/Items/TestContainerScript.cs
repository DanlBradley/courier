using UnityEngine;

namespace Items
{
    public class TestContainerScript : MonoBehaviour
    {
        [SerializeField] private ItemDefinition itemDefToAdd;
        
        
        private void Start()
        {
            //Initialize an itemInstance
            var itemToAdd = new Item(itemDefToAdd);
            
            //Make the container
            Container myContainer = new Container(new Vector2Int(5, 4));
            Debug.Log($"Added a container of size {myContainer.GetSize()}");
            
            //Add an item
            AddItem(myContainer, itemToAdd);
            AddItem(myContainer, itemToAdd);
            AddItem(myContainer, itemToAdd);
            AddItem(myContainer, itemToAdd);
            AddItem(myContainer, itemToAdd);
            AddItem(myContainer, itemToAdd);
        }

        private void AddItem(Container container, Item item)
        {
            bool itemAdded = container.TryAddItem(item);
            Debug.Log(itemAdded
                ? $"Added a {item.itemDef.name} to the container."
                : $"Failed to add a {item.itemDef.name} to the container! Container is full."); 
        }
    }
}
