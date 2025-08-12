using UnityEngine;

/// <summary>
/// 상태 효과(버프, 디버프)의 속성을 정의하는 ScriptableObject입니다.
/// 독, 화상, 능력치 강화 등 다양한 효과를 데이터로 만들어 관리할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "StatusEffectData_", menuName = "GameData/StatusEffectData")]
public class StatusEffectDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string effectId;         // 효과의 고유 ID
    public string effectName;       // UI에 표시될 이름
    public Sprite icon;             // UI에 표시될 아이콘

    [Header("효과 속성")]
    public float duration;          // 효과의 기본 지속 시간 (초)
    public bool isBuff;             // 이 효과가 버프인지 디버프인지 여부

    [Header("능력치 변경 효과")]
    // 이 값들은 CharacterStats의 비율(Ratio) 값에 더해집니다.
    public float damageRatioBonus;      // 공격력 비율 보너스
    public float attackSpeedRatioBonus; // 공격 속도 비율 보너스
    public float moveSpeedRatioBonus;   // 이동 속도 비율 보너스
    // ... 기타 필요한 스탯 보너스 ...

    [Header("지속 피해/회복 효과")]
    public float damageOverTime;    // 초당 입히는 데미지 (디버프)
    public float healOverTime;      // 초당 회복하는 체력 (버프)
}
