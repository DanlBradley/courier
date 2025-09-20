using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "DialogueActionMappings", menuName = "Courier/Dialogue/Action Mappings", order = 1)]
    public class DialogueActionMappingsData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The list of all dialogue action mappings")]
        private List<DialogueActionMapping> mappings = new();
        public IReadOnlyList<DialogueActionMapping> Mappings => mappings;
    }
    
    [System.Serializable]
    public class DialogueActionMapping
    {
        [Tooltip("The dialogue ID this mapping applies to (leave empty to match any dialogue)")]
        public string DialogueId;
            
        [Tooltip("The dialogue text pattern to match (use * as wildcard)")]
        public string ChoicePattern;
            
        [Tooltip("The system that should handle this action (Quest, Shop, etc.)")]
        public ActionCategory ActionCategory;
            
        [Tooltip("The specific instance identifier within that category")]
        public string ActionId;
    }

    public enum ActionCategory
    {
        Shop,
        AcceptQuest,
        ProgressQuest,
        CompleteQuest
    }
}