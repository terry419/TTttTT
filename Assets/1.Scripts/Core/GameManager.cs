// ./TTttTT/Assets/1.Scripts/Core/GameManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.AsyncOperations; // AsyncOperationHandle 사용

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex, GameOver, Shop, Rest, Event, GameWon }
    public GameState CurrentState { get; private set; }
    public CharacterDataSO SelectedCharacter { get; set; }
    public int AllocatedPoints { get; set; }

    public event Action<GameState, GameState> OnBeforeStateChange;
    public event Action<GameState, GameState> OnAfterStateChange;

    public event Action<GameState> OnGameStateChanged;

    private SceneTransitionManager sceneTransitionManager;
    private InputManager inputManager;
    private GameObject _gameplaySessionInstance;
    private AsyncOperationHandle<GameObject> _gameplaySessionHandle;
    private RoundManager _currentRoundManager;

    private void Awake()
    {
        Debug.Log($"[GameManager] Awake() 호출됨. (ID: {GetInstanceID()})");
        if (!ServiceLocator.IsRegistered<GameManager>())
        {
            ServiceLocator.Register<GameManager>(this);
            DontDestroyOnLoad(transform.root.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            this.OnBeforeStateChange += HandleStateChangeCleanup;
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }
    private void Start()
    {
        sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();
        inputManager = ServiceLocator.Get<InputManager>();

        if (inputManager != null)
        {
            inputManager.LinkToGameManager(this);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (inputManager != null)
        {
            inputManager.UnlinkFromGameManager(this);
        }
        // [추가] 오브젝트 파괴 시 이벤트 구독 해제
        this.OnBeforeStateChange -= HandleStateChangeCleanup;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    public void ReportRoundManagerReady(RoundManager roundManager)
    {
        Debug.Log($"[GameManager] RoundManager로부터 준비 보고를 받았습니다. 라운드 시작 절차를 진행합니다.");
        _currentRoundManager = roundManager;
        StartCoroutine(StartRoundRoutine());
    }

    private IEnumerator StartRoundRoutine()
    {
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        yield return new WaitUntil(() => mapManager != null && campaignManager != null && mapManager.IsMapInitialized);

        MapNode currentNode = mapManager.CurrentNode;
        if (currentNode == null)
        {
            Debug.LogError("[GameManager] 에러! MapManager로부터 현재 노드 정보를 가져올 수 없습니다!");
            yield break;
        }

        RoundDataSO roundToStart = campaignManager.GetRoundDataForNode(currentNode);
        if (roundToStart != null && _currentRoundManager != null)
        {
            yield return StartCoroutine(_currentRoundManager.StartRound(roundToStart));
        }
    }
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        Debug.Log($"[GameManager] 상태 변경 요청: {CurrentState} -> {newState}");
        OnBeforeStateChange?.Invoke(CurrentState, newState); // [추가] 상태 변경 직전 이벤트 방송

        GameState oldState = CurrentState;

        if (newState == GameState.PointAllocation && _gameplaySessionInstance == null)
        {
            InstantiateGameplaySessionAndChangeState(newState).Forget();
            return;
        }

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        if (newState == GameState.Pause) { Time.timeScale = 0; return; }
        else if (newState == GameState.GameOver) { StartCoroutine(GameOverRoutine()); return; }

        Time.timeScale = 1;

        string sceneName = GetSceneNameForState(newState);
        if (newState == GameState.Gameplay)
        {
            // [수정] 변경된 상수 이름을 사용합니다.
            sceneName = SceneNames.Gameplay;
        }

        if (!string.IsNullOrEmpty(sceneName) && SceneManager.GetActiveScene().name != sceneName)
        {
            sceneTransitionManager.LoadScene(sceneName);
        }

        OnAfterStateChange?.Invoke(oldState, CurrentState); // [추가] 상태 변경 완료 후 이벤트 방송
    }
    private void HandleStateChangeCleanup(GameState from, GameState to)
    {
        // 게임 세션이 끝나고 메인 메뉴로 돌아갈 때 _GameplaySession 인스턴스를 파괴합니다.
        if (to == GameState.MainMenu && _gameplaySessionInstance != null)
        {
            Debug.Log("[GameManager] 메인 메뉴로 복귀. _GameplaySession 인스턴스를 파괴합니다.");
            Addressables.ReleaseInstance(_gameplaySessionHandle);
            _gameplaySessionInstance = null;
        }
    }

    /// <summary>
    /// [리팩토링] 게임 세션 프리팹을 생성하고, 데이터 초기화 후 상태를 변경하는 비동기 메서드
    /// </summary>
    private async UniTaskVoid InstantiateGameplaySessionAndChangeState(GameState targetState)
    {
        _gameplaySessionHandle = Addressables.InstantiateAsync(PrefabKeys.GameplaySession);
        _gameplaySessionInstance = await _gameplaySessionHandle.Task;

        if (_gameplaySessionInstance == null)
        {
            Debug.LogError("[GameManager] _GameplaySession 프리팹 인스턴스화에 실패했습니다!");
            return;
        }

        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        if (playerDataManager != null && SelectedCharacter != null)
        {
            playerDataManager.ResetRunData(SelectedCharacter);
        }

        // 상태를 변경하고 씬을 로드합니다.
        ChangeState(targetState);
    }

    // (이하 다른 메서드들은 기존과 동일)
    public void SetupForTest(CharacterDataSO character, int allocatedPoints)
    {
        Debug.Log($"[GameManager] 테스트 모드 설정: Character={character.characterId}, Points={allocatedPoints}");
        SelectedCharacter = character;
        AllocatedPoints = allocatedPoints;
    }

    public async UniTask PreloadAssetsForRound(RoundDataSO roundData)
    {
        Debug.Log("--- [GameManager] v9.0 프리로딩 시작 ---");
        var poolManager = ServiceLocator.Get<PoolManager>();
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        var preloadTasks = new List<UniTask>();
        var preloadRequests = new Dictionary<string, int>();

        void AddOrUpdatePreloadRequest(AssetReferenceGameObject assetRef, int count)
        {
            if (assetRef != null && assetRef.RuntimeKeyIsValid() && count > 0)
            {
                string key = assetRef.AssetGUID;
                if (preloadRequests.ContainsKey(key))
                    preloadRequests[key] = Mathf.Max(preloadRequests[key], count);
                else
                    preloadRequests.Add(key, count);
            }
        }

        if (playerDataManager != null && playerDataManager.CurrentRunData != null)
        {
            foreach (var card in playerDataManager.CurrentRunData.equippedCards)
            {
                foreach (var moduleEntry in card.CardData.modules)
                {
                    if (moduleEntry.moduleReference.RuntimeKeyIsValid())
                    {
                        CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);
                        if (module is IPreloadable preloadableModule)
                        {
                            foreach (var prefabRef in preloadableModule.GetPrefabsToPreload())
                            {
                                AddOrUpdatePreloadRequest(prefabRef, card.CardData.preloadCount);
                            }
                        }
                    }
                }
            }
        }

        if (roundData != null)
        {
            foreach (var wave in roundData.waves)
            {
                if (wave.monsterData != null)
                {
                    AddOrUpdatePreloadRequest(wave.monsterData.prefabRef, wave.preloadCount > 0 ? wave.preloadCount : wave.count);
                }
            }
        }


        Debug.Log($"[GameManager] 총 {preloadRequests.Count} 종류의 프리팹에 대한 프리로드를 실행합니다.");
        foreach (var request in preloadRequests)
        {
            preloadTasks.Add(poolManager.Preload(request.Key, request.Value));
        }

        await UniTask.WhenAll(preloadTasks);
        Debug.Log("--- [GameManager] 모든 프리로딩 비동기 대기 완료 ---");
    }

    private IEnumerator GameOverRoutine()
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager != null)
        {
            poolManager.ClearAndDestroyEntirePool();
        }

        var popupController = ServiceLocator.Get<PopupController>();
        if (popupController != null)
        {
            popupController.ShowError("GAME OVER", 3f);
        }
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1;

        ChangeState(GameState.MainMenu);
    }

    private string GetSceneNameForState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu: return SceneNames.MainMenu;
            case GameState.CharacterSelect: return SceneNames.CharacterSelect;
            case GameState.PointAllocation: return SceneNames.PointAllocation;
            case GameState.Reward: return SceneNames.CardReward;
            case GameState.Codex: return SceneNames.Codex;
            case GameState.Shop: return SceneNames.Shop;
            case GameState.Rest: return SceneNames.Rest;
            case GameState.Event: return SceneNames.Event;
            default: return "";
        }
    }

    private IEnumerator StartRoundAfterSceneLoad()
    {
        Debug.Log("--- [GameManager] StartRoundAfterSceneLoad 코루틴 시작 ---");

        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        yield return null;

        float timeout = 5f;
        float timer = 0f;
        RoundManager roundManager = null;
        while (roundManager == null || mapManager == null || campaignManager == null)
        {
            roundManager = FindObjectOfType<RoundManager>();
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("[GameManager] 시간 초과! 씬 내 매니저(Round, Map, Campaign) 중 하나를 찾을 수 없습니다.");
                yield break;
            }
            yield return null;
        }
        Debug.Log("1. [GameManager] 모든 매니저 인스턴스를 성공적으로 찾았습니다.");

        timer = 0f;
        while (!mapManager.IsMapInitialized)
        {
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("[GameManager] 시간 초과! MapManager가 초기화되지 않았습니다.");
                yield break;
            }
            yield return null;
        }
        Debug.Log("2. [GameManager] MapManager 초기화 완료됨을 확인했습니다.");

        MapNode currentNode = mapManager.CurrentNode;
        if (currentNode == null)
        {
            Debug.LogError("3. [GameManager] 에러! MapManager로부터 현재 노드 정보를 가져올 수 없습니다!");
            yield break;
        }

        Debug.Log($"3. [GameManager] 현재 노드(Y:{currentNode.Position.y})에 맞는 라운드 데이터를 찾습니다.");
        RoundDataSO roundToStart = campaignManager.GetRoundDataForNode(currentNode);

        if (roundToStart != null)
        {
            Debug.Log($"4. [GameManager] RoundManager에게 '{roundToStart.name}' 라운드 시작을 요청합니다.");
            yield return StartCoroutine(roundManager.StartRound(roundToStart));
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogError($"4. [GameManager] 에러! '{currentNode.Position}' 노드에 해당하는 라운드 데이터를 찾지 못했습니다!");
        }
        Debug.Log("--- [GameManager] StartRoundAfterSceneLoad 코루틴 정상 종료 ---");
    }
}