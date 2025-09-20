using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Dialogue
{
    public class DialogueActionRegistry : MonoBehaviour
    {
        [Tooltip("One or more mapping assets containing dialogue action mappings")] [SerializeField]
        private List<DialogueActionMappingsData> mappingAssets = new();
        private readonly Dictionary<ActionCategory, Action<string>> _registeredHandlers = new();
        
        public void RegisterActionHandler(ActionCategory actionCategory, Action<string> handler)
        { _registeredHandlers[actionCategory] = handler; }
        
        public bool ProcessChoice(string dialogueId, string choiceText)
        {
            bool actionTriggered = false;
            foreach (var mappingAsset in mappingAssets)
            {
                if (mappingAsset == null) continue;
                foreach (var mapping in mappingAsset.Mappings)
                {
                    bool dialogueMatches = string.IsNullOrEmpty(mapping.DialogueId) || dialogueId == mapping.DialogueId;
                    bool textMatches = IsMatch(choiceText, mapping.ChoicePattern);
                    if (!dialogueMatches || !textMatches) continue;
                        
                    if (_registeredHandlers.TryGetValue(mapping.ActionCategory, out var handler))
                    {
                        handler(mapping.ActionId);
                        actionTriggered = true;
                    }
                    else
                    { Debug.LogWarning($"No handler registered for action category: {mapping.ActionCategory}"); }
                }
            }
            return actionTriggered;
        }
        
        private static bool IsMatch(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}