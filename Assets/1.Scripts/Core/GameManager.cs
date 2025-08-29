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
    private void OnDestroy()
    {
        Debug.Log($"[생명주기] GameManager (ID: {GetInstanceID()}) - OnDestroy() 호출됨.");
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        CurrentState = newState;

        inputManager ??= ServiceLocator.Get<InputManager>();
        if (inputManager != null)
        {
            Debug.Log($"[INPUT TRACE] GameManager.ChangeState: InputManager에게 입력 모드 변경 알림 전송. (요청 모드: {(newState == GameState.Gameplay ? "Gameplay" : "UI")})");
            if (newState == GameManager.GameState.Gameplay)
            {
                inputManager.EnableGameplayControls();
            }
            else
            {
                inputManager.EnableUIControls();
                Debug.LogError("[INPUT TRACE] GameManager.ChangeState: InputManager를 찾을 수 없어 입력 모드를 변경할 수 없습니다!");
            }
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
            Debug.Log($"[INPUT TRACE] GameManager.ChangeState: SceneTransitionManager에게 '{sceneName}' 씬 로드 요청.");
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
            poolManager.DestroyAllPooledObjects();
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

}