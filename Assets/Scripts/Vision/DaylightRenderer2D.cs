using GameServices;
using Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vision
{
    public class DaylightRenderer2D : MonoBehaviour
    {
        [SerializeField] private Light2D globalLight;
        [SerializeField] private float maxLightIntensity;
        private const float Dawn = 6f;
        private const float Peak = 14f;
        private const float Dusk = 20f;
        private ClockService _clockService;

        private void Start() { _clockService = ServiceLocator.GetService<ClockService>(); }
        private void OnEnable() { TickService.Instance.OnTick += UpdateGlobalLight; }
        private void OnDisable() { TickService.Instance.OnTick -= UpdateGlobalLight; }

        private void UpdateGlobalLight()
        {
            WorldTime time = _clockService.GetCurrentTime();
            int currentMinutes = time.Minute + (time.Hour * 60);
            float lightIntensity = GetLightIntensity(currentMinutes);
            globalLight.intensity = lightIntensity * maxLightIntensity;
        }
    
        private static float GetLightIntensity(int minutes)
        {
            float hour = minutes / 60f;
            if (hour is <= Dawn or >= Dusk) return 0f;
            float t;
            if (hour <= Peak) { t = (hour - Dawn) / (Peak - Dawn); }
            else { t = (Dusk - hour) / (Dusk - Peak); }
            return Mathf.SmoothStep(0f, 1f, t);
        }
    }
}
