using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex, GameOver, Shop, Rest, Event }
    public GameState CurrentState { get; private set; }
    public CharacterDataSO SelectedCharacter { get; set; }
    public int AllocatedPoints { get; set; }

    private SceneTransitionManager sceneTransitionManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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