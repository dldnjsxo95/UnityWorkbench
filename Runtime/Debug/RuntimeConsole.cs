using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LWT.UnityWorkbench.Core;

namespace LWT.UnityWorkbench.DebugTools
{
    /// <summary>
    /// In-game debug console for executing commands and viewing logs.
    /// </summary>
    public class RuntimeConsole : PersistentMonoSingleton<RuntimeConsole>
    {
        [Header("Settings")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField] private bool _showOnStart = false;
        [SerializeField] private int _maxLogCount = 100;
        [SerializeField] private bool _captureUnityLogs = true;

        [Header("Visual")]
        [SerializeField] private float _consoleHeight = 300f;
        [SerializeField] private int _fontSize = 14;
        [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 0.85f);

        private bool _isVisible;
        private string _inputText = "";
        private Vector2 _scrollPosition;
        private List<LogEntry> _logs = new List<LogEntry>();
        private List<string> _commandHistory = new List<string>();
        private int _historyIndex = -1;

        private static Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>();

        private GUIStyle _consoleStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _logStyle;
        private bool _stylesInitialized;

        public bool IsVisible => _isVisible;

        protected override void Awake()
        {
            base.Awake();
            RegisterDefaultCommands();

            if (_captureUnityLogs)
            {
                Application.logMessageReceived += HandleUnityLog;
            }

            _isVisible = _showOnStart;
        }

        protected override void OnDestroy()
        {
            if (_captureUnityLogs)
            {
                Application.logMessageReceived -= HandleUnityLog;
            }
            base.OnDestroy();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                Toggle();
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            InitStyles();
            DrawConsole();
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            _consoleStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTexture(1, 1, _backgroundColor) }
            };

            _inputStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = _fontSize,
                normal = { textColor = Color.white }
            };

            _logStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = _fontSize,
                wordWrap = true,
                richText = true
            };

            _stylesInitialized = true;
        }

        private void DrawConsole()
        {
            float width = Screen.width;

            // Console background
            GUI.Box(new Rect(0, 0, width, _consoleHeight), "", _consoleStyle);

            GUILayout.BeginArea(new Rect(5, 5, width - 10, _consoleHeight - 10));

            // Log area
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(_consoleHeight - 40));

            foreach (var log in _logs)
            {
                _logStyle.normal.textColor = log.Color;
                GUILayout.Label(log.Message, _logStyle);
            }

            GUILayout.EndScrollView();

            // Input area
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName("ConsoleInput");
            _inputText = GUILayout.TextField(_inputText, _inputStyle, GUILayout.Height(25));

            if (GUILayout.Button("Execute", GUILayout.Width(80), GUILayout.Height(25)))
            {
                ExecuteInput();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();

            // Handle input
            HandleInputEvents();

            // Focus input
            if (_isVisible)
            {
                GUI.FocusControl("ConsoleInput");
            }
        }

        private void HandleInputEvents()
        {
            Event e = Event.current;
            if (e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    ExecuteInput();
                    e.Use();
                    break;

                case KeyCode.UpArrow:
                    NavigateHistory(-1);
                    e.Use();
                    break;

                case KeyCode.DownArrow:
                    NavigateHistory(1);
                    e.Use();
                    break;

                case KeyCode.Tab:
                    AutoComplete();
                    e.Use();
                    break;

                case KeyCode.Escape:
                    Hide();
                    e.Use();
                    break;
            }
        }

        private void ExecuteInput()
        {
            if (string.IsNullOrWhiteSpace(_inputText)) return;

            string input = _inputText.Trim();
            _inputText = "";

            // Add to history
            _commandHistory.Add(input);
            _historyIndex = _commandHistory.Count;

            // Log the command
            Log($"> {input}", Color.yellow);

            // Parse and execute
            var parts = ParseCommand(input);
            if (parts.Length == 0) return;

            string commandName = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();

            if (_commands.TryGetValue(commandName, out var command))
            {
                try
                {
                    command.Action(args);
                }
                catch (Exception ex)
                {
                    LogError($"Error: {ex.Message}");
                }
            }
            else
            {
                LogError($"Unknown command: {commandName}. Type 'help' for available commands.");
            }

            // Auto scroll to bottom
            _scrollPosition.y = float.MaxValue;
        }

        private string[] ParseCommand(string input)
        {
            var parts = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        parts.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                parts.Add(current.ToString());
            }

            return parts.ToArray();
        }

        private void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0) return;

            _historyIndex = Mathf.Clamp(_historyIndex + direction, 0, _commandHistory.Count - 1);
            _inputText = _commandHistory[_historyIndex];
        }

        private void AutoComplete()
        {
            if (string.IsNullOrEmpty(_inputText)) return;

            string partial = _inputText.ToLower();
            var matches = _commands.Keys.Where(k => k.StartsWith(partial)).ToList();

            if (matches.Count == 1)
            {
                _inputText = matches[0] + " ";
            }
            else if (matches.Count > 1)
            {
                Log($"Suggestions: {string.Join(", ", matches)}", Color.cyan);
            }
        }

        private void HandleUnityLog(string message, string stackTrace, LogType type)
        {
            Color color = type switch
            {
                LogType.Error => Color.red,
                LogType.Exception => Color.red,
                LogType.Warning => Color.yellow,
                _ => Color.white
            };

            AddLog(new LogEntry { Message = message, Color = color, Type = type });
        }

        #region Public API

        public void Toggle()
        {
            _isVisible = !_isVisible;
        }

        public void Show()
        {
            _isVisible = true;
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public void Log(string message)
        {
            Log(message, Color.white);
        }

        public void Log(string message, Color color)
        {
            AddLog(new LogEntry { Message = message, Color = color, Type = LogType.Log });
        }

        public void LogWarning(string message)
        {
            Log(message, Color.yellow);
        }

        public void LogError(string message)
        {
            Log(message, Color.red);
        }

        public void LogSuccess(string message)
        {
            Log(message, Color.green);
        }

        private void AddLog(LogEntry entry)
        {
            _logs.Add(entry);
            while (_logs.Count > _maxLogCount)
            {
                _logs.RemoveAt(0);
            }
        }

        public void Clear()
        {
            _logs.Clear();
        }

        /// <summary>
        /// Register a console command.
        /// </summary>
        public static void RegisterCommand(string name, string description, Action<string[]> action)
        {
            name = name.ToLower();
            _commands[name] = new ConsoleCommand
            {
                Name = name,
                Description = description,
                Action = action
            };
        }

        /// <summary>
        /// Unregister a console command.
        /// </summary>
        public static void UnregisterCommand(string name)
        {
            _commands.Remove(name.ToLower());
        }

        #endregion

        #region Default Commands

        private void RegisterDefaultCommands()
        {
            RegisterCommand("help", "Show available commands", args =>
            {
                Log("=== Available Commands ===", Color.cyan);
                foreach (var cmd in _commands.Values.OrderBy(c => c.Name))
                {
                    Log($"  {cmd.Name} - {cmd.Description}");
                }
            });

            RegisterCommand("clear", "Clear the console", args =>
            {
                Clear();
            });

            RegisterCommand("fps", "Toggle FPS display", args =>
            {
                var monitor = FindFirstObjectByType<PerformanceMonitor>();
                if (monitor != null)
                {
                    monitor.Toggle();
                    LogSuccess("FPS display toggled");
                }
                else
                {
                    LogWarning("PerformanceMonitor not found in scene");
                }
            });

            RegisterCommand("timescale", "Set time scale (timescale <value>)", args =>
            {
                if (args.Length == 0)
                {
                    Log($"Current timescale: {Time.timeScale}");
                    return;
                }

                if (float.TryParse(args[0], out float scale))
                {
                    Time.timeScale = Mathf.Clamp(scale, 0f, 10f);
                    LogSuccess($"Timescale set to {Time.timeScale}");
                }
                else
                {
                    LogError("Invalid value. Usage: timescale <number>");
                }
            });

            RegisterCommand("quit", "Quit the application", args =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });

            RegisterCommand("scene", "Load a scene (scene <name>)", args =>
            {
                if (args.Length == 0)
                {
                    Log($"Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                    return;
                }

                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(args[0]);
                    LogSuccess($"Loading scene: {args[0]}");
                }
                catch (Exception ex)
                {
                    LogError($"Failed to load scene: {ex.Message}");
                }
            });

            RegisterCommand("god", "Toggle god mode (invincibility)", args =>
            {
                // This would integrate with CharacterStats if available
                Log("God mode command - implement in your game!", Color.cyan);
            });

            RegisterCommand("give", "Give item (give <itemId> [amount])", args =>
            {
                if (args.Length == 0)
                {
                    LogError("Usage: give <itemId> [amount]");
                    return;
                }

                string itemId = args[0];
                int amount = args.Length > 1 && int.TryParse(args[1], out int a) ? a : 1;

                // Try to find player inventory
                var inventory = FindFirstObjectByType<Gameplay.Inventory>();
                if (inventory != null)
                {
                    int remaining = inventory.AddItem(itemId, amount);
                    if (remaining < amount)
                    {
                        LogSuccess($"Added {amount - remaining}x {itemId}");
                    }
                    else
                    {
                        LogError($"Could not add item: {itemId}");
                    }
                }
                else
                {
                    LogWarning("No Inventory found in scene");
                }
            });

            RegisterCommand("setlevel", "Set player level (setlevel <level>)", args =>
            {
                if (args.Length == 0 || !int.TryParse(args[0], out int level))
                {
                    LogError("Usage: setlevel <level>");
                    return;
                }

                var levelSystem = FindFirstObjectByType<Gameplay.LevelSystem>();
                if (levelSystem != null)
                {
                    levelSystem.SetLevel(level);
                    LogSuccess($"Level set to {level}");
                }
                else
                {
                    LogWarning("No LevelSystem found in scene");
                }
            });

            RegisterCommand("addxp", "Add experience (addxp <amount>)", args =>
            {
                if (args.Length == 0 || !int.TryParse(args[0], out int amount))
                {
                    LogError("Usage: addxp <amount>");
                    return;
                }

                var levelSystem = FindFirstObjectByType<Gameplay.LevelSystem>();
                if (levelSystem != null)
                {
                    levelSystem.AddExperience(amount);
                    LogSuccess($"Added {amount} XP");
                }
                else
                {
                    LogWarning("No LevelSystem found in scene");
                }
            });

            RegisterCommand("heal", "Heal player to full", args =>
            {
                var stats = FindFirstObjectByType<Gameplay.CharacterStats>();
                if (stats != null)
                {
                    stats.FullHeal();
                    LogSuccess("Player healed to full");
                }
                else
                {
                    LogWarning("No CharacterStats found in scene");
                }
            });

            RegisterCommand("damage", "Damage player (damage <amount>)", args =>
            {
                if (args.Length == 0 || !float.TryParse(args[0], out float amount))
                {
                    LogError("Usage: damage <amount>");
                    return;
                }

                var stats = FindFirstObjectByType<Gameplay.CharacterStats>();
                if (stats != null)
                {
                    stats.TakeDamage(amount);
                    LogSuccess($"Dealt {amount} damage");
                }
                else
                {
                    LogWarning("No CharacterStats found in scene");
                }
            });
        }

        #endregion

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D tex = new Texture2D(width, height);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private struct LogEntry
        {
            public string Message;
            public Color Color;
            public LogType Type;
        }

        private class ConsoleCommand
        {
            public string Name;
            public string Description;
            public Action<string[]> Action;
        }
    }
}
