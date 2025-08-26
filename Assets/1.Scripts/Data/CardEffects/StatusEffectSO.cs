using UnityEngine;

/// <summary>
/// �ǰݵ� ��󿡰� ������ ���� �̻��� �ο��ϴ� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyStatus_", menuName = "GameData/v8.0/Modules/ApplyStatusEffect")]
public class ApplyStatusEffectSO : CardEffectSO
{
    [Header("���� �̻� ����")]
    [Tooltip("������ ������ ���� �̻� ������(��, ȭ�� ��)�Դϴ�.")]
    public StatusEffectDataSO statusToApply;

    /// <summary>
    /// �� ����� ����ü�� ������ �������� ��(OnHit) ����Ǵ� ���� �Ϲ����Դϴ�.
    /// </summary>
    public ApplyStatusEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// �ǰݵ� ���(context.HitTarget)���� ���� �̻��� �����մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����. ���: {context.HitTarget?.name}");

        if (statusToApply == null)
        {
            Debug.LogWarning($"[ApplyStatusEffectSO] '{this.name}' ��⿡ ������ StatusEffectDataSO�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (context.HitTarget != null)
        {
            // StatusEffectManager�� ���� ��󿡰� ���� �̻� ȿ���� �����ϵ��� ��û�մϴ�.
            ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.HitTarget.gameObject, statusToApply);
        }
    }
}