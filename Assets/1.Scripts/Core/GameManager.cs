using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 게임의 전체 상태 머신을 관리하며, 씬 전환과 주요 시스템 간 조율을 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// 게임의 현재 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        CharacterSelect, // 기존 Allocation을 CharacterSelect로 변경
        PointAllocation, // PointAllocation 씬을 위한 새로운 상태 추가
        Gameplay,
        Reward,
        Pause,
        Codex
    }

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
        ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// 게임 상태를 변경하는 메서드입니다.
    /// </summary>
    /// <param name="newState">새로운 게임 상태</param>
    public void ChangeState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                sceneTransitionManager.LoadScene("MainMenu");
                Time.timeScale = 1;
                break;
            case GameState.CharacterSelect:
                sceneTransitionManager.LoadScene("CharacterSelect");
                Time.timeScale = 1;
                break;
            case GameState.PointAllocation: // 새로 추가된 PointAllocation 상태
                sceneTransitionManager.LoadScene("PointAllocation");
                Time.timeScale = 1;
                break;
            case GameState.Gameplay:
                sceneTransitionManager.LoadScene("Gameplay");
                StartCoroutine(StartRoundAfterSceneLoad());
                break;
            case GameState.Reward:
                sceneTransitionManager.LoadScene("CardReward");
                Time.timeScale = 0;
                break;
            case GameState.Pause:
                Time.timeScale = 0;
                break;
            case GameState.Codex:
                sceneTransitionManager.LoadScene("Codex");
                Time.timeScale = 1;
                break;
        }
    }

    private IEnumerator StartRoundAfterSceneLoad()
    {
        yield return null;
        RoundManager roundManager = RoundManager.Instance;
        if (roundManager != null)
        {
            roundManager.StartRound();
        }
        else
        {
            Debug.LogError("Gameplay 씬에서 RoundManager를 찾을 수 없습니다!");
        }
        Time.timeScale = 1f;
    }
}