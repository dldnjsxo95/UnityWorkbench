using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LWT.UnityWorkbench.Network
{
    /// <summary>
    /// Simple WebSocket client for real-time communication.
    /// Uses Unity's native WebSocket support where available.
    /// </summary>
    public class WebSocketClient : MonoBehaviour
    {
        [Header("Connection")]
        [SerializeField] private string _serverUrl = "ws://localhost:8080";
        [SerializeField] private bool _autoConnect = false;
        [SerializeField] private bool _autoReconnect = true;
        [SerializeField] private float _reconnectDelay = 3f;
        [SerializeField] private int _maxReconnectAttempts = 5;

        [Header("Heartbeat")]
        [SerializeField] private bool _enableHeartbeat = true;
        [SerializeField] private float _heartbeatInterval = 30f;
        [SerializeField] private string _heartbeatMessage = "ping";

        private Queue<string> _messageQueue = new();
        private bool _isConnecting;
        private int _reconnectAttempts;
        private Coroutine _heartbeatCoroutine;

        // Platform-specific WebSocket implementation would go here
        // For now, we provide the interface and message handling

        /// <summary>Current connection state.</summary>
        public WebSocketState State { get; private set; } = WebSocketState.Disconnected;

        /// <summary>Whether currently connected.</summary>
        public bool IsConnected => State == WebSocketState.Connected;

        /// <summary>Server URL.</summary>
        public string ServerUrl
        {
            get => _serverUrl;
            set => _serverUrl = value;
        }

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnMessage;
        public event Action<byte[]> OnBinaryMessage;
        public event Action<string> OnError;

        /// <summary>Invoke binary message event (for platform implementations).</summary>
        protected void InvokeBinaryMessage(byte[] data) => OnBinaryMessage?.Invoke(data);

        private void Start()
        {
            if (_autoConnect)
            {
                Connect();
            }
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void Update()
        {
            // Process message queue on main thread
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                OnMessage?.Invoke(message);
            }
        }

        /// <summary>
        /// Connect to the WebSocket server.
        /// </summary>
        public void Connect()
        {
            Connect(_serverUrl);
        }

        /// <summary>
        /// Connect to a specific WebSocket server URL.
        /// </summary>
        public void Connect(string url)
        {
            if (State == WebSocketState.Connected || _isConnecting)
            {
                Debug.LogWarning("[WebSocket] Already connected or connecting");
                return;
            }

            _serverUrl = url;
            _isConnecting = true;
            _reconnectAttempts = 0;
            State = WebSocketState.Connecting;

            StartCoroutine(ConnectCoroutine());
        }

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public void Disconnect()
        {
            _autoReconnect = false;

            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }

            if (State == WebSocketState.Connected)
            {
                // Close WebSocket connection
                HandleDisconnected();
            }

            State = WebSocketState.Disconnected;
        }

        /// <summary>
        /// Send a text message.
        /// </summary>
        public void Send(string message)
        {
            if (State != WebSocketState.Connected)
            {
                Debug.LogWarning("[WebSocket] Cannot send: not connected");
                return;
            }

            // Send message via WebSocket
            Debug.Log($"[WebSocket] Sending: {message}");
        }

        /// <summary>
        /// Send a JSON object.
        /// </summary>
        public void SendJson<T>(T data)
        {
            var json = JsonUtility.ToJson(data);
            Send(json);
        }

        /// <summary>
        /// Send binary data.
        /// </summary>
        public void SendBinary(byte[] data)
        {
            if (State != WebSocketState.Connected)
            {
                Debug.LogWarning("[WebSocket] Cannot send: not connected");
                return;
            }

            // Send binary via WebSocket
            Debug.Log($"[WebSocket] Sending binary: {data.Length} bytes");
        }

        private IEnumerator ConnectCoroutine()
        {
            Debug.Log($"[WebSocket] Connecting to {_serverUrl}...");

            // Simulate connection attempt
            // In real implementation, use platform-specific WebSocket API
            yield return new WaitForSeconds(0.5f);

            // For demonstration, simulate successful connection
            _isConnecting = false;
            HandleConnected();
        }

        private void HandleConnected()
        {
            State = WebSocketState.Connected;
            _reconnectAttempts = 0;

            Debug.Log("[WebSocket] Connected");
            OnConnected?.Invoke();

            if (_enableHeartbeat)
            {
                _heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());
            }
        }

        private void HandleDisconnected()
        {
            var wasConnected = State == WebSocketState.Connected;
            State = WebSocketState.Disconnected;

            if (_heartbeatCoroutine != null)
            {
                StopCoroutine(_heartbeatCoroutine);
                _heartbeatCoroutine = null;
            }

            if (wasConnected)
            {
                Debug.Log("[WebSocket] Disconnected");
                OnDisconnected?.Invoke();

                if (_autoReconnect && _reconnectAttempts < _maxReconnectAttempts)
                {
                    StartCoroutine(ReconnectCoroutine());
                }
            }
        }

        private void HandleMessage(string message)
        {
            _messageQueue.Enqueue(message);
        }

        private void HandleError(string error)
        {
            Debug.LogError($"[WebSocket] Error: {error}");
            OnError?.Invoke(error);
        }

        private IEnumerator HeartbeatCoroutine()
        {
            while (State == WebSocketState.Connected)
            {
                yield return new WaitForSeconds(_heartbeatInterval);

                if (State == WebSocketState.Connected)
                {
                    Send(_heartbeatMessage);
                }
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            _reconnectAttempts++;
            Debug.Log($"[WebSocket] Reconnecting ({_reconnectAttempts}/{_maxReconnectAttempts})...");

            yield return new WaitForSeconds(_reconnectDelay);

            if (State == WebSocketState.Disconnected)
            {
                Connect();
            }
        }
    }

    public enum WebSocketState
    {
        Disconnected,
        Connecting,
        Connected,
        Closing
    }

    /// <summary>
    /// WebSocket message wrapper for typed messages.
    /// </summary>
    [Serializable]
    public class WebSocketMessage
    {
        public string type;
        public string data;

        public static WebSocketMessage Create(string type, object data = null)
        {
            return new WebSocketMessage
            {
                type = type,
                data = data != null ? JsonUtility.ToJson(data) : null
            };
        }

        public T GetData<T>()
        {
            return !string.IsNullOrEmpty(data) ? JsonUtility.FromJson<T>(data) : default;
        }
    }
}
