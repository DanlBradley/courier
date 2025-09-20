using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button continueBtn;
        [SerializeField] private Button newGameBtn;
        [SerializeField] private Button loadGameBtn;
        [SerializeField] private Button optionsBtn;
        [SerializeField] private Button exitBtn;
        [SerializeField] private SceneAsset newGameScene;
        
        [Header("Load Game Menu Elements")]
        [SerializeField] private GameObject loadGamePanel;
        [SerializeField] private GameObject saveFileButtonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private Button backBtn;

        private string[] saveFiles;

        private void Awake()
        {
            continueBtn.onClick.AddListener(Continue);
            newGameBtn.onClick.AddListener(NewGame);
            loadGameBtn.onClick.AddListener(LoadGame);
            optionsBtn.onClick.AddListener(Options);
            exitBtn.onClick.AddListener(Exit);
            backBtn.onClick.AddListener(() => loadGamePanel.SetActive(false));
            
            saveFiles = SaveTools.GetAllSaveFiles();
            if (saveFiles == null || saveFiles.Length < 1) { continueBtn.gameObject.SetActive(false); }
        }

        private void Continue()
        {
            SaveTools.LoadLatestSaveFile(saveFiles[0], newGameScene.name);
        }
        private void NewGame() { SaveTools.LoadNewGame(newGameScene.name); }
        private void LoadGame() { PopulateSaveFileButtons(); loadGamePanel.SetActive(true); }
        private void Options() { Debug.LogError("Options not implemented!"); }
        private void Exit() { Debug.LogError("Exit not implemented!"); }
        
        private void PopulateSaveFileButtons()
        {
            foreach (Transform child in buttonContainer) { Destroy(child.gameObject); }
            
            foreach (string saveFile in saveFiles)
            {
                GameObject saveFileObj = Instantiate(saveFileButtonPrefab, buttonContainer);
                GameObject saveFileBtnObj = saveFileObj.transform.Find("SaveFileBtn").gameObject;
                GameObject saveFileDeleteBtnObj = saveFileObj.transform.Find("SaveFileDeleteBtn").gameObject;
                
                //Populate the Save File Button
                Button button = saveFileBtnObj.GetComponent<Button>();
                TMP_Text buttonText = saveFileBtnObj.GetComponentInChildren<TMP_Text>();
                buttonText.text = SaveTools.GetSaveDisplayInfo(saveFile);
                button.onClick.AddListener(() => SaveTools.LoadSaveFile(saveFile, newGameScene.name));
                
                //Populate the Delete Save File Button
                Button deleteButton = saveFileDeleteBtnObj.GetComponent<Button>();
                deleteButton.onClick.AddListener(() =>
                { if (SaveTools.DeleteSaveFile(saveFile)) { PopulateSaveFileButtons(); } });
            }
        }
    }
}
