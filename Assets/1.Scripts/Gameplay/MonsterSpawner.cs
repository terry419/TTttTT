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
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
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

    private IEnumerator SpawnRoutine(List<Wave> waves)
    {
        yield return new WaitForSeconds(1f);

        foreach (var wave in waves)
        {
            for (int i = 0; i < wave.count; i++)
            {
                // [수정] wave.monsterPrefab -> wave.monsterName 으로 변경
                SpawnMonster(wave.monsterName);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
            yield return new WaitForSeconds(wave.delayAfterWave);
        }
        Debug.Log("모든 웨이브가 완료되었습니다.");
    }

    private void SpawnMonster(string monsterName)
    {
        GameObject monsterPrefab = DataManager.Instance.GetMonsterPrefab(monsterName);

        if (monsterPrefab == null || playerTransform == null)
        {
            Debug.LogWarning($"프리팹 DB에서 '{monsterName}'을 찾을 수 없습니다.");
            return;
        }

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector3 spawnPosition = playerTransform.position + (Vector3)(randomDirection * randomDistance);

        GameObject monster = PoolManager.Instance.Get(monsterPrefab);
        monster.transform.position = spawnPosition;

        MonsterController mc = monster.GetComponent<MonsterController>();
        if (mc != null) mc.SetInvulnerable(0.3f);
    }
}