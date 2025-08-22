using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerHealthBar))]
public class CharacterStats : MonoBehaviour
{
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
    public bool isInvulnerable = false;

    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;
    public float cardSelectionInterval = 10f;

    void Awake()
    {
        playerHealthBar = GetComponent<PlayerHealthBar>();
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void OnDestroy()
    {
        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.UnregisterPlayer();
        }
    }

    private void Die()
    {
        Debug.Log("[CharacterStats] 플레이어가 사망했습니다. 게임오버 상태로 전환합니다.");
        gameObject.SetActive(false);
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.GameOver);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > finalHealth) currentHealth = finalHealth;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);
    }

    public void ApplyPermanentStats(CharacterPermanentStats permanentStats)
    {
        if (permanentStats == null) return;
        boosterDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Attack, 0f);
        boosterAttackSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.AttackSpeed, 0f);
        boosterMoveSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.MoveSpeed, 0f);
        boosterHealthRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Health, 0f);
        boosterCritDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.CritMultiplier, 0f);
    }

    public void ApplyAllocatedPoints(int points, CharacterPermanentStats permStats)
    {
        if (points <= 0 || permStats == null)
        {
            Debug.Log("포인트가 0 이하이거나 permanentStats가 없어 분배를 시작하지 않음.");
            return;
        }

        List<StatType> availableStats = permStats.GetUnlockedStats();
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

    // [오류 수정] 함수 내용 복원
    private float GetWeightForStat(StatType stat)
    {
        return stat == StatType.Health ? 0.02f : 0.01f;
    }

    public void CalculateFinalStats()
    {
        float dmgRatio = boosterDamageRatio + buffDamageRatio + cardDamageRatio + artifactDamageRatio;
        float atkSpdRatio = boosterAttackSpeedRatio + buffAttackSpeedRatio + cardAttackSpeedRatio + artifactAttackSpeedRatio;
        float moveRatio = boosterMoveSpeedRatio + buffMoveSpeedRatio + cardMoveSpeedRatio + artifactMoveSpeedRatio;
        float hpRatio = boosterHealthRatio + buffHealthRatio + cardHealthRatio + artifactHealthRatio;
        float critRateRatio = boosterCritRateRatio + buffCritRateRatio + cardCritRateRatio + artifactCritRateRatio;
        float critDmgRatio = boosterCritDamageRatio + buffCritDamageRatio + cardCritDamageRatio + artifactCritDamageRatio;

        finalDamage = stats.baseDamage * (1 + dmgRatio);
        finalAttackSpeed = stats.baseAttackSpeed * (1 + atkSpdRatio);
        finalMoveSpeed = stats.baseMoveSpeed * (1 + moveRatio);
        finalHealth = stats.baseHealth * (1 + hpRatio);
        finalCritRate = stats.baseCritRate * (1 + critRateRatio);
        finalCritDamage = stats.baseCritDamage * (1 + critDmgRatio);

        Debug.Log($"[CharacterStats] 값 보정 전 최종 공격 속도: {finalAttackSpeed}");
        if (finalAttackSpeed <= 0)
        {
            finalAttackSpeed = 0.1f;
            Debug.LogWarning($"[CharacterStats] 공격 속도가 0 이하로 계산되어 최소값({finalAttackSpeed})으로 보정되었습니다.");
        }
        if (finalMoveSpeed < 0) finalMoveSpeed = 0;
        if (finalDamage < 0) finalDamage = 0;
        if (finalCritRate < 0) finalCritRate = 0;
        if (finalCritDamage < 0) finalCritDamage = 0;
        if (finalHealth < 1) finalHealth = 1;

        Debug.Log("[CharacterStats] 보정된 최종 능력치로 OnFinalStatsCalculated 이벤트를 방송합니다.");
        OnFinalStatsCalculated?.Invoke();
    }

    // [오류 수정] 함수 내용 복원
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    // [오류 수정] 함수 내용 복원
    public static BaseStats CalculatePreviewStats(BaseStats baseStats, int allocatedPoints)
    {
        BaseStats previewStats = new BaseStats();

        previewStats.baseDamage = baseStats.baseDamage;
        previewStats.baseAttackSpeed = baseStats.baseAttackSpeed;
        previewStats.baseMoveSpeed = baseStats.baseMoveSpeed;
        previewStats.baseHealth = baseStats.baseHealth;
        previewStats.baseCritRate = baseStats.baseCritRate;
        previewStats.baseCritDamage = baseStats.baseCritDamage;

        float healthGeneBoosterRatio = allocatedPoints * 0.02f;
        float otherStatsGeneBoosterRatio = allocatedPoints * 0.01f;

        previewStats.baseHealth = baseStats.baseHealth * (1 + healthGeneBoosterRatio);
        previewStats.baseDamage = baseStats.baseDamage * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseAttackSpeed = baseStats.baseAttackSpeed * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseMoveSpeed = baseStats.baseMoveSpeed * (1 + otherStatsGeneBoosterRatio);
        previewStats.baseCritDamage = baseStats.baseCritDamage * (1 + otherStatsGeneBoosterRatio);

        return previewStats;
    }
}