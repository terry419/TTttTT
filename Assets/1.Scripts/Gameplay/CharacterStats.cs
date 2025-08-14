using UnityEngine;
using UnityEngine.Events;

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

    [Header("이벤트")]
    public UnityEvent OnFinalStatsCalculated = new UnityEvent();

    private PlayerHealthBar playerHealthBar;

    void Awake()
    {
        playerHealthBar = GetComponent<PlayerHealthBar>();
        // CalculateFinalStats와 체력 초기화는 PlayerInitializer가 담당하므로 여기서 호출하지 않습니다.
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > finalHealth) currentHealth = finalHealth;
        playerHealthBar.UpdateHealth(currentHealth, finalHealth);
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
            Debug.LogError($"[CharacterStats] 능력치 음수 오류! " +
                $"Damage:{finalDamage}, AtkSpd:{finalAttackSpeed}, MoveSpd:{finalMoveSpeed}, " +
                $"Health:{finalHealth}, CritRate:{finalCritRate}, CritDmg:{finalCritDamage}");
            Invoke(nameof(QuitGame), 5f);
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