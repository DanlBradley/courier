using GameServices;
using Systems.EventRadio;
using UnityEngine;

namespace Systems
{
    public class NoiseSource : MonoBehaviour
    {
        private GameEvent onNoiseCreated;
        
        public void CreateNoise(float noiseVolume)
        {
            onNoiseCreated = GameManager.Instance.OnNoiseSourceCreated;
            onNoiseCreated.Raise(this, noiseVolume);
        }
    }
}