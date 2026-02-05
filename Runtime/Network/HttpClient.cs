using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LWT.UnityWorkbench.Network
{
    /// <summary>
    /// HTTP client for making web requests.
    /// Supports GET, POST, PUT, PATCH, DELETE methods.
    /// </summary>
    public class HttpClient : MonoBehaviour
    {
        private static HttpClient _instance;
        public static HttpClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<HttpClient>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[HttpClient]");
                        _instance = go.AddComponent<HttpClient>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private float _defaultTimeout = 30f;
        [SerializeField] private int _maxRetries = 3;
        [SerializeField] private float _retryDelay = 1f;

        private readonly Dictionary<string, string> _defaultHeaders = new();

        /// <summary>Default timeout for requests in seconds.</summary>
        public float DefaultTimeout
        {
            get => _defaultTimeout;
            set => _defaultTimeout = value;
        }

        /// <summary>Maximum retry attempts for failed requests.</summary>
        public int MaxRetries
        {
            get => _maxRetries;
            set => _maxRetries = value;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #region Header Management

        /// <summary>Set a default header for all requests.</summary>
        public void SetDefaultHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
        }

        /// <summary>Remove a default header.</summary>
        public void RemoveDefaultHeader(string key)
        {
            _defaultHeaders.Remove(key);
        }

        /// <summary>Set authorization header (Bearer token).</summary>
        public void SetAuthToken(string token)
        {
            SetDefaultHeader("Authorization", $"Bearer {token}");
        }

        /// <summary>Clear authorization header.</summary>
        public void ClearAuthToken()
        {
            RemoveDefaultHeader("Authorization");
        }

        #endregion

        #region Request Methods

        /// <summary>Send a GET request.</summary>
        public void Get(string url, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendRequest(HttpMethod.GET, url, null, callback, headers));
        }

        /// <summary>Send a POST request with JSON body.</summary>
        public void Post(string url, object body, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendRequest(HttpMethod.POST, url, body, callback, headers));
        }

        /// <summary>Send a PUT request with JSON body.</summary>
        public void Put(string url, object body, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendRequest(HttpMethod.PUT, url, body, callback, headers));
        }

        /// <summary>Send a PATCH request with JSON body.</summary>
        public void Patch(string url, object body, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendRequest(HttpMethod.PATCH, url, body, callback, headers));
        }

        /// <summary>Send a DELETE request.</summary>
        public void Delete(string url, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendRequest(HttpMethod.DELETE, url, null, callback, headers));
        }

        /// <summary>Send a POST request with form data.</summary>
        public void PostForm(string url, WWWForm form, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendFormRequest(url, form, callback, headers));
        }

        /// <summary>Download a file.</summary>
        public void DownloadFile(string url, Action<byte[]> onSuccess, Action<string> onError, Action<float> onProgress = null)
        {
            StartCoroutine(DownloadFileCoroutine(url, onSuccess, onError, onProgress));
        }

        /// <summary>Download a texture.</summary>
        public void DownloadTexture(string url, Action<Texture2D> onSuccess, Action<string> onError)
        {
            StartCoroutine(DownloadTextureCoroutine(url, onSuccess, onError));
        }

        #endregion

        #region Coroutines

        private IEnumerator SendRequest(HttpMethod method, string url, object body,
            Action<HttpResponse> callback, Dictionary<string, string> headers, int retryCount = 0)
        {
            using var request = CreateRequest(method, url, body);
            ApplyHeaders(request, headers);

            request.timeout = (int)_defaultTimeout;

            yield return request.SendWebRequest();

            var response = new HttpResponse(request);

            // Retry on network error
            if (response.IsNetworkError && retryCount < _maxRetries)
            {
                Debug.LogWarning($"[HttpClient] Request failed, retrying ({retryCount + 1}/{_maxRetries}): {url}");
                yield return new WaitForSeconds(_retryDelay);
                yield return SendRequest(method, url, body, callback, headers, retryCount + 1);
                yield break;
            }

            callback?.Invoke(response);
        }

        private IEnumerator SendFormRequest(string url, WWWForm form,
            Action<HttpResponse> callback, Dictionary<string, string> headers)
        {
            using var request = UnityWebRequest.Post(url, form);
            ApplyHeaders(request, headers);

            request.timeout = (int)_defaultTimeout;

            yield return request.SendWebRequest();

            callback?.Invoke(new HttpResponse(request));
        }

        private IEnumerator DownloadFileCoroutine(string url, Action<byte[]> onSuccess,
            Action<string> onError, Action<float> onProgress)
        {
            using var request = UnityWebRequest.Get(url);
            request.timeout = (int)_defaultTimeout * 2;

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                onProgress?.Invoke(request.downloadProgress);
                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.data);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        private IEnumerator DownloadTextureCoroutine(string url, Action<Texture2D> onSuccess, Action<string> onError)
        {
            using var request = UnityWebRequestTexture.GetTexture(url);
            request.timeout = (int)_defaultTimeout;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                onSuccess?.Invoke(texture);
            }
            else
            {
                onError?.Invoke(request.error);
            }
        }

        #endregion

        #region Helper Methods

        private UnityWebRequest CreateRequest(HttpMethod method, string url, object body)
        {
            UnityWebRequest request;
            byte[] bodyRaw = null;

            if (body != null)
            {
                var json = JsonUtility.ToJson(body);
                bodyRaw = Encoding.UTF8.GetBytes(json);
            }

            switch (method)
            {
                case HttpMethod.GET:
                    request = UnityWebRequest.Get(url);
                    break;

                case HttpMethod.POST:
                    request = new UnityWebRequest(url, "POST");
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    break;

                case HttpMethod.PUT:
                    request = new UnityWebRequest(url, "PUT");
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    break;

                case HttpMethod.PATCH:
                    request = new UnityWebRequest(url, "PATCH");
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Content-Type", "application/json");
                    break;

                case HttpMethod.DELETE:
                    request = UnityWebRequest.Delete(url);
                    request.downloadHandler = new DownloadHandlerBuffer();
                    break;

                default:
                    throw new ArgumentException($"Unsupported HTTP method: {method}");
            }

            return request;
        }

        private void ApplyHeaders(UnityWebRequest request, Dictionary<string, string> headers)
        {
            // Apply default headers
            foreach (var header in _defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // Apply request-specific headers (override defaults)
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        #endregion
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    /// <summary>
    /// HTTP response wrapper.
    /// </summary>
    public class HttpResponse
    {
        public long StatusCode { get; }
        public string Body { get; }
        public byte[] RawData { get; }
        public string Error { get; }
        public bool IsSuccess { get; }
        public bool IsNetworkError { get; }
        public bool IsHttpError { get; }
        public Dictionary<string, string> Headers { get; }

        public HttpResponse(UnityWebRequest request)
        {
            StatusCode = request.responseCode;
            Body = request.downloadHandler?.text ?? string.Empty;
            RawData = request.downloadHandler?.data;
            Error = request.error;
            IsNetworkError = request.result == UnityWebRequest.Result.ConnectionError;
            IsHttpError = request.result == UnityWebRequest.Result.ProtocolError;
            IsSuccess = request.result == UnityWebRequest.Result.Success;

            Headers = new Dictionary<string, string>();
            var responseHeaders = request.GetResponseHeaders();
            if (responseHeaders != null)
            {
                foreach (var header in responseHeaders)
                {
                    Headers[header.Key] = header.Value;
                }
            }
        }

        /// <summary>Parse response body as JSON.</summary>
        public T Parse<T>()
        {
            return JsonUtility.FromJson<T>(Body);
        }

        /// <summary>Try to parse response body as JSON.</summary>
        public bool TryParse<T>(out T result)
        {
            try
            {
                result = JsonUtility.FromJson<T>(Body);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
