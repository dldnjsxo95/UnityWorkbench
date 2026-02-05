# UnityWorkbench

Unity 게임/앱 개발을 위한 모듈형 프레임워크입니다.

## 설치

### Unity Package Manager (Git URL)
```
https://github.com/dldnjsxo95/UnityWorkbench.git?path=Assets/UnityWorkbench
```

### 수동 설치
`Assets/UnityWorkbench` 폴더를 프로젝트에 복사

## 요구사항

- Unity 2021.3+
- TextMeshPro
- Input System (선택)

## 모듈 구조

| 모듈 | 네임스페이스 | 용도 |
|------|-------------|------|
| Core | `LWT.UnityWorkbench.Core` | Singleton, EventBus, ObjectPool, StateMachine |
| Utilities | `LWT.UnityWorkbench.Utilities` | Extensions, Timer, Easing, Math |
| Data | `LWT.UnityWorkbench.Data` | SaveSystem, Settings, ScriptableObjects |
| UI | `LWT.UnityWorkbench.UI` | UIManager, Popup, Menu, Loading |
| Audio | `LWT.UnityWorkbench.Audio` | AudioManager, Music, SFX |
| Camera | `LWT.UnityWorkbench.Camera` | CameraController, Shake, Effects |
| Input | `LWT.UnityWorkbench.InputHandling` | InputManager, Buffer, Rebinding |
| SceneManagement | `LWT.UnityWorkbench.SceneManagement` | SceneLoader, Transitions |
| Gameplay | `LWT.UnityWorkbench.Gameplay` | Inventory, Quest, Stats, Skills |
| Debug | `LWT.UnityWorkbench.Debug` | RuntimeConsole, FPSCounter |
| App | `LWT.UnityWorkbench.App` | AppManager, DeepLink, AppInfo |
| Network | `LWT.UnityWorkbench.Network` | HttpClient, RestAPI, WebSocket |
| Localization | `LWT.UnityWorkbench.Localization` | 다국어 지원 |
| Notification | `LWT.UnityWorkbench.Notification` | Local/Push 알림, InApp 알림 |
| Permission | `LWT.UnityWorkbench.Permission` | 런타임 권한 관리 |
| Attributes | `LWT.UnityWorkbench.Attributes` | Inspector 커스텀 속성 |

## 빠른 시작

```csharp
using LWT.UnityWorkbench.Core;
using LWT.UnityWorkbench.UI;

// 이벤트 발행/구독
EventBus<GameStartedEvent>.Publish(new GameStartedEvent());
EventBus<GameStartedEvent>.Subscribe(e => Debug.Log("Game Started!"));

// 오브젝트 풀링
var pool = new ObjectPool<Bullet>(() => new Bullet(), 10);
var bullet = pool.Get();
pool.Release(bullet);

// UI 팝업
PopupManager.Instance.ShowToast("Hello!");

// 씬 로딩
SceneLoader.Load("GameScene");
```

## 라이선스

MIT License
