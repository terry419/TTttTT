using System.Collections;
using UnityEngine;
using System.Collections.Generic; // [1] using 문 추가
using System.Linq;                 // [2] using 문 추가

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

    // ▼▼▼ [3] Inspector에서 보상 카드 수를 설정할 변수 추가 ▼▼▼
    [Header("보상 설정")]
    [SerializeField] private int numberOfRewardChoices = 3;

    // --- Private State Fields --- //
    private MonsterSpawner monsterSpawner;
    private RoundDataSO currentRoundData;
    private int killCount;
    private float roundTimer;
    private bool isRoundActive;
    private Coroutine roundTimerCoroutine; // [1] 코루틴을 저장할 변수 추가

    // --- Unity Lifecycle Methods --- //
    void Awake()
    {
        monsterSpawner = GetComponent<MonsterSpawner>();
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: MonsterSpawner 컴포넌트를 찾을 수 없습니다! 몬스터가 스폰되지 않습니다.", this.gameObject);
        }
    }

    // ▼▼▼ [2] OnEnable, OnDisable 함수를 추가/수정하여 GameManager의 이벤트를 구독 ▼▼▼
    void OnEnable()
    {
        MonsterController.OnMonsterDied += HandleMonsterDied;
        
        // GameManager가 존재할 때만 이벤트를 구독하도록 예외 처리
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

    /// <summary>
    /// 새로운 라운드를 시작합니다.
    /// </summary>
    /// <param name="roundData">시작할 라운드의 데이터</param>
    public IEnumerator StartRound(RoundDataSO roundData)
    {
        if (isRoundActive)
        {
            Debug.LogWarning($"[{GetType().Name}] 경고: 이미 라운드가 진행 중일 때 StartRound가 호출되었습니다. 이전 라운드를 강제 종료하고 새 라운드를 시작합니다.");
            yield return StartCoroutine(EndRoundCoroutine(false));
        }

        currentRoundData = roundData;
        Debug.Log($"[{GetType().Name}] 새로운 라운드 시작: '{currentRoundData.name}' (목표 킬: {currentRoundData.killGoal}, 제한 시간: {currentRoundData.roundDuration}초)");

        // --- 플레이어 스탯 로그 (라운드 시작) ---
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            var playerStats = playerController.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("--- 라운드 시작 플레이어 스탯 ---");
                sb.AppendLine($"체력: {playerStats.currentHealth:F1} / {playerStats.FinalHealth:F1}");
                sb.AppendLine($"공격력 보너스: {playerStats.FinalDamageBonus:F2}%");
                sb.AppendLine($"공격 속도: {playerStats.FinalAttackSpeed:F2}");
                sb.AppendLine($"이동 속도: {playerStats.FinalMoveSpeed:F2}");
                sb.AppendLine($"치명타 확률: {playerStats.FinalCritRate:F2}%");
                sb.AppendLine($"치명타 피해: {playerStats.FinalCritDamage:F2}%");
                Debug.Log(sb.ToString());
            }
        }
        // --- 플레이어 스탯 로그 끝 ---

        // 라운드 상태 초기화
        killCount = 0;
        roundTimer = currentRoundData.roundDuration;
        isRoundActive = true;

        // 이벤트 구독
        // MonsterController.OnMonsterDied += HandleMonsterDied; // <-- 이 줄을 제거하세요!

        // UI 및 다른 시스템에 라운드 시작 알림
        OnRoundStarted?.Invoke(currentRoundData);

        // 몬스터 스폰 시작
        if (monsterSpawner != null)
        {
            // [수정] 사용자님이 공유해주신 RoundDataSO의 정확한 변수명인 'waves'를 사용합니다.
            monsterSpawner.StartSpawning(currentRoundData.waves);
        }

        // 라운드 타이머 코루틴 시작
        roundTimerCoroutine = StartCoroutine(RoundTimerCoroutine()); // 코루틴 참조를 저장
        yield return roundTimerCoroutine;
    }

    // --- Coroutines --- //

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
        roundTimerCoroutine = null; // 코루틴이 끝났으므로 참조를 비워줍니다.
        Debug.Log($"[{GetType().Name}] 라운드 종료 코루틴 시작. (승리: {wasKillGoalReached})");

        // --- 플레이어 스탯 로그 (라운드 종료) ---
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            var playerStats = playerController.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("--- 라운드 종료 플레이어 스탯 ---");
                sb.AppendLine($"체력: {playerStats.currentHealth:F1} / {playerStats.FinalHealth:F1}");
                sb.AppendLine($"공격력 보너스: {playerStats.FinalDamageBonus:F2}%");
                sb.AppendLine($"공격 속도: {playerStats.FinalAttackSpeed:F2}");
                sb.AppendLine($"이동 속도: {playerStats.FinalMoveSpeed:F2}");
                sb.AppendLine($"치명타 확률: {playerStats.FinalCritRate:F2}%");
                sb.AppendLine($"치명타 피해: {playerStats.FinalCritDamage:F2}%");
                Debug.Log(sb.ToString());
            }
        }
        // --- 플레이어 스탯 로그 끝 ---

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

        // ============ [핵심 추가 기능: 승리 시 보상 생성] ============
        if (wasKillGoalReached)
        {
            Debug.Log($"[{GetType().Name}] 라운드 승리! 카드 보상을 생성합니다.");
            var dataManager = ServiceLocator.Get<DataManager>();
            if (dataManager != null)
            {
                List<CardDataSO> allCards = dataManager.GetAllCards();
                List<CardDataSO> rewardChoices = new List<CardDataSO>();
                
                // 카드 데이터가 충분한지 확인
                if (allCards.Count >= numberOfRewardChoices)
                {
                    // 가중치에 따라 랜덤 카드 선택 (중복 없음)
                    List<CardDataSO> selectableCards = new List<CardDataSO>(allCards);
                    for (int i = 0; i < numberOfRewardChoices; i++)
                    {
                        if (selectableCards.Count == 0) break;

                        float totalWeight = selectableCards.Sum(card => card.rewardAppearanceWeight);

                        // ▼▼▼▼▼ [핵심 수정] 이 부분을 추가하세요 ▼▼▼▼▼
                        // 만약 모든 카드의 가중치가 0이라면, 가중치 없이 완전 랜덤으로 하나를 고릅니다.
                        if (totalWeight <= 0)
                        {
                            int randomIndex = Random.Range(0, selectableCards.Count);
                            rewardChoices.Add(selectableCards[randomIndex]);
                            selectableCards.RemoveAt(randomIndex);
                            continue; // 다음 카드를 뽑기 위해 for문의 다음 루프로 넘어갑니다.
                        }
                        // ▲▲▲▲▲ [여기까지 추가] ▲▲▲▲▲

                        float randomPoint = Random.Range(0, totalWeight);
                        float currentWeight = 0f;
                        CardDataSO selectedCard = null;

                        foreach (var card in selectableCards)
                        {
                            currentWeight += card.rewardAppearanceWeight;
                            if (randomPoint <= currentWeight)
                            {
                                selectedCard = card;
                                break;
                            }
                        }

                        // 만약 부동소수점 오류 등으로 선택이 안된 경우 마지막 카드를 선택
                        if (selectedCard == null && selectableCards.Count > 0)
                        {
                            selectedCard = selectableCards.Last();
                        }

                        if (selectedCard != null)
                        {
                            rewardChoices.Add(selectedCard);
                            selectableCards.Remove(selectedCard);
                        }
                    }
                }

                // 생성된 보상이 있으면 RewardManager의 대기열에 추가
                if (rewardChoices.Count > 0)
                {
                    rewardManager.EnqueueReward(rewardChoices);
                }
                else
                {
                    Debug.LogWarning($"[{GetType().Name}] 보상으로 제시할 카드를 생성하지 못했습니다. (카드 데이터 부족 가능성)");
                }
            }
        }
        else
        {
            Debug.Log($"[{GetType().Name}] 라운드 패배. 카드 보상을 생성하지 않습니다.");
        }
        // =============================================================
        }
        else
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: RewardManager를 찾을 수 없어 라운드 결과를 기록할 수 없습니다!");
        }

        // PoolManager를 통해 활성화된 모든 오브젝트(몬스터, 총알 등)를 정리합니다.
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager != null)
        {
            poolManager.ClearAllActiveObjects();
        }

        Debug.Log($"[{GetType().Name}] GameManager 상태 변경 요청: {GameManager.GameState.Reward}");
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Reward);
    }

    // --- Event Handlers --- //

    // ▼▼▼ [4] GameManager의 상태 변경을 감지할 핸들러 함수 추가 ▼▼▼
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // 게임 상태가 '게임오버'로 바뀌면, 라운드 매니저의 모든 활동을 즉시 중단시킵니다.
        if (newState == GameManager.GameState.GameOver)
        {
            Debug.Log($"[{GetType().Name}] 게임오버 상태를 감지했습니다. 라운드 타이머를 강제 종료합니다.");
            isRoundActive = false;
            
            // 실행 중인 타이머 코루틴이 있다면 중지시킵니다.
            if (roundTimerCoroutine != null)
            {
                StopCoroutine(roundTimerCoroutine);
                roundTimerCoroutine = null;
            }
        }
    }

    // --- Event Handlers --- //

    private void HandleMonsterDied(MonsterController monster)
    {
        if (!isRoundActive) return;

        killCount++;
        Debug.Log($"[{GetType().Name}] 몬스터 처치. 현재 킬 수: {killCount}/{currentRoundData.killGoal}");
        OnKillCountChanged?.Invoke(killCount, currentRoundData.killGoal);

        if (killCount >= currentRoundData.killGoal)
        {
            Debug.Log($"[{GetType().Name}] 목표 킬 수를 달성했습니다! 라운드를 승리로 종료합니다.");
            StartCoroutine(EndRoundCoroutine(true));
        }
    }
}