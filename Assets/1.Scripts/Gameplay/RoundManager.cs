// --- 파일명: RoundManager.cs (수정본 2) ---
// 역할: 라운드의 시작, 진행, 종료 등 핵심 게임 흐름을 관리합니다.
// 수정 내용: 2라운드부터 UI가 제대로 초기화되지 않는 문제를 해결하기 위해,
//           라운드 시작 시점에 명확한 '방송(이벤트)'을 보내도록 수정했습니다.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    // [리팩토링 3-1] '라운드 시작' 전용 방송 채널 추가
    // 라운드에 필요한 모든 초기 정보(지속 시간, 목표 킬 수 등)를 담아서 방송합니다.
    public static event System.Action<RoundDataSO> OnRoundStarted;

    public static event System.Action<int, int> OnKillCountChanged;
    public static event System.Action<float> OnTimerChanged;
    public static event System.Action<bool> OnRoundEnded;

    [System.Serializable]
    public class PreloadItem
    {
        public GameObject prefab;
        public int count;
    }

    [Header("사전 로드 설정 (수동)")]
    [SerializeField] private List<PreloadItem> monsterPreloads;
    [SerializeField] private List<PreloadItem> effectPreloads;

    private MonsterSpawner monsterSpawner;
    private RoundDataSO currentRoundData;
    private float remainingTime;
    private int killCount;
    private bool isRoundActive = false;

    void OnEnable()
    {
        MonsterController.OnMonsterDied += HandleMonsterDied;
        Debug.Log("[RoundManager] OnMonsterDied 이벤트 구독 완료.");
    }

    void OnDisable()
    {
        MonsterController.OnMonsterDied -= HandleMonsterDied;
        Debug.Log("[RoundManager] OnMonsterDied 이벤트 구독 해지 완료.");
    }

    private void DoPreload()
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;
        foreach (var item in monsterPreloads) poolManager.Preload(item.prefab, item.count);
        foreach (var item in effectPreloads) poolManager.Preload(item.prefab, item.count);
    }

    public IEnumerator StartRound(RoundDataSO roundDataToStart)
    {
        Debug.Log("[RoundManager] 라운드 시작 프로세스...");

        // 라운드 시작 시점에 프리로딩을 먼저 수행하여 타이밍 문제를 해결합니다.
        DoPreload();

        while (monsterSpawner == null)
        {
            monsterSpawner = FindObjectOfType<MonsterSpawner>();
            if (monsterSpawner == null)
            {
                Debug.LogWarning("[RoundManager] MonsterSpawner를 아직 찾지 못했습니다. 1프레임 대기합니다.");
                yield return null;
            }
        }
        Debug.Log("[RoundManager] 모든 컴포넌트 참조 완료. 라운드를 시작합니다.");

        currentRoundData = roundDataToStart;
        remainingTime = currentRoundData.roundDuration;
        killCount = 0;
        isRoundActive = true;

        // [타이밍 문제 해결 1] 여기서 한 프레임을 기다립니다!
        // 이 한 프레임 동안 씬에 새로 생긴 HUDController 같은 객체들이
        // Awake()와 OnEnable()을 실행하여 방송 들을 준비를 마칠 시간을 줍니다.
        yield return null;

        Debug.Log($"[RoundManager] OnRoundStarted 이벤트 방송 시도: {currentRoundData.name}");
        OnRoundStarted?.Invoke(currentRoundData);

        monsterSpawner.StartSpawning(currentRoundData.waves);
    }

    private void HandleMonsterDied(MonsterController monster)
    {
        if (!isRoundActive) return;
        killCount++;

        // 킬 카운트가 변경될 때마다 방송하는 것은 그대로 유지합니다.
        Debug.Log($"[RoundManager] 몬스터 사망으로 OnKillCountChanged 이벤트 방송 시도: {killCount} / {currentRoundData.killGoal}");
        OnKillCountChanged?.Invoke(killCount, currentRoundData.killGoal);

        if (killCount >= currentRoundData.killGoal) StartCoroutine(EndRoundCoroutine(true));
    }

    void Update()
    {
        if (!isRoundActive) return;
        remainingTime -= Time.deltaTime;

        // 타이머가 변경될 때마다 방송하는 것도 그대로 유지합니다.
        OnTimerChanged?.Invoke(remainingTime);

        if (remainingTime <= 0)
        {
            remainingTime = 0;
            StartCoroutine(EndRoundCoroutine(false));
        }
    }

    // --- 이하 코드는 변경 없음 ---
    private IEnumerator EndRoundCoroutine(bool wasKillGoalReached)
    {
        if (!isRoundActive) yield break;

        isRoundActive = false;
        Debug.Log($"[RoundManager] 라운드 종료. (킬 수 달성: {wasKillGoalReached})");
        OnRoundEnded?.Invoke(wasKillGoalReached);

        if (monsterSpawner != null) monsterSpawner.StopSpawning();

        yield return StartCoroutine(CleanupAllMonsters());
        yield return StartCoroutine(CleanupAllBullets());
        yield return StartCoroutine(CleanupAllVFX());

        if (wasKillGoalReached)
        {
            GenerateCardReward();
            // 모든 정리가 끝난 후, 안전하게 씬 전환을 요청합니다.
            ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Reward);
        }
        else
        {
            ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
        }
    }
    private void GenerateCardReward() {/*...*/}
    private IEnumerator CleanupAllMonsters()
    {
        // 씬에 있는 모든 MonsterController를 찾아서 풀에 반환합니다.
        foreach (var monster in FindObjectsOfType<MonsterController>())
        {
            ServiceLocator.Get<PoolManager>().Release(monster.gameObject);
        }
        yield return null; // 모든 작업이 반영되도록 한 프레임 대기합니다.
    }

    private IEnumerator CleanupAllBullets()
    {
        // 씬에 있는 모든 BulletController를 찾아서 풀에 반환합니다.
        foreach (var bullet in FindObjectsOfType<BulletController>())
        {
            ServiceLocator.Get<PoolManager>().Release(bullet.gameObject);
        }
        yield return null;
    }

    private IEnumerator CleanupAllVFX()
    {
        // 씬에 있는 모든 DamagingZone(VFX)을 찾아서 풀에 반환합니다.
        foreach (var vfx in FindObjectsOfType<DamagingZone>())
        {
            ServiceLocator.Get<PoolManager>().Release(vfx.gameObject);
        }
        yield return null;
    }
}