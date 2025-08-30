using UnityEngine;

/// <summary>
/// ������ �ɷ�ġ�� �����ϴ� Ŭ�����Դϴ�.
/// EntityStats�� ��ӹ޾� ���� �ɷ�ġ �ý����� ����ϸ�,
/// MonsterController�� �����Ͽ� ���� ������ �ൿ�� ������ �ݴϴ�.
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
    /// MonsterDataSO�κ��� ������ �⺻ �ɷ�ġ�� �����մϴ�.
    /// </summary>
    public void Initialize(MonsterDataSO monsterData)
    {
        // ScriptableObject�� �����͸� ������� BaseStats�� �����մϴ�.
        baseStats = new BaseStats
        {
            baseHealth = monsterData.maxHealth,
            baseMoveSpeed = monsterData.moveSpeed,
            // ���ʹ� CharacterStats�� �ٸ� ������ ���� �� �����Ƿ�, �ִ� �͸� �ʱ�ȭ�մϴ�.
            // ���� ���, ���Ϳ��� �⺻ ���ݷ�, ġ��Ÿ�� ���� ���� �� �ֽ��ϴ�.
            baseDamage = monsterData.contactDamage // ���� �������� �⺻ ���ݷ����� Ȱ��
        };

        // ü�� �ʱ�ȭ �� ���� ���� ���
        currentHealth = FinalHealth;
        CalculateFinalStats();
    }

    /// <summary>
    /// ���ظ� �޴� ������ �������̵��մϴ�.
    /// ���� ���� ó���� MonsterController�� �����մϴ�.
    /// </summary>
    public override void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"[MonsterStats] ���� ���� ����! ���: {gameObject.name}, ������: {damage}, ���� ü��: {currentHealth}");

        // ���� ������ �ð� ȿ���� ��Ÿ ó���� Controller�� ���
        monsterController.HandleDamageFeedback(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    /// <summary>
    /// ��� ������ �������̵��մϴ�.
    /// ���� ��� ó���� MonsterController�� �����մϴ�.
    /// </summary>
    public override void Die()
    {
        monsterController.Die();
    }
}