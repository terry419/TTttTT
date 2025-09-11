// 파일 경로: Assets/1.Scripts/Gameplay/BossStageManager.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class BossStageManager : MonoBehaviour
{
    [Header("카메라 참조")]
    [SerializeField] private CameraFollow playerCamera;
    [SerializeField] private CameraFollow bossCamera;

    [Header("스폰할 대상")]
    [Tooltip("플레이어 프리팹의 Addressable 키(GUID)를 가진 에셋")]
    [SerializeField] private AssetReferenceGameObject playerPrefabRef;
    [Tooltip("보스 프리팹")]
    [SerializeField] private GameObject bossPrefab;
    [Tooltip("보스에게 적용할 CharacterDataSO 에셋")]
    [SerializeField] private CharacterDataSO bossCharacterData;

    async void Start()
    {
        // --- [디버그 로그 1] ---
        Debug.Log("[BossStageManager] Start() 함수 실행 시작.");

        // --- 플레이어 스폰 ---
        var playerObject = await Addressables.InstantiateAsync(playerPrefabRef, new Vector3(0, 0, 0), Quaternion.identity).Task;

        // --- [디버그 로그 2] ---
        if (playerObject != null)
        {
            Debug.Log($"[BossStageManager] 플레이어 스폰 성공: {playerObject.name}, 위치: {playerObject.transform.position}");
        }
        else
        {
            Debug.LogError("[BossStageManager] 플레이어 스폰 실패!");
            return; // 플레이어 생성 실패 시 중단
        }

        if (playerCamera != null)
        {
            playerCamera.target = playerObject.transform;
            // --- [디버그 로그 3] ---
            Debug.Log($"[BossStageManager] Player_Camera의 Target을 '{playerCamera.target.name}'으로 설정 완료.");
        }
        else
        {
            Debug.LogError("[BossStageManager] playerCamera 참조가 없습니다!");
        }

        if (playerObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.StartAutoAttackLoop();
            Debug.Log("[BossStageManager] PlayerController의 자동 공격 루프를 시작합니다.");
        }


        // --- 보스 스폰 ---
        if (bossPrefab != null)
        {
            var bossObject = Instantiate(bossPrefab, new Vector3(2000, 0, 0), Quaternion.identity);

            // --- [디버그 로그 4] ---
            if (bossObject != null)
            {
                Debug.Log($"[BossStageManager] 보스 스폰 성공: {bossObject.name}, 위치: {bossObject.transform.position}");
            }
            else
            {
                Debug.LogError("[BossStageManager] 보스 스폰 실패!");
                return; // 보스 생성 실패 시 중단
            }

            if (bossObject.TryGetComponent<BossController>(out var bossController))
            {
                bossController.Initialize(bossCharacterData);
                // --- [추가] 보스 공격 시작 ---
                bossController.StartAutoAttackLoop();
                Debug.Log("[BossStageManager] BossController의 자동 공격 루프를 시작합니다.");
            }

            if (bossCamera != null)
            {
                bossCamera.target = bossObject.transform;
                // --- [디버그 로그 5] ---
                Debug.Log($"[BossStageManager] Boss_Camera의 Target을 '{bossCamera.target.name}'으로 설정 완료.");
            }
            else
            {
                Debug.LogError("[BossStageManager] bossCamera 참조가 없습니다!");
            }
        }
    }
}