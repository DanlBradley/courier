using Character;
using GameServices;
using UnityEngine;

namespace Items.Behaviors
{
    [CreateAssetMenu(menuName = "Courier/Items/Behaviors/Drinkable")]
    public class ConsumableBehavior : ItemBehavior
    {
        [Header("Consumable Settings")]
        public float maxCapacity = 60f;
        public float initialAmount = 60f;
        public ConsumableType type;
        public float amountPerUse = 20f;
        public bool destroyOnConsume;
    
        public override void InitializeItemState(Item item)
        {
            item.SetState($"{type}_current", initialAmount);
            item.SetState($"{type}_max", maxCapacity);
        }
    
        public override bool CanUse(Item item, UseContext context)
        {
            if (context.useType != UseType.Hotkey) return false;
            float current = item.GetState($"{type}_current", 0f);
            if (current <= 0) return false;
            return true;
        }
    
        public override void Use(Item item, UseContext useContext)
        {
            float current = item.GetState($"{type}_current", 0f);
            float drinkAmount = Mathf.Min(current, amountPerUse);
        
            item.SetState($"{type}_current", current - drinkAmount);
            var characterStatus = GameManager.Instance.GetPlayer().GetComponent<CharacterStatus>();
            Debug.Log($"Consumed {drinkAmount} {type}. Remaining: {current - drinkAmount}");
            switch (type)
            {
                case ConsumableType.Water:
                    characterStatus.UpdateVital(drinkAmount, Vitals.Hydration);
                    break;
                case ConsumableType.Food:
                    characterStatus.UpdateVital(drinkAmount, Vitals.Satiety);
                    break;
            }

            if (destroyOnConsume && (current - drinkAmount) < 1f)
            { ServiceLocator.GetService<InventoryService>().TryDestroyItem(item); }
        }
    
        public float GetCurrentAmount(Item item) => item.GetState($"{type}_current", 0f);
        public float GetMaxCapacity(Item item) => item.GetState($"{type}_max", maxCapacity);
    }

    public enum ConsumableType
    {
        Water,
        Food,
        Other
    }
}