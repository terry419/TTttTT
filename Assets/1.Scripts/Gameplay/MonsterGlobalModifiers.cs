// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterGlobalModifiers.cs (신규 파일)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 몬스터의 행동과 관계없이, 특정 조건에 따라 항상 적용되는 '글로벌 패시브' 효과를 관리합니다.
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

        // 0.2초마다 한 번씩만 체크하여 성능 확보
        if (Time.frameCount % 12 != 0) return;

        foreach (var rule in _rules)
        {
            bool conditionMet = rule.condition.Decide(_controller);
            bool wasApplied = _isEffectApplied[rule];

            if (conditionMet && !wasApplied)
            {
                // 조건이 충족되었는데 효과가 적용 안 된 상태 -> 효과 적용
                _controller.ApplySelfStatusEffect(rule.effectToApply);
                _isEffectApplied[rule] = true;
            }
            else if (!conditionMet && wasApplied && rule.removeWhenConditionIsFalse)
            {
                // 조건이 해제되었고 효과가 적용된 상태이며, 자동 제거 옵션이 켜져있을 때 -> 효과 제거
                _controller.RemoveSelfStatusEffect(rule.effectToApply.effectId);
                _isEffectApplied[rule] = false;
            }
        }
    }
}