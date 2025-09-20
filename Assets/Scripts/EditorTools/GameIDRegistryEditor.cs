#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    [CustomEditor(typeof(GameIDRegistry))]
    public class GameIDRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            EditorGUILayout.Space();
        
            GameIDRegistry registry = (GameIDRegistry)target;

            if (!GUILayout.Button("Refresh IDs from Resources", GUILayout.Height(30))) return;
            registry.RefreshIDsFromResources();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log("ID Registry refreshed manually.");
        }
    }
}
#endif