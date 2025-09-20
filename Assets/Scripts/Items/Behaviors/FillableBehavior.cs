using EnvironmentTools;
using UnityEngine;

namespace Items.Behaviors
{
    [CreateAssetMenu(menuName = "Courier/Items/Behaviors/Fillable")]
    public class FillableBehavior : ItemBehavior
    {
        public string liquidType = "water";
        public override void InitializeItemState(Item item) { }

        public override bool CanUse(Item item, UseContext context)
        {
            if (context.useType != UseType.Interact) return false;
            if (context.target == null) return false;
            if (context.target.GetComponent<WaterSource>() is null) return false;
            
            float current = item.GetState($"{liquidType}_current", 0f);
            float max = item.GetState($"{liquidType}_max", 0f);
            return current < max;
        }

        public override void Use(Item item, UseContext context)
        {
            float max = item.GetState($"{liquidType}_max", 0f);
            item.SetState($"{liquidType}_current", max);
            Debug.Log($"Refilled {liquidType} up to {max} units");
        }
    }
}