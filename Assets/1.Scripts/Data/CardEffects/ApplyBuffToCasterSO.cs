using UnityEngine;

/// <summary>
/// ������(Caster) �ڽſ��� ������ ���� ȿ��(�ַ� ����)�� �ο��ϴ� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyBuffToCaster_", menuName = "GameData/v8.0/Modules/ApplyBuffToCaster")]
public class ApplyBuffToCasterSO : CardEffectSO
{
    [Header("���� ����")]
    [Tooltip("�����ڿ��� ������ ���� �̻� ������(�ַ� ����)�Դϴ�.")]
    public StatusEffectDataSO buffToApply;

    public ApplyBuffToCasterSO()
    {
        // ������ �Ϲ������� �߻� ������ �ߵ��մϴ�.
        trigger = EffectTrigger.OnFire;
    }

    /// <summary>
    /// ������(context.Caster)���� ���� �̻��� �����մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����. ���: {context.Caster?.name}");

        if (buffToApply == null)
        {
            Debug.LogWarning($"[ApplyBuffToCasterSO] '{this.name}' ��⿡ ������ ������ �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (context.Caster != null)
        {
            ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.Caster.gameObject, buffToApply);
        }
    }
}