using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex, GameOver, Shop, Rest, Event }
    public GameState CurrentState { get; private set; }
    public CharacterDataSO SelectedCharacter { get; set; }
    public int AllocatedPoints { get; set; }
    public bool isFirstRound = true;

    public event System.Action<GameState> OnGameStateChanged;

    private SceneTransitionManager sceneTransitionManager;

    private void Awake()
    {
        ServiceLocator.Register<GameManager>(this);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- [추가] ---
    // 씬 로드가 '완전히' 끝났을 때 Unity가 이 함수를 자동으로 호출해줍니다.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] '{scene.name}' 씬 로드 완료.");
        // 현재 게임 상태가 Gameplay일 경우에만 라운드 시작 로직을 실행합니다.
        if (CurrentState == GameState.Gameplay)
        {
            // 이제 씬에 있는 모든 오브젝트(HUD 포함)가 준비된 상태이므로,
            // 여기서 라운드를 시작하면 안전합니다.
            StartCoroutine(StartRoundAfterSceneLoad());
        }
    }

    private void Start()
    {
        sceneTransitionManager = SceneTransitionManager.Instance;
        if (sceneTransitionManager == null) Debug.LogError("!!! GameManager: SceneTransitionManager를 찾을 수 없음!!!");
    }

    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;
        Debug.Log($"[GameManager] 상태 변경 시작: {CurrentState} -> {newState}");
        CurrentState = newState;

        // 상태 변경 이벤트를 방송합니다.
        OnGameStateChanged?.Invoke(newState);

        string sceneName = "";
        switch (newState)
        {
            case GameState.MainMenu: sceneName = "MainMenu"; break;
            case GameState.CharacterSelect: sceneName = "CharacterSelect"; break;
            case GameState.PointAllocation: sceneName = "PointAllocation"; break;
            case GameState.Gameplay: sceneName = "Gameplay"; break;
            case GameState.Reward: sceneName = "CardReward"; break;
            case GameState.Codex: sceneName = "Codex"; break;
            case GameState.Shop: sceneName = "Shop"; break;
            case GameState.Rest: sceneName = "Rest"; break;
            case GameState.Event: sceneName = "Event"; break;
            case GameState.Pause:
                Time.timeScale = 0;
                return;
            case GameState.GameOver:
                StartCoroutine(GameOverRoutine());
                return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            sceneTransitionManager.LoadScene(sceneName);
        }

        if (newState == GameState.Gameplay)
        {
            StartCoroutine(StartRoundAfterSceneLoad());
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    private IEnumerator GameOverRoutine()
    {
        if (PopupController.Instance != null)
        {
            PopupController.Instance.ShowError("GAME OVER", 3f);
        }
        yield return new WaitForSeconds(3f);
        ChangeState(GameState.MainMenu);
    }

    private IEnumerator StartRoundAfterSceneLoad()
    {
        // --- 기존 디버그 로그 보존 ---
        Debug.Log("--- [GameManager] StartRoundAfterSceneLoad 코루틴 시작 ---");

#if UNITY_EDITOR
        // --- [테스트 모드 자동 설정] ---
        // 만약 MapManager가 준비되지 않은 상태로 이 코루틴이 시작되었다면,
        // Gameplay 씬에서 바로 테스트를 시작한 것으로 간주합니다.
        if (MapManager.Instance == null || !MapManager.Instance.IsMapInitialized)
        {
            Debug.LogWarning("[GameManager] 테스트 모드 감지: 필수 데이터 자동 설정 시작...");

            // 1. 씬에서 MapGenerator를 찾아 맵을 생성하고 MapManager를 초기화합니다.
            MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
            if (mapGenerator != null)
            {
                List<MapNode> mapData = mapGenerator.Generate();
                MapManager.Instance.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
                Debug.Log("[GameManager] 테스트용 맵 데이터 생성 및 초기화 완료.");
            }
            else
            {
                Debug.LogError("[GameManager] 테스트 모드 설정 실패: 씬에서 MapGenerator를 찾을 수 없습니다!");
                yield break; // 맵 생성 없이는 진행 불가
            }

            // 2. 테스트에 필요한 기본값들을 GameManager에 설정합니다.
            if (SelectedCharacter == null)
            {
                SelectedCharacter = ServiceLocator.Get<DataManager>().GetCharacter("warrior");
                Debug.Log("[GameManager] 테스트용 기본 캐릭터 'warrior' 설정 완료.");
            }
            AllocatedPoints = 0; // 테스트 시에는 추가 포인트 없음
            isFirstRound = true; // 첫 라운드처럼 동작하도록 설정
        }
#endif
        yield return null;

        float timeout = 5f;
        float timer = 0f;

        // ★★★ 핵심 수정: RoundManager.Instance 대신 FindObjectOfType 사용 (CS0117 오류 해결) ★★★
        RoundManager roundManager = null;
        while (roundManager == null || MapManager.Instance == null || CampaignManager.Instance == null)
        {
            roundManager = FindObjectOfType<RoundManager>();

            // --- 기존 디버그 로그 보존 ---

            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("[GameManager] 시간 초과! 씬 내 매니저 중 하나를 찾을 수 없습니다. 라운드를 시작할 수 없습니다.");
                yield break;
            }
            yield return null;
        }
        // --- 기존 디버그 로그 보존 ---
        Debug.Log("1. [GameManager] 모든 매니저 인스턴스를 성공적으로 찾았습니다.");

        // --- 기존 디버그 로그 보존 ---
        Debug.Log("[GameManager] MapManager의 내부 초기화 대기 시작...");
        timer = 0f;
        while (!MapManager.Instance.IsMapInitialized)
        {
            // --- 기존 디버그 로그 보존 ---
            Debug.LogWarning("[진단] MapManager가 아직 준비되지 않았습니다. 대기합니다...");
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("[GameManager] 시간 초과! MapManager가 초기화되지 않았습니다. 라운드를 시작할 수 없습니다.");
                yield break;
            }
            yield return null;
        }
        // --- 기존 디버그 로그 보존 ---
        Debug.Log("2. [GameManager] MapManager 초기화 완료됨을 확인했습니다.");

        CampaignManager.Instance.SelectAndStartRandomCampaign();

        MapNode currentNode = MapManager.Instance.CurrentNode;
        if (currentNode == null)
        {
            Debug.LogError("3. [GameManager] 에러! MapManager로부터 현재 노드 정보를 가져올 수 없습니다!");
            yield break;
        }

        // --- 기존 디버그 로그 보존 ---
        Debug.Log($"3. [GameManager] 현재 노드(Y:{currentNode.Position.y})에 맞는 라운드 데이터를 찾습니다.");

        // ★★★ 핵심 수정: RoundDataSO 타입 사용 및 실제 함수명 사용 (CS1061, CS0246 오류 해결) ★★★
        RoundDataSO roundToStart = CampaignManager.Instance.GetRoundDataForNode(currentNode);

        if (roundToStart != null)
        {
            // --- 기존 디버그 로그 보존 ---
            Debug.Log($"4. [GameManager] RoundManager에게 '{roundToStart.name}' 라운드 시작을 요청합니다.");

            // ★★★ 핵심 수정: StartRound를 코루틴으로 호출 ★★★
            yield return StartCoroutine(roundManager.StartRound(roundToStart));
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogError($"4. [GameManager] 에러! '{currentNode.Position}' 노드에 해당하는 라운드 데이터를 찾지 못했습니다!");
        }
        // --- 기존 디버그 로그 보존 ---
        Debug.Log("--- [GameManager] StartRoundAfterSceneLoad 코루틴 정상 종료 ---");
    }
}