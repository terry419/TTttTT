using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData_", menuName = "GameData/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string characterId;
    public string characterName;
    public Sprite illustration;
    [TextArea(3, 5)]
    public string description;

    [Header("기본 능력치")]
    public BaseStats baseStats;

    [Header("시작 아이템")]
    public CardDataSO startingCard; // 이 줄이 반드시 필요해!

    [Header("초기 포인트")]
    public int initialAllocationPoints;
}