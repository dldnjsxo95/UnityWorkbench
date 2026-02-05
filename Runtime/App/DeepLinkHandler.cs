using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.App
{
    /// <summary>
    /// Handles deep links and URL schemes for the app.
    /// Supports custom URL schemes (myapp://action/param) and universal links.
    /// </summary>
    public class DeepLinkHandler : MonoBehaviour
    {
        private static DeepLinkHandler _instance;
        public static DeepLinkHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<DeepLinkHandler>();
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private string _urlScheme = "myapp";
        [SerializeField] private bool _processOnStart = true;

        private readonly Dictionary<string, Action<DeepLinkData>> _handlers = new();
        private string _pendingDeepLink;

        /// <summary>The URL scheme this app responds to.</summary>
        public string UrlScheme => _urlScheme;

        /// <summary>Last received deep link URL.</summary>
        public string LastDeepLink { get; private set; }

        /// <summary>Event fired when a deep link is received.</summary>
        public event Action<DeepLinkData> OnDeepLinkReceived;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to deep link events
            Application.deepLinkActivated += OnDeepLinkActivated;

            // Check for deep link on start
            if (_processOnStart && !string.IsNullOrEmpty(Application.absoluteURL))
            {
                _pendingDeepLink = Application.absoluteURL;
            }
        }

        private void Start()
        {
            // Process pending deep link after all systems are initialized
            if (!string.IsNullOrEmpty(_pendingDeepLink))
            {
                ProcessDeepLink(_pendingDeepLink);
                _pendingDeepLink = null;
            }
        }

        private void OnDestroy()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        private void OnDeepLinkActivated(string url)
        {
            ProcessDeepLink(url);
        }

        /// <summary>
        /// Register a handler for a specific action path.
        /// </summary>
        /// <param name="action">Action path (e.g., "open", "share", "user")</param>
        /// <param name="handler">Handler callback</param>
        public void RegisterHandler(string action, Action<DeepLinkData> handler)
        {
            _handlers[action.ToLower()] = handler;
        }

        /// <summary>
        /// Unregister a handler for an action path.
        /// </summary>
        public void UnregisterHandler(string action)
        {
            _handlers.Remove(action.ToLower());
        }

        /// <summary>
        /// Manually process a deep link URL.
        /// </summary>
        public void ProcessDeepLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            LastDeepLink = url;
            var data = ParseDeepLink(url);

            if (data != null)
            {
                Debug.Log($"[DeepLink] Processing: {url}");
                Debug.Log($"[DeepLink] Action: {data.Action}, Params: {data.Parameters.Count}");

                // Try to find registered handler
                if (!string.IsNullOrEmpty(data.Action) &&
                    _handlers.TryGetValue(data.Action.ToLower(), out var handler))
                {
                    handler.Invoke(data);
                }

                OnDeepLinkReceived?.Invoke(data);
            }
        }

        /// <summary>
        /// Parse a deep link URL into structured data.
        /// </summary>
        public DeepLinkData ParseDeepLink(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            try
            {
                var uri = new Uri(url);
                var data = new DeepLinkData
                {
                    RawUrl = url,
                    Scheme = uri.Scheme,
                    Host = uri.Host,
                    Path = uri.AbsolutePath.TrimStart('/'),
                    Query = uri.Query
                };

                // Extract action from path (first segment)
                var pathParts = data.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length > 0)
                {
                    data.Action = pathParts[0];
                    data.PathSegments = pathParts;
                }

                // Parse query parameters
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var query = uri.Query.TrimStart('?');
                    var pairs = query.Split('&');

                    foreach (var pair in pairs)
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = Uri.UnescapeDataString(keyValue[0]);
                            var value = Uri.UnescapeDataString(keyValue[1]);
                            data.Parameters[key] = value;
                        }
                    }
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DeepLink] Failed to parse URL: {url}, Error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Build a deep link URL.
        /// </summary>
        public string BuildDeepLink(string action, Dictionary<string, string> parameters = null)
        {
            var url = $"{_urlScheme}://{action}";

            if (parameters != null && parameters.Count > 0)
            {
                var queryParts = new List<string>();
                foreach (var kvp in parameters)
                {
                    queryParts.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
                }
                url += "?" + string.Join("&", queryParts);
            }

            return url;
        }
    }

    /// <summary>
    /// Parsed deep link data.
    /// </summary>
    public class DeepLinkData
    {
        public string RawUrl { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public string Action { get; set; }
        public string[] PathSegments { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>Get a parameter value by key.</summary>
        public string GetParameter(string key, string defaultValue = null)
        {
            return Parameters.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>Get a path segment by index.</summary>
        public string GetPathSegment(int index, string defaultValue = null)
        {
            return index < PathSegments.Length ? PathSegments[index] : defaultValue;
        }
    }
}
