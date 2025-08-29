using UnityEngine;

/// <summary>
/// OnHit ������ ���ط��� ���� ������ŭ ü���� ȸ���ϴ� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_Lifesteal_", menuName = "GameData/v8.0/Modules/LifestealEffect")]
public class LifestealEffectSO : CardEffectSO
{
    [Header("���� ����")]
    [Tooltip("���ط� ��� ȸ���� ü���� ���� (%)")]
    [Range(0f, 100f)]
    public float lifestealPercentage = 10f;

    public LifestealEffectSO()
    {
        // ������ �Ϲ������� �ǰ� ������ �ߵ��մϴ�.
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// ���� ���� ������ EffectExecutor �Ǵ� BulletController����
    /// �� ����� ���� ���ο� lifestealPercentage ���� Ȯ���Ͽ� ó���ϰ� �˴ϴ�.
    /// ���� �� ����� Execute�� ����Ӵϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        if (context.Caster != null && context.DamageDealt > 0)
        {
            float healAmount = context.DamageDealt * (lifestealPercentage / 100f);
            context.Caster.Heal(healAmount);
        }
    }
}