using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic; // List 사용을 위해 추가


[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : MonoBehaviour
{
    // ... (기존 변수 선언은 그대로) ...
    [Header("기본 능력치")]
    public BaseStats stats;

    [Header("카드 효과 비율")]
    public float cardDamageRatio;
    public float cardAttackSpeedRatio;
    public float cardMoveSpeedRatio;
    public float cardHealthRatio;
    public float cardCritRateRatio;
    public float cardCritDamageRatio;

    [Header("유물 효과 비율")]
    public float artifactDamageRatio;
    public float artifactAttackSpeedRatio;
    public float artifactMoveSpeedRatio;
    public float artifactHealthRatio;
    public float artifactCritRateRatio;
    public float artifactCritDamageRatio;

    [Header("버프 효과 비율")]
    public float buffDamageRatio;
    public float buffAttackSpeedRatio;
    public float buffMoveSpeedRatio;
    public float buffHealthRatio;
    public float buffCritRateRatio;
    public float buffCritDamageRatio;

    [Header("유전자 증폭제 효과 비율")]
    public float boosterDamageRatio;
    public float boosterAttackSpeedRatio;
    public float boosterMoveSpeedRatio;
    public float boosterHealthRatio;
    public float boosterCritRateRatio;
    public float boosterCritDamageRatio;

    [Header("최종 능력치 (계산 결과)")]
    public float finalDamage;
    public float finalAttackSpeed;
    public float finalMoveSpeed;
    public float finalHealth;
    public float finalCritRate;
    public float finalCritDamage;

    [Header("현재 상태 (런타임)")]
    public float currentHealth;
    public bool isInvulnerable = false; // 디버그용 무적 모드 플래그

    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;

    public float cardSelectionInterval = 10f; // 카드 룰렛 주기 (초)

    void Awake()
    {
        playerHealthBar = GetComponent<PlayerHealthBar>();
        // CalculateFinalStats와 체력 초기화는 PlayerInitializer가 담당하므로 여기서 호출하지 않습니다.
    }
    public void TakeDamage(float damage)
    {
        // 무적 상태일 경우 데미지를 받지 않음
        if (isInvulnerable) return;

        currentHealth -= damage;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);

        // [수정] 체력이 0 이하가 되면 Die() 함수 호출
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void OnDestroy()
    {
        // DebugManager가 살아있을 경우에만 등록 해제 호출
        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.UnregisterPlayer();
        }
    }

    private void Die()
    {
        // [추가] 죽었을 때 게임오버 상태로 전환 요청
        Debug.Log("[CharacterStats] 플레이어가 사망했습니다. 게임오버 상태로 전환합니다.");
        gameObject.SetActive(false); // 플레이어 오브젝트를 비활성화
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > finalHealth) currentHealth = finalHealth;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);
    }

    // --- 스탯 적용 로직 (PlayerInitializer로부터 이전됨) ---

    /// <summary>
    /// 영구 스탯 데이터(유전자 증폭제로 강화된)를 현재 능력치 비율에 적용합니다.
    /// PlayerInitializer가 데이터를 가져오면, 이 메서드를 호출하여 실제 스탯에 반영합니다.
    /// </summary>
    public void ApplyPermanentStats(CharacterPermanentStats permanentStats)
    {

        if (permanentStats == null) return;
        boosterDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Attack, 0f);
        boosterAttackSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.AttackSpeed, 0f);
        boosterMoveSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.MoveSpeed, 0f);
        boosterHealthRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Health, 0f);
        boosterCritDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.CritMultiplier, 0f);
    }

    /// <summary>
    /// 포인트 분배 씬에서 할당된 포인트를 능력치 비율에 적용합니다.
    /// PlayerInitializer가 데이터를 가져오면, 이 메서드를 호출하여 실제 스탯에 반영합니다.
    /// </summary>
    public void ApplyAllocatedPoints(int points, CharacterPermanentStats permStats)
    {

        if (points <= 0 || permStats == null)
        {
            Debug.Log("포인트가 0 이하이거나 permanentStats가 없어 분배를 시작하지 않음.");
            return;
        }

        List<StatType> availableStats = permStats.GetUnlockedStats();

        Debug.Log($"[최종 확인] 분배 가능한 능력치 개수(availableStats.Count): {availableStats.Count}");


        if (availableStats.Count == 0) return;

        for (int i = 0; i < points; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            float weight = GetWeightForStat(targetStat);

            switch (targetStat)
            {
                case StatType.Attack: boosterDamageRatio += weight; break;
                case StatType.AttackSpeed: boosterAttackSpeedRatio += weight; break;
                case StatType.MoveSpeed: boosterMoveSpeedRatio += weight; break;
                case StatType.Health: boosterHealthRatio += weight; break;
                case StatType.CritMultiplier: boosterCritDamageRatio += weight; break;
            }
        }
    }

    /// <summary>
    /// 스탯 타입에 따른 가중치를 반환합니다. (ApplyAllocatedPoints에서 사용)
    /// </summary>
    private float GetWeightForStat(StatType stat)
    {
        return stat == StatType.Health ? 0.02f : 0.01f;
    }

    // ... (나머지 함수들은 그대로) ...
    public void CalculateFinalStats()
    {
        // 유전자 증폭제 비율도 함께 합산
        float dmgRatio = boosterDamageRatio + buffDamageRatio + cardDamageRatio + artifactDamageRatio;
        float atkSpdRatio = boosterAttackSpeedRatio + buffAttackSpeedRatio + cardAttackSpeedRatio + artifactAttackSpeedRatio;
        float moveRatio = boosterMoveSpeedRatio + buffMoveSpeedRatio + cardMoveSpeedRatio + artifactMoveSpeedRatio;
        float hpRatio = boosterHealthRatio + buffHealthRatio + cardHealthRatio + artifactHealthRatio;
        float critRateRatio = boosterCritRateRatio + buffCritRateRatio + cardCritRateRatio + artifactCritRateRatio;
        float critDmgRatio = boosterCritDamageRatio + buffCritDamageRatio + cardCritDamageRatio + artifactCritDamageRatio;

        finalDamage = stats.baseDamage * (1 + dmgRatio);
        finalAttackSpeed = stats.baseAttackSpeed * (1 + atkSpdRatio);
        finalMoveSpeed = stats.baseMoveSpeed * (1 + moveRatio);
        // 체력은 기본 체력에 비율을 적용한 후, 유전자 증폭제 포인트로 얻은 추가 체력을 더해줍니다.
        finalHealth = stats.baseHealth * (1 + hpRatio);
        finalCritRate = stats.baseCritRate * (1 + critRateRatio);
        finalCritDamage = stats.baseCritDamage * (1 + critDmgRatio);

        OnFinalStatsCalculated?.Invoke();

        // 음수 검사
        if (finalDamage < 0 || finalAttackSpeed < 0 || finalMoveSpeed < 0 ||
            finalHealth < 0 || finalCritRate < 0 || finalCritDamage < 0)
        {
            if (finalAttackSpeed <= 0) finalAttackSpeed = 0.1f;
            if (finalMoveSpeed < 0) finalMoveSpeed = 0;
            if (finalDamage < 0) finalDamage = 0;
            if (finalCritRate < 0) finalCritRate = 0;
            if (finalCritDamage < 0) finalCritDamage = 0;
            if (finalHealth < 1) finalHealth = 1;

            Debug.LogError($"[CharacterStats] 능력치 음수 오류! " +
                $"Damage:{finalDamage}, AtkSpd:{finalAttackSpeed}, MoveSpd:{finalMoveSpeed}, " +
                $"Health:{finalHealth}, CritRate:{finalCritRate}, CritDmg:{finalCritDamage}");
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    private void QuitGame()
    {
        Application.Quit();
    }

    public static BaseStats CalculatePreviewStats(BaseStats baseStats, int allocatedPoints)
    {
        BaseStats previewStats = new BaseStats();

        // 기본 능력치 복사 (이 값들이 최종 계산의 base가 됩니다)
        previewStats.baseDamage = baseStats.baseDamage;
        previewStats.baseAttackSpeed = baseStats.baseAttackSpeed;
        previewStats.baseMoveSpeed = baseStats.baseMoveSpeed;
        previewStats.baseHealth = baseStats.baseHealth;
        previewStats.baseCritRate = baseStats.baseCritRate;
        previewStats.baseCritDamage = baseStats.baseCritDamage;

        // project_plan.md의 1포인트당 가중치를 사용하여 '유전자 증폭제 비율'을 계산합니다.
        // 이 비율은 최종 능력치 계산 공식의 '증폭제_X_비율'에 해당합니다.
        float healthGeneBoosterRatio = allocatedPoints * 0.02f; // 체력 가중치
        float otherStatsGeneBoosterRatio = allocatedPoints * 0.01f; // 공격력, 공격속도, 이동속도, 크리티컬 배율 가중치

        // 치명타 확률은 할당된 포인트에 영향을 받지 않으므로, 해당 비율은 0입니다.


        // 최종 능력치 계산 공식 적용: Total stat = base stat * [1 + (해당 스탯의 유전자증폭체가중체)]
        // (카드 및 유물 가중치는 이 미리보기 계산에서는 고려하지 않습니다. 이는 인게임에서 동적으로 적용될 부분입니다.)
        previewStats.baseHealth = baseStats.baseHealth * (1 + healthGeneBoosterRatio);
        previewStats.baseDamage = baseStats.baseDamage * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseAttackSpeed = baseStats.baseAttackSpeed * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseMoveSpeed = baseStats.baseMoveSpeed * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseCritDamage = baseStats.baseCritDamage * (1 + otherStatsGeneBoosterRatio);
        // 치명타 확률은 할당된 포인트에 영향을 받지 않으므로, 기본값을 그대로 사용합니다.
        // previewStats.baseCritRate는 이미 baseStats.baseCritRate로 초기화되어 있으므로 추가 계산 불필요.

        return previewStats;
    }
}