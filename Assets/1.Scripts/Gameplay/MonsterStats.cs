using UnityEngine;

/// <summary>
/// 몬스터의 능력치를 관리하는 클래스입니다.
/// EntityStats를 상속받아 공통 능력치 시스템을 사용하며,
/// MonsterController와 연동하여 실제 몬스터의 행동에 영향을 줍니다.
/// </summary>
[RequireComponent(typeof(MonsterController))]
public class MonsterStats : EntityStats
{
    private MonsterController monsterController;

    protected override void Awake()
    {
        base.Awake();
        monsterController = GetComponent<MonsterController>();
    }

    /// <summary>
    /// MonsterDataSO로부터 몬스터의 기본 능력치를 설정합니다.
    /// </summary>
    public void Initialize(MonsterDataSO monsterData)
    {
        // ScriptableObject의 데이터를 기반으로 BaseStats를 설정합니다.
        baseStats = new BaseStats
        {
            baseHealth = monsterData.maxHealth,
            baseMoveSpeed = monsterData.moveSpeed,
            // 몬스터는 CharacterStats와 다른 스탯을 가질 수 있으므로, 있는 것만 초기화합니다.
            // 예를 들어, 몬스터에게 기본 공격력, 치명타율 등이 없을 수 있습니다.
            baseDamage = monsterData.contactDamage // 접촉 데미지를 기본 공격력으로 활용
        };

        // 체력 초기화 및 최종 스탯 계산
        currentHealth = FinalHealth;
        CalculateFinalStats();
    }

    /// <summary>
    /// 피해를 받는 로직을 오버라이드합니다.
    /// 실제 피해 처리는 MonsterController에 위임합니다.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"[MonsterStats] 몬스터 피해 입음! 대상: {gameObject.name}, 데미지: {damage}, 현재 체력: {currentHealth}");

        // 실제 데미지 시각 효과나 기타 처리는 Controller가 담당
        monsterController.HandleDamageFeedback(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// 사망 로직을 오버라이드합니다.
    /// 실제 사망 처리는 MonsterController에 위임합니다.
    /// </summary>
    public override void Die()
    {
        monsterController.Die();
    }
}