using Character;
using EnvironmentTools;
using GameServices;
using UnityEditor;
using UnityEngine;

namespace Items
{
    public abstract class ItemBehavior : ScriptableObject
    {
        public abstract void InitializeItemState(Item item);
        public abstract bool CanUse(Item item, UseContext context);
        public abstract void Use(Item item, UseContext context);
    }
    
    public enum UseType
    {
        Hotkey,
        Interact,
        ContextMenu
    }

    public class UseContext
    {
        public UseType useType;
        public GameObject user;
        public GameObject target;
        public Vector3 location;
    
        public UseContext(UseType type, GameObject user, GameObject target = null)
        {
            this.useType = type;
            this.user = user;
            this.target = target;
            this.location = user.transform.position;
        }
    }
}