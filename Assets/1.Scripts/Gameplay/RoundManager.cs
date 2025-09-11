// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/RoundManager.cs
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 개별 전투 라운드의 시작, 진행, 종료를 관리하는 클래스입니다.
/// 몬스터 스폰, 킬 카운트, 제한 시간 등 라운드와 관련된 모든 핵심 로직을 담당합니다.
/// </summary>
public class RoundManager : MonoBehaviour
{
    // --- Events --- //
    public static event System.Action<RoundDataSO> OnRoundStarted;
    public static event System.Action<int, int> OnKillCountChanged;
    public static event System.Action<float> OnTimerChanged;
    public static event System.Action<bool> OnRoundEnded; // bool: 승리 여부

    [Header("보상 설정")]
    [SerializeField] private int numberOfRewardChoices = 3;

    // --- Private State Fields --- //
    private MonsterSpawner monsterSpawner;
    private RoundDataSO currentRoundData;
    private int killCount;
    private float roundTimer;
    private bool isRoundActive;
    private Coroutine roundTimerCoroutine;

    // --- Unity Lifecycle Methods --- //
    void Awake()
    {
        monsterSpawner = GetComponent<MonsterSpawner>();
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: MonsterSpawner 컴포넌트를 찾을 수 없습니다! 몬스터가 스폰되지 않습니다.", this.gameObject);
        }
    }

    void Start()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.ReportRoundManagerReady(this);
        }
        else
        {
            Debug.LogError($"[{GetType().Name}] GameManager를 찾을 수 없어 라운드를 시작할 수 없습니다!");
        }
    }

    void OnEnable()
    {
        MonsterController.OnMonsterDied += HandleMonsterDied;

        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    void OnDisable()
    {
        MonsterController.OnMonsterDied -= HandleMonsterDied;

        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    // --- Public Methods --- //
    public IEnumerator StartRound(RoundDataSO roundData)
    {
        if (isRoundActive)
        {
            Debug.LogWarning($"[{GetType().Name}] 경고: 이미 라운드가 진행 중일 때 StartRound가 호출되었습니다. 이전 라운드를 강제 종료하고 새 라운드를 시작합니다.");
            yield return StartCoroutine(EndRoundCoroutine(false));
        }

        currentRoundData = roundData;
        Debug.Log($"[{GetType().Name}] 새로운 라운드 시작: '{currentRoundData.name}' (목표 킬: {currentRoundData.killGoal}, 제한 시간: {currentRoundData.roundDuration}초)");

        var cardManager = ServiceLocator.Get<CardManager>();
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        if (cardManager != null && playerDataManager != null && playerDataManager.CurrentRunData != null)
        {
            // 카드 정보 로깅... (이전과 동일)
        }

        // --- 플레이어 스탯 로그 (라운드 시작) ---
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            var playerStats = playerController.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("--- 라운드 시작 플레이어 스탯 ---");
                sb.AppendLine($"체력: {playerStats.GetCurrentHealth():F1} / {playerStats.FinalHealth:F1}");
                sb.AppendLine($"공격력 보너스: {playerStats.FinalDamageBonus:F2}%");
                sb.AppendLine($"공격 속도: {playerStats.FinalAttackSpeed:F2}");
                sb.AppendLine($"이동 속도: {playerStats.FinalMoveSpeed:F2}");
                sb.AppendLine($"치명타 확률: {playerStats.FinalCritRate:F2}%");
                sb.AppendLine($"치명타 피해: {playerStats.FinalCritDamage:F2}%");
                Debug.Log(sb.ToString());
            }
        }
        // --- 플레이어 스탯 로그 끝 ---

        killCount = 0;
        roundTimer = currentRoundData.roundDuration;
        isRoundActive = true;
        OnRoundStarted?.Invoke(currentRoundData);

        // 몬스터 스폰 시작
        if (monsterSpawner != null)
        {
            // [수정] 위에서 선언한 playerController 변수를 그대로 사용합니다.
            if (playerController != null)
            {
                Transform playerTransform = playerController.transform;
                // 몬스터는 플레이어 주변에 생성되어 플레이어를 공격합니다.
                monsterSpawner.StartSpawning(currentRoundData.waves, playerTransform, playerTransform);
            }
            else
            {
                Debug.LogError("[RoundManager] PlayerController를 찾을 수 없어 몬스터 스폰을 시작할 수 없습니다!");
            }
        }

        roundTimerCoroutine = StartCoroutine(RoundTimerCoroutine());
        yield return roundTimerCoroutine;
    }

    private IEnumerator RoundTimerCoroutine()
    {
        Debug.Log($"[{GetType().Name}] 라운드 타이머 코루틴이 시작되었습니다.");
        while (roundTimer > 0 && isRoundActive)
        {
            roundTimer -= Time.deltaTime;
            OnTimerChanged?.Invoke(roundTimer);
            yield return null;
        }

        if (isRoundActive)
        {
            Debug.Log($"[{GetType().Name}] 시간 초과. 라운드를 종료합니다.");
            StartCoroutine(EndRoundCoroutine(false));
        }
    }

    private IEnumerator EndRoundCoroutine(bool wasKillGoalReached)
    {
        if (!isRoundActive) yield break;

        isRoundActive = false;
        roundTimerCoroutine = null;
        Debug.Log($"[{GetType().Name}] 라운드 종료 코루틴 시작. (승리: {wasKillGoalReached})");

        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            var playerStats = playerController.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("--- 라운드 종료 플레이어 스탯 ---");
                sb.AppendLine($"체력: {playerStats.GetCurrentHealth():F1} / {playerStats.FinalHealth:F1}");
                sb.AppendLine($"공격력 보너스: {playerStats.FinalDamageBonus:F2}%");
                sb.AppendLine($"공격 속도: {playerStats.FinalAttackSpeed:F2}");
                sb.AppendLine($"이동 속도: {playerStats.FinalMoveSpeed:F2}");
                sb.AppendLine($"치명타 확률: {playerStats.FinalCritRate:F2}%");
                sb.AppendLine($"치명타 피해: {playerStats.FinalCritDamage:F2}%");
                Debug.Log(sb.ToString());
            }
        }

        MonsterController.OnMonsterDied -= HandleMonsterDied;
        OnRoundEnded?.Invoke(wasKillGoalReached);

        if (monsterSpawner != null)
        {
            monsterSpawner.StopSpawning();
        }

        var rewardManager = ServiceLocator.Get<RewardManager>();
        if (rewardManager != null)
        {
            rewardManager.LastRoundWon = wasKillGoalReached;
            Debug.Log($"[{GetType().Name}] RewardManager에 라운드 결과({(wasKillGoalReached ? "승리" : "패배")})를 기록했습니다.");

            if (wasKillGoalReached)
            {
                Debug.Log($"[{GetType().Name}] 라운드 승리! 카드 보상을 생성합니다.");
                var rewardGenService = ServiceLocator.Get<RewardGenerationService>();
                if (rewardGenService != null)
                {
                    List<NewCardDataSO> rewardChoices = rewardGenService.GenerateRewards(numberOfRewardChoices);
                    if (rewardChoices != null && rewardChoices.Count > 0)
                    {
                        rewardManager.EnqueueReward(rewardChoices);
                        Debug.Log($"[{GetType().Name}] {rewardChoices.Count}개의 카드 보상을 생성하여 RewardManager에 추가했습니다.");
                    }
                    else
                    {
                        Debug.LogWarning($"[{GetType().Name}] 보상으로 제시할 카드를 생성하지 못했습니다.");
                    }
                }
                else
                {
                    Debug.LogError($"[{GetType().Name}] CRITICAL: RewardGenerationService를 찾을 수 없어 보상을 생성할 수 없습니다!");
                }
            }
            else
            {
                Debug.Log($"[{GetType().Name}] 라운드 패배. 카드 보상을 생성하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: RewardManager를 찾을 수 없어 라운드 결과를 기록할 수 없습니다!");
        }

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager != null)
        {
            poolManager.ReturnAllActiveObjectsToPool();
        }

        Debug.Log($"[{GetType().Name}] GameManager 상태 변경 요청: {GameManager.GameState.Reward}");
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Reward);
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.GameOver)
        {
            Debug.Log($"[{GetType().Name}] 게임오버 상태를 감지했습니다. 라운드 타이머를 강제 종료합니다.");
            isRoundActive = false;

            if (roundTimerCoroutine != null)
            {
                StopCoroutine(roundTimerCoroutine);
                roundTimerCoroutine = null;
            }
        }
    }

    private void HandleMonsterDied(MonsterController monster)
    {
        if (!isRoundActive) return;

        if (monster.countsTowardKillGoal)
        {
            killCount++;
            Log.Info(Log.LogCategory.GameManager, $"라운드 몬스터 처치. 현재 킬 수: {killCount}/{currentRoundData.killGoal}");
            OnKillCountChanged?.Invoke(killCount, currentRoundData.killGoal);

            if (killCount >= currentRoundData.killGoal)
            {
                Log.Info(Log.LogCategory.GameManager, "목표 킬 수를 달성했습니다! 라운드를 승리로 종료합니다.");
                StartCoroutine(EndRoundCoroutine(true));
            }
        }
        else
        {
            Log.Info(Log.LogCategory.GameManager, "소환된 하수인을 처치했습니다. (킬 카운트 미포함)");
        }
    }
    private void OnDestroy()
    {
        OnRoundStarted = null;
        OnKillCountChanged = null;
        OnTimerChanged = null;
        OnRoundEnded = null;
    }
}