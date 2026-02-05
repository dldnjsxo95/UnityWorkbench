# UnityWorkbench API Reference

## Core Module

### EventBus<T>
타입 기반 이벤트 시스템
```csharp
static void Subscribe(Action<T> handler)
static void Unsubscribe(Action<T> handler)
static void Publish(T eventData)
static void Clear()
```

### ObjectPool<T>
제네릭 오브젝트 풀
```csharp
ObjectPool(Func<T> createFunc, int initialSize = 10)
T Get()
void Release(T item)
void Clear()
int CountInactive { get; }
```

### GameObjectPool
GameObject 전용 풀
```csharp
GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
GameObject Get()
GameObject Get(Vector3 position, Quaternion rotation)
void Release(GameObject obj)
void ReleaseAll()
```

### MonoSingleton<T>
일반 싱글톤 (씬 전환 시 파괴)
```csharp
static T Instance { get; }
static bool HasInstance { get; }
```

### PersistentMonoSingleton<T>
영구 싱글톤 (DontDestroyOnLoad)
```csharp
static T Instance { get; }
static bool HasInstance { get; }
```

### StateMachine<TState>
유한 상태 머신
```csharp
void AddState(TState state, IState handler)
void ChangeState(TState newState)
void Update()
TState CurrentState { get; }
TState PreviousState { get; }
event Action<TState, TState> OnStateChanged
```

---

## UI Module

### UIManager
화면 스택 관리
```csharp
static UIManager Instance
void ShowScreen<T>() where T : UIScreen
void ShowScreen(string screenId)
void HideScreen<T>() where T : UIScreen
void HideCurrentScreen()
void GoBack()
void ClearHistory()
T GetScreen<T>() where T : UIScreen
UIScreen CurrentScreen { get; }
```

### PopupManager
팝업/토스트 관리
```csharp
static PopupManager Instance
void ShowToast(string message, float duration = 2f)
void ShowPopup<T>() where T : PopupBase
void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
void HidePopup<T>() where T : PopupBase
void HideAllPopups()
```

### LoadingManager
로딩 화면 관리
```csharp
static LoadingManager Instance
void ShowLoadingScreen(string message = null)
void HideLoadingScreen()
void UpdateProgress(float progress, string message = null)
void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
bool IsLoading { get; }
float Progress { get; }
event Action<float> OnProgressChanged
```

### SceneLoader (Static)
씬 로딩 유틸리티
```csharp
static void Load(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
static void Load(int buildIndex, LoadSceneMode mode = LoadSceneMode.Single)
static void LoadAdditive(string sceneName)
static void Unload(string sceneName)
static void ReloadCurrent()
static string GetCurrentSceneName()
static int GetCurrentSceneBuildIndex()
static bool IsSceneLoaded(string sceneName)
static int GetSceneCountInBuildSettings()
```

---

## Audio Module

### AudioManager
오디오 총괄 관리
```csharp
static AudioManager Instance
void PlaySFX(string clipName)
void PlaySFX(AudioClip clip)
void PlaySFXAtPoint(string clipName, Vector3 position)
void PlayMusic(string clipName, bool loop = true)
void PlayMusic(AudioClip clip, bool loop = true)
void StopMusic()
void PauseMusic()
void ResumeMusic()
void CrossfadeMusic(AudioClip newClip, float duration = 1f)
void SetMasterVolume(float volume)
void SetMusicVolume(float volume)
void SetSFXVolume(float volume)
float MasterVolume { get; }
float MusicVolume { get; }
float SFXVolume { get; }
```

---

## Data Module

### SaveManager
저장/불러오기
```csharp
static SaveManager Instance
void Save<T>(string slotName, T data)
T Load<T>(string slotName)
bool HasSave(string slotName)
void DeleteSave(string slotName)
void DeleteAllSaves()
string[] GetAllSaveSlots()
```

### SettingsManager
설정 관리 (PlayerPrefs 래퍼)
```csharp
static SettingsManager Instance
void SetInt(string key, int value)
void SetFloat(string key, float value)
void SetString(string key, string value)
void SetBool(string key, bool value)
int GetInt(string key, int defaultValue = 0)
float GetFloat(string key, float defaultValue = 0f)
string GetString(string key, string defaultValue = "")
bool GetBool(string key, bool defaultValue = false)
void DeleteKey(string key)
void DeleteAll()
void Save()
```

---

## Gameplay Module

### Inventory
인벤토리 시스템
```csharp
int AddItem(string itemId, int amount = 1)
int RemoveItem(string itemId, int amount = 1)
bool HasItem(string itemId, int amount = 1)
int GetItemCount(string itemId)
void Clear()
int SlotCount { get; }
int UsedSlots { get; }
event Action<string, int> OnItemAdded
event Action<string, int> OnItemRemoved
```

### CharacterStats
캐릭터 스탯
```csharp
float GetStat(StatType type)
float GetBaseStat(StatType type)
void SetBaseStat(StatType type, float value)
void AddModifier(StatType type, StatModifier modifier)
void RemoveModifier(StatType type, StatModifier modifier)
void RemoveAllModifiers(StatType type)
event Action<StatType, float, float> OnStatChanged
```

### LevelSystem
레벨/경험치
```csharp
void AddExperience(int amount)
void SetLevel(int level)
void SetExperience(int exp)
int CurrentLevel { get; }
int CurrentExperience { get; }
int ExperienceToNextLevel { get; }
float LevelProgress { get; }
event Action<int> OnLevelUp
event Action<int> OnExperienceChanged
```

---

## Network Module

### HttpClient
HTTP 요청
```csharp
static HttpClient Instance
void Get(string url, Action<HttpResponse> callback, Dictionary<string, string> headers = null)
void Post(string url, object body, Action<HttpResponse> callback, ...)
void Put(string url, object body, Action<HttpResponse> callback, ...)
void Patch(string url, object body, Action<HttpResponse> callback, ...)
void Delete(string url, Action<HttpResponse> callback, ...)
void PostForm(string url, WWWForm form, Action<HttpResponse> callback, ...)
void DownloadFile(string url, Action<byte[]> onSuccess, Action<string> onError, Action<float> onProgress = null)
void DownloadTexture(string url, Action<Texture2D> onSuccess, Action<string> onError)
void SetDefaultHeader(string key, string value)
void SetAuthToken(string token)
void ClearAuthToken()
```

### HttpResponse
HTTP 응답
```csharp
long StatusCode { get; }
string Body { get; }
byte[] RawData { get; }
string Error { get; }
bool IsSuccess { get; }
bool IsNetworkError { get; }
bool IsHttpError { get; }
Dictionary<string, string> Headers { get; }
T Parse<T>()
bool TryParse<T>(out T result)
```

### RestApiClient
REST API 클라이언트
```csharp
RestApiClient(string baseUrl)
RestApiClient SetHeader(string key, string value)
RestApiClient SetAuthToken(string token)
RestApiClient SetApiKey(string apiKey, string headerName = "X-API-Key")
void Get<T>(string endpoint, Action<ApiResponse<T>> callback)
void Post<TReq, TRes>(string endpoint, TReq data, Action<ApiResponse<TRes>> callback)
void Put<TReq, TRes>(string endpoint, TReq data, Action<ApiResponse<TRes>> callback)
void Delete<T>(string endpoint, Action<ApiResponse<T>> callback)
void GetById<T>(string resource, string id, Action<ApiResponse<T>> callback)
void Create<TReq, TRes>(string resource, TReq data, Action<ApiResponse<TRes>> callback)
```

---

## Localization Module

### LocalizationManager
다국어 관리
```csharp
static LocalizationManager Instance
void SetLanguage(string languageCode)
string Get(string key)
string Get(string key, params object[] args)
bool HasKey(string key)
bool IsLanguageAvailable(string languageCode)
string CurrentLanguage { get; }
IReadOnlyList<LanguageData> AvailableLanguages { get; }
event Action<string> OnLanguageChanged
```

### L (Static Shortcut)
간편 접근
```csharp
static string Get(string key)
static string Get(string key, params object[] args)
static bool Has(string key)
static string CurrentLanguage { get; }
static void SetLanguage(string languageCode)
```

---

## App Module

### AppManager
앱 라이프사이클
```csharp
static AppManager Instance
void Quit()
void OpenURL(string url)
void SetTargetFrameRate(int frameRate)
void SetScreenSleepTimeout(SleepTimeout timeout)
AppState State { get; }
bool IsPaused { get; }
bool HasFocus { get; }
string Version { get; }
bool IsMobile { get; }
event Action OnAppPaused
event Action OnAppResumed
event Action OnAppFocusGained
event Action OnAppFocusLost
event Action OnAppQuitting
```

