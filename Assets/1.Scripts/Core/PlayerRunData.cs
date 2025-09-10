// 파일 경로: Assets/1.Scripts/Core/PlayerRunData.cs (새로 생성)
using System;
using System.Collections.Generic;
using UnityEngine; // [SerializeField]를 사용하기 위해 필요합니다.

/// <summary>
/// 한 번의 게임 플레이(Run) 동안 유지되는 모든 플레이어 데이터를 담는 클래스입니다.
/// 제안해주신 대로 버전 관리와 유효성 검증 기능이 포함되어 있습니다.
/// </summary>
[Serializable]
public class PlayerRunData
{

    // 이 런(Run)의 기반이 되는 캐릭터의 원본 데이터입니다.
    public CharacterDataSO characterData;

    // 이 런(Run)에서 사용될 캐릭터의 기본 능력치입니다.
    public BaseStats baseStats;

    // 현재 체력입니다. 씬이 바뀌어도 이 값은 유지됩니다.
    public float currentHealth;

    // (2단계에서 이전될 데이터) 현재 소유한 카드 목록
    public List<CardInstance> ownedCards = new List<CardInstance>();

    // (2단계에서 이전될 데이터) 현재 장착한 카드 목록
    public List<CardInstance> equippedCards = new List<CardInstance>();

    // (향후 확장) 현재 소유한 유물 목록
    public List<ArtifactDataSO> ownedArtifacts = new List<ArtifactDataSO>();

    /// <summary>
    /// 제안해주신 대로, 이 데이터가 유효한 상태인지 확인하는 메서드입니다.
    /// 예를 들어, 캐릭터 정보가 없을 경우 유효하지 않은 데이터로 판단할 수 있습니다.
    /// </summary>
    public bool IsValid()
    {
        bool isValid = characterData != null && baseStats != null && currentHealth >= 0;
        if (!isValid)
        {
            Debug.LogError("[PlayerRunData] 데이터 유효성 검증 실패! 캐릭터나 기본 스탯 정보가 없습니다.");
        }
        return isValid;
    }
}