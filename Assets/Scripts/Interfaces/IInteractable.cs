using UnityEngine;

namespace Interfaces
{
    public interface IInteractable
    {
        bool CanInteract(GameObject interactor);
        void StartInteraction(GameObject interactor, System.Action onComplete, System.Action onCancel);
        void CancelInteraction();
        GameObject GetGameObject();
    }
}