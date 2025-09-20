using System;
using GameServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [SerializeField] private Button quitBtn;
        [SerializeField] private SceneAsset mainMenuScene;

        private void Awake()
        {
            quitBtn.onClick.AddListener(Quit);
        }

        private void Quit()
        {
            gameObject.SetActive(false);

            // Use GameManager's cleanup method if available
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
            // then load scene mgr
            SceneManager.LoadScene(mainMenuScene.name, LoadSceneMode.Single);
        }
    }
}
