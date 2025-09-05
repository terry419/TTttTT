// ���: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterGlobalModifiers.cs (�ű� ����)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ������ �ൿ�� �������, Ư�� ���ǿ� ���� �׻� ����Ǵ� '�۷ι� �нú�' ȿ���� �����մϴ�.
/// </summary>
public class MonsterGlobalModifiers : MonoBehaviour
{
    private MonsterController _controller;
    private List<GlobalModifierRule> _rules;
    private Dictionary<GlobalModifierRule, bool> _isEffectApplied = new Dictionary<GlobalModifierRule, bool>();

    void Awake()
    {
        _controller = GetComponent<MonsterController>();
    }

    public void Initialize(List<GlobalModifierRule> rules)
    {
        _rules = rules;
        if (_rules != null)
        {
            foreach (var rule in _rules)
            {
                _isEffectApplied[rule] = false;
            }
        }
    }

    void Update()
    {
        if (_rules == null || _rules.Count == 0) return;

        // 0.2�ʸ��� �� ������ üũ�Ͽ� ���� Ȯ��
        if (Time.frameCount % 12 != 0) return;

        foreach (var rule in _rules)
        {
            bool conditionMet = rule.condition.Decide(_controller);
            bool wasApplied = _isEffectApplied[rule];

            if (conditionMet && !wasApplied)
            {
                // ������ �����Ǿ��µ� ȿ���� ���� �� �� ���� -> ȿ�� ����
                _controller.ApplySelfStatusEffect(rule.effectToApply);
                _isEffectApplied[rule] = true;
            }
            else if (!conditionMet && wasApplied && rule.removeWhenConditionIsFalse)
            {
                // ������ �����Ǿ��� ȿ���� ����� �����̸�, �ڵ� ���� �ɼ��� �������� �� -> ȿ�� ����
                _controller.RemoveSelfStatusEffect(rule.effectToApply.effectId);
                _isEffectApplied[rule] = false;
            }
        }
    }
}