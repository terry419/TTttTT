using UnityEngine;
using BaseStats;

/// <summary>
/// 캐릭터(워리어, 아처, 메이지 등)의 고유한 기본 데이터를 정의하는 ScriptableObject입니다.
/// 캐릭터 선택창 UI, 초기 스탯 설정 등 캐릭터와 관련된 데이터를 중앙에서 관리하는 데 사용됩니다.
/// </summary>
[CreateAssetMenu(fileName = "CharacterData_", menuName = "GameData/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string characterId;      // 캐릭터의 고유 ID (예: "warrior")
    public string characterName;    // UI에 표시될 캐릭터 이름
    public Sprite illustration;     // 캐릭터 선택창 등에 표시될 일러스트

    [TextArea(3, 5)]
    public string description;      // 캐릭터에 대한 간단한 설명

    [Header("기본 능력치")]
    // CharacterStats 스크립트에 정의된 BaseStats 클래스를 사용하여
    // 해당 캐릭터의 순수한 기본 능력치를 저장합니다.
    public BaseStats baseStats;

    [Header("초기 포인트")]
    public int initialAllocationPoints; // 캐릭터별 초기 할당 가능 포인트
}
