using GameServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class WorldMapManager : MonoBehaviour
    {
        [SerializeField] private Button woodsBtn;
        [SerializeField] private SceneAsset woodsScene;
        [SerializeField] private Button mtnVillageBtn;
        [SerializeField] private SceneAsset mtnVillageScene;
        private SaveService saveService;

        private void Awake()
        {
            woodsBtn.onClick.AddListener(() => LoadRegion(woodsScene.name));
            mtnVillageBtn.onClick.AddListener(() => LoadRegion(mtnVillageScene.name));
            saveService = ServiceLocator.GetService<SaveService>();

        }

        private void LoadRegion(string sceneToLoad)
        {
            // Hide the world map panel (this component)
            if (transform != null)
            {
                transform.gameObject.SetActive(false);
            }
            
            // Switch back to exploration control scheme
            GameStateManager.Instance.ChangeState(GameState.Exploration);

            PlayerPrefs.SetString("SceneSwap", "temp");
            saveService.SaveGame("temp");
            SaveTools.LoadSaveFile("temp",sceneToLoad);
        }
    }
}
