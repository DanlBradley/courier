using GameServices;
using Interfaces;
using UnityEditor;
using UnityEngine;
using Utils;

namespace EnvironmentTools
{
    [RequireComponent(typeof(TimedInteraction))]
    public class LevelExitHandler : MonoBehaviour, ITimedInteractionHandler
    {
        [SerializeField] private bool canBeUsed = true;
        [SerializeField] private SceneAsset sceneToExitTo;
        private SaveService saveService;

        private void Awake()
        {
            saveService = ServiceLocator.GetService<SaveService>();
        }

        public void OnInteractionComplete(GameObject interactor)
        {
            LoadRegion(sceneToExitTo.name);
        }

        public bool CanPerformInteraction(GameObject interactor)
        {
            return canBeUsed;
        }
        
        private void LoadRegion(string sceneToLoad)
        {
            PlayerPrefs.SetString("SceneSwap", "temp");
            saveService.SaveGame("temp");
            SaveTools.LoadSaveFile("temp",sceneToLoad);
        }
    }
}