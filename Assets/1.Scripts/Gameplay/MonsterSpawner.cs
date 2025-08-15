// --- 파일명: MonsterSpawner.cs (오류 수정) ---
// 경로: Assets/1.Scripts/Gameplay/MonsterSpawner.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 위치 설정")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minSpawnRadius = 10f;
    [SerializeField] private float maxSpawnRadius = 15f;

    private Coroutine spawnCoroutine;

    public void StartSpawning(List<Wave> waves)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine(waves));
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnRoutine(List<Wave> waves)
    {
        yield return new WaitForSeconds(1f);

        foreach (var wave in waves)
        {
            if (wave.monsterData == null)
            {
                Debug.LogWarning("Wave에 몬스터 데이터가 설정되지 않아 해당 웨이브를 건너뜁니다.");
                continue;
            }

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

    private void SpawnMonster(MonsterDataSO monsterData, Vector3 center)
    {
        if (monsterData == null)
        {
            Debug.LogWarning("[MonsterSpawner] 스폰 실패! 전달된 MonsterDataSO가 null입니다.");
            return;
        }

        // [수정] monsterData.prefabName 대신 monsterData.prefab을 직접 사용합니다.
        GameObject monsterPrefab = monsterData.prefab;

        if (monsterPrefab == null)
        {
            Debug.LogError($"[MonsterSpawner] 스폰 실패! '{monsterData.monsterName}' 데이터에 프리팹이 연결되지 않았습니다.");
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = center + (Vector3)(randomDirection * randomDistance);

        GameObject monsterInstance = PoolManager.Instance.Get(monsterPrefab);
        monsterInstance.transform.position = spawnPosition;

        MonsterController mc = monsterInstance.GetComponent<MonsterController>();
        if (mc != null)
        {
            mc.Initialize(monsterData);
            mc.SetInvulnerable(0.3f);
        }
    }
}
