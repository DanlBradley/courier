using GameServices;
using Interfaces;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Vision
{
    public class DaylightRenderer3D : MonoBehaviour
    {
        [SerializeField] private Light globalLight;
        [SerializeField] private float maxLightIntensity;
        private const float Dawn = 6f;
        private const float Peak = 14f;
        private const float Dusk = 20f;
        private ClockService _clockService;
        
        [SerializeField] private float sunRotationSpeed = 2f; // Adjust for smoothness
        private Quaternion targetSunRotation;
        private Quaternion currentSunRotation;
        private void OnEnable() { TickService.Instance.OnTick += UpdateGlobalLight; }
        private void OnDisable() { TickService.Instance.OnTick -= UpdateGlobalLight; }
        
        private void Start() 
        { 
            _clockService = ServiceLocator.GetService<ClockService>(); 
            currentSunRotation = globalLight.transform.rotation;
            targetSunRotation = currentSunRotation;
        }

        private void Update()
        {
            currentSunRotation = Quaternion.Slerp(currentSunRotation, targetSunRotation, sunRotationSpeed * Time.deltaTime);
            globalLight.transform.rotation = currentSunRotation;
        }

        private void UpdateGlobalLight()
        {
            WorldTime time = _clockService.GetCurrentTime();
            int currentMinutes = time.Minute + (time.Hour * 60);
            float lightIntensity = GetLightIntensity(currentMinutes);
            globalLight.intensity = lightIntensity * maxLightIntensity;

            UpdateSunAngle(currentMinutes);
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
        
        private void UpdateSunAngle(int minutes)
        {
            float hour = minutes / 60f;
            float sunProgress = (hour - Dawn) / (Dusk - Dawn);
            sunProgress = Mathf.Clamp01(sunProgress);
            float sunAngle = Mathf.Lerp(-90f, 90f, sunProgress);
            targetSunRotation = Quaternion.Euler(sunAngle, 30f, 0f);
        }
    }
}