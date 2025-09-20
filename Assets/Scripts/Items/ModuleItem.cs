using UnityEngine;

namespace Items
{
    public class ModuleItem : ContainerItem
    {
        private string moduleId;
        public string ModuleId => moduleId;
        
        public ModuleDefinition ModuleDefinition => (ModuleDefinition)itemDef;
        
        public ModuleItem(ModuleDefinition definition) : base(definition)
        {
            moduleId = itemDef.id;
        }
    }
}