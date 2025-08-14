// --- 파일명: MonsterSpawner.cs ---
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

    public void SpawnBurstAt(string monsterName, int count, Vector3 centerPosition)
    {
        Debug.Log($"[MonsterSpawner] 이벤트 발생! {centerPosition}에 {monsterName} {count}마리 즉시 소환.");
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterName, centerPosition);
        }
    }

    public void StartSpawning(List<Wave> waves)
    {
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine(waves));
        Debug.Log("몬스터 스폰을 시작합니다.");
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // [수정] 이 함수 전체를 아래 내용으로 교체해 줘.
    private IEnumerator SpawnRoutine(List<Wave> waves)
    {
        yield return new WaitForSeconds(1f); // 라운드 시작 후 최초 대기

        foreach (var wave in waves)
        {
            switch (wave.spawnType)
            {
                case SpawnType.Spread:
                    // Spread 타입은 스폰이 끝난 후 딜레이
                    float spawnInterval = (wave.count > 1 && wave.duration > 0) ? wave.duration / wave.count : 0.5f;
                    for (int i = 0; i < wave.count; i++)
                    {
                        SpawnMonster(wave.monsterName, playerTransform.position);
                        yield return new WaitForSeconds(spawnInterval);
                    }
                    yield return new WaitForSeconds(wave.delayAfterWave);
                    break;

                case SpawnType.Burst:
                    // Burst 타입은 딜레이 후 한 번에 스폰
                    yield return new WaitForSeconds(wave.delayAfterWave); // 먼저 딜레이만큼 기다림!

                    Debug.Log($"[MonsterSpawner] 버스트 스폰! {wave.monsterName} {wave.count}마리 즉시 소환.");
                    for (int i = 0; i < wave.count; i++)
                    {
                        SpawnMonster(wave.monsterName, playerTransform.position);
                    }
                    break;
            }
        }
        Debug.Log("모든 웨이브가 완료되었습니다.");
    }

    private void SpawnMonster(string monsterName, Vector3 center)
    {
        GameObject monsterPrefab = DataManager.Instance.GetMonsterPrefab(monsterName);
        if (monsterPrefab == null)
        {
            Debug.LogWarning($"프리팹 DB에서 '{monsterName}'을 찾을 수 없습니다.");
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = center + (Vector3)(randomDirection * randomDistance);

        GameObject monster = PoolManager.Instance.Get(monsterPrefab);
        monster.transform.position = spawnPosition;

        MonsterController mc = monster.GetComponent<MonsterController>();
        if (mc != null) mc.SetInvulnerable(0.3f);
    }
}