using UnityEngine;
using UnityEngine.Serialization;

namespace Character.AI.AIState
{
    [CreateAssetMenu(fileName = "NpcConfig", menuName = "Courier/Character/NPC Config")]
    public class NpcConfig : ScriptableObject
    {
        public AIStateType[] aiStateTypes;

        [FormerlySerializedAs("enemyType")] [Header("Base NPC Type")]
        public NpcType npcType = NpcType.Humanoid;
        [Header("Vision and Hearing Config")]
        public float baseVisionDistance;
        public float maxFOV;
        public bool canSee = true;
        public bool canHear = true;
        public float memory = 3.5f;
        public float alertedMemory = 8f;
        public string[] enemyTypes;
        
        
        
        /// <summary>
        /// Measured in dBA. The average noise floor should be ~30 except for highly sensitive enemies. In addition,
        /// noise floor falloff rate is twice the (logarithmic) rate of real humans, for balance purposes.
        /// Ref: https://decibelpro.app/content/images/size/w1600/2021/10/8---How-Loud-Is-55-Decibels-1.jpg
        /// </summary>
        public float noiseAttenuationFloor = 30;

        [Header("Movement Config")] 
        public NavAgentData navAgentData;
        
        [Header("Attack Config")]
        public float attackCooldown = 2f;

        public float engagementDistance = 6f;
        
        //Need to reconfigure this to be based on weapon vs. npcConfig
        public float attackRange = 1f;
        public LayerMask targetableLayer;
        
        [Header("Beamos Config")]
        public float beamSpeed = 3f;
    }
    public enum NpcType
    {
        Humanoid,
        Beamos
    }

    [System.Serializable]
    public struct NavAgentData
    {
        public float movementSpeed;
        public float angularSpeed;
        public float stoppingDistance;
    }
}