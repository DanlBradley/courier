using UnityEngine;
using System.Collections.Generic;

namespace Items
{
    [CreateAssetMenu(fileName = "BackpackFrameDefinition", menuName = "Courier/Items/Backpack Frame Definition")]
    public class BackpackFrameDefinition : ContainerDefinition
    {
        [Header("Starting Modules")]
        public List<ModuleDefinition> defaultModules = new();
    }
}