using System;
using System.Collections;
using Interfaces;
using UnityEngine;
using Utils;
#if GAIA_2023_PRO
using Gaia;
#endif

namespace GameServices
{
    public class ClockService: Service, ISaveable
    {
        [Header("Clock Settings")]
        [SerializeField] private uint timeScale = 5;
        [SerializeField] private float lerpSpeed = 5f;
        
        [Header("Manual Time Control")]
        [SerializeField] private bool enableManualTimeControl;
        [SerializeField, Range(0, 1439)] private int manualTimeOfDayMinutes = 480;
        
        [Header("Gaia Integration")]
        [SerializeField] private bool enableGaiaIntegration = true;
        [SerializeField] private bool debugTimeUpdates;
        
        private WorldTime targetWorldTime = new();
        private float currentTimeFloat;
        private int lastManualTime = -1;
        private IEnumerator timeSmoothingCoroutine;

        public override void Initialize()
        {
            InitializeGaiaIntegration();
            Logs.Log("Clock service initialized.", "GameServices");
        }

        private void Update()
        {
            if (enableManualTimeControl && lastManualTime != manualTimeOfDayMinutes)
            {
                SetTimeOfDay(manualTimeOfDayMinutes);
                lastManualTime = manualTimeOfDayMinutes;
            }

            UpdateSmoothTime();
            UpdateGaiaTime();
        }

        public WorldTime GetCurrentTime() { return targetWorldTime; }
        
        private void OnEnable() { TickService.Instance.OnTick += ProgressTime; }
        private void OnDisable() { TickService.Instance.OnTick -= ProgressTime; }

        private void ProgressTime()
        {
            if (enableManualTimeControl) return;
            targetWorldTime.AddMinutes(timeScale);
        }
        
        private void UpdateSmoothTime()
        {
            float targetMinutes = targetWorldTime.TotalMinutes;
    
            // Use a proper lerp factor (0-1 range) instead of minutes per second
            float lerpFactor = 1f - Mathf.Exp(-lerpSpeed * Time.deltaTime);
            currentTimeFloat = Mathf.Lerp(currentTimeFloat, targetMinutes, lerpFactor);
    
            // Snap to target if very close
            if (Mathf.Abs(currentTimeFloat - targetMinutes) < 0.01f)
            {
                currentTimeFloat = targetMinutes;
            }
        }
        
        public void SetTimeOfDay(int minutesInDay)
        {
            minutesInDay = Mathf.Clamp(minutesInDay, 0, 1439);
            
            targetWorldTime.SetTimeOfDay((uint)minutesInDay);
            
            if (enableManualTimeControl) { currentTimeFloat = targetWorldTime.TotalMinutes; }
        }
        
        private void InitializeGaiaIntegration()
        {
            if (!enableGaiaIntegration) return;

            #if GAIA_2023_PRO
            GaiaAPI.SetTimeOfDayEnabled(false);
            
            if (debugTimeUpdates)
            { Debug.Log("Gaia time of day automatic progression disabled. ClockService now controls time."); }
            
            UpdateGaiaTime();
            #else
            Debug.LogWarning(
                "Gaia integration enabled but GAIA_2023_PRO not defined. Make sure Gaia Pro is properly installed.");
            #endif
        }

        private void UpdateGaiaTime()
        {
            if (!enableGaiaIntegration) return;
            try
            {
                float totalMinutesInDay = currentTimeFloat % (24 * 60);
                int hour = Mathf.FloorToInt(totalMinutesInDay / 60f);
                float minute = totalMinutesInDay % 60f;
                
                #if GAIA_2023_PRO
                GaiaAPI.SetTimeOfDayHour(hour);
                GaiaAPI.SetTimeOfDayMinute(minute);
                #endif
                
                if (debugTimeUpdates && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"Updated Gaia time to {hour:D2}:{minute:F2} " +
                              $"(Float time: {currentTimeFloat:F2}, Total day minutes: {totalMinutesInDay:F2})");
                }
            }
            catch (System.Exception e)
            { Debug.LogError($"Failed to update Gaia time: {e.Message}"); }
        }

        public void AddTime(int minutesToAdd)
        {
            Debug.Log("Current time: " + GetCurrentTime());
            targetWorldTime.AddMinutes((uint)minutesToAdd);
            Debug.Log($"Added {minutesToAdd} minutes to current time.");
            Debug.Log("New time: " + GetCurrentTime());
            if (!enableManualTimeControl) return;
            manualTimeOfDayMinutes = targetWorldTime.MinutesInDay;
            lastManualTime = manualTimeOfDayMinutes;
        }

        public void SetTime(int timeInMinutes)
        {
            targetWorldTime.SetAbsoluteTime((uint)timeInMinutes);
        }

        public string GetFormattedTime()
        {
            var time = GetCurrentTime();
            return $"Year {time.Year}, Month {time.Month}, Day {time.Day} - {time.Hour:D2}:{time.Minute:D2}";
        }

        [ContextMenu("Set to Dawn (6:00)")]
        private void SetToDawn() => SetTimeOfDay(360);
        
        [ContextMenu("Set to Noon (12:00)")]
        private void SetToNoon() => SetTimeOfDay(720);
        
        [ContextMenu("Set to Dusk (18:00)")]
        private void SetToDusk() => SetTimeOfDay(1080);
        
        [ContextMenu("Set to Midnight (0:00)")]
        private void SetToMidnight() => SetTimeOfDay(0);

        public string SaveID => "clock_service";
        public object CaptureState() { return new ClockServiceSaveData(targetWorldTime); }

        public void RestoreState(object saveData)
        {
            if (saveData is ClockServiceSaveData savedClock) 
            { targetWorldTime = new WorldTime(savedClock.totalTimeInMinutes); }
            currentTimeFloat = targetWorldTime.TotalMinutes;
        }
    }

    [Serializable]
    public class ClockServiceSaveData
    {
        public uint totalTimeInMinutes;
        public ClockServiceSaveData(WorldTime worldTime) { totalTimeInMinutes = worldTime.TotalMinutes; }
    }
    
    public class WorldTime
    {
        internal WorldTime(uint time = 0) { totalMinutes = time; }
        private uint totalMinutes;
    
        private const int MinutesPerHour = 60;
        private const int HoursPerDay = 24;
        private const int DaysPerMonth = 28;
        private const int MonthsPerYear = 13;
    
        private const int MinutesPerDay = MinutesPerHour * HoursPerDay;
        private const int MinutesPerMonth = MinutesPerDay * DaysPerMonth;
        private const int MinutesPerYear = MinutesPerMonth * MonthsPerYear;
    
        public int Minute => (int)((totalMinutes % MinutesPerHour));
        public int Hour => (int)((totalMinutes / MinutesPerHour) % HoursPerDay);
        public int Day => (int)((totalMinutes / MinutesPerDay) % DaysPerMonth) + 1;
        public int Month => (int)((totalMinutes / MinutesPerMonth) % MonthsPerYear) + 1;
        public int Year => (int)(totalMinutes / MinutesPerYear) + 1;
        public int MinutesInDay => (int)(totalMinutes % MinutesPerDay);
        public uint TotalMinutes => totalMinutes;

        public void AddMinutes(uint minutes) => totalMinutes += minutes;

        public void SetTimeOfDay(uint minutesInDay)
        {
            uint daysElapsed = totalMinutes / MinutesPerDay;
            totalMinutes = (daysElapsed * MinutesPerDay) + (minutesInDay % MinutesPerDay);
        }
        
        public void SetAbsoluteTime(uint minutes) { totalMinutes = minutes; }
        
        public override string ToString()
        { return $"Year {Year}, Month {Month}, Day {Day} - {Hour:D2}:{Minute:D2}"; }
    }
}