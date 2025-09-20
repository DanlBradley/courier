using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Courier/Dialogue/Dialogue Asset", order = 1)]
    public class DialogueAsset : ScriptableObject
    {
        [Tooltip("Unique identifier for this dialogue")]
        public string dialogueId;
        
        [Tooltip("The Ink JSON asset")]
        public TextAsset inkAsset;

        [Tooltip("Description or notes about this dialogue")]
        [TextArea(2, 5)]
        public string description;
    }
}