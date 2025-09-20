#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Ink.Runtime;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public class InkPathReference : EditorWindow
    {
        private TextAsset inkJsonAsset;
        private string pathListing = "";
        private Vector2 scrollPosition;
    
        [MenuItem("Tools/Ink/Path Reference")]
        public static void ShowWindow()
        {
            GetWindow<InkPathReference>("Ink Path Reference");
        }
    
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Ink Path Reference", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        
            EditorGUILayout.HelpBox(
                "Select an Ink JSON asset to see its paths.\n" +
                "You can use these paths in your dialogue action mappings.", 
                MessageType.Info);
        
            TextAsset newInkJsonAsset = (TextAsset)EditorGUILayout.ObjectField(
                "Ink JSON Asset", inkJsonAsset, typeof(TextAsset), false);
            
            if (newInkJsonAsset != inkJsonAsset)
            {
                inkJsonAsset = newInkJsonAsset;
                if (inkJsonAsset != null)
                {
                    ExtractPaths();
                }
                else
                {
                    pathListing = "";
                }
            }
        
            EditorGUILayout.Space();
        
            if (!string.IsNullOrEmpty(pathListing))
            {
                if (GUILayout.Button("Copy All Paths"))
                {
                    EditorGUIUtility.systemCopyBuffer = pathListing;
                }
            
                EditorGUILayout.Space();
            
                EditorGUILayout.LabelField("Paths:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                EditorGUILayout.TextArea(pathListing, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }
    
        private void ExtractPaths()
        {
            try
            {
                // Create a temporary story
                Story story = new Story(inkJsonAsset.text);
            
                // Extract the paths
                StringBuilder sb = new StringBuilder();
                HashSet<string> knots = new HashSet<string>();
                HashSet<string> stitches = new HashSet<string>();
            
                // Analyze the JSON directly for more reliable extraction
                string jsonText = inkJsonAsset.text;
                int index = 0;
            
                while (true)
                {
                    index = jsonText.IndexOf("\"^", index);
                    if (index == -1) break;
                
                    // Find the end of the path
                    int endIndex = jsonText.IndexOf("\"", index + 1);
                    if (endIndex == -1) break;
                
                    // Extract the path (removing the ^ symbol)
                    string path = jsonText.Substring(index + 2, endIndex - index - 2);
                
                    // Add to appropriate collection
                    if (path.Contains("."))
                    {
                        stitches.Add(path);
                    }
                    else
                    {
                        knots.Add(path);
                    }
                
                    index = endIndex;
                }
            
                // Build the output
                sb.AppendLine("=== KNOTS ===");
                foreach (string knot in knots)
                {
                    sb.AppendLine(knot);
                }
            
                sb.AppendLine("\n=== STITCHES ===");
                foreach (string stitch in stitches)
                {
                    sb.AppendLine(stitch);
                }
            
                pathListing = sb.ToString();
            }
            catch (System.Exception e)
            {
                pathListing = $"Error extracting paths: {e.Message}";
                Debug.LogError($"Error extracting Ink paths: {e}");
            }
        }
    }
}
#endif