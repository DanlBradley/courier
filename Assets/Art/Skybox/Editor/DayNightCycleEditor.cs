using UnityEngine;
using UnityEditor;

namespace Evets
{
    [CustomEditor(typeof(DayNightCycle))]
    public class DayNightCycleEditor : Editor
    {
        private SerializedProperty currentTimeOfDay;
        private SerializedProperty dayDuration;
        private SerializedProperty pauseTime;
        private SerializedProperty use24HourFormat;
        private SerializedProperty currentTimeDisplay;
        private SerializedProperty sunTransform;
        private SerializedProperty moonTransform;
        private SerializedProperty moon1Transform;
        private SerializedProperty moon2Transform;
        private SerializedProperty sunriseTime;
        private SerializedProperty sunsetTime;
        private SerializedProperty moon1Offset;
        private SerializedProperty moon2Offset;
        
        private GUIStyle timeDisplayStyle;
        
        private void OnEnable()
        {
            currentTimeOfDay = serializedObject.FindProperty("currentTimeOfDay");
            dayDuration = serializedObject.FindProperty("dayDuration");
            pauseTime = serializedObject.FindProperty("pauseTime");
            use24HourFormat = serializedObject.FindProperty("use24HourFormat");
            currentTimeDisplay = serializedObject.FindProperty("currentTimeDisplay");
            sunTransform = serializedObject.FindProperty("sunTransform");
            moonTransform = serializedObject.FindProperty("moonTransform");
            moon1Transform = serializedObject.FindProperty("moon1Transform");
            moon2Transform = serializedObject.FindProperty("moon2Transform");
            sunriseTime = serializedObject.FindProperty("sunriseTime");
            sunsetTime = serializedObject.FindProperty("sunsetTime");
            moon1Offset = serializedObject.FindProperty("moon1Offset");
            moon2Offset = serializedObject.FindProperty("moon2Offset");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DayNightCycle dayNightCycle = (DayNightCycle)target;
            
            // Initialize style
            if (timeDisplayStyle == null)
            {
                timeDisplayStyle = new GUIStyle(GUI.skin.label);
                timeDisplayStyle.fontSize = 24;
                timeDisplayStyle.fontStyle = FontStyle.Bold;
                timeDisplayStyle.alignment = TextAnchor.MiddleCenter;
            }
            
            // Time Display
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(currentTimeDisplay.stringValue, timeDisplayStyle, GUILayout.Height(40));
            EditorGUILayout.Space();
            
            // Time Slider
            EditorGUI.BeginChangeCheck();
            float newTime = EditorGUILayout.Slider("Time of Day", currentTimeOfDay.floatValue, 0f, 24f);
            if (EditorGUI.EndChangeCheck())
            {
                currentTimeOfDay.floatValue = newTime;
                serializedObject.ApplyModifiedProperties();
                dayNightCycle.CurrentTimeOfDay = newTime;
            }
            
            // Quick Time Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Sunrise"))
            {
                dayNightCycle.SetToSunrise();
                serializedObject.Update();
            }
            if (GUILayout.Button("Noon"))
            {
                dayNightCycle.SetToNoon();
                serializedObject.Update();
            }
            if (GUILayout.Button("Sunset"))
            {
                dayNightCycle.SetToSunset();
                serializedObject.Update();
            }
            if (GUILayout.Button("Midnight"))
            {
                dayNightCycle.SetToMidnight();
                serializedObject.Update();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Time Settings
            EditorGUILayout.LabelField("Time Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(dayDuration);
            EditorGUILayout.PropertyField(pauseTime);
            EditorGUILayout.PropertyField(use24HourFormat);
            
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginHorizontal();
                if (pauseTime.boolValue)
                {
                    if (GUILayout.Button("Resume Time"))
                    {
                        dayNightCycle.ResumeTime();
                        serializedObject.Update();
                    }
                }
                else
                {
                    if (GUILayout.Button("Pause Time"))
                    {
                        dayNightCycle.PauseTime();
                        serializedObject.Update();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            
            // Celestial Body References
            EditorGUILayout.LabelField("Celestial Body References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sunTransform);
            EditorGUILayout.PropertyField(moonTransform);
            EditorGUILayout.PropertyField(moon1Transform);
            EditorGUILayout.PropertyField(moon2Transform);
            
            EditorGUILayout.Space();
            
            // Celestial Body Settings
            EditorGUILayout.LabelField("Celestial Body Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sunriseTime);
            EditorGUILayout.PropertyField(sunsetTime);
            EditorGUILayout.PropertyField(moon1Offset);
            EditorGUILayout.PropertyField(moon2Offset);
            
            EditorGUILayout.Space();
            
            // Info Display
            EditorGUILayout.LabelField("Current Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Is Day: " + (dayNightCycle.IsDay ? "Yes" : "No"));
            EditorGUILayout.LabelField("Sun Angle: " + dayNightCycle.SunAngle.ToString("F1") + "°");
            EditorGUILayout.LabelField("Normalized Time: " + dayNightCycle.NormalizedTime.ToString("F3"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}