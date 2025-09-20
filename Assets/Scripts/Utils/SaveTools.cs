using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GameServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public static class SaveTools
    {
        
        private static string SaveDirectory => Application.persistentDataPath + "/saves/";
        private const string SAVE_EXTENSION = ".crr";
        
        public static void LoadSaveFile(string saveName, string sceneToLoad)
        {
            PlayerPrefs.SetString("GameLoaded", saveName);
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
        
        //TODO: Not implemented yet. Just does the same thing as LoadSaveFile.
        public static void LoadLatestSaveFile(string saveName, string sceneToLoad)
        {
            PlayerPrefs.SetString("GameLoaded", saveName);
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }

        public static void LoadNewGame(string sceneToLoad)
        {
            PlayerPrefs.SetString("GameLoaded", "false");
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }

        public static bool DeleteSaveFile(string saveName)
        {
            string path = SaveDirectory + saveName + SAVE_EXTENSION;
            if (!File.Exists(path)) { return false; }
            File.Delete(path);
            return true;
        }
        
        public static string[] GetAllSaveFiles()
        {
            if (!Directory.Exists(SaveDirectory)) { return Array.Empty<string>(); }
            //drop temp save files -- used for scene swapping
            List<string> files = GetCourierSaveFiles().ToList();
            var filesToRemove = files.Where(file => Path.GetFileNameWithoutExtension(file) == "temp").ToList();
            foreach (var file in filesToRemove) { files.Remove(file); }
            
            for (int i = 0; i < files.Count; i++) { files[i] = Path.GetFileNameWithoutExtension(files[i]); }
            return files.ToArray();
        }

        public static string GetSaveDisplayInfo(string saveName)
        {
            SaveData saveData = ReadSaveFile(saveName);
            if (saveData == null) return saveName;
    
            if (!saveData.saveableStates.TryGetValue("clock_service", out var clockData) ||
                clockData is not ClockServiceSaveData clockSave)
            { return $"{saveName}\n{saveData.saveTime:MM/dd/yyyy HH:mm}"; }
            
            WorldTime worldTime = new WorldTime(clockSave.totalTimeInMinutes);
            return $"{saveName}\nYear {worldTime.Year}, Month {worldTime.Month}, Day {worldTime.Day}" +
                   $"\n{saveData.saveTime:MM/dd/yyyy HH:mm}";
        }

        
        
        private static SaveData ReadSaveFile(string saveName)
        {
            string path = SaveDirectory + saveName + SAVE_EXTENSION;
            if (!File.Exists(path)) return null;
    
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using FileStream stream = new FileStream(path, FileMode.Open);
                return formatter.Deserialize(stream) as SaveData;
            }
            catch (Exception e) 
            { 
                Debug.LogError($"Failed to read save file {saveName}: {e.Message}"); 
                return null; 
            }
        }
        
        private static string[] GetCourierSaveFiles()
        { return Directory.GetFiles(SaveDirectory, "*" + SAVE_EXTENSION); }
    }
}