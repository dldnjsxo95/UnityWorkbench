# Changelog

All notable changes to UnityWorkbench will be documented in this file.

## [1.0.0] - 2024-02-05

### Added

#### Core Module
- `EventBus<T>` - 타입 기반 이벤트 시스템
- `ObjectPool<T>`, `GameObjectPool` - 오브젝트 풀링
- `MonoSingleton<T>`, `PersistentMonoSingleton<T>` - 싱글톤 패턴
- `StateMachine<T>`, `HierarchicalStateMachine` - 상태 머신
- `ServiceLocator` - 서비스 로케이터 패턴

#### UI Module
- `UIManager` - 화면 스택 관리
- `PopupManager` - 팝업/토스트 시스템
- `LoadingManager`, `LoadingScreen` - 로딩 화면
- `MenuNavigator`, `MenuButton`, `MenuGroup` - 메뉴 네비게이션

#### Audio Module
- `AudioManager` - 오디오 총괄 관리
- `MusicPlayer` - BGM 재생 (크로스페이드 지원)
- `SFXPlayer` - 효과음 재생 (풀링 지원)

#### Camera Module
- `CameraController` - 타겟 추적 카메라
- `CameraShake` - 카메라 흔들림 효과
- `CameraEffects` - 페이드, 줌, 플래시 효과
- `FreeLookCamera` - 3인칭 오빗 카메라
- `Camera2D` - 2D 카메라 (데드존, 바운드)

#### Input Module
- `InputManager` - 입력 관리
- `InputBuffer` - 입력 버퍼링
- `ComboDetector` - 콤보 입력 감지
- `InputRebinding` - 런타임 키 리바인딩
- `VirtualJoystick`, `VirtualButton` - 터치 입력

#### Scene Management Module
- `SceneLoader` (Instance) - 비동기 씬 로딩
- `SceneLoader` (Static) - 간편 씬 유틸리티
- `SceneTransition` - 전환 효과 (Fade, Wipe, Circle)
- `SceneData`, `SceneDatabase` - 씬 메타데이터

#### Gameplay Module
- `Inventory`, `InventorySlot` - 인벤토리 시스템
- `ItemBase`, `ItemDatabase` - 아이템 시스템
- `CharacterStats`, `StatModifier` - 캐릭터 스탯
- `LevelSystem` - 레벨/경험치 시스템
- `QuestManager`, `QuestBase` - 퀘스트 시스템
- `SkillUser`, `SkillBase` - 스킬 시스템

#### Data Module
- `SaveManager` - 저장/불러오기
- `JsonSaveSystem`, `BinarySaveSystem` - 직렬화 구현
- `SettingsManager` - 게임 설정
- `RuntimeValue<T>` - 반응형 데이터
- `RuntimeSet<T>` - 런타임 컬렉션

#### App Module (NEW)
- `AppManager` - 앱 라이프사이클 관리
- `DeepLinkHandler` - 딥링크/URL 스킴
- `AppInfo` - 앱/디바이스 정보

#### Network Module (NEW)
- `HttpClient` - HTTP 요청 (GET/POST/PUT/PATCH/DELETE)
- `RestApiClient` - REST API 클라이언트
- `WebSocketClient` - WebSocket 통신

#### Localization Module (NEW)
- `LocalizationManager` - 다국어 관리
- `LocalizedText` - UI 텍스트 자동 번역
- `L` - 간편 접근 유틸리티

#### Notification Module (NEW)
- `NotificationManager` - 로컬/푸시 알림
- `InAppNotification` - 인앱 Toast/Banner/Alert

#### Permission Module (NEW)
- `PermissionManager` - 런타임 권한 관리

#### Debug Module
- `RuntimeConsole` - 런타임 디버그 콘솔
- `FPSCounter` - FPS 표시
- `DebugOverlay` - 디버그 오버레이

#### Utilities Module
- `Timer`, `Cooldown`, `Stopwatch` - 타이머 유틸
- `Easing` - 이징 함수
- `MathUtility` - 수학 유틸
- `RandomUtility` - 랜덤 유틸
- Extension methods - Transform, Vector, Collection, String, Color, GameObject

#### Attributes Module
- `[ReadOnly]` - 읽기 전용 필드
- `[Required]` - 필수 필드
- `[ShowIf]`, `[HideIf]` - 조건부 표시
- `[MinMax]` - 범위 슬라이더
- `[Button]` - 버튼 메서드
- `[Separator]` - 구분선
- `[InfoBox]` - 정보 박스
- `[TagSelector]`, `[LayerSelector]`, `[SceneSelector]` - 선택기

### Architecture
- 모듈별 Assembly Definition 분리 (16개)
- 명확한 의존성 계층 구조
- IL2CPP 스트리핑 최적화 지원
