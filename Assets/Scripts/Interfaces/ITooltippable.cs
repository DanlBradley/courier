using UnityEngine;

namespace Interfaces
{
    public interface ITooltippable
    {
        string GetTooltipTitle();
        string GetTooltipDescription();
        string GetTooltipDetails(); // Optional extra info like "Size: 2x3" or "Requires: Level 5"
        float GetTooltipDelay(); // Allow different delays, return 0.5f for default
    }
}