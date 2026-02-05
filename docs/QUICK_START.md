# UnityWorkbench 빠른 시작 가이드

## 1. 설치

### Git URL (권장)
Unity Package Manager → Add package from git URL:
```
https://github.com/YOUR_USERNAME/UnityWorkbench.git?path=Assets/UnityWorkbench
```

### 수동 설치
`Assets/UnityWorkbench` 폴더를 프로젝트의 `Assets` 폴더에 복사

## 2. 필수 패키지

Package Manager에서 다음 패키지 설치:
- **TextMeshPro** (필수)
- **Input System** (선택, Input 모듈 사용 시)

## 3. 기본 사용법

### 싱글톤 생성
```csharp
using LWT.UnityWorkbench.Core;

public class GameManager : PersistentMonoSingleton<GameManager>
{
    // 씬 전환 시에도 유지됨
    public int Score { get; set; }
}

// 사용
GameManager.Instance.Score = 100;
```

### 이벤트 시스템
```csharp
using LWT.UnityWorkbench.Core;

// 이벤트 정의
public struct PlayerDiedEvent : IEvent
{
    public string PlayerId;
}

// 구독
void OnEnable()
{
    EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
}

void OnDisable()
{
    EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
}

void OnPlayerDied(PlayerDiedEvent e)
{
    Debug.Log($"Player {e.PlayerId} died!");
}

// 발행
EventBus<PlayerDiedEvent>.Publish(new PlayerDiedEvent { PlayerId = "player1" });
```

### 오브젝트 풀링
```csharp
using LWT.UnityWorkbench.Core;

// GameObject 풀
[SerializeField] private GameObject bulletPrefab;
private GameObjectPool bulletPool;

void Start()
{
    bulletPool = new GameObjectPool(bulletPrefab, 20);
}

void Fire()
{
    var bullet = bulletPool.Get(firePoint.position, firePoint.rotation);
    // 사용 후
    bulletPool.Release(bullet);
}
```

### UI 팝업
```csharp
using LWT.UnityWorkbench.UI;

// 토스트
PopupManager.Instance.ShowToast("저장 완료!");

// 확인 다이얼로그
PopupManager.Instance.ShowConfirm(
    "게임 종료",
    "정말 종료하시겠습니까?",
    () => Application.Quit(),  // 확인
    null                        // 취소
);
```

### 씬 로딩
```csharp
using LWT.UnityWorkbench.UI;

// 간단한 로딩
SceneLoader.Load("GameScene");

// 로딩 화면과 함께
LoadingManager.Instance.LoadScene("GameScene");
```

### 오디오
```csharp
using LWT.UnityWorkbench.Audio;

// 효과음
AudioManager.Instance.PlaySFX("click");

// 배경음악
AudioManager.Instance.PlayMusic("bgm_title");

// 볼륨 조절
AudioManager.Instance.SetMasterVolume(0.8f);
```

### HTTP 요청
```csharp
using LWT.UnityWorkbench.Network;

// GET 요청
HttpClient.Instance.Get("https://api.example.com/users", response =>
{
    if (response.IsSuccess)
    {
        var users = response.Parse<UserList>();
    }
});

// REST API 클라이언트
var api = new RestApiClient("https://api.example.com/v1");
api.SetAuthToken("your_token");

api.Get<User>("/users/1", response =>
{
    response.OnSuccess(user => Debug.Log(user.name))
            .OnError((error, code) => Debug.LogError(error));
});
```

### 다국어
```csharp
using LWT.UnityWorkbench.Localization;

// 텍스트 가져오기
string welcomeText = L.Get("ui.welcome");
string formatted = L.Get("ui.score", score);

// 언어 변경
LocalizationManager.Instance.SetLanguage("ko");
```

### 권한 요청 (모바일)
```csharp
using LWT.UnityWorkbench.Permission;

PermissionManager.Instance.RequestPermission(AppPermission.Camera, granted =>
{
    if (granted)
    {
        StartCamera();
    }
    else
    {
        ShowPermissionDeniedMessage();
    }
});
```

## 4. 씬 설정

### 필수 매니저 오브젝트
게임 시작 씬에 다음 매니저들을 배치:

1. **빈 GameObject 생성** → 이름: `[Managers]`
2. **컴포넌트 추가**:
   - `AudioManager`
   - `UIManager`
   - `PopupManager` (Canvas 필요)
   - `LoadingManager` (Canvas 필요)

또는 매니저들은 자동 생성되므로 `Instance`에 처음 접근할 때 생성됩니다.

## 5. Assembly Definition 참조

프로젝트의 asmdef에서 필요한 모듈 참조:
```json
{
    "references": [
        "LWT.UnityWorkbench.Core",
        "LWT.UnityWorkbench.UI",
        "LWT.UnityWorkbench.Audio"
    ]
}
```

## 6. 추천 프로젝트 구조

```
Assets/
├── _Project/                    # 프로젝트 코드
│   ├── Scripts/
│   │   ├── LWT.Project.asmdef   # 프로젝트 asmdef
│   │   ├── Managers/
│   │   ├── UI/
│   │   └── Gameplay/
│   ├── Prefabs/
│   ├── Scenes/
│   └── Resources/
└── UnityWorkbench/              # 패키지 (수정하지 않음)
```

## 7. 자주 묻는 질문

### Q: 특정 모듈만 사용하고 싶어요
A: 필요한 모듈의 asmdef만 참조하면 됩니다. 사용하지 않는 모듈은 빌드에서 제외됩니다.

### Q: 네임스페이스 충돌이 발생해요
A: Input 모듈은 `LWT.UnityWorkbench.InputHandling` 네임스페이스를 사용합니다 (UnityEngine.Input 충돌 방지).

### Q: 싱글톤이 자동 생성되지 않아요
A: `Instance` 프로퍼티에 처음 접근할 때 자동 생성됩니다. 초기화 순서가 중요한 경우 시작 씬에 미리 배치하세요.

### Q: 이벤트가 호출되지 않아요
A: `OnDisable()`에서 `Unsubscribe`를 호출했는지 확인하세요. 객체가 비활성화되면 구독도 해제해야 합니다.
