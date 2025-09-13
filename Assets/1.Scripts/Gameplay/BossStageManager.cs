using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

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

    private BossStageDataSO _bossStageData;

    async void Start()
    {
        Debug.Log("[BossStageManager] Start() 함수 시작.");

        // --- 데이터 로드 ---
        var gameManager = ServiceLocator.Get<GameManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var mapManager = ServiceLocator.Get<MapManager>(); // MapManager 가져오기
        if (gameManager == null || campaignManager == null || mapManager == null)
        {
            Debug.LogError("[BossStageManager] GameManager, CampaignManager, 또는 MapManager를 찾을 수 없습니다!");
            return;
        }

        // CurrentNode를 GameManager 대신 MapManager에서 가져옵니다.
        RoundDataSO currentRoundData = campaignManager.GetRoundDataForNode(mapManager.CurrentNode);
        if (currentRoundData == null || currentRoundData.bossStageData == null)
        {
            Debug.LogError("[BossStageManager] 현재 라운드의 BossStageData를 찾을 수 없습니다! RoundDataSO에 BossStageData가 할당되었는지 확인하세요.");
            return;
        }
        _bossStageData = currentRoundData.bossStageData;
        Debug.Log($"[BossStageManager] 보스 스테이지 데이터 '{_bossStageData.name}' 로드 완료.");

        // --- 플레이어 생성 ---
        var playerObject = await Addressables.InstantiateAsync(playerPrefabRef, new Vector3(0, 0, 0), Quaternion.identity).Task;
        if (playerObject == null)
        {
            Debug.LogError("[BossStageManager] 플레이어 생성 실패!");
            return;
        }
        if (playerCamera != null) playerCamera.target = playerObject.transform;
        // PlayerController의 StartAutoAttackLoop는 PlayerInitializer가 담당하므로 여기서 호출하지 않습니다.
        playerObject.GetComponent<PlayerController>()?.StartAutoAttackLoop();


        // --- 보스 생성 ---
        GameObject bossObject = null;
        // BossStageDataSO에서 bossPrefab을 직접 가져와 사용합니다.
        if (_bossStageData.bossPrefab != null && _bossStageData.bossPrefab.RuntimeKeyIsValid())
        {
            bossObject = await Addressables.InstantiateAsync(_bossStageData.bossPrefab, new Vector3(2000, 0, 0), Quaternion.identity).Task;
            if (bossObject == null)
            {
                Debug.LogError("[BossStageManager] 보스 생성 실패!");
                return;
            }
            if (bossObject.TryGetComponent<BossController>(out var bossController))
            {
                bossController.Initialize(_bossStageData.bossCharacterData);
                bossController.StartAutoAttackLoop();
            }
            if (bossCamera != null) bossCamera.target = bossObject.transform;
        }
        else
        {
            Debug.LogError("[BossStageManager] BossStageData에 유효한 보스 프리팹이 설정되지 않았습니다!");
            return;
        }

        // --- 몬스터 스폰 시작 ---
        List<Wave> wavesToSpawn = _bossStageData.waves;
        if (wavesToSpawn == null || wavesToSpawn.Count == 0)
        {
            Debug.LogWarning("[BossStageManager] 보스 스테이지에 스폰할 몬스터 웨이브가 없습니다.");
        }

        // 플레이어 아레나 스포너 활성화 (플레이어를 공격)
        if (playerArenaSpawner != null && playerArenaBoundary != null)
        {
            Debug.Log("[BossStageManager] 플레이어 아레나 스포너를 활성화하고 스폰을 시작합니다. 공격 대상: 플레이어");
            playerArenaSpawner.mapBoundary = playerArenaBoundary;
            playerArenaSpawner.StartSpawning(wavesToSpawn, playerObject.transform, playerObject.transform);
        }

        // 보스 아레나 스포너 활성화 (보스를 공격)
        if (bossArenaSpawner != null && bossArenaBoundary != null)
        {
            Debug.Log("[BossStageManager] 보스 아레나 스포너를 활성화하고 스폰을 시작합니다. 공격 대상: 보스");
            bossArenaSpawner.mapBoundary = bossArenaBoundary;
            bossArenaSpawner.StartSpawning(wavesToSpawn, bossObject.transform, bossObject.transform);
        }
    }
}