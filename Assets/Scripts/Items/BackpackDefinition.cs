using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "BackpackDefinition", menuName = "Courier/Items/Backpack Definition")]
    public class BackpackDefinition : ItemDefinition
    {
        [Header("Compartment Definitions")]
        public List<ContainerDefinition> defaultCompartments = new();
    }

    public class BackpackItem : Item
    {
        private List<ContainerItem> compartments = new();
        public BackpackItem(BackpackDefinition backpackDef) : base(backpackDef) { }
        public List<ContainerItem> GetAllCompartments() => compartments;
        public void AddCompartment(ContainerItem compartment) { compartments.Add(compartment); }
    }
}