# UnityWorkbench - AI Context Document

> 이 문서는 AI 어시스턴트가 UnityWorkbench 패키지를 빠르게 이해하기 위한 컨텍스트 문서입니다.

## 패키지 개요

**UnityWorkbench**는 Unity 게임/앱 개발을 위한 모듈형 프레임워크입니다.
- **위치**: `Assets/UnityWorkbench/`
- **루트 네임스페이스**: `LWT.UnityWorkbench`
- **Assembly Definition**: 모듈별 분리 (16개 asmdef)

## 디렉토리 구조

```
Assets/UnityWorkbench/
├── Runtime/                    # 런타임 코드
│   ├── App/                    # 앱 라이프사이클, 딥링크
│   ├── Attributes/             # Inspector 커스텀 속성
│   ├── Audio/                  # 오디오 시스템
│   ├── Camera/                 # 카메라 시스템
│   ├── Core/                   # 핵심 시스템
│   │   ├── EventSystem/        # EventBus, GameEvent
│   │   ├── ObjectPooling/      # 오브젝트 풀
│   │   ├── Singleton/          # 싱글톤 패턴
│   │   └── StateMachine/       # 상태 머신
│   ├── Data/                   # 데이터 관리
│   │   ├── SaveLoad/           # 저장/불러오기
│   │   ├── ScriptableObjects/  # SO 유틸리티
│   │   └── Settings/           # 게임 설정
│   ├── Debug/                  # 디버그 도구
│   ├── Gameplay/               # 게임플레이 시스템
│   │   ├── Inventory/          # 인벤토리
│   │   ├── Quests/             # 퀘스트
│   │   ├── Skills/             # 스킬
│   │   └── Stats/              # 캐릭터 스탯
│   ├── Input/                  # 입력 시스템
│   ├── Localization/           # 다국어 지원
│   ├── Network/                # 네트워크 통신
│   ├── Notification/           # 알림 시스템
│   ├── Permission/             # 권한 관리
│   ├── SceneManagement/        # 씬 관리
│   ├── UI/                     # UI 시스템
│   │   ├── Loading/            # 로딩 화면
│   │   ├── Manager/            # UI 매니저
│   │   ├── Menu/               # 메뉴 네비게이션
│   │   └── Popup/              # 팝업/토스트
│   └── Utilities/              # 유틸리티
│       ├── Async/              # 코루틴 유틸
│       ├── Extensions/         # 확장 메서드
│       ├── Math/               # 수학 유틸
│       ├── Random/             # 랜덤 유틸
│       └── Timer/              # 타이머
└── Editor/                     # 에디터 확장
```

## 모듈별 핵심 클래스

### Core (`LWT.UnityWorkbench.Core`)
```csharp
// 싱글톤
public class GameManager : MonoSingleton<GameManager> { }
public class AudioManager : PersistentMonoSingleton<AudioManager> { }

// 이벤트 버스
EventBus<T>.Subscribe(Action<T> handler);
EventBus<T>.Publish(T eventData);
EventBus<T>.Unsubscribe(Action<T> handler);

// 오브젝트 풀
ObjectPool<T> pool = new ObjectPool<T>(createFunc, initialSize);
T obj = pool.Get();
pool.Release(obj);

// GameObject 풀
GameObjectPool pool = new GameObjectPool(prefab, initialSize);
GameObject obj = pool.Get();
pool.Release(obj);

// 상태 머신
StateMachine<TState> sm = new StateMachine<TState>();
sm.AddState(state, new StateHandler());
sm.ChangeState(newState);
```

### UI (`LWT.UnityWorkbench.UI`)
```csharp
// UI 매니저
UIManager.Instance.ShowScreen<T>();
UIManager.Instance.HideScreen<T>();
UIManager.Instance.GoBack();

// 팝업
PopupManager.Instance.ShowToast("메시지", duration);
PopupManager.Instance.ShowConfirm("제목", "내용", onConfirm, onCancel);

// 로딩
LoadingManager.Instance.ShowLoadingScreen("로딩 중...");
LoadingManager.Instance.UpdateProgress(0.5f);
LoadingManager.Instance.HideLoadingScreen();

// 씬 로딩 (static utility)
SceneLoader.Load("SceneName");
SceneLoader.LoadAdditive("SceneName");
SceneLoader.ReloadCurrent();
```

### Audio (`LWT.UnityWorkbench.Audio`)
```csharp
AudioManager.Instance.PlaySFX("clip_name");
AudioManager.Instance.PlayMusic("music_name");
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.5f);
AudioManager.Instance.SetSFXVolume(1.0f);
```

### Data (`LWT.UnityWorkbench.Data`)
```csharp
// 저장/불러오기
SaveManager.Instance.Save("slot1", saveData);
var data = SaveManager.Instance.Load<SaveData>("slot1");

// 설정
SettingsManager.Instance.SetInt("quality", 2);
int quality = SettingsManager.Instance.GetInt("quality", 1);

// RuntimeValue (반응형 데이터)
RuntimeInt health = ScriptableObject.CreateInstance<RuntimeInt>();
health.OnValueChanged += (old, current) => UpdateUI();
health.Value = 100;
```

### Gameplay (`LWT.UnityWorkbench.Gameplay`)
```csharp
// 인벤토리
Inventory inventory = GetComponent<Inventory>();
inventory.AddItem("item_id", 5);
inventory.RemoveItem("item_id", 2);
bool has = inventory.HasItem("item_id", 3);

// 스탯
CharacterStats stats = GetComponent<CharacterStats>();
float hp = stats.GetStat(StatType.Health);
stats.ModifyStat(StatType.Attack, 10, StatModifierType.Flat);

// 레벨
LevelSystem level = GetComponent<LevelSystem>();
level.AddExperience(100);
level.SetLevel(5);
```

### Network (`LWT.UnityWorkbench.Network`)
```csharp
// HTTP 요청
HttpClient.Instance.Get(url, response => {
    if (response.IsSuccess) {
        var data = response.Parse<MyData>();
    }
});

HttpClient.Instance.Post(url, requestBody, response => { });

// REST API 클라이언트
var api = new RestApiClient("https://api.example.com/v1");
api.SetAuthToken("bearer_token");
api.Get<User>("/users/1", response => { });
api.Post<CreateUserRequest, User>("/users", data, response => { });
```

### Localization (`LWT.UnityWorkbench.Localization`)
```csharp
// 기본 사용
string text = L.Get("ui.button.start");
string formatted = L.Get("ui.welcome", playerName);

// 언어 변경
LocalizationManager.Instance.SetLanguage("ko");
string current = LocalizationManager.Instance.CurrentLanguage;

// UI 컴포넌트
// LocalizedText 컴포넌트를 UI Text에 추가하고 Key 설정
```

### App (`LWT.UnityWorkbench.App`)
```csharp
// 앱 상태
AppManager.Instance.OnAppPaused += () => SaveGame();
AppManager.Instance.OnAppResumed += () => RefreshData();
bool isPaused = AppManager.Instance.IsPaused;

// 딥링크
DeepLinkHandler.Instance.RegisterHandler("open", data => {
    string id = data.GetParameter("id");
});

// 앱 정보
string version = AppInfo.Version;
bool hasInternet = AppInfo.HasInternet;
string deviceInfo = AppInfo.GetDeviceInfoString();
```

### Permission (`LWT.UnityWorkbench.Permission`)
```csharp
// 권한 확인
bool has = PermissionManager.Instance.HasPermission(AppPermission.Camera);

// 권한 요청
PermissionManager.Instance.RequestPermission(AppPermission.Camera, granted => {
    if (granted) StartCamera();
});
```

### Notification (`LWT.UnityWorkbench.Notification`)
```csharp
// 로컬 알림 예약
int id = NotificationManager.Instance.Schedule(
    "제목", "내용",
    DateTime.Now.AddHours(1)
);

// 인앱 알림
InAppNotification.Instance.ShowToast("저장 완료!");
InAppNotification.Instance.ShowBanner("새 메시지", "내용", onClick: () => { });
```

## Assembly Definition 의존성

```
Core (기본)
├── Attributes (독립)
├── Utilities → Core
├── Data → Core
├── Audio → Core
├── Camera → Core, Utilities
├── Input → Core, InputSystem
├── Debug → Core, Gameplay
├── UI → Core, Utilities, TMP, InputSystem
│   ├── SceneManagement → Core, UI
│   └── Gameplay → Core, Data, UI
├── App → Core
├── Network → Core
├── Localization → Core, TMP
├── Notification → Core, TMP
└── Permission → Core
```

## 컨벤션

### 네이밍
- **Private 필드**: `_camelCase` (언더스코어 prefix)
- **Public 프로퍼티**: `PascalCase`
- **메서드**: `PascalCase`
- **이벤트**: `On` prefix (예: `OnValueChanged`)

### 싱글톤 접근
```csharp
// MonoSingleton - 씬에 없으면 자동 생성
ManagerClass.Instance.Method();

// HasInstance로 null 체크
if (ManagerClass.HasInstance) { }
```

### 이벤트 패턴
```csharp
// 구독
EventBus<MyEvent>.Subscribe(OnMyEvent);

// 구독 해제 (OnDestroy에서 필수)
EventBus<MyEvent>.Unsubscribe(OnMyEvent);

// 발행
EventBus<MyEvent>.Publish(new MyEvent { Data = value });
```

## 자주 사용되는 패턴

### 초기화 순서
1. `Awake()` - 컴포넌트 참조 캐싱
2. `OnEnable()` - 이벤트 구독
3. `Start()` - 초기 상태 설정
4. `OnDisable()` - 이벤트 구독 해제

### ScriptableObject 기반 설정
```csharp
[CreateAssetMenu(menuName = "Game/Config")]
public class GameConfig : ScriptableObject
{
    public float PlayerSpeed = 5f;
}
```

## 확장 방법

### 새 이벤트 추가
```csharp
// Events.cs 또는 해당 모듈에 정의
public struct PlayerDiedEvent : IEvent
{
    public int PlayerId;
    public Vector3 Position;
}
```

### 새 상태 추가
```csharp
public class MyGameState : StateBase
{
    public override void Enter() { }
    public override void Update() { }
    public override void Exit() { }
}
```

## 주의사항

1. **Input 네임스페이스**: `LWT.UnityWorkbench.InputHandling` (UnityEngine.Input 충돌 방지)
2. **SceneUtility**: `UnityEngine.SceneManagement.SceneUtility` 전체 경로 사용
3. **이벤트 구독 해제**: `OnDestroy()`에서 반드시 `Unsubscribe` 호출
4. **싱글톤 초기화**: `Awake()`에서 인스턴스 설정, `DontDestroyOnLoad` 처리
