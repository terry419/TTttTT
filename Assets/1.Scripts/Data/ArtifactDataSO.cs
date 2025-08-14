using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactData_", menuName = "GameData/ArtifactData")]
public class ArtifactDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string artifactID;
    public string artifactName;
    [TextArea(3, 5)]
    public string description;

    [Header("유물 속성")]
    public CardRarity rarity;

    [Header("효과 파라미터")]
    public float attackBoostRatio;
    public float healthBoostRatio;
    public float moveSpeedBoostRatio;
    public float critChanceBoostRatio;
    public float critDamageBoostRatio;
    public float lifestealBoostRatio;

    [Header("슬롯 및 확률 보너스")]
    public int ownedCardSlotBonus;
    public float specificCardTriggerChanceBonus;

    [Header("시각 정보")]
    public Sprite icon;
}