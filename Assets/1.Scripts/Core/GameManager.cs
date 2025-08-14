using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex }
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
        if (newState == CurrentState && CurrentState != GameState.MainMenu) return;

        Debug.Log($"[GameManager] 상태 변경: {CurrentState} -> {newState}");
        CurrentState = newState;

        string sceneName = "";
        switch (newState)
        {
            case GameState.MainMenu: sceneName = "MainMenu"; break;
            case GameState.CharacterSelect: sceneName = "CharacterSelect"; break;
            case GameState.PointAllocation: sceneName = "PointAllocation"; break;
            case GameState.Gameplay: sceneName = "Gameplay"; break; // 또는 Gameplay_New
            case GameState.Reward: sceneName = "CardReward"; break;
            case GameState.Codex: sceneName = "Codex"; break;
            case GameState.Pause:
                Time.timeScale = 0;
                return;
        }

        sceneTransitionManager.LoadScene(sceneName);

        if (newState == GameState.Gameplay)
        {
            StartCoroutine(StartRoundAfterSceneLoad());
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    /// <summary>
    /// [수정] RoundManager가 준비될 때까지 확실하게 기다리는 로직으로 변경
    /// </summary>
    private IEnumerator StartRoundAfterSceneLoad()
    {
        // 씬 로드가 완료될 때까지 한 프레임 대기합니다.
        yield return null;
        Debug.Log("--- [GameManager] Gameplay 씬 로드 완료, RoundManager를 기다립니다... ---");

        // RoundManager.Instance가 준비될 때까지(Awake가 실행될 때까지) 계속 기다립니다.
        while (RoundManager.Instance == null)
        {
            yield return null; // 한 프레임 더 대기
        }

        Debug.Log("1. [GameManager] RoundManager.Instance를 성공적으로 찾음. StartRound() 호출.");
        RoundManager.Instance.StartRound();
        Time.timeScale = 1f;
    }
}