// ���: ./TTttTT/Assets/1.Scripts/AI/Behaviors/MonsterBehavior.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [����� AI �ý����� �ٽ� ���赵 1/3]
/// '�߰�', '����' �� ��� ��ü���� '�ൿ' ��ǰ���� �ݵ�� ����� �ϴ� �⺻ ����(���赵)�Դϴ�.
/// ScriptableObject�� ����Ͽ�, �� �ൿ�� ����(.asset)�� ����� �����ϰ� ������ �� �ֽ��ϴ�.
/// </summary>
public abstract class MonsterBehavior : ScriptableObject
{
    [Header("�ൿ ��ȯ ��Ģ ���")]
    [Tooltip("�� �ൿ�� �ٸ� �ൿ���� ��ȯ�� �� �ִ� '��Ģ'���� ����Դϴ�.")]
    public List<Transition> transitions = new List<Transition>();

    [Header("�ൿ �нú� ȿ�� (Behavioral Passives)")]
    [Tooltip("�� �ൿ�� ������ �� ���� �ڽſ��� ������ ���� ȿ���Դϴ�.")]
    public MonsterStatusEffectSO effectToApplyOnEnter;
    [Tooltip("�� �ൿ�� ���� �� ���� �ڽſ��Լ� ������ ���� ȿ���Դϴ�.")]
    public MonsterStatusEffectSO effectToRemoveOnExit;

    /// <summary>
    /// �� �ൿ�� ó�� ���۵� �� �� �� �� ȣ��Ǵ� �ʱ�ȭ �Լ��Դϴ�.
    /// (��: ���� ������ ������ ó�� �� ���� ���)
    /// </summary>
    /// <param name="monster">�� �ൿ�� �����ϴ� ������ '����'</param>
    public virtual void OnEnter(MonsterController monster)
    {
        if (effectToApplyOnEnter != null)
        {
            monster.ApplySelfStatusEffect(effectToApplyOnEnter);
        }
    }
    /// <summary>
    /// �� �ൿ�� Ȱ��ȭ�� ���� �� ������(�Ǵ� �ֱ�������) ����Ǵ� �ٽ� �����Դϴ�.
    /// </summary>
    /// <param name="monster">�� �ൿ�� �����ϴ� ������ '����'</param>
    public abstract void OnExecute(MonsterController monster);

    /// <summary>
    /// �� �ൿ�� �ٸ� �ൿ���� ��ȯ�Ǳ� ����, �� �� �� ȣ��Ǵ� ������ �Լ��Դϴ�.
    /// (��: ����ߴ� Ÿ�̸ӳ� �������� �ʱ�ȭ)
    /// </summary>
    /// <param name="monster">�� �ൿ�� �����ϴ� ������ '����'</param>
    public virtual void OnExit(MonsterController monster)
    {
        if (effectToRemoveOnExit != null)
        {
            monster.RemoveSelfStatusEffect(effectToRemoveOnExit.effectId);
        }
    }
    /// <summary>
    /// ��� �ൿ ��ǰ���� �������� ����ϴ� '���� ��ȯ �˻�' ����Դϴ�.
    /// transitions ��Ͽ� �ִ� ��� '��Ģ'�� �ϳ��� Ȯ���Ͽ�, ������ ������ ���� �ൿ���� ��ȯ��ŵ�ϴ�.
    /// </summary>
    /// <param name="monster">�� �ൿ�� �����ϴ� ������ '����'</param>
    protected void CheckTransitions(MonsterController monster)
    {
        if (transitions == null || transitions.Count == 0) return;

        foreach (var transition in transitions)
        {
            // 1. �� ��ȯ ��Ģ�� '����' ��ǰ���� ���� ���¸� �ٲ��� ����ϴ�.
            if (transition.decision != null && transition.decision.Decide(monster))
            {
                // 2. "�ٲ��!" (true) ��� ���� ����, ������ ���� �ൿ���� ��ȯ�մϴ�.
                if (transition.nextBehavior != null)
                {
                    monster.ChangeBehavior(transition.nextBehavior);
                    return; // �� ���� �ϳ��� ��ȯ�� ó���ϰ� ��� �����մϴ�.
                }
            }
        }
    }
    private void OnValidate()
    {
        // transitions ����Ʈ�� �ִ� ��� ��Ģ���� �˻��մϴ�.
        for (int i = 0; i < transitions.Count; i++)
        {
            var transition = transitions[i];

            // ���� 'Decision' ������ ����ִٸ�, Console â�� ��� ���ϴ�.
            if (transition.decision == null)
            {
                Debug.LogWarning($"[Asset Validator] '{this.name}' ������ Transitions ��� {i}��° �׸� 'Decision'�� ������� �ʾҽ��ϴ�.");
            }

            // ���� 'Next Behavior' ������ ����ִٸ�, Console â�� ��� ���ϴ�.
            if (transition.nextBehavior == null)
            {
                Debug.LogWarning($"[Asset Validator] '{this.name}' ������ Transitions ��� {i}��° �׸� 'Next Behavior'�� ������� �ʾҽ��ϴ�.");
            }
        }
    }

}