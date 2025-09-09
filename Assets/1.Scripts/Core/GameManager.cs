// 파일명: GameManager.cs (수정 완료)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex, GameOver, Shop, Rest, Event, GameWon }
    public GameState CurrentState { get; private set; }
    public CharacterDataSO SelectedCharacter { get; set; }
    public int AllocatedPoints { get; set; }

    private float? lastPlayerHealth = null;

    public event System.Action<GameState> OnGameStateChanged;

    private SceneTransitionManager sceneTransitionManager;
    private InputManager inputManager;

    private void Awake()
    {
        Debug.Log($"[GameManager] Awake() 호출됨. (ID: {GetInstanceID()})");
        if (!ServiceLocator.IsRegistered<GameManager>())
        {
            ServiceLocator.Register<GameManager>(this);
            DontDestroyOnLoad(transform.root.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (!ServiceLocator.IsRegistered<RewardGenerationService>())
            {
                GameObject serviceGO = new GameObject("RewardGenerationService");
                serviceGO.AddComponent<RewardGenerationService>();
            }
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    private void Start()
    {
        Debug.Log($"[GameManager] Start() 호출됨. (ID: {GetInstanceID()}) - 다른 매니저 참조 시작");
        sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();
        inputManager = ServiceLocator.Get<InputManager>();

        if (inputManager != null)
        {
            inputManager.LinkToGameManager(this);
        }

        if (sceneTransitionManager == null)
        {
            Debug.LogError("[GameManager] Start()에서 SceneTransitionManager를 찾지 못했습니다!");
        }
        if (inputManager == null)
        {
            Debug.LogError("[GameManager] Start()에서 InputManager를 찾지 못했습니다!");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] GameManager (ID: {GetInstanceID()}) - OnDestroy() 호출됨.");
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (inputManager != null)
        {
            inputManager.UnlinkFromGameManager(this);
        }
    }

    private void OnEnable()
    {
        Debug.Log($"[생명주기] GameManager (ID: {GetInstanceID()}) - OnEnable() 호출됨.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] '{scene.name}' 씬 로드 완료. Mode: {mode}");
        if (CurrentState == GameState.Gameplay && mode == LoadSceneMode.Single)
        {
            StartCoroutine(StartRoundAfterSceneLoad());
        }
    }

    public void SetupForTest(CharacterDataSO character, int allocatedPoints)
    {
        Debug.Log($"[GameManager] 테스트 모드 설정: Character={character.characterId}, Points={allocatedPoints}");
        SelectedCharacter = character;
        AllocatedPoints = allocatedPoints;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState && CurrentState != GameState.Gameplay) return;
        Debug.Log($"[GameManager] 상태 변경: {CurrentState} -> {newState}");

        // 제안해주신 초기화 순서 보장 로직입니다.
        if (newState == GameState.PointAllocation)
        {
            // isFirstRound는 이제 PlayerDataManager가 관리하므로 GameManager는 신경쓰지 않아도 됩니다.
            // 바로 새 런 데이터를 초기화하도록 요청합니다.
            var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
            if (playerDataManager != null && SelectedCharacter != null)
            {
                playerDataManager.ResetRunData(SelectedCharacter);
            }
        }
        if (newState == GameState.Reward)
        {
            var rewardManager = ServiceLocator.Get<RewardManager>();
            var campaignManager = ServiceLocator.Get<CampaignManager>();
            var mapManager = ServiceLocator.Get<MapManager>();
            CampaignDataSO currentCampaign = campaignManager?.GetCurrentCampaign();

            if (rewardManager != null && rewardManager.LastRoundWon && mapManager != null && currentCampaign != null)
            {
                MapNode currentNode = mapManager.CurrentNode;
                int totalRounds = currentCampaign.rounds.Count;
                int currentRoundIndex = currentNode.Position.y;

                if (currentRoundIndex >= totalRounds - 1)
                {
                    Debug.Log("[GameManager] 최종 라운드 클리어! 게임 승리 상태로 전환합니다.");
                    ChangeState(GameState.GameWon);
                    return;
                }
            }
        }

        if (CurrentState == GameState.Gameplay && newState != GameState.Gameplay)
        {
            var cardManager = ServiceLocator.Get<CardManager>();
            cardManager?.StopCardSelectionLoop();

            var poolManager = ServiceLocator.Get<PoolManager>();
            if (poolManager != null)
            {
                Debug.Log($"[GameManager] Gameplay 씬을 떠나므로 모든 풀링된 오브젝트를 파괴합니다.");
                poolManager.ClearAndDestroyEntirePool();
            }
        }

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        if (newState == GameState.GameWon)
        {
            Debug.Log("[GameManager] 게임 승리! 엔딩 씬을 재생해야 하지만, 현재는 메인 메뉴로 바로 이동합니다.");
            // 나중에 엔딩 씬이 추가되면 아래 로직을 수정하여 엔딩 씬으로 이동시키세요.
            ChangeState(GameState.MainMenu);
            return;
        }
        else if (newState == GameState.Pause)
        {
            Time.timeScale = 0;
            return;
        }
        else if (newState == GameState.GameOver)
        {
            StartCoroutine(GameOverRoutine());
            return;
        }

        Time.timeScale = 1;

        string sceneName = GetSceneNameForState(newState);
        if (newState == GameState.Gameplay)
        {
            sceneName = SceneNames.GamePlay;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            sceneTransitionManager.LoadScene(sceneName);
        }
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
                        if (module is ProjectileEffectSO pModule)
                        {
                            AddOrUpdatePreloadRequest(pModule.bulletPrefabReference, card.CardData.preloadCount);
                        }
                    }
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

        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            cardManager.ClearAndResetDeck();
        }
        
        ChangeState(GameState.MainMenu);
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

    #region 체력 관리 메서드



    public void ModifyHealth(float amount)
    {
        if (lastPlayerHealth.HasValue)
        {
            lastPlayerHealth += amount;
            Debug.Log($"[GameManager] 외부에서 체력 변경: {amount}. 현재 저장된 체력: {lastPlayerHealth}");
        }
        else
        {
            Debug.LogWarning("[GameManager] 저장된 체력 정보가 없어 ModifyHealth를 수행할 수 없습니다.");
        }
    }


    #endregion
}
