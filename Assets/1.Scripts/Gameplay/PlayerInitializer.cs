using UnityEngine;

/// <summary>
/// Gameplay 씬이 시작될 때, GameManager에 저장된 캐릭터 선택 데이터를 기반으로
/// 플레이어의 능력치를 초기화하는 역할을 담당합니다.
/// 이 스크립트는 Player 프리팹에 추가되어야 합니다.
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class PlayerInitializer : MonoBehaviour
{
    void Start()
    {
        CharacterDataSO selectedCharacter = GameManager.Instance.SelectedCharacter;

        if (selectedCharacter != null)
        {
            Debug.Log($"선택된 캐릭터: {selectedCharacter.characterName}의 데이터로 플레이어를 초기화합니다.");
            CharacterStats stats = GetComponent<CharacterStats>();
            
            // CharacterStats의 기본 능력치(baseStats)를 선택된 캐릭터의 데이터로 덮어씁니다.
            stats.stats = selectedCharacter.baseStats;
            
            // 변경된 기본 능력치를 기반으로 최종 능력치를 다시 계산하고 적용합니다.
            stats.CalculateFinalStats();
        }
        else
        { 
            // 이 경우는 보통 테스트를 위해 Gameplay 씬을 바로 시작했을 때 발생합니다.
            Debug.LogWarning("선택된 캐릭터 데이터가 없습니다. Player 프리팹의 기본 능력치로 시작합니다.");
        }
    }
}
