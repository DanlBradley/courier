using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace Items
{
    public class BackpackFrame
    {
        public BackpackFrameDefinition definition { get; }
        public ContainerItem moduleGridItem { get; }

        public BackpackFrame(BackpackFrameDefinition frameDef, bool loadDefaultModules = true)
        {
            definition = frameDef;
            moduleGridItem = new ContainerItem(definition);
            // moduleGrid = new Container(definition.frameGridSize);
            
            if (!loadDefaultModules) return;
            foreach (var moduleDef in definition.defaultModules)
            {
                var module = new ModuleItem(moduleDef);
                moduleGridItem.storage.TryAddItem(module);
            }
        }
    
        public IEnumerable<ModuleItem> GetAttachedModules()
        {
            return moduleGridItem.storage.GetAllItems().OfType<ModuleItem>().Select(item => item);
        }
        public IEnumerable<GridItem> GetAttachedModulesAsGridItems()
        {
            return moduleGridItem.storage.GetAllGridItems();
        }
    }
    
    [Serializable]
    public class BackpackFrameSaveData
    {
        public ContainerSaveData moduleGridContainer;
        public List<ModuleSaveData> modules;

        public BackpackFrameSaveData(BackpackFrame frame)
        {
            moduleGridContainer = new ContainerSaveData(frame.moduleGridItem);
            
            modules = new List<ModuleSaveData>();
            foreach (var moduleGriditem in frame.GetAttachedModulesAsGridItems())
            { modules.Add(new ModuleSaveData(moduleGriditem)); }
        }
    }
    
    [Serializable]
    public class ModuleSaveData
    {
        public ContainerSaveData moduleContainer;
        public int gridX;
        public int gridY;

        public ModuleSaveData(GridItem moduleGridItem)
        {
            var moduleRootPosition = moduleGridItem.GetRootPosition();
            moduleContainer = new ContainerSaveData(moduleGridItem.GetItem() as ContainerItem);
            gridX = moduleRootPosition.x;
            gridY = moduleRootPosition.y;
        }
    }
}