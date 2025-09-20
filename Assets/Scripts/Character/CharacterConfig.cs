using UnityEngine;

namespace Character
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Courier/Character/CharacterConfig")]
    public class CharacterConfig : ScriptableObject
    {
        public float maxHealth;
        public float maxMana;
        public float maxEnergy;
        public float maxHunger;
        public float maxThirst;
        public float restingHeartRate = 60f;
        public float maxHeartRate = 180f;
    }
}
