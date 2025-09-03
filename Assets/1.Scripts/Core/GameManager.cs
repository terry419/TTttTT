// 파일명: GameManager.cs (리팩토링 완료)
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks; 
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public enum GameState { MainMenu, CharacterSelect, PointAllocation, Gameplay, Reward, Pause, Codex, GameOver, Shop, Rest, Event }
    public GameState CurrentState { get; private set; }
    public CharacterDataSO SelectedCharacter { get; set; }
    public int AllocatedPoints { get; set; }
    public bool isFirstRound = true;

    // 라운드 간 플레이어 체력을 보존하기 위한 변수
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

            // 보상 생성 서비스가 없다면 동적으로 생성
            if (!ServiceLocator.IsRegistered<RewardGenerationService>())
            {
                GameObject serviceGO = new GameObject("RewardGenerationService");
                serviceGO.AddComponent<RewardGenerationService>();
                // DontDestroyOnLoad는 RewardGenerationService의 Awake에서 처리됩니다.
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
        Debug.Log($"[GameManager] OnEnable() 호출됨. (ID: {GetInstanceID()})");
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
        isFirstRound = true;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState && CurrentState != GameState.Gameplay) return;
        Debug.Log($"[GameManager] 상태 변경: {CurrentState} -> {newState}");

        // Gameplay 상태를 벗어날 때 루프 중지 및 풀 정리
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

        if (inputManager == null)
        {
            Debug.LogWarning("[GameManager] InputManager가 아직 참조되지 않았습니다. 다음 프레임에 처리될 예정입니다.");
        }

        OnGameStateChanged?.Invoke(newState);

        if (newState == GameState.Pause)
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
        var cardManager = ServiceLocator.Get<CardManager>();
        var prefabProvider = ServiceLocator.Get<PrefabProvider>();
        var resourceManager = ServiceLocator.Get<ResourceManager>(); // ResourceManager 참조

        // ... (기존의 null 체크 로직) ...

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


        foreach (var card in cardManager.equippedCards) // equippedCards는 이제 CardInstance 리스트입니다.
        {
            foreach (var moduleEntry in card.CardData.modules) // card.CardData.modules로 접근
            {
                if (moduleEntry.moduleReference.RuntimeKeyIsValid())
                {
                    CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);
                    if (module is ProjectileEffectSO pModule)
                    {
                        // card.preloadCount -> card.CardData.preloadCount로 접근
                        AddOrUpdatePreloadRequest(pModule.bulletPrefabReference, card.CardData.preloadCount);
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
        yield return new WaitForSecondsRealtime(3f); // Use real-time seconds
        Time.timeScale = 1; // Resume game time before changing scene

        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            cardManager.ClearAndResetDeck(); // 카드 매니저 초기화
        }

        // 만약 ArtifactManager도 초기화해야 한다면 비슷한 함수를 만들어 호출합니다.
        // var artifactManager = ServiceLocator.Get<ArtifactManager>();
        // if (artifactManager != null)
        // {
        //     artifactManager.ClearAndResetArtifacts(); 
        // }

        isFirstRound = true; // '첫 라운드'라는 표시도 다시 true로!
        ResetSavedHealth(); // 저장된 체력 정보 리셋

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

    /// <summary>
    /// 다른 씬(이벤트, 상점 등)에서 플레이어의 현재 체력을 가져옵니다.
    /// </summary>
    public float? GetCurrentHealth()
    {
        return lastPlayerHealth;
    }

    /// <summary>
    /// 다른 씬에서 플레이어의 체력을 특정 값으로 설정합니다.
    /// </summary>
    public void SetCurrentHealth(float health)
    {
        lastPlayerHealth = health;
        Debug.Log($"[DEBUG-HEALTH] GameManager.SetCurrentHealth: 체력 저장 요청을 받았습니다. 저장된 체력: {lastPlayerHealth}");
    }

    /// <summary>
    /// 다른 씬에서 플레이어의 체력을 회복하거나 감소시킵니다.
    /// </summary>
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

    /// <summary>
    /// 게임 오버 또는 새 게임 시작 시, 저장된 체력 정보를 리셋합니다.
    /// </summary>
    public void ResetSavedHealth()
    {
        lastPlayerHealth = null;
    }

    #endregion
}