using UnityEngine;

namespace Tests
{
    public class FramerateLimiter : MonoBehaviour
    {
        [Header("Framerate Settings")]
        [SerializeField] private bool enableLimit = false;
        [SerializeField] private int targetFramerate = 60;
        [SerializeField] private bool showFPSOverlay = true;
        
        [Header("Quick Presets")]
        [SerializeField] private bool vsyncEnabled = true;
        
        private float deltaTime;
        private float fps;
        private float minFps = float.MaxValue;
        private float maxFps;
        private float avgFps;
        private int frameCount;
        private float elapsedTime;
        
        private void Start()
        {
            ApplySettings();
        }
        
        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            fps = 1.0f / deltaTime;
            
            frameCount++;
            elapsedTime += Time.unscaledDeltaTime;
            
            if (elapsedTime >= 1f)
            {
                avgFps = frameCount / elapsedTime;
                minFps = Mathf.Min(minFps, fps);
                maxFps = Mathf.Max(maxFps, fps);
                frameCount = 0;
                elapsedTime = 0;
            }
            
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SetFramerateLimit(30);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                SetFramerateLimit(60);
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                SetFramerateLimit(120);
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                SetFramerateLimit(144);
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                DisableLimit();
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                vsyncEnabled = !vsyncEnabled;
                ApplySettings();
                Debug.Log($"VSync: {(vsyncEnabled ? "ON" : "OFF")}");
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                showFPSOverlay = !showFPSOverlay;
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                ResetStats();
            }
        }
        
        private void SetFramerateLimit(int limit)
        {
            targetFramerate = limit;
            enableLimit = true;
            ApplySettings();
            Debug.Log($"Framerate limited to {limit} FPS");
        }
        
        private void DisableLimit()
        {
            enableLimit = false;
            ApplySettings();
            Debug.Log("Framerate limit disabled (unlimited)");
        }
        
        private void ApplySettings()
        {
            QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
            Application.targetFrameRate = enableLimit ? targetFramerate : -1;
            
            Time.fixedDeltaTime = 1f / 50f;
            Time.maximumDeltaTime = 0.333f;
        }
        
        private void ResetStats()
        {
            minFps = float.MaxValue;
            maxFps = 0;
            avgFps = 0;
            frameCount = 0;
            elapsedTime = 0;
        }
        
        private void OnGUI()
        {
            if (!showFPSOverlay) return;
            
            int yOffset = 10;
            int xOffset = Screen.width - 250;
            int lineHeight = 20;
            
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = GetFPSColor(fps);
            
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), $"FPS: {fps:F1}", style);
            yOffset += lineHeight;
            
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), $"Avg: {avgFps:F1} | Min: {minFps:F1} | Max: {maxFps:F1}", style);
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), $"Frame Time: {deltaTime * 1000:F2}ms", style);
            yOffset += lineHeight;
            
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), $"Fixed Update: {Time.fixedDeltaTime * 1000:F1}ms ({1f/Time.fixedDeltaTime:F0} Hz)", style);
            yOffset += lineHeight;
            
            string limitStatus = enableLimit ? $"{targetFramerate} FPS" : "Unlimited";
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), $"Limit: {limitStatus} | VSync: {(vsyncEnabled ? "ON" : "OFF")}", style);
            yOffset += lineHeight + 10;
            
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), "--- Controls ---", style);
            yOffset += lineHeight;
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), "F1: 30 FPS | F2: 60 FPS", style);
            yOffset += lineHeight;
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), "F3: 120 FPS | F4: 144 FPS", style);
            yOffset += lineHeight;
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), "F5: Unlimited | F6: Toggle VSync", style);
            yOffset += lineHeight;
            GUI.Label(new Rect(xOffset, yOffset, 240, lineHeight), "F7: Toggle Overlay | F8: Reset Stats", style);
        }
        
        private Color GetFPSColor(float currentFps)
        {
            if (currentFps >= 60) return Color.green;
            if (currentFps >= 30) return Color.yellow;
            return Color.red;
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                ApplySettings();
            }
        }
    }
}