using System;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "EquipmentDefinition", menuName = "Courier/Items/Equipment Definition")]
    public class EquipmentDefinition : ItemDefinition
    {
        [Header("Equipment Config")] 
        public EquipmentSlot slot;
    }

    public class EquipmentItem : Item
    {
        public EquipmentItem(EquipmentDefinition equipmentDefinition) : base(equipmentDefinition)
        { if (equipmentDefinition == null) throw new ArgumentException("ContainerDefinition cannot be null"); }
        
        public EquipmentDefinition GetEquipmentDefinition() { return (EquipmentDefinition)itemDef; }
    }
    
    public enum EquipmentSlot 
    { 
        Head, 
        Torso, 
        Legs, 
        Feet, 
        Hands,
        MainHand,
        OffHand,
        TwoHanded,
        Accessory
    }
}