// 파일 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterSpawner.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading; // SemaphoreSlim 사용을 위해 추가
using Cysharp.Threading.Tasks; // UniTask, SemaphoreSlim.WaitAsync 사용을 위해 추가

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 위치 설정")]
    [SerializeField] private float minSpawnRadius = 10f;
    [SerializeField] private float maxSpawnRadius = 15f;

    [Header("성능 최적화 설정")]
    [Tooltip("동시에 스폰(Instantiate) 요청을 보낼 최대 몬스터 수입니다. PoolManager의 과부하를 방지합니다.")]
    [SerializeField] private int maxConcurrentSpawns = 15;

    private Transform playerTransform;
    private MapBoundary mapBoundary; // [추가] 맵 경계 참조

    // WaveState 클래스를 도입하여 코루틴과 웨이브 데이터를 함께 관리합니다.
    private class WaveState
    {
        public Wave WaveData;
        public Coroutine Runner;
    }
    private readonly List<WaveState> _runningWaveStates = new List<WaveState>();
    private SemaphoreSlim _spawnLimiter; // 동시 스폰 수를 제어하기 위한 세마포

    public void StartSpawning(List<Wave> waves)
    {
        Debug.Log("[MonsterSpawner] StartSpawning 호출됨.");
        if (_runningWaveStates.Count > 0) StopSpawning();

        // 동시 스폰 제한을 위한 세마포 초기화
        _spawnLimiter = new SemaphoreSlim(maxConcurrentSpawns, maxConcurrentSpawns);

        StartCoroutine(SpawnRoutine(waves));
    }

    public void StopSpawning()
    {
        Debug.Log("[MonsterSpawner] StopSpawning 호출됨.");
        // 모든 실행 중인 코루틴 중지
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
        Debug.Log($"[MonsterSpawner] 스폰 루틴 시작. 전달받은 웨이브 개수: {waves.Count}");

        // playerTransform과 mapBoundary를 상위 코루틴에서 한 번만 찾습니다.
        while (playerTransform == null || mapBoundary == null)
        {
            if (playerTransform == null)
            {
                var playerController = ServiceLocator.Get<PlayerController>();
                if (playerController != null)
                {
                    playerTransform = playerController.transform;
                    Debug.Log("[MonsterSpawner] 성공: PlayerController를 찾아 playerTransform에 할당했습니다.");
                }
            }

            if (mapBoundary == null)
            {
                mapBoundary = FindObjectOfType<MapBoundary>();
                if (mapBoundary != null)
                {
                    Debug.Log("[MonsterSpawner] 성공: MapBoundary를 찾아 할당했습니다.");
                }
            }
            
            if (playerTransform == null || mapBoundary == null)
            {
                 yield return null;
            }
        }
        
        if (mapBoundary == null)
        {
            Debug.LogWarning("[MonsterSpawner] MapBoundary 컴포넌트를 찾지 못했습니다. 몬스터가 맵 경계 내에서 스폰되지 않을 수 있습니다.");
        }

        foreach (var wave in waves)
        {
            var newState = new WaveState { WaveData = wave };
            newState.Runner = StartCoroutine(SpawnSingleWaveRoutine(newState));
            _runningWaveStates.Add(newState);
        }
    }

    private IEnumerator SpawnSingleWaveRoutine(WaveState state)
    {
        try
        {
            // 1. 이 웨이브의 고유한 '시작 지연 시간'만큼 대기합니다.
            if (state.WaveData.delayAfterWave > 0)
            {
                yield return new WaitForSeconds(state.WaveData.delayAfterWave);
            }

            if (state.WaveData.monsterData == null)
            {
                Debug.LogWarning("Wave에 몬스터 데이터가 설정되지 않아 해당 웨이브를 건너뜁니다.");
                yield break;
            }

            Debug.Log($"[MonsterSpawner] '{state.WaveData.monsterData.name}' 웨이브 스폰 시작. (지연: {state.WaveData.delayAfterWave}초, 타입: {state.WaveData.spawnType}, 수량: {state.WaveData.count})");

            switch (state.WaveData.spawnType)
            {
                case SpawnType.Spread:
                    float spawnInterval = (state.WaveData.count > 1 && state.WaveData.duration > 0) ? state.WaveData.duration / (float)state.WaveData.count : 0.5f;
                    for (int i = 0; i < state.WaveData.count; i++)
                    {
                        if (playerTransform == null) yield break;
                        SpawnMonster(state.WaveData.monsterData, playerTransform.position);
                        yield return new WaitForSeconds(spawnInterval);
                    }
                    break;

                case SpawnType.Burst:
                    for (int i = 0; i < state.WaveData.count; i++)
                    {
                        if (playerTransform == null) yield break;
                        SpawnMonster(state.WaveData.monsterData, playerTransform.position);
                    }
                    break;
            }
        }
        finally
        {
            // 코루틴이 끝나면(성공하든, 중간에 멈추든) 반드시 리스트에서 자기 자신을 제거합니다.
            _runningWaveStates.Remove(state);
            Debug.Log($"[MonsterSpawner] '{state.WaveData.monsterData?.name ?? "Unknown"}' 웨이브 코루틴 완료. 남은 웨이브 코루틴: {_runningWaveStates.Count}개");
        }
    }

    private async void SpawnMonster(MonsterDataSO monsterData, Vector3 center)
    {
        // 스폰 제한 수에 도달하면 여기서 대기하여 PoolManager의 과부하를 막습니다.
        await _spawnLimiter.WaitAsync();

        try
        {
            if (monsterData == null || monsterData.prefabRef == null || !monsterData.prefabRef.RuntimeKeyIsValid())
            {
                Debug.LogError($"[MonsterSpawner] 스폰 실패! MonsterData 또는 PrefabRef가 유효하지 않습니다: '{monsterData?.monsterName ?? "NULL"}'");
                return;
            }
            string key = monsterData.prefabRef.AssetGUID;

            // 1. 플레이어 주변에 스폰 위치를 계산합니다. (기존 로직)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 spawnPosition = center + (Vector3)(randomDirection * randomDistance);

            // 2. [추가] 계산된 위치가 맵 경계를 벗어나지 않도록 위치를 보정(Clamp)합니다.
            if (mapBoundary != null)
            {
                Vector2 mapSize = mapBoundary.MapSize;
                float minX = -mapSize.x / 2;
                float maxX = mapSize.x / 2;
                float minY = -mapSize.y / 2;
                float maxY = mapSize.y / 2;

                spawnPosition.x = Mathf.Clamp(spawnPosition.x, minX, maxX);
                spawnPosition.y = Mathf.Clamp(spawnPosition.y, minY, maxY);
            }

            GameObject monsterInstance = await ServiceLocator.Get<PoolManager>().GetAsync(key);
            if (monsterInstance == null) return;

            var renderer = monsterInstance.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 20;
            }

            monsterInstance.transform.position = spawnPosition;

            MonsterController mc = monsterInstance.GetComponent<MonsterController>();
            if (mc != null)
            {
                mc.Initialize(monsterData);
                mc.SetInvulnerable(0.3f);
            }
        }
        finally
        {
            // 스폰 작업이 끝나면 제한 슬롯을 1개 반환하여 다른 대기 중인 스폰이 진행되도록 합니다.
            _spawnLimiter.Release();
        }
    }
}