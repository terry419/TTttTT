// 파일 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterSpawner.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 위치 설정")]
    [SerializeField] private float minSpawnRadius = 10f;
    [SerializeField] private float maxSpawnRadius = 15f;

    [Header("성능 최적화 설정")]
    [Tooltip("동시에 스폰(Instantiate) 요청을 보낼 최대 몬스터 수입니다. PoolManager의 과부하를 방지합니다.")]
    [SerializeField] private int maxConcurrentSpawns = 15;

    // [수정] Inspector에서 직접 할당하도록 public으로 변경
    [Header("맵 경계 (선택 사항)")]
    public MapBoundary mapBoundary;

    private Transform _spawnCenter;
    private Transform _target;

    private class WaveState
    {
        public Wave WaveData;
        public Coroutine Runner;
    }
    private readonly List<WaveState> _runningWaveStates = new List<WaveState>();
    private SemaphoreSlim _spawnLimiter;

    // [수정] StartSpawning 메서드가 spawnCenter와 target을 받도록 변경
    public void StartSpawning(List<Wave> waves, Transform spawnCenter, Transform target)
    {
        Debug.Log("[MonsterSpawner] StartSpawning 호출됨.");
        if (_runningWaveStates.Count > 0) StopSpawning();

        this._spawnCenter = spawnCenter;
        this._target = target;

        _spawnLimiter = new SemaphoreSlim(maxConcurrentSpawns, maxConcurrentSpawns);
        StartCoroutine(SpawnRoutine(waves));
    }

    public void StopSpawning()
    {
        Debug.Log("[MonsterSpawner] StopSpawning 호출됨.");
        foreach (var state in _runningWaveStates)
        {
            if (state.Runner != null)
            {
                StopCoroutine(state.Runner);
            }
        }
        _runningWaveStates.Clear();
    }

    private IEnumerator SpawnRoutine(List<Wave> waves)
    {
        Debug.Log($"[MonsterSpawner] 스폰 루틴 시작. 웨이브 개수: {waves.Count}");

        // [삭제] playerTransform과 mapBoundary를 찾는 로직을 제거합니다. (외부에서 주입받음)

        foreach (var wave in waves)
        {
            var newState = new WaveState { WaveData = wave };
            newState.Runner = StartCoroutine(SpawnSingleWaveRoutine(newState));
            _runningWaveStates.Add(newState);
        }
        yield return null; // Coroutine으로 유지하기 위해 추가
    }

    private IEnumerator SpawnSingleWaveRoutine(WaveState state)
    {
        try
        {
            if (state.WaveData.delayAfterWave > 0)
            {
                yield return new WaitForSeconds(state.WaveData.delayAfterWave);
            }

            if (state.WaveData.monsterData == null)
            {
                Debug.LogWarning("Wave에 몬스터 데이터가 설정되지 않아 해당 웨이브를 건너뜁니다.");
                yield break;
            }

            switch (state.WaveData.spawnType)
            {
                case SpawnType.Spread:
                    float spawnInterval = (state.WaveData.count > 1 && state.WaveData.duration > 0) ? state.WaveData.duration / (float)state.WaveData.count : 0.5f;
                    for (int i = 0; i < state.WaveData.count; i++)
                    {
                        if (_spawnCenter == null) yield break;
                        // [수정] 스폰 위치 기준을 _spawnCenter로, 공격 대상을 _target으로 전달
                        SpawnMonster(state.WaveData.monsterData, _spawnCenter.position, _target);
                        yield return new WaitForSeconds(spawnInterval);
                    }
                    break;

                case SpawnType.Burst:
                    for (int i = 0; i < state.WaveData.count; i++)
                    {
                        if (_spawnCenter == null) yield break;
                        // [수정] 스폰 위치 기준을 _spawnCenter로, 공격 대상을 _target으로 전달
                        SpawnMonster(state.WaveData.monsterData, _spawnCenter.position, _target);
                    }
                    break;
            }
        }
        finally
        {
            _runningWaveStates.Remove(state);
        }
    }

    // [수정] SpawnMonster 메서드에 target 매개변수 추가
    private async void SpawnMonster(MonsterDataSO monsterData, Vector3 center, Transform target)
    {
        await _spawnLimiter.WaitAsync();
        try
        {
            if (monsterData == null || !monsterData.prefabRef.RuntimeKeyIsValid())
            {
                Debug.LogError($"[MonsterSpawner] 스폰 실패! MonsterData 또는 PrefabRef가 유효하지 않습니다.");
                return;
            }
            string key = monsterData.prefabRef.AssetGUID;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPosition = center + (Vector3)(randomDirection * randomDistance);

            if (mapBoundary != null)
            {
                Vector2 mapSize = mapBoundary.MapSize;
                // [수정] 맵 경계 계산 시 MapBoundary의 월드 위치를 중심으로 계산합니다.
                Vector3 boundaryCenter = mapBoundary.transform.position;
                float minX = boundaryCenter.x - mapSize.x / 2;
                float maxX = boundaryCenter.x + mapSize.x / 2;
                float minY = boundaryCenter.y - mapSize.y / 2;
                float maxY = boundaryCenter.y + mapSize.y / 2;

                spawnPosition.x = Mathf.Clamp(spawnPosition.x, minX, maxX);
                spawnPosition.y = Mathf.Clamp(spawnPosition.y, minY, maxY);
            }

            GameObject monsterInstance = await ServiceLocator.Get<PoolManager>().GetAsync(key);
            if (monsterInstance == null) return;

            monsterInstance.transform.position = spawnPosition;

            var renderer = monsterInstance.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 20;
            }

            MonsterController mc = monsterInstance.GetComponent<MonsterController>();
            if (mc != null)
            {

                // [수정] Initialize 호출 시 target을 전달
                mc.Initialize(monsterData, target);
                mc.SetInvulnerable(0.3f);
            }
        }
        finally
        {
            _spawnLimiter.Release();
        }
    }
}