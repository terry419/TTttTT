// ���� ���: Assets/1.Scripts/Gameplay/BossStageManager.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public class BossStageManager : MonoBehaviour
{
    [Header("ī�޶� ����")]
    [SerializeField] private CameraFollow playerCamera;
    [SerializeField] private CameraFollow bossCamera;

    [Header("������ ���")]
    [Tooltip("�÷��̾� �������� Addressable Ű(GUID)�� ���� ����")]
    [SerializeField] private AssetReferenceGameObject playerPrefabRef;
    [Tooltip("���� ������")]
    [SerializeField] private GameObject bossPrefab;
    [Tooltip("�������� ������ CharacterDataSO ����")]
    [SerializeField] private CharacterDataSO bossCharacterData;

    async void Start()
    {
        // --- [����� �α� 1] ---
        Debug.Log("[BossStageManager] Start() �Լ� ���� ����.");

        // --- �÷��̾� ���� ---
        var playerObject = await Addressables.InstantiateAsync(playerPrefabRef, new Vector3(0, 0, 0), Quaternion.identity).Task;

        // --- [����� �α� 2] ---
        if (playerObject != null)
        {
            Debug.Log($"[BossStageManager] �÷��̾� ���� ����: {playerObject.name}, ��ġ: {playerObject.transform.position}");
        }
        else
        {
            Debug.LogError("[BossStageManager] �÷��̾� ���� ����!");
            return; // �÷��̾� ���� ���� �� �ߴ�
        }

        if (playerCamera != null)
        {
            playerCamera.target = playerObject.transform;
            // --- [����� �α� 3] ---
            Debug.Log($"[BossStageManager] Player_Camera�� Target�� '{playerCamera.target.name}'���� ���� �Ϸ�.");
        }
        else
        {
            Debug.LogError("[BossStageManager] playerCamera ������ �����ϴ�!");
        }

        if (playerObject.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.StartAutoAttackLoop();
            Debug.Log("[BossStageManager] PlayerController�� �ڵ� ���� ������ �����մϴ�.");
        }


        // --- ���� ���� ---
        if (bossPrefab != null)
        {
            var bossObject = Instantiate(bossPrefab, new Vector3(2000, 0, 0), Quaternion.identity);

            // --- [����� �α� 4] ---
            if (bossObject != null)
            {
                Debug.Log($"[BossStageManager] ���� ���� ����: {bossObject.name}, ��ġ: {bossObject.transform.position}");
            }
            else
            {
                Debug.LogError("[BossStageManager] ���� ���� ����!");
                return; // ���� ���� ���� �� �ߴ�
            }

            if (bossObject.TryGetComponent<BossController>(out var bossController))
            {
                bossController.Initialize(bossCharacterData);
                // --- [�߰�] ���� ���� ���� ---
                bossController.StartAutoAttackLoop();
                Debug.Log("[BossStageManager] BossController�� �ڵ� ���� ������ �����մϴ�.");
            }

            if (bossCamera != null)
            {
                bossCamera.target = bossObject.transform;
                // --- [����� �α� 5] ---
                Debug.Log($"[BossStageManager] Boss_Camera�� Target�� '{bossCamera.target.name}'���� ���� �Ϸ�.");
            }
            else
            {
                Debug.LogError("[BossStageManager] bossCamera ������ �����ϴ�!");
            }
        }
    }
}