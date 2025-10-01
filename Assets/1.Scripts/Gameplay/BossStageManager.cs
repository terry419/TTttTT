using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

public class BossStageManager : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private CameraFollow playerCamera;
    [SerializeField] private CameraFollow bossCamera;

    [Header("스폰 관련 설정")]
    [SerializeField] private AssetReferenceGameObject playerPrefabRef;
    [SerializeField] private MonsterSpawner playerArenaSpawner;
    [SerializeField] private MonsterSpawner bossArenaSpawner;
    [SerializeField] private MapBoundary playerArenaBoundary;
    [SerializeField] private MapBoundary bossArenaBoundary;

    // UI 및 데이터 제공을 위한 이벤트
    public static event Action<float> OnElapsedTimeChanged;
    public static event Action<int, int> OnKillCountsChanged;

    private BossStageDataSO _bossStageData;
    private GameObject _playerObject;
    private GameObject _bossObject;
    private EntityStats _bossStats;
    private bool _isStageEnded = false;

    // 실시간 상태 변수
    private float elapsedTime;
    private int playerArenaKills_Cumulative;
    private int bossArenaKills_Cumulative;

    // 상호작용 스폰용 변수
    private int playerArenaKills_Periodic;
    private int bossArenaKills_Periodic;
    private float reinforcementTimer;

    void OnEnable()
    {
        MonsterController.OnMonsterDied += HandleMonsterDied;
    }

    void OnDisable()
    {
        MonsterController.OnMonsterDied -= HandleMonsterDied;
    }

    async void Start()
    {
        Time.timeScale = 1f; // 스테이지 시작 시 시간 정상화

        // --- 데이터 로드 ---
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        RoundDataSO currentRoundData = campaignManager.GetRoundDataForNode(mapManager.CurrentNode);
        _bossStageData = currentRoundData.bossStageData;

        // --- 시스템 초기화 ---
        elapsedTime = 0f;
        playerArenaKills_Cumulative = 0;
        bossArenaKills_Cumulative = 0;
        OnKillCountsChanged?.Invoke(playerArenaKills_Cumulative, bossArenaKills_Cumulative);
        OnElapsedTimeChanged?.Invoke(elapsedTime);

        // 상호작용 스폰 시스템 초기화
        if (_bossStageData.reinforcementInterval > 0)
        {
            reinforcementTimer = _bossStageData.reinforcementInterval;
        }
        playerArenaKills_Periodic = 0;
        bossArenaKills_Periodic = 0;

        // --- 플레이어 및 보스 생성 (기존과 동일) ---
        _playerObject = await Addressables.InstantiateAsync(playerPrefabRef, new Vector3(0, 0, 0), Quaternion.identity).Task;
        if (playerCamera != null) playerCamera.target = _playerObject.transform;
        _playerObject.GetComponent<PlayerController>()?.StartAutoAttackLoop();

        _bossObject = await Addressables.InstantiateAsync(_bossStageData.bossPrefab, new Vector3(2000, 0, 0), Quaternion.identity).Task;
        _bossStats = _bossObject.GetComponent<EntityStats>();
        if (_bossObject.TryGetComponent<BossController>(out var bossController))
        {
            bossController.Initialize(_bossStageData.bossCharacterData);
            bossController.StartAutoAttackLoop();
        }
        if (bossCamera != null) bossCamera.target = _bossObject.transform;

        // --- 몬스터 스폰 시작 (기존과 동일) ---
        List<Wave> wavesToSpawn = _bossStageData.waves;
        if (wavesToSpawn != null && wavesToSpawn.Count > 0)
        {
            if (playerArenaSpawner != null && playerArenaBoundary != null)
            {
                playerArenaSpawner.mapBoundary = playerArenaBoundary;
                playerArenaSpawner.StartSpawning(wavesToSpawn, _playerObject.transform, _playerObject.transform);
            }
            if (bossArenaSpawner != null && bossArenaBoundary != null)
            {
                bossArenaSpawner.mapBoundary = bossArenaBoundary;
                bossArenaSpawner.StartSpawning(wavesToSpawn, _bossObject.transform, _bossObject.transform);
            }
        }
    }

    void Update()
    {
        if (_isStageEnded) return;

        // 1. 경과 시간 처리
        elapsedTime += Time.deltaTime;
        OnElapsedTimeChanged?.Invoke(elapsedTime);

        // 2. 주기적 증원 처리
        if (_bossStageData.reinforcementInterval > 0)
        {
            reinforcementTimer -= Time.deltaTime;
            if (reinforcementTimer <= 0f)
            {
                int reinforcementsForPlayer = Mathf.FloorToInt(bossArenaKills_Periodic * _bossStageData.reinforcementRatio);
                int reinforcementsForBoss = Mathf.FloorToInt(playerArenaKills_Periodic * _bossStageData.reinforcementRatio);

                if (reinforcementsForPlayer > 0 && playerArenaSpawner != null)
                {
                    playerArenaSpawner.SpawnReinforcements(reinforcementsForPlayer, _bossStageData.reinforcementMonsters);
                }
                if (reinforcementsForBoss > 0 && bossArenaSpawner != null)
                {
                    bossArenaSpawner.SpawnReinforcements(reinforcementsForBoss, _bossStageData.reinforcementMonsters);
                }

                playerArenaKills_Periodic = 0;
                bossArenaKills_Periodic = 0;
                reinforcementTimer = _bossStageData.reinforcementInterval;
            }
        }

        // 3. 승리 조건 확인
        if (_bossStats != null && _bossStats.CurrentHealth <= 0)
        {
            HandlePlayerVictory();
        }
    }

    private void HandleMonsterDied(MonsterController deadMonster)
    {
        if (_isStageEnded) return;

        bool isPlayerArena = deadMonster.transform.position.x < 1000;

        if (isPlayerArena)
        {
            playerArenaKills_Periodic++;
            playerArenaKills_Cumulative++;

            if (_bossStageData.milestoneKillTarget > 0 && playerArenaKills_Cumulative % _bossStageData.milestoneKillTarget == 0)
            {
                if (bossArenaSpawner != null && _bossStageData.specialMonsterData != null)
                {
                    bossArenaSpawner.SpawnSpecialMonster(_bossStageData.specialMonsterData);
                }
            }
        }
        else // Boss Arena
        {
            bossArenaKills_Periodic++;
            bossArenaKills_Cumulative++;

            if (_bossStageData.milestoneKillTarget > 0 && bossArenaKills_Cumulative % _bossStageData.milestoneKillTarget == 0)
            {
                if (playerArenaSpawner != null && _bossStageData.specialMonsterData != null)
                {
                    playerArenaSpawner.SpawnSpecialMonster(_bossStageData.specialMonsterData);
                }
            }
        }
        
        // 누적 킬 카운트 UI 업데이트 이벤트 호출
        OnKillCountsChanged?.Invoke(playerArenaKills_Cumulative, bossArenaKills_Cumulative);
    }

    private void HandlePlayerVictory()
    {
        if (_isStageEnded) return;
        _isStageEnded = true;

        if (playerArenaSpawner != null) playerArenaSpawner.StopSpawning();
        if (bossArenaSpawner != null) bossArenaSpawner.StopSpawning();

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager != null) poolManager.ReturnAllActiveObjectsToPool();

        var rewardManager = ServiceLocator.Get<RewardManager>();
        if (rewardManager != null) rewardManager.LastRoundWon = true;

        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Reward);
    }
}