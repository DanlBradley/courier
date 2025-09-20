using System.Collections.Generic;
using Quests;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace EditorTools
{
    public class QuestDefinitionWizard : EditorWindow
    {
        // Basic Info
        private string questID = "";
        private new string title = "";
        private string description = "";
        
        // Lists
        private List<string> requirements = new();
        private List<string> objectives = new();
        private List<QuestReward> rewards = new();
        
        // UI State
        private Vector2 scrollPosition;
        private bool showRequirements = true;
        private bool showObjectives = true;
        private bool showRewards = true;
        private bool showProperties = true;
        
        // Template selection
        private int selectedTemplate = 0;
        private string[] templateNames = new string[] { "None", "Kill Quest", "Collect Quest", "Delivery Quest", "Escort Quest" };
        
        [MenuItem("Tools/Forage/Quest Definition Wizard")]
        public static void ShowWindow()
        {
            GetWindow<QuestDefinitionWizard>("Quest Definition Wizard");
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Title
            GUILayout.Label("Quest Definition Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawEditPanel();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawEditPanel()
        {
            // Template Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Template:", GUILayout.Width(80));
            int newTemplate = EditorGUILayout.Popup(selectedTemplate, templateNames);
            if (newTemplate != selectedTemplate)
            {
                selectedTemplate = newTemplate;
                if (selectedTemplate > 0)
                {
                    if (EditorUtility.DisplayDialog("Apply Template", 
                        "Apply the selected template? This will replace your current quest data.", 
                        "Apply", "Cancel"))
                    {
                        ApplyTemplate(selectedTemplate);
                    }
                    else
                    {
                        selectedTemplate = 0;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Basic Info
            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Quest ID:", GUILayout.Width(80));
            questID = EditorGUILayout.TextField(questID);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Title:", GUILayout.Width(80));
            title = EditorGUILayout.TextField(title);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.LabelField("Description:");
            description = EditorGUILayout.TextArea(description, GUILayout.Height(60));
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Requirements
            showRequirements = EditorGUILayout.Foldout(showRequirements, "Requirements", true, EditorStyles.foldoutHeader);
            if (showRequirements)
            {
                for (int i = 0; i < requirements.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Requirement {i + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        requirements.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type:", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID:", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Description:", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                
                if (GUILayout.Button("Add Requirement"))
                {
                    requirements.Add(questID);
                }
            }
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Objectives
            showObjectives = EditorGUILayout.Foldout(showObjectives, "Objectives", true, EditorStyles.foldoutHeader);
            if (showObjectives)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Objective {i + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        objectives.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID:", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Description:", GUILayout.Width(80));
                    objectives[i] = EditorGUILayout.TextField(objectives[i]);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
            }
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Rewards
            showRewards = EditorGUILayout.Foldout(showRewards, "Rewards", true, EditorStyles.foldoutHeader);
            if (showRewards)
            {
                for (int i = 0; i < rewards.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label($"Reward {i + 1}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Remove", GUILayout.Width(80)))
                    {
                        rewards.RemoveAt(i);
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        continue;
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("ID:", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Description:", GUILayout.Width(80));
                    rewards[i].description = EditorGUILayout.TextField(rewards[i].description);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type:", GUILayout.Width(80));
                    rewards[i].rewardType = (RewardType)EditorGUILayout.EnumPopup(rewards[i].rewardType);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Amount:", GUILayout.Width(80));
                    rewards[i].amount = EditorGUILayout.IntField(rewards[i].amount);
                    EditorGUILayout.EndHorizontal();
                    
                    // Only show item ID for item rewards
                    if (rewards[i].rewardType == RewardType.Item)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Item ID:", GUILayout.Width(80));
                        rewards[i].itemID = EditorGUILayout.TextField(rewards[i].itemID);
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
                
                if (GUILayout.Button("Add Reward"))
                {
                    rewards.Add(new QuestReward
                    {
                        description = "",
                        rewardType = RewardType.Experience,
                        amount = 0,
                        itemID = ""
                    });
                }
            }
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Properties
            showProperties = EditorGUILayout.Foldout(showProperties, "Properties", true, EditorStyles.foldoutHeader);
            if (showProperties)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Cancelable:", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Repeatable:", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Fail on Death:", GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            EditorGUILayout.Space();
            DrawSeparator();
            
            // Create Quest Button
            EditorGUILayout.BeginHorizontal();
            
            // Validate button
            GUI.enabled = !string.IsNullOrWhiteSpace(questID) && !string.IsNullOrWhiteSpace(title) && objectives.Count > 0;
            if (GUILayout.Button("Validate Quest Data", GUILayout.Height(30)))
            {
                ValidateQuestData();
            }
            
            // Create button
            GUI.enabled = !string.IsNullOrWhiteSpace(questID) && !string.IsNullOrWhiteSpace(title) && objectives.Count > 0;
            if (GUILayout.Button("Create Quest Definition", GUILayout.Height(30)))
            {
                CreateQuestDefinition();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space();
        }
        
        private void ValidateQuestData()
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            
            // Check required fields
            if (string.IsNullOrWhiteSpace(questID))
                errors.Add("Quest ID is required.");
            else if (!IsValidID(questID))
                errors.Add("Quest ID must only contain letters, numbers, and underscores.");
                
            if (string.IsNullOrWhiteSpace(title))
                errors.Add("Title is required.");
                
            if (string.IsNullOrWhiteSpace(description))
                warnings.Add("Quest description is empty.");
                
            // Check objectives
            if (objectives.Count == 0)
                errors.Add("At least one objective is required.");
            
            // Check rewards
            if (rewards.Count == 0)
                warnings.Add("Quest has no rewards.");
                
            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].amount <= 0)
                    warnings.Add($"Reward {i+1} has an amount of {rewards[i].amount}, which might cause issues.");
                    
                if (rewards[i].rewardType == RewardType.Item && string.IsNullOrWhiteSpace(rewards[i].itemID))
                    warnings.Add($"Item reward {i+1} has an empty item ID.");
            }
            
            // Display results
            if (errors.Count > 0 || warnings.Count > 0)
            {
                string message = "";
                
                if (errors.Count > 0)
                {
                    message += "ERRORS:\n";
                    foreach (var error in errors)
                    {
                        message += $"• {error}\n";
                    }
                    message += "\n";
                }
                
                if (warnings.Count > 0)
                {
                    message += "WARNINGS:\n";
                    foreach (var warning in warnings)
                    {
                        message += $"• {warning}\n";
                    }
                }
                
                EditorUtility.DisplayDialog("Validation Results", message, "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Results", "No issues found! The quest data is valid.", "OK");
            }
        }
        
        private bool IsValidID(string id)
        {
            // Check if ID only contains letters, numbers, and underscores
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
            }
            return true;
        }
        
        private void CreateQuestDefinition()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(questID) || string.IsNullOrWhiteSpace(title) || objectives.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Quest ID, title, and at least one objective are required!", "OK");
                return;
            }
            
            // Create the quest definition asset
            QuestDefinition questDef = CreateInstance<QuestDefinition>();
            
            // Fill in the quest definition
            questDef.questID = questID;
            questDef.title = title;
            questDef.description = description;
            
            // Requirements
            questDef.requiredQuestIDs = new List<string>(requirements);
            
            // Objectives and rewards
            questDef.rewards = new List<QuestReward>(rewards);

            
            // Save the asset
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Quest Definition",
                questID,
                "asset",
                "Save quest definition asset"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(questDef, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", "Quest Definition created successfully!", "OK");
                
                // Open the asset in the inspector
                Selection.activeObject = questDef;
            }
        }
        
        private void ApplyTemplate(int templateIndex)
        {
            switch (templateIndex)
            {
                case 1: // Kill Quest
                    ApplyKillQuestTemplate();
                    break;
                case 2: // Collect Quest
                    ApplyCollectQuestTemplate();
                    break;
                case 3: // Delivery Quest
                    ApplyDeliveryQuestTemplate();
                    break;
                case 4: // Escort Quest
                    ApplyEscortQuestTemplate();
                    break;
            }
        }
        
        private void ApplyKillQuestTemplate()
        {
            string enemyType = "Wolf";
            int count = 5;
            
            // Basic Info
            questID = "kill_" + enemyType.ToLower() + "_quest";
            title = $"Hunt {enemyType}s";
            description = $"The local village is being threatened by {enemyType}s. Hunt down {count} {enemyType}s to make the area safe again.";
            
            // Requirements
            requirements.Clear();
            
            // Objectives
            objectives.Clear();
            objectives.Add($"Kill {enemyType}s");
            
            // Rewards
            rewards.Clear();
            rewards.Add(new QuestReward
            {
                description = "Gold Reward",
                rewardType = RewardType.Currency,
                amount = 100,
                itemID = ""
            });
        }
        
        private void ApplyCollectQuestTemplate()
        {
            string itemType = "Herb";
            int count = 10;
            
            // Basic Info
            questID = "collect_" + itemType.ToLower() + "_quest";
            title = $"Gather {itemType}s";
            description = $"The local healer needs medicinal {itemType}s. Collect {count} {itemType}s from the forest.";
            
            // Requirements
            requirements.Clear();
            
            // Objectives
            objectives.Clear();
            objectives.Add($"Collect {itemType}s");
            
            // Rewards
            rewards.Clear();
            rewards.Add(new QuestReward
            {
                description = "Healing Potion",
                rewardType = RewardType.Item,
                amount = 1,
                itemID = "healing_potion"
            });
        }
        
        private void ApplyDeliveryQuestTemplate()
        {
            // Basic Info
            questID = "deliver_package_quest";
            title = "Delivery Service";
            description = "Deliver a package from the village elder to the outpost commander. Be careful, the route might be dangerous.";
            
            // Requirements
            requirements.Clear();
            
            // Objectives
            objectives.Clear();
            objectives.Add("Get the package from the elder");
            objectives.Add("Deliver the package to the outpost");
            
            // Rewards
            rewards.Clear();
            rewards.Add(new QuestReward
            {
                description = "Gold Reward",
                rewardType = RewardType.Currency,
                amount = 150,
                itemID = ""
            });
        }
        
        private void ApplyEscortQuestTemplate()
        {
            // Basic Info
            questID = "escort_merchant_quest";
            title = "Escort the Merchant";
            description = "A merchant needs an escort to the neighboring town. Protect them from bandits along the way.";

            
            // Objectives
            objectives.Clear();
            objectives.Add("Meet the merchant at the town square");
            objectives.Add("Escort the merchant to the neighboring town");
            objectives.Add("Defeat bandits who attack the merchant");
            
            // Rewards
            rewards.Clear();
            rewards.Add(new QuestReward
            {
                description = "Gold Reward",
                rewardType = RewardType.Currency,
                amount = 200,
                itemID = ""
            });
            rewards.Add(new QuestReward
            {
                description = "Experience Points",
                rewardType = RewardType.Experience,
                amount = 500,
                itemID = ""
            });
        }
    }
}
#endif