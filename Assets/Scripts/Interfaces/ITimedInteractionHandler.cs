using UnityEngine;

namespace Interfaces
{
    public interface ITimedInteractionHandler
    {
        void OnInteractionComplete(GameObject interactor);
        bool CanPerformInteraction(GameObject interactor);
    }
}