// 파일명: GameManager.cs (리팩토링 완료)
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        Debug.Log($"[GameManager] Awake() 호출됨. (ID: {GetInstanceID()})");
        if (!ServiceLocator.IsRegistered<GameManager>())
        {
            ServiceLocator.Register<GameManager>(this);
            DontDestroyOnLoad(transform.root.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
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

    private void Start()
    {
        sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();
        if (sceneTransitionManager == null) Debug.LogError("!!! GameManager: SceneTransitionManager를 찾을 수 없음!!!");
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState && CurrentState != GameState.Gameplay) return;

        Debug.Log($"[GameManager] 상태 변경: {CurrentState} -> {newState}");
        CurrentState = newState;
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
            Debug.Log($"[GameManager] 씬 로드 요청: {sceneName}");
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

    /// <summary>
    /// 지정된 라운드 데이터와 현재 장착 카드를 기반으로 필요한 에셋만 동적으로 프리로드합니다.
    /// </summary>
    public IEnumerator PreloadAssetsForRound(RoundDataSO roundData, System.Action onComplete)
    {
        Debug.Log("--- [GameManager] v8.0 동적 리소스 프리로딩 시작 ---");
        var poolManager = ServiceLocator.Get<PoolManager>();
        var cardManager = ServiceLocator.Get<CardManager>();
        if (poolManager == null || cardManager == null)
        {
            Debug.LogError("[GameManager] PoolManager 또는 CardManager를 찾을 수 없습니다!");
            yield break;
        }

        var preloadRequests = new Dictionary<AssetReferenceGameObject, int>();

        // 헬퍼 함수: AssetReferenceGameObject에 대한 요청을 추가/갱신
        void AddOrUpdatePreloadRequest(AssetReferenceGameObject assetRef, int count)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid()) return;
            if (preloadRequests.ContainsKey(assetRef))
            {
                preloadRequests[assetRef] = Mathf.Max(preloadRequests[assetRef], count);
            }
            else
            {
                preloadRequests.Add(assetRef, count);
            }
        }

        // --- 데이터 수집 단계 ---

        // 1. 다음 라운드 몬스터 수집 (변경 없음)
        if (roundData != null && roundData.waves != null)
        {
            foreach (var wave in roundData.waves)
            {
                // AssetReference로 변경되었다면 이 부분도 수정 필요, 현재는 GameObject 직접 참조
                if (wave.monsterData != null && wave.monsterData.prefab != null)
                {
                    // poolManager.Preload는 GameObject를 받으므로 이 부분은 그대로 둡니다.
                }
            }
        }

        // 2. 현재 장착된 모든 카드(구버전, 신버전 모두)의 프리팹 수집
        // 현재 CardManager는 구버전 CardDataSO만 다루므로, 해당 로직은 유지합니다.
        foreach (var card in cardManager.equippedCards)
        {
            if (card.bulletPrefab != null) AddOrUpdatePreloadRequest(new AssetReferenceGameObject(card.bulletPrefab.name), card.bulletPreloadCount);
            if (card.effectPrefab != null) AddOrUpdatePreloadRequest(new AssetReferenceGameObject(card.effectPrefab.name), card.effectPreloadCount);
        }

        // [핵심] v8.0 NewCardDataSO의 모듈 프리팹 수집
        // TODO: CardManager가 NewCardDataSO를 관리하게 되면 아래 로직 활성화
        /*
        foreach (var newCard in cardManager.GetEquippedNewCards()) // 가상의 함수
        {
            foreach (var moduleEntry in newCard.modules)
            {
                if (moduleEntry.moduleReference.RuntimeKeyIsValid())
                {
                    var handle = moduleEntry.moduleReference.LoadAssetAsync<CardEffectSO>();
                    yield return handle;

                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result is IPreloadable preloadableModule)
                    {
                        foreach (var prefabRef in preloadableModule.GetPrefabsToPreload())
                        {
                            AddOrUpdatePreloadRequest(prefabRef, newCard.preloadCount);
                        }
                    }
                    moduleEntry.moduleReference.ReleaseAsset(); // 핸들 정리
                }
            }
        }
        */

        // --- 프리로드 실행 단계 ---
        Debug.Log($"[GameManager] 총 {preloadRequests.Count} 종류의 Addressable 프리팹에 대한 프리로드를 실행합니다.");
        foreach (var request in preloadRequests)
        {
            // Addressable 프리팹을 실제로 로드하여 PoolManager에 전달
            var loadHandle = request.Key.LoadAssetAsync<GameObject>();
            yield return loadHandle;

            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                poolManager.Preload(loadHandle.Result, request.Value);
            }
            else
            {
                Debug.LogError($"[GameManager] Addressable 프리팹 로딩 실패: {request.Key.AssetGUID}");
            }
            // 주의: 여기서 핸들을 바로 Release하면 PoolManager가 원본을 잃을 수 있으므로,
            // 씬이 끝날 때 일괄 해제하는 것이 더 안전합니다.
        }

        yield return null;
        Debug.Log("--- [GameManager] 동적 프리로딩 완료 ---");
        onComplete?.Invoke();
    }

    private IEnumerator GameOverRoutine()
    {
        // ▼▼▼ [추가] 씬을 전환하기 전에 모든 풀링된 오브젝트를 파괴합니다. ▼▼▼
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager != null)
        {
            poolManager.DestroyAllPooledObjects();
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        var popupController = ServiceLocator.Get<PopupController>();
        if (popupController != null)
        {
            popupController.ShowError("GAME OVER", 3f);
        }
        yield return new WaitForSecondsRealtime(3f); // Use real-time seconds
        Time.timeScale = 1; // Resume game time before changing scene

        // ▼▼▼ 메인 메뉴 씬으로 바꾸기 직전에 이 부분을 추가하세요! ▼▼▼
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

#if UNITY_EDITOR
        if (mapManager == null || !mapManager.IsMapInitialized)
        {
            Debug.LogWarning("[GameManager] 테스트 모드 감지: 필수 데이터 자동 설정 시작...");
            MapGenerator mapGenerator = FindObjectOfType<MapGenerator>();
            if (mapGenerator != null)
            {
                List<MapNode> mapData = mapGenerator.Generate();
                mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
                Debug.Log("[GameManager] 테스트용 맵 데이터 생성 및 초기화 완료.");
            }
            else
            {
                Debug.LogError("[GameManager] 테스트 모드 설정 실패: 씬에서 MapGenerator를 찾을 수 없습니다!");
                yield break;
            }

            if (SelectedCharacter == null)
            {
                SelectedCharacter = ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
                Debug.Log("[GameManager] 테스트용 기본 캐릭터 'warrior' 설정 완료.");
            }
            AllocatedPoints = 0;
            isFirstRound = true;
        }
#endif
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