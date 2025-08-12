using UnityEngine;
using System.Collections;

/// <summary>
/// 단일 라운드 내 몬스터의 배치, 스폰 위치, 주기를 관리합니다.
/// RoundManager의 제어를 받으며, 기획에 명시된 스폰 규칙을 따릅니다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject monsterPrefab; // 스폰할 몬스터 프리팹
    [SerializeField] private int spawnAmountPerBatch = 10; // 한 번에 스폰할 몬스터 수
    [SerializeField] private float spawnInterval = 10f; // 스폰 주기 (초)
    [SerializeField] private int maxBatches = 10; // 최대 스폰 횟수 (총 100마리)

    [Header("스폰 위치 설정")]
    [SerializeField] private Transform playerTransform; // 플레이어의 위치 참조
    [SerializeField] private float minSpawnRadius = 10f; // 플레이어로부터 최소 스폰 반경
    [SerializeField] private float maxSpawnRadius = 15f; // 플레이어로부터 최대 스폰 반경

    private Coroutine spawnCoroutine;
    private int batchesSpawned = 0;

    /// <summary>
    /// 몬스터 스폰 코루틴을 시작합니다. RoundManager에 의해 호출됩니다.
    /// </summary>
    public void StartSpawning()
    {
        // 이미 실행 중인 코루틴이 있다면 중지하고 새로 시작합니다.
        if (spawnCoroutine != null)
        { 
            StopCoroutine(spawnCoroutine);
        }
        batchesSpawned = 0;
        spawnCoroutine = StartCoroutine(SpawnRoutine());
        Debug.Log("몬스터 스폰을 시작합니다.");
    }

    /// <summary>
    /// 몬스터 스폰 코루틴을 중지합니다. RoundManager에 의해 호출됩니다.
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        { 
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            Debug.Log("몬스터 스폰을 중지합니다.");
        }
    }

    private IEnumerator SpawnRoutine()
    {
        // 기획서: 라운드 시작 1초 후 첫 스폰
        yield return new WaitForSeconds(1f);

        while (batchesSpawned < maxBatches)
        {
            SpawnBatch();
            batchesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
        Debug.Log("모든 몬스터 배치가 완료되었습니다.");
    }

    /// <summary>
    /// 한 무리의 몬스터를 스폰합니다.
    /// </summary>
    private void SpawnBatch()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("몬스터 프리팹이 할당되지 않았습니다!");
            return;
        }
        if (playerTransform == null)
        { 
            // 플레이어가 없을 경우를 대비한 안전장치. 실제로는 FindObjectOfType 등으로 찾아야 함.
            Debug.LogError("플레이어 위치가 할당되지 않았습니다!");
            return;
        }

        Debug.Log($"{spawnAmountPerBatch}마리의 몬스터를 스폰합니다.");
        for (int i = 0; i < spawnAmountPerBatch; i++)
        {
            // 플레이어 주변의 랜덤한 위치를 계산합니다.
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPosition = playerTransform.position + (Vector3)(randomDirection * randomDistance);

            GameObject monster = PoolManager.Instance.Get(monsterPrefab);
            monster.transform.position = spawnPosition;
            monster.transform.rotation = Quaternion.identity;

            // 기획서: 스폰 후 0.3초간 무적
            MonsterController mc = monster.GetComponent<MonsterController>();
            if (mc != null)
            {
                mc.SetInvulnerable(0.3f);
            }
        }
    }
}
