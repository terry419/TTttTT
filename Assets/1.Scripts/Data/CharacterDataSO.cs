// --- 파일명: CharacterDataSO.cs ---
using UnityEngine;
using System.Collections.Generic; // List를 사용하기 위해 추가

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
    public List<NewCardDataSO> startingCards;
    public List<ArtifactDataSO> startingArtifacts; // [추가] 시작 유물 목록

    [Header("초기 포인트")]
    public int initialAllocationPoints;
}