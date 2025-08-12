using UnityEngine;

/// <summary>
/// Gameplay 씬이 시작될 때, GameManager에 저장된 캐릭터 선택 데이터와
/// 할당된 포인트를 기반으로 플레이어의 최종 능력치를 초기화하는 역할을 담당합니다.
/// 이 스크립트는 Player 프리팹에 추가되어야 합니다.
/// </summary>
[RequireComponent(typeof(CharacterStats))]
public class PlayerInitializer : MonoBehaviour
{
    void Start()
    {
        // 싱글톤 인스턴스들을 가져옵니다.
        GameManager gameManager = GameManager.Instance;
        CharacterStats playerStats = GetComponent<CharacterStats>();

        // GameManager에 선택된 캐릭터 데이터가 있는지 확인합니다.
        if (gameManager != null && gameManager.SelectedCharacter != null)
        {
            Debug.Log($"선택된 캐릭터: {gameManager.SelectedCharacter.characterName}의 데이터로 플레이어를 초기화합니다.");

            // 1. GameManager로부터 선택된 캐릭터의 기본 스탯과 할당된 포인트를 가져옵니다.
            BaseStats originalBaseStats = gameManager.SelectedCharacter.baseStats;
            int allocatedPoints = gameManager.AllocatedPoints;

            Debug.Log($"가져온 할당 포인트: {allocatedPoints}");

            // 2. 할당된 포인트를 사용하여 강화된 '새로운 기본 능력치'를 계산합니다.
            // 이 계산은 CharacterStats에 이미 만들어 둔 static 메서드를 활용합니다.
            BaseStats boostedStats = CharacterStats.CalculatePreviewStats(originalBaseStats, allocatedPoints);

            // 3. 플레이어의 CharacterStats 컴포넌트의 기본 능력치(stats)를 이 강화된 능력치로 덮어씁니다.
            playerStats.stats = boostedStats;

            // 4. 변경된 기본 능력치를 기반으로 최종 능력치를 다시 계산하고, 현재 체력을 최대 체력으로 설정합니다.
            playerStats.CalculateFinalStats();
            playerStats.currentHealth = playerStats.finalHealth;

            Debug.Log($"포인트 적용 완료. 최종 체력: {playerStats.finalHealth}, 최종 공격력: {playerStats.finalDamage}");
        }
        else
        {
            // 이 경우는 보통 테스트를 위해 Gameplay 씬을 바로 시작했을 때 발생합니다.
            Debug.LogWarning("선택된 캐릭터 데이터가 없습니다. Player 프리팹의 기본 능력치로 시작합니다.");
            // 이 경우에도 초기 능력치 계산을 한 번 실행해주는 것이 안전합니다.
            playerStats.CalculateFinalStats();
            playerStats.currentHealth = playerStats.finalHealth;
        }
    }
}