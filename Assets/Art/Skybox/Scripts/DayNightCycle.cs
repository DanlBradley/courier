using System;
using GameServices;
using Interfaces;
using UnityEngine;

namespace Evets
{
    // [ExecuteAlways]
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float currentTimeOfDay = 12f; // 0-24 hours
        [SerializeField] private float dayDuration = 60f; // Real seconds for a full day cycle
        [SerializeField] private bool pauseTime = false;
        [SerializeField] [Range(0.1f, 10f)] private float timeScale = 1f; // Time rate modifier
        
        [Header("Time Display")]
        [SerializeField] private bool use24HourFormat = true;
        [SerializeField] private string currentTimeDisplay = "12:00";
        [SerializeField] private float interpolationSpeed = 2f;
        [SerializeField] public float CurrentTimeOfDay;
        
        [Header("Celestial Body References")]
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Transform moonTransform;
        [SerializeField] private Transform moon1Transform;
        [SerializeField] private Transform moon2Transform;
        
        [Header("Celestial Body Settings")]
        [SerializeField] private float sunriseTime = 6f; // 6 AM
        [SerializeField] private float sunsetTime = 18f; // 6 PM
        [SerializeField] private Vector3 sunRotationAxis = Vector3.right;
        [SerializeField] private Vector3 moonRotationAxis = Vector3.right;
        [SerializeField] private float moon1Offset = 2f; // Hours offset from main moon
        [SerializeField] private float moon2Offset = -3f; // Hours offset from main moon
        
        public event Action OnSunrise;
        public event Action OnSunset;
        public event Action OnNoon;
        public event Action OnMidnight;
        
        private float previousHour = -1f;
        private ClockService _clockService;

        private float visualTimeOfDay;
        
        public float NormalizedTime => currentTimeOfDay / 24f;
        
        public bool IsDay => currentTimeOfDay >= sunriseTime && currentTimeOfDay < sunsetTime;
        
        public bool IsNight => !IsDay;
        
        public float SunAngle => (currentTimeOfDay - 6f) * 15f;
        
        private void OnValidate()
        {
            UpdateCelestialBodies();
            UpdateTimeDisplay();
        }
        
        private void Start()
        {
            _clockService = ServiceLocator.GetService<ClockService>(); 
            UpdateCelestialBodies();
            UpdateTimeDisplay();
        }

        private void OnEnable() { TickService.Instance.OnTick += UpdateGameTime; }
        private void OnDisable() { TickService.Instance.OnTick -= UpdateGameTime; }

        private void Update()
        {
            if (!pauseTime && Application.isPlaying)
            {
                // Update time with time scale modifier
                visualTimeOfDay = Mathf.Lerp(visualTimeOfDay, CurrentTimeOfDay, 
                    Time.deltaTime * interpolationSpeed);
                
                // Check for time events
                CheckTimeEvents();
            }
            
            UpdateCelestialBodies();
        }
        
        private void UpdateGameTime()
        {
            WorldTime time = _clockService.GetCurrentTime();
            CurrentTimeOfDay = time.Hour + time.Minute / 60.0f;
            visualTimeOfDay = CurrentTimeOfDay;
        }
        
        private void UpdateCelestialBodies()
        {
            if (sunTransform != null)
            {
                float sunAngle = (currentTimeOfDay - 6f) * 15f; // Sun at horizon at 6 AM
                sunTransform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);
            }
            
            if (moonTransform != null)
            {
                float moonAngle = (currentTimeOfDay - 18f) * 15f; // Moon at horizon at 6 PM
                moonTransform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);
            }
            
            if (moon1Transform != null)
            {
                float moon1Time = Mathf.Repeat(currentTimeOfDay + moon1Offset, 24f);
                float moon1Angle = (moon1Time - 18f) * 15f;
                moon1Transform.rotation = Quaternion.Euler(moon1Angle, 170f, 0f);
            }
            
            if (moon2Transform != null)
            {
                float moon2Time = Mathf.Repeat(currentTimeOfDay + moon2Offset, 24f);
                float moon2Angle = (moon2Time - 18f) * 15f;
                moon2Transform.rotation = Quaternion.Euler(moon2Angle, 170f, 0f);
            }
        }
        
        private void UpdateTimeDisplay()
        {
            int hours = Mathf.FloorToInt(currentTimeOfDay);
            int minutes = Mathf.FloorToInt((currentTimeOfDay - hours) * 60f);
            
            if (use24HourFormat)
            {
                currentTimeDisplay = $"{hours:D2}:{minutes:D2}";
            }
            else
            {
                int displayHours = hours % 12;
                if (displayHours == 0) displayHours = 12;
                string ampm = hours < 12 ? "AM" : "PM";
                currentTimeDisplay = $"{displayHours}:{minutes:D2} {ampm}";
            }
        }
        
        private void CheckTimeEvents()
        {
            int currentHour = Mathf.FloorToInt(currentTimeOfDay);
            
            if (currentHour != previousHour)
            {
                // Sunrise event
                if (currentHour == Mathf.FloorToInt(sunriseTime))
                {
                    OnSunrise?.Invoke();
                }
                
                // Sunset event
                if (currentHour == Mathf.FloorToInt(sunsetTime))
                {
                    OnSunset?.Invoke();
                }
                
                // Noon event
                if (currentHour == 12)
                {
                    OnNoon?.Invoke();
                }
                
                // Midnight event
                if (currentHour == 0)
                {
                    OnMidnight?.Invoke();
                }
                
                previousHour = currentHour;
            }
        }
        
        // Public methods for time control
        public void SetTime(float hour)
        {
            CurrentTimeOfDay = hour;
        }
        
        public void SetTime(int hour, int minute)
        {
            CurrentTimeOfDay = hour + (minute / 60f);
        }
        
        public void PauseTime()
        {
            pauseTime = true;
        }
        
        public void ResumeTime()
        {
            pauseTime = false;
        }
        
        public void TogglePause()
        {
            pauseTime = !pauseTime;
        }
        
        public void SetDayDuration(float seconds)
        {
            dayDuration = Mathf.Max(1f, seconds);
        }
        
        public void SetTimeScale(float scale)
        {
            timeScale = Mathf.Clamp(scale, 0.1f, 10f);
        }
        
        public float GetTimeScale()
        {
            return timeScale;
        }
        
        // Helper methods for specific times
        public void SetToSunrise()
        {
            CurrentTimeOfDay = sunriseTime;
        }
        
        public void SetToNoon()
        {
            CurrentTimeOfDay = 12f;
        }
        
        public void SetToSunset()
        {
            CurrentTimeOfDay = sunsetTime;
        }
        
        public void SetToMidnight()
        {
            CurrentTimeOfDay = 0f;
        }
    }
}