using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Interfaces;
using UnityEngine;
using Utils;

namespace GameServices
{
    [Serializable]
    public class SaveData
    {
        public Dictionary<string, object> saveableStates = new();
        public DateTime saveTime = DateTime.Now;
        public string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
    
    public class SaveService : Service
    {
        private static string SaveDirectory => Application.persistentDataPath + "/saves/";
        private const string SAVE_EXTENSION = ".crr";
        
        public override void Initialize()
        {
            if (!Directory.Exists(SaveDirectory)) { Directory.CreateDirectory(SaveDirectory); }
            Logs.Log("Save Service Initialized.", "GameServices");
        }
        
        public void SaveGame(string saveName)
        {
            SaveData saveData = new SaveData();
            
            ISaveable[] saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>().ToArray();
            
            Debug.Log($"Found {saveables.Length} saveable objects");
            
            foreach (ISaveable saveable in saveables)
            {
                string id = saveable.SaveID;
                
                if (string.IsNullOrEmpty(id))
                { Debug.LogWarning($"Saveable object {saveable} has null or empty SaveID, skipping..."); continue; }
                
                if (saveData.saveableStates.ContainsKey(id))
                { Debug.LogWarning($"Duplicate SaveID found: {id}. Overwriting previous state."); }
                
                object state = saveable.CaptureState();
                if (state != null) { saveData.saveableStates[id] = state; }
            }
            
            WriteSaveFile(saveName, saveData);
            Debug.Log($"Game saved successfully to {saveName} with {saveData.saveableStates.Count} objects");
        }
        
        public void LoadGame(string saveName)
        {
            SaveData saveData = ReadSaveFile(saveName);
            if (saveData == null) { Debug.LogWarning($"Failed to load save file: {saveName}"); return; }
            ISaveable[] saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>().ToArray();
            
            Debug.Log($"Loading {saveData.saveableStates.Count} saved states to {saveables.Length} saveable objects");
            
            int restoredCount = 0;
            
            foreach (ISaveable saveable in saveables)
            {
                string id = saveable.SaveID;
                if (string.IsNullOrEmpty(id)) continue;
                if (saveData.saveableStates.TryGetValue(id, out var state))
                {
                    try { saveable.RestoreState(state); restoredCount++; }
                    catch (Exception e) { Debug.LogError($"Failed to restore state for {id}: {e.Message}"); }
                }
                else { Debug.LogWarning($"No saved state found for {id}"); }
            }
            
            Debug.Log($"Game loaded successfully. Restored {restoredCount} objects from save.");
        }
        
        public bool SaveExists(string saveName)
        {
            string path = GetSavePath(saveName);
            return File.Exists(path);
        }
        
        public void DeleteSave(string saveName)
        {
            string path = GetSavePath(saveName);
            
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted save file: {saveName}");
            }
            else { Debug.LogWarning($"Save file not found: {saveName}"); }
        }
        
        public string[] GetAllSaveFiles()
        {
            if (!Directory.Exists(SaveDirectory)) { return Array.Empty<string>(); }
            string[] files = Directory.GetFiles(SaveDirectory, "*" + SAVE_EXTENSION);
            for (int i = 0; i < files.Length; i++) { files[i] = Path.GetFileNameWithoutExtension(files[i]); }
            return files;
        }
        
        public SaveData GetSaveMetadata(string saveName) { return ReadSaveFile(saveName); }
        
        private void WriteSaveFile(string saveName, SaveData data)
        {
            string path = GetSavePath(saveName);
            
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using FileStream stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, data);
            }
            catch (Exception e) { Debug.LogWarning($"Failed to write save file {saveName}: {e.Message}"); throw; }
        }
        
        private SaveData ReadSaveFile(string saveName)
        {
            string path = GetSavePath(saveName);
            if (!File.Exists(path)) { Debug.LogWarning($"Save file does not exist: {path}"); return null; }
            
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using FileStream stream = new FileStream(path, FileMode.Open);
                return formatter.Deserialize(stream) as SaveData;
            }
            catch (Exception e) { Debug.LogError($"Failed to read save file {saveName}: {e.Message}"); return null; }
        }
        
        private string GetSavePath(string saveName) { return SaveDirectory + saveName + SAVE_EXTENSION; }
        
        public void QuickSave() { SaveGame("quicksave"); }
        
        public void QuickLoad()
        {
            if (SaveExists("quicksave")) { LoadGame("quicksave"); }
            else { Debug.LogWarning("No quicksave found"); }
        }
        
        public void AutoSave() { SaveGame($"autosave_{DateTime.Now:yyyyMMdd_HHmmss}"); }
    }
}