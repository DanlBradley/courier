using Interfaces;
using UnityEngine;

namespace Character.Player
{
    public class PlayerInventory : MonoBehaviour, IContainerOwner
    {
        public string GetOwnerID()
        {
            return gameObject.name;
        }
    }
}