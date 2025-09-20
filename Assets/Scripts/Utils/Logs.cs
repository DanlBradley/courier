using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Logs
    {
        private static HashSet<string> _enabledTypes = new();
        private static Severity _minSeverity = Severity.Low;

        public static void SetTypeFilter(params string[] types) => _enabledTypes = new HashSet<string>(types);
        public static void SetMinSeverity(Severity severity) => _minSeverity = severity;

        public static void Log(string message, string type = " ", Severity severity = Severity.Low)
        {
            if (ShouldLog(type, severity))
                Debug.Log(FormatMessage(message, type, severity));
        }

        public static void LogWarning(string message, string type = "", Severity severity = Severity.Medium)
        {
            if (ShouldLog(type, severity))
                Debug.LogWarning(FormatMessage(message, type, severity));
        }

        public static void LogError(string message, string type = "", Severity severity = Severity.High)
        {
            if (ShouldLog(type, severity))
                Debug.LogError(FormatMessage(message, type, severity));
        }

        private static bool ShouldLog(string type, Severity severity)
        {
            return true;
        }

        private static string FormatMessage(string message, string type, Severity severity)
        {
            string severityText = severity.ToString().ToUpper();
            string typePrefix = string.IsNullOrEmpty(type) ? "" : type;
            
            return $"[{typePrefix}][{severityText}]: {message}";
        }
    }

    public enum Severity { Low, Medium, High }
}