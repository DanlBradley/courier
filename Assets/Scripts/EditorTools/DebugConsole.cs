using System;
using System.Collections.Generic;
using System.Linq;
using GameServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTools
{
    public class DebugConsole : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TMP_InputField commandInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private ScrollRect outputScrollRect;
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        [SerializeField] private int maxOutputLines = 100;
    
        [Header("Settings")]
        [SerializeField] private bool startHidden = true;
    
        private readonly Dictionary<string, CommandInfo> commands = new();
        private readonly List<string> commandHistory = new();
        private int historyIndex = -1;
        private readonly List<string> outputLines = new();
    
        private static DebugConsole _instance;
        public delegate string CommandHandler(string[] args);
        private class CommandInfo
        {
            public readonly CommandHandler handler;
            public readonly string description;
            public readonly string usage;
        
            public CommandInfo(CommandHandler handler, string description, string usage)
            {
                this.handler = handler;
                this.description = description;
                this.usage = usage;
            }
        }
    
        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; } _instance = this;
            RegisterCommand("help", CmdHelp, "Show all available commands", "help [command]");
            RegisterCommand("clear", CmdClear, "Clear the console output", "clear");
        }

        private void Start()
        {
            if (startHidden) consolePanel.SetActive(false);
            commandInput.onSubmit.AddListener(_ => ProcessCommand());
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                if (consolePanel.activeSelf)
                {
                    consolePanel.SetActive(!consolePanel.activeSelf);
                    commandInput.Select();
                    GameStateManager.Instance.ChangeState(GameState.Exploration);
                }
                else if (!consolePanel.activeSelf && GameStateManager.Instance.CurrentState == GameState.Exploration)
                {
                    consolePanel.SetActive(!consolePanel.activeSelf);
                    GameStateManager.Instance.ChangeState(GameState.UI);
                }
                
            }
        
            if (!consolePanel.activeSelf) return;
            
            if (Input.GetKeyDown(KeyCode.UpArrow) && commandHistory.Count > 0)
            {
                historyIndex = Mathf.Clamp(historyIndex + 1, 0, commandHistory.Count - 1);
                commandInput.text = commandHistory[commandHistory.Count - 1 - historyIndex];
                commandInput.caretPosition = commandInput.text.Length;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && historyIndex > -1)
            {
                historyIndex--;
            
                if (historyIndex == -1) commandInput.text = "";
                else commandInput.text = commandHistory[commandHistory.Count - 1 - historyIndex];
                
                commandInput.caretPosition = commandInput.text.Length;
            }
        }
    
        private void ProcessCommand()
        {
            string input = commandInput.text.Trim();
            if (string.IsNullOrEmpty(input)) return;
            
            commandHistory.Add(input);
            historyIndex = -1;
            LogOutput($"> {input}");
            string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            
            string cmdName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();
        
            if (commands.TryGetValue(cmdName, out CommandInfo cmd))
            {
                try
                {
                    string result = cmd.handler(args);
                    if (!string.IsNullOrEmpty(result)) LogOutput(result);
                }
                catch (Exception e) { LogOutput($"Error executing command: {e.Message}"); }
            }
            else { LogOutput($"Unknown command: {cmdName}. Type 'help' for available commands."); }
        
            commandInput.text = "";
            commandInput.Select();
        }
    
        public static void RegisterCommand(string name, CommandHandler handler, string description, string usage)
        {
            if (_instance == null) { Debug.LogError("DebugConsole instance not found!"); return; }
        
            name = name.ToLower();
        
            if (_instance.commands.ContainsKey(name))
            { Debug.LogWarning($"Command '{name}' is already registered. Overwriting."); }
        
            _instance.commands[name] = new CommandInfo(handler, description, usage);
        }
    
        // Internal log method
        private void LogOutput(string text)
        {
            outputLines.Add(text);
            if (outputLines.Count > maxOutputLines) outputLines.RemoveAt(0);
            outputText.text = string.Join("\n", outputLines);
            Canvas.ForceUpdateCanvases();
            outputScrollRect.normalizedPosition = new Vector2(0, 0);
        }
    
        #region Default Commands
    
        // Help command
        private string CmdHelp(string[] args)
        {
            if (args.Length == 0)
            {
                // List all commands
                string result = "Available commands:";
            
                foreach (var pair in commands.OrderBy(p => p.Key))
                {
                    result += $"\n{pair.Key} - {pair.Value.description}";
                }
            
                result += "\n\nType 'help <command>' for detailed usage.";
                return result;
            }

            // Show help for specific command
            string cmdName = args[0].ToLower();
            
            if (commands.TryGetValue(cmdName, out CommandInfo cmd))
            { return $"{cmdName}: {cmd.description}\nUsage: {cmd.usage}"; }
            return $"Command '{cmdName}' not found.";
        }
    
        // Clear command
        private string CmdClear(string[] args)
        {
            outputLines.Clear();
            outputText.text = "";
            return null;
        }
    
        #endregion
    }
}