using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactData_", menuName = "GameData/ArtifactData")]
public class ArtifactDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string artifactID;      // 유물의 고유 ID (예: "artifact_common_001")
    public string artifactName;    // UI에 표시될 유물 이름
    [TextArea(3, 5)]
    public string description;     // 유물에 대한 설명

    [Header("유물 속성")]
    public CardRarity rarity;      // 유물의 희귀도 (CardRarity 열거형 사용)

    [Header("효과 파라미터")]
    // project_plan.md에 언급된 효과별 파라미터 예시
    public float attackBoostRatio;      // 공격력 증폭 비율
    public float healthBoostRatio;      // 체력 증폭 비율
    public float moveSpeedBoostRatio;   // 이동 속도 증폭 비율
    public float critChanceBoostRatio;  // 치명타 확률 증폭 비율
    public float critDamageBoostRatio;  // 치명타 피해 증폭 비율
    public float lifestealBoostRatio;   // 흡혈 효과 증폭 비율

    [Header("슬롯 및 확률 보너스")]
    public int ownedCardSlotBonus;      // 소유 카드 슬롯 증가량
    public float specificCardTriggerChanceBonus; // 특정 카드 발동 확률 보너스

    // TODO: project_plan.md의 유물 시스템 섹션에 있는 구체적인 유물 효과들을 반영하여 필드 추가
    // 예: 상점 가격 변경 등

    [Header("시각 정보")]
    public Sprite icon;                // 유물 아이콘
}
