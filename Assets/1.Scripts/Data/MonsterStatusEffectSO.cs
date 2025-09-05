// 경로: ./TTttTT/Assets/1/Scripts/Data/MonsterStatusEffectSO.cs (신규 파일)
using UnityEngine;

[CreateAssetMenu(fileName = "MSE_", menuName = "Monster AI/Monster Status Effect")]
public class MonsterStatusEffectSO : ScriptableObject
{
    [Header("기본 정보")]
    public string effectId;

    [Header("효과 속성")]
    public float duration;

    [Header("능력치 변경 효과 (%)")]
    public float moveSpeedBonus;
    public float contactDamageBonus;
    public float damageTakenBonus;
    // ... 향후 몬스터에게 필요한 스탯이 생기면 여기에 추가합니다 ...

    [Header("지속 피해/회복 효과")]
    public float damageOverTime;
    public float healOverTime;
}