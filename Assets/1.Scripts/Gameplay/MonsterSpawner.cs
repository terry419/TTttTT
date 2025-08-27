using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 위치 설정")]
    private Transform playerTransform;
    [SerializeField] private float minSpawnRadius = 10f;
    [SerializeField] private float maxSpawnRadius = 15f;

    private Coroutine spawnCoroutine;

    // ★★★ 핵심 수정: StartSpawning의 인자를 List<Wave>로 수정 (Assets.txt 기반) ★★★
    public void StartSpawning(List<Wave> waves)
    {
        Debug.Log("[MonsterSpawner] StartSpawning called.");
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine(waves));
    }

    // ★★★ 핵심 수정: StopSpawning() 함수 복원 (CS1061 오류 해결) ★★★
    public void StopSpawning()
    {
        Debug.Log("[MonsterSpawner] StopSpawning called.");
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine(List<Wave> waves)
    {
        // --- 기존 디버그 로그 보존 ---
        Debug.Log($"[MonsterSpawner] 스폰 루틴 시작. 전달받은 웨이브 개수: {waves.Count}");

        // playerTransform을 찾을 때까지 대기
        while (playerTransform == null)
        {
            var playerController = ServiceLocator.Get<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                Debug.Log($"[MonsterSpawner] 성공: ServiceLocator를 통해 PlayerController를 찾아 playerTransform에 할당했습니다.");
            }
            else
            {
                // 아직 Player가 생성/등록되지 않았으면 한 프레임 대기 후 다시 시도
                yield return null; 
            }
        }

        yield return new WaitForSeconds(1f);

        foreach (var wave in waves)
        {
            if (wave.monsterData == null)
            {
                // --- 기존 디버그 로그 보존 ---
                Debug.LogWarning("Wave에 몬스터 데이터가 설정되지 않아 해당 웨이브를 건너뜁니다.");
                continue;
            }

            // ▼▼▼ 방어 코드 추가 ▼▼▼
            if (playerTransform == null)
            {
                Debug.LogWarning("[MonsterSpawner] Player가 파괴되어 스폰을 중단합니다.");
                yield break; // 코루틴 즉시 종료
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            switch (wave.spawnType)
            {
                case SpawnType.Spread:
                    float spawnInterval = (wave.count > 1 && wave.duration > 0) ? wave.duration / wave.count : 0.5f;
                    for (int i = 0; i < wave.count; i++)
                    {
                        SpawnMonster(wave.monsterData, playerTransform.position);
                        yield return new WaitForSeconds(spawnInterval);
                    }
                    yield return new WaitForSeconds(wave.delayAfterWave);
                    break;

                case SpawnType.Burst:
                    yield return new WaitForSeconds(wave.delayAfterWave);
                    for (int i = 0; i < wave.count; i++)
                    {
                        SpawnMonster(wave.monsterData, playerTransform.position);
                    }
                    break;
            }
        }
    }

    // ★★★ 핵심 수정: MonsterData를 MonsterDataSO로 변경 (CS0246 오류 해결) ★★★
    private async void SpawnMonster(MonsterDataSO monsterData, Vector3 center)
    {
        if (monsterData == null)
        {
            Debug.LogWarning("[MonsterSpawner] 스폰 실패! 전달된 MonsterDataSO가 null입니다.");
            return;
        }

        string key = monsterData.prefab.name;

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError($"[MonsterSpawner] 스폰 실패! '{monsterData.monsterName}' 데이터의 프리팹 이름이 유효하지 않습니다.");
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = center + (Vector3)(randomDirection * randomDistance);

        GameObject monsterInstance = await ServiceLocator.Get<PoolManager>().GetAsync(key);

        if (monsterInstance == null) return;
        monsterInstance.transform.position = spawnPosition;

        MonsterController mc = monsterInstance.GetComponent<MonsterController>();
        if (mc != null)
        {
            mc.Initialize(monsterData);
            mc.SetInvulnerable(0.3f);
        }
    }
}