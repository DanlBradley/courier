namespace Items
{
    /// <summary>
    /// Wraps a BackpackFrame's module grid to make it compatible with InventoryUI
    /// </summary>
    public class FrameGridContainerWrapper : ContainerItem
    {
        private BackpackFrame frame;

        public FrameGridContainerWrapper(
            ContainerDefinition definition, BackpackFrame backpackFrame) : base(definition)
        {
            frame = backpackFrame;
        }
        
        public override Container storage => frame.moduleGridItem.storage;
    }
}