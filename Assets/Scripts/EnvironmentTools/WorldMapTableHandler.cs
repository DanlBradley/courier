using System;
using System.ComponentModel.Design;
using GameServices;
using Interfaces;
using UnityEngine;

namespace EnvironmentTools
{
    [RequireComponent(typeof(TimedInteraction))]
    public class WorldMapTableHandler : MonoBehaviour, ITimedInteractionHandler
    {
        [SerializeField] private bool canBeUsed = true;

        private UIService uiService;

        private void Awake()
        {
            uiService = ServiceLocator.GetService<UIService>();
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            uiService.ToggleWorldMapMenu();
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            return canBeUsed;
        }
    }
}
