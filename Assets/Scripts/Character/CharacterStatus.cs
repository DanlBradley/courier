using System;
using GameServices;
using Interfaces;
using StatusEffects;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Character
{
    public enum Vitals
    {
        Health,
        Mana,
        Energy,
        Satiety, // percentage full
        Hydration, // percentage full
        Temperature, //fahrenheit
        HeartRate // BPM
    }
    public class CharacterStatus : MonoBehaviour, ISaveable
    {
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Mana { get; private set; }
        public float MaxMana { get; private set; }
        public float Energy { get; private set; }
        public float MaxEnergy { get; private set; }
        public float Satiety { get; private set; }
        public float MaxSatiety { get; private set; }
        public float Hydration { get; private set; }
        public float MaxHydration { get; private set; }
        /// <summary>
        /// This isn't literal body temp - just a "what it currently feels like" temp. It gets decided by a combination
        /// of the temperature outside, environmental conditions (shade vs. no shade), and equipment.
        /// </summary>
        public float BodyTemp { get; private set; }
        public float RestingHeartRate { get; private set; }
        public float MaxHeartRate { get; private set; }
        public float HeartRate { get; private set; }
        
        public event Action OnStatusUpdate;

        [SerializeField] private SceneAsset mainMenuScene;
        [SerializeField] private float satietyDecayRate = 0.01f;
        [SerializeField] private float hydrationDecayRate = 0.1f;
        [SerializeField] private float energyDecayRate = 0.1f;
        [SerializeField] private CharacterConfig cc;
        [SerializeField] private VitalsEffectsConfig vitalsEffectsConfig;

        private bool loadSaveData;
        
        private void Start() { InitializeStats(); }

        private void InitializeStats()
        {
            MaxHealth = cc.maxHealth;
            MaxMana = cc.maxMana;
            MaxEnergy = cc.maxEnergy;
            MaxSatiety = cc.maxHunger;
            MaxHydration = cc.maxThirst;
            RestingHeartRate = cc.restingHeartRate;
            MaxHeartRate = cc.maxHeartRate;

            if (loadSaveData) return;
            HeartRate = RestingHeartRate;
            BodyTemp = 74f;
            Health = MaxHealth;
            Mana = MaxMana;
            Energy = MaxEnergy;
            Satiety = MaxSatiety;
            Hydration = MaxHydration;
        }
        
        private void OnEnable() { TickService.Instance.OnTick += OnVitalDecay; }
        private void OnDisable() { TickService.Instance.OnTick -= OnVitalDecay; }

        private void OnVitalDecay()
        {
            var statusManager = GetComponent<StatusEffectManager>();
    
            var (satietyAdd, satietyMult, satietyOverride) = statusManager?.GetModifierValues("SatietyDecayRate") ?? (0f, 1f, null);
            var (hydrationAdd, hydrationMult, hydrationOverride) = statusManager?.GetModifierValues("HydrationDecayRate") ?? (0f, 1f, null);
            var (energyAdd, energyMult, energyOverride) = statusManager?.GetModifierValues("EnergyDecayRate") ?? (0f, 1f, null);
    
            float finalSatietyDecay = satietyOverride ?? (satietyDecayRate * satietyMult + satietyAdd);
            float finalHydrationDecay = hydrationOverride ?? (hydrationDecayRate * hydrationMult + hydrationAdd);
            float finalEnergyDecay = energyOverride ?? (energyDecayRate * energyMult + energyAdd);
    
            UpdateVital(-finalSatietyDecay, Vitals.Satiety);
            UpdateVital(-finalHydrationDecay, Vitals.Hydration);
            UpdateVital(-finalEnergyDecay, Vitals.Energy);
        }
        
        /// <summary>
        /// Multi-use method to update any vital of the character. Specify vital with the "Vitals" enum.
        /// </summary>
        /// <param name="diff">The amount the vital should change by.</param>
        /// <param name="vitals">The vital signature being updated.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void UpdateVital(float diff, Vitals vitals)
        {
            switch (vitals)
            {
                case Vitals.Health:
                    Health += diff;
                    Health = Mathf.Clamp(Health, 0f, MaxHealth);
                    if (Health <= 0f) Die();
                    break;
                case Vitals.Mana:
                    Mana += diff;
                    Mana = Mathf.Clamp(Mana, 0f, MaxMana);
                    break;
                case Vitals.Energy:
                    Energy += diff;
                    Energy = Mathf.Clamp(Energy, 0f, MaxEnergy);
                    break;
                case Vitals.Satiety:
                    Satiety += diff;
                    Satiety = Mathf.Clamp(Satiety, 0f, MaxSatiety);
                    break;
                case Vitals.Hydration:
                    Hydration += diff;
                    Hydration = Mathf.Clamp(Hydration, 0f, MaxHydration);
                    break;
                case Vitals.Temperature:
                    BodyTemp += diff;
                    break;
                case Vitals.HeartRate:
                    HeartRate += diff;
                    HeartRate = Mathf.Clamp(HeartRate, 20f, 200f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(vitals), vitals, null);
            }

            CheckVitalThresholds();
            OnStatusUpdate?.Invoke();
        }

        private void Die()
        {
            SceneManager.LoadScene(mainMenuScene.name, LoadSceneMode.Single);
        }
        
        private void CheckVitalThresholds()
        {
            foreach (var thresholdEffect in vitalsEffectsConfig.VitalsEffects)
            {
                float currentValue = GetVital(thresholdEffect.vital);
                bool shouldBeActive = thresholdEffect.CheckThreshold(currentValue);
                switch (shouldBeActive)
                {
                    case true when !thresholdEffect.isCurrentlyActive:
                        ServiceLocator.GetService<StatusEffectService>().ApplyEffectToTarget(
                            gameObject, thresholdEffect.effectId);
                        thresholdEffect.isCurrentlyActive = true;
                        Debug.Log("Vital threshold applied!" + thresholdEffect.effectId);
                        break;
                    case false when thresholdEffect.isCurrentlyActive:
                        GetComponent<StatusEffectManager>().RemoveStatusEffect(thresholdEffect.effectId);
                        thresholdEffect.isCurrentlyActive = false;
                        break;
                }
            }
        }

        private float GetVitalPercentage(Vitals vital)
        {
            return vital switch
            {
                Vitals.Satiety => Satiety / MaxSatiety,
                Vitals.Hydration => Hydration / MaxHydration,
                Vitals.Energy => Energy / MaxEnergy,
                Vitals.Health => Health / MaxHealth,
                Vitals.HeartRate => HeartRate / MaxHeartRate,
                _ => 0f
            };
        }

        private float GetVital(Vitals vital)
        {
            return vital switch
            {
                Vitals.Satiety => Satiety,
                Vitals.Hydration => Hydration,
                Vitals.Energy => Energy,
                Vitals.Health => Health,
                Vitals.HeartRate => HeartRate,
                _ => 0f
            };
        }


        public string SaveID => $"{gameObject.name}_{name}";
        public object CaptureState() { return new CharacterStatusSaveData(this); }

        public void RestoreState(object saveData)
        {
            CharacterStatusSaveData data = saveData as CharacterStatusSaveData;
            Health = data.health;
            Mana = data.mana;
            Energy = data.energy;
            Satiety = data.satiety;
            Hydration = data.hydration;
            loadSaveData = true;
        }
    }
    
    [Serializable] 
    public class CharacterStatusSaveData
    {
        public float health;
        public float mana;
        public float energy;
        public float satiety;
        public float hydration;

        public CharacterStatusSaveData(CharacterStatus characterStatus)
        {
            health = characterStatus.Health;
            mana = characterStatus.Mana;
            energy = characterStatus.Energy;
            satiety = characterStatus.Satiety;
            hydration = characterStatus.Hydration;
        }
    }
}
