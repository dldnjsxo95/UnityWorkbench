using System;
using System.Collections.Generic;
using UnityEngine;

namespace LWT.UnityWorkbench.Network
{
    /// <summary>
    /// REST API client for simplified API interactions.
    /// Wraps HttpClient with base URL and common patterns.
    /// </summary>
    public class RestApiClient
    {
        private readonly string _baseUrl;
        private readonly Dictionary<string, string> _defaultHeaders = new();
        private HttpClient _httpClient;

        /// <summary>Base URL for all requests.</summary>
        public string BaseUrl => _baseUrl;

        /// <summary>
        /// Create a new REST API client.
        /// </summary>
        /// <param name="baseUrl">Base URL (e.g., "https://api.example.com/v1")</param>
        public RestApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = HttpClient.Instance;
        }

        #region Header Management

        /// <summary>Set a default header for all requests.</summary>
        public RestApiClient SetHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
            return this;
        }

        /// <summary>Set authorization header (Bearer token).</summary>
        public RestApiClient SetAuthToken(string token)
        {
            _defaultHeaders["Authorization"] = $"Bearer {token}";
            return this;
        }

        /// <summary>Set API key header.</summary>
        public RestApiClient SetApiKey(string apiKey, string headerName = "X-API-Key")
        {
            _defaultHeaders[headerName] = apiKey;
            return this;
        }

        /// <summary>Clear all custom headers.</summary>
        public RestApiClient ClearHeaders()
        {
            _defaultHeaders.Clear();
            return this;
        }

        #endregion

        #region CRUD Operations

        /// <summary>GET request to fetch a resource.</summary>
        public void Get<T>(string endpoint, Action<ApiResponse<T>> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Get(url, response => callback?.Invoke(new ApiResponse<T>(response)), _defaultHeaders);
        }

        /// <summary>GET request with raw response.</summary>
        public void Get(string endpoint, Action<HttpResponse> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Get(url, callback, _defaultHeaders);
        }

        /// <summary>POST request to create a resource.</summary>
        public void Post<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Post(url, data, response => callback?.Invoke(new ApiResponse<TResponse>(response)), _defaultHeaders);
        }

        /// <summary>POST request with raw response.</summary>
        public void Post<T>(string endpoint, T data, Action<HttpResponse> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Post(url, data, callback, _defaultHeaders);
        }

        /// <summary>PUT request to update a resource.</summary>
        public void Put<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Put(url, data, response => callback?.Invoke(new ApiResponse<TResponse>(response)), _defaultHeaders);
        }

        /// <summary>PATCH request for partial update.</summary>
        public void Patch<TRequest, TResponse>(string endpoint, TRequest data, Action<ApiResponse<TResponse>> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Patch(url, data, response => callback?.Invoke(new ApiResponse<TResponse>(response)), _defaultHeaders);
        }

        /// <summary>DELETE request to remove a resource.</summary>
        public void Delete<T>(string endpoint, Action<ApiResponse<T>> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Delete(url, response => callback?.Invoke(new ApiResponse<T>(response)), _defaultHeaders);
        }

        /// <summary>DELETE request with raw response.</summary>
        public void Delete(string endpoint, Action<HttpResponse> callback)
        {
            var url = BuildUrl(endpoint);
            _httpClient.Delete(url, callback, _defaultHeaders);
        }

        #endregion

        #region Resource Shortcuts

        /// <summary>Get a single resource by ID.</summary>
        public void GetById<T>(string resource, string id, Action<ApiResponse<T>> callback)
        {
            Get<T>($"{resource}/{id}", callback);
        }

        /// <summary>Get a list of resources.</summary>
        public void GetList<T>(string resource, Action<ApiResponse<T>> callback, int? page = null, int? limit = null)
        {
            var endpoint = resource;
            var queryParams = new List<string>();

            if (page.HasValue) queryParams.Add($"page={page.Value}");
            if (limit.HasValue) queryParams.Add($"limit={limit.Value}");

            if (queryParams.Count > 0)
            {
                endpoint += "?" + string.Join("&", queryParams);
            }

            Get<T>(endpoint, callback);
        }

        /// <summary>Create a new resource.</summary>
        public void Create<TRequest, TResponse>(string resource, TRequest data, Action<ApiResponse<TResponse>> callback)
        {
            Post(resource, data, callback);
        }

        /// <summary>Update a resource by ID.</summary>
        public void Update<TRequest, TResponse>(string resource, string id, TRequest data, Action<ApiResponse<TResponse>> callback)
        {
            Put($"{resource}/{id}", data, callback);
        }

        /// <summary>Delete a resource by ID.</summary>
        public void DeleteById<T>(string resource, string id, Action<ApiResponse<T>> callback)
        {
            Delete<T>($"{resource}/{id}", callback);
        }

        #endregion

        private string BuildUrl(string endpoint)
        {
            endpoint = endpoint.TrimStart('/');
            return $"{_baseUrl}/{endpoint}";
        }
    }

    /// <summary>
    /// Typed API response wrapper.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; }
        public long StatusCode { get; }
        public T Data { get; }
        public string Error { get; }
        public string RawBody { get; }

        public ApiResponse(HttpResponse response)
        {
            IsSuccess = response.IsSuccess;
            StatusCode = response.StatusCode;
            RawBody = response.Body;
            Error = response.Error;

            if (IsSuccess && !string.IsNullOrEmpty(response.Body))
            {
                try
                {
                    Data = JsonUtility.FromJson<T>(response.Body);
                }
                catch (Exception e)
                {
                    Error = $"JSON parse error: {e.Message}";
                }
            }
        }

        /// <summary>Execute action if successful.</summary>
        public ApiResponse<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess && Data != null)
            {
                action?.Invoke(Data);
            }
            return this;
        }

        /// <summary>Execute action if failed.</summary>
        public ApiResponse<T> OnError(Action<string, long> action)
        {
            if (!IsSuccess)
            {
                action?.Invoke(Error ?? "Unknown error", StatusCode);
            }
            return this;
        }
    }

    /// <summary>
    /// Common API response structures.
    /// </summary>
    [Serializable]
    public class ApiListResponse<T>
    {
        public T[] items;
        public int total;
        public int page;
        public int limit;
    }

    [Serializable]
    public class ApiErrorResponse
    {
        public string error;
        public string message;
        public int code;
    }
}