### AppInfo (Static)
앱/디바이스 정보
```csharp
static string Version { get; }
static string BundleId { get; }
static RuntimePlatform Platform { get; }
static bool IsMobile { get; }
static bool IsEditor { get; }
static bool HasInternet { get; }
static bool IsOnWiFi { get; }
static string DeviceId { get; }
static string DeviceModel { get; }
static float BatteryLevel { get; }
static string PersistentDataPath { get; }
static string GetFullVersionString()
static string GetDeviceInfoString()
static string GetAppInfoString()
```

### DeepLinkHandler
딥링크 처리
```csharp
static DeepLinkHandler Instance
void RegisterHandler(string action, Action<DeepLinkData> handler)
void UnregisterHandler(string action)
void ProcessDeepLink(string url)
DeepLinkData ParseDeepLink(string url)
string BuildDeepLink(string action, Dictionary<string, string> parameters = null)
string LastDeepLink { get; }
event Action<DeepLinkData> OnDeepLinkReceived
```

---

## Permission Module

### PermissionManager
권한 관리
```csharp
static PermissionManager Instance
bool HasPermission(AppPermission permission)
void RequestPermission(AppPermission permission, Action<bool> callback = null)
void RequestPermissions(AppPermission[] permissions, Action<Dictionary<AppPermission, bool>> callback)
bool ShouldShowRationale(AppPermission permission)
void OpenAppSettings()
void RequestCameraPermission(Action<bool> callback)
void RequestMicrophonePermission(Action<bool> callback)
void RequestLocationPermission(Action<bool> callback, bool finePrecision = true)
void RequestStoragePermission(Action<bool> callback)
event Action<AppPermission> OnPermissionGranted
event Action<AppPermission> OnPermissionDenied
```

---

## Notification Module

### NotificationManager
로컬 알림
```csharp
static NotificationManager Instance
int Schedule(string title, string body, DateTime fireTime, string data = null)
int ScheduleWithDelay(string title, string body, TimeSpan delay, string data = null)
void Cancel(int notificationId)
void CancelAll()
bool IsScheduled(int notificationId)
void SetNotificationsEnabled(bool enabled)
void SetBadgeCount(int count)
void ClearBadge()
event Action<NotificationData> OnNotificationReceived
event Action<NotificationData> OnNotificationOpened
```

### InAppNotification
인앱 알림
```csharp
static InAppNotification Instance
void ShowToast(string message, float duration = 0)
void ShowToast(string message, NotificationType type, float duration = 0)
void ShowBanner(string title, string message, float duration = 0, Action onClick = null)
void ShowAlert(string title, string message, string buttonText = "OK", Action onConfirm = null)
void ShowConfirmAlert(string title, string message, string confirmText, string cancelText, Action onConfirm, Action onCancel)
void DismissBanner()
void ClearAll()
```

---

## Utilities Module

### Timer
타이머
```csharp
Timer(float duration, Action onComplete, bool loop = false)
void Start()
void Stop()
void Pause()
void Resume()
void Reset()
bool IsRunning { get; }
float Progress { get; }
float RemainingTime { get; }
```

### Cooldown
쿨다운
```csharp
Cooldown(float duration)
bool Use()
void Reset()
bool IsReady { get; }
float RemainingTime { get; }
float Progress { get; }
```

### Easing
이징 함수
```csharp
static float Linear(float t)
static float EaseInQuad(float t)
static float EaseOutQuad(float t)
static float EaseInOutQuad(float t)
static float EaseInCubic(float t)
static float EaseOutCubic(float t)
static float EaseInOutCubic(float t)
static float EaseInElastic(float t)
static float EaseOutElastic(float t)
static float EaseInOutElastic(float t)
static float EaseInBounce(float t)
static float EaseOutBounce(float t)
```

### Extensions
확장 메서드 (Transform, Vector, Collection, String, Color, GameObject)
```csharp
// Transform
transform.SetX(float x)
transform.SetY(float y)
transform.SetZ(float z)
transform.Reset()

// Vector
vector.With(x?, y?, z?)
vector.Flat()  // Y를 0으로

// Collection
list.Shuffle()
list.RandomElement()
list.IsNullOrEmpty()

// String
string.IsNullOrEmpty()
string.Truncate(int maxLength)

// Color
color.WithAlpha(float alpha)

// GameObject
gameObject.GetOrAddComponent<T>()
gameObject.SetLayerRecursively(int layer)
```
