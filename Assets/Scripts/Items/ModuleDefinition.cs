using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "ModuleDefinition", menuName = "Courier/Items/Module Definition")]
    public class ModuleDefinition : ContainerDefinition
    {
        [Header("Module Properties")]
        public ModuleType moduleType = ModuleType.GeneralPurpose;
        public Vector2Int moduleSize = new(1, 2); // Size on the frame grid
        
        [Header("Module Constraints")]
        public bool waterproof = false;
        public float weightModifier = 1.0f;
        public ItemCategory[] allowedCategories; // null = all categories allowed

        public ModuleItem CreateModuleFromDefinition() { return new ModuleItem(this); }
    }
    
    public enum ModuleType
    {
        GeneralPurpose,
        Liquids,
        Mushrooms,
        Ore,
        Waterproof,
        Specialized
    }
    
    public enum ItemCategory
    {
        General,
        Food,
        Liquid,
        Ore,
        Mushroom,
        Tool,
        Package
    }
}