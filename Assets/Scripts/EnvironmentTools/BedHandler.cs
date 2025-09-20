using GameServices;
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    /// <summary>
    /// The bed handler is an interactable that lets the player go to sleep. Several things happen here:
    /// 1. They pass same amount of time (8 hours default)
    /// 2. Based on time passed, they heal fatigue
    /// 3. They SAVE their game if it is a camp bed.
    /// </summary>
    [RequireComponent(typeof(TimedInteraction))]
    public class BedHandler : MonoBehaviour, ITimedInteractionHandler
    {
        private ClockService clockService;
        private SaveService saveService;
        private void Start()
        {
            clockService = ServiceLocator.GetService<ClockService>();
            saveService = ServiceLocator.GetService<SaveService>();
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            Debug.Log("Adding clock time!");
            clockService.AddTime(60*8);
            
            //save game
            Debug.Log("Saving...");
            saveService.SaveGame("TestSave1");
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            Debug.Log("Interacting with Bed");
            return true;
        }
    }
}