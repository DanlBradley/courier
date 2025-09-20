using System;
using GameServices;
using Interfaces;
using UnityEngine;

namespace EditorTools
{
    public class ClockDebugCommands: MonoBehaviour
    {
        private ClockService clockService;

        private void Start()
        {
            clockService = ServiceLocator.GetService<ClockService>();
        
            if (clockService == null) { Debug.LogError("ClockService not found!"); return; }
        
            DebugConsole.RegisterCommand("clock.now", Now, 
                "Get the current time.", "clock.now");
            DebugConsole.RegisterCommand("clock.add", AddTime, 
                "Progress time by X minutes.", "clock.add [number of minutes to add]");
        }

        private string Now(string[] args)
        {
            var currentTime = clockService.GetCurrentTime();
            string hourString = currentTime.Hour + ":" + currentTime.Minute;
            string calendarString = "day " + currentTime.Day + ", month " + 
                                    currentTime.Month + ", year " + currentTime.Year;
            return $"The time is: {hourString}, and the date is: {calendarString}";
        }

        private string AddTime(string[] args)
        {
            bool parsed = int.TryParse(args[0], out int minutesToAdd);
            if (!parsed) { Debug.LogError("Failed to parse field to int: " + args[0]);}
            clockService.AddTime(minutesToAdd);
            return $"Added {args[0]} to the clock.";
        }
    }
}