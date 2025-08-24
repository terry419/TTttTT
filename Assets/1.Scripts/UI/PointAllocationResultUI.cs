// --- 파일명: PointAllocationResultUI.cs (최종 수정본) ---

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PointAllocationResultUI : MonoBehaviour
{
    // ... (UI 참조 변수들은 이전과 동일) ...
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDamageText;

    // ✨ [1번 문제 해결] 파라미터를 분배된 포인트 딕셔너리로 변경
    public void UpdateDisplay(BaseStats baseStats, Dictionary<StatType, int> distributedPoints)
    {
        if (distributedPoints == null) // Confirm 전
        {
            damageText.text = $"Base Damage ({baseStats.baseDamage:F0}) + Gene Boost (??) = ???";
            attackSpeedText.text = $"Base Attack Speed ({baseStats.baseAttackSpeed:F1}) + Gene Boost (??) = ???";
            moveSpeedText.text = $"Base Move Speed ({baseStats.baseMoveSpeed:F1}) + Gene Boost (??) = ???";
            healthText.text = $"Base Health ({baseStats.baseHealth:F0}) + Gene Boost (??) = ???";
            critRateText.text = $"Base Crit Rate ({baseStats.baseCritRate:F0}%) + Gene Boost (??) = ???";
            critDamageText.text = $"Base Crit Damage ({baseStats.baseCritDamage:F0}%) + Gene Boost (??) = ???";
        }
        else // Confirm 후
        {
            // 각 스탯에 실제 분배된 포인트 개수를 가져옴
            int attackPoints = distributedPoints[StatType.Attack];
            int attackSpeedPoints = distributedPoints[StatType.AttackSpeed];
            int moveSpeedPoints = distributedPoints[StatType.MoveSpeed];
            int healthPoints = distributedPoints[StatType.Health];
            float baseCritRatePercent = baseStats.baseCritRate / 100f;
            float baseCritDmgPercent = baseStats.baseCritDamage / 100f;
            int critDmgPoints = distributedPoints[StatType.CritMultiplier];

            int critRatePoints = distributedPoints[StatType.CritRate];
            float finalCritRate = baseStats.baseCritRate * (1 + critRatePoints * 0.01f);


            // 최종 능력치 계산
            float finalDamage = baseStats.baseDamage * (1 + attackPoints * 0.01f);
            float finalAttackSpeed = baseStats.baseAttackSpeed * (1 + attackSpeedPoints * 0.01f);
            float finalMoveSpeed = baseStats.baseMoveSpeed * (1 + moveSpeedPoints * 0.01f);
            float finalHealth = baseStats.baseHealth * (1 + healthPoints * 0.02f);
            float finalCritDamage = baseCritDmgPercent * (1 + critDmgPoints * 0.01f);

            // ✨ 실제 분배된 포인트 개수를 UI에 표시
            damageText.text = $"Base Damage ({baseStats.baseDamage:F0}) + Gene Boost ({attackPoints}) = {finalDamage:F2}";
            attackSpeedText.text = $"Base Attack Speed ({baseStats.baseAttackSpeed:F1}) + Gene Boost ({attackSpeedPoints}) = {finalAttackSpeed:F2}";
            moveSpeedText.text = $"Base Move Speed ({baseStats.baseMoveSpeed:F1}) + Gene Boost ({moveSpeedPoints}) = {finalMoveSpeed:F2}";
            healthText.text = $"Base Health ({baseStats.baseHealth:F0}) + Gene Boost ({healthPoints}) = {finalHealth:F2}";
            critRateText.text = $"Base Crit Rate ({baseStats.baseCritRate:F0}%) + Gene Boost ({critRatePoints}) = {finalCritRate:F2}%"; // 포맷을 F2로 변경
            critDamageText.text = $"Base Crit Damage ({baseStats.baseCritDamage:F0}%) + Gene Boost ({critDmgPoints}) = {finalCritDamage * 100:F0}%";
        }
    }
}