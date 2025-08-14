using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CharacterStats))]
public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 기본 카드")]
    [SerializeField] private CardDataSO defaultCard;

    void Start()
    {
        Debug.Log("--- [PlayerInitializer] 플레이어 초기화 시작 ---");

        CharacterStats playerStats = GetComponent<CharacterStats>();
        SpriteRenderer playerSpriteRenderer = GetComponent<SpriteRenderer>();

        GameManager gameManager = GameManager.Instance;
        DataManager dataManager = DataManager.Instance;
        ProgressionManager progressionManager = ProgressionManager.Instance;

        if (gameManager == null) Debug.LogError("!!! [PlayerInitializer] GameManager를 찾을 수 없음!");
        if (dataManager == null) Debug.LogError("!!! [PlayerInitializer] DataManager를 찾을 수 없음!");
        if (progressionManager == null) Debug.LogError("!!! [PlayerInitializer] ProgressionManager를 찾을 수 없음!");

        CharacterDataSO characterToLoad = null;

        if (gameManager != null && gameManager.SelectedCharacter != null)
        {
            characterToLoad = gameManager.SelectedCharacter;
            Debug.Log($"B. [PlayerInitializer] 선택된 캐릭터 '{characterToLoad.characterName}' 로드.");
        }
        else
        {
            Debug.LogWarning("B. [PlayerInitializer] 선택된 캐릭터 없음, 기본 'warrior' 로드 시도.");
            characterToLoad = dataManager.GetCharacter("warrior");
        }

        if (characterToLoad != null)
        {
            playerStats.stats = characterToLoad.baseStats;
            playerSpriteRenderer.sprite = characterToLoad.illustration;
            Debug.Log($"C. [PlayerInitializer] '{characterToLoad.characterName}'의 능력치와 스프라이트 적용 완료.");

            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            ApplyPermanentStats(playerStats, permanentStats);

            int allocatedPoints = gameManager.AllocatedPoints;
            if (allocatedPoints > 0)
            {
                ApplyAllocatedPoints(playerStats, allocatedPoints, permanentStats);
                Debug.Log($"D. [PlayerInitializer] {allocatedPoints} 포인트 분배 적용 완료.");
            }
        }
        if (defaultCard != null && CardManager.Instance != null)
        {
            CardManager.Instance.AddCard(defaultCard); // 먼저 소유 목록에 추가
            CardManager.Instance.Equip(defaultCard);   // 그 다음 장착
            Debug.Log($"테스트용 기본 카드 '{defaultCard.cardName}' 자동 장착 완료.");
        }

        else
        {
            Debug.LogError("!!! [PlayerInitializer] CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
        }

        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.finalHealth;
        Debug.Log("--- [PlayerInitializer] 플레이어 초기화 완료 ---");
    }

    // ... (ApplyPermanentStats 등 나머지 함수는 그대로) ...
    private void ApplyPermanentStats(CharacterStats playerStats, CharacterPermanentStats permanentStats)
    {
        if (permanentStats == null) return;
        playerStats.boosterDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Attack, 0f);
        playerStats.boosterAttackSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.AttackSpeed, 0f);
        playerStats.boosterMoveSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.MoveSpeed, 0f);
        playerStats.boosterHealthRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Health, 0f);
        playerStats.boosterCritDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.CritMultiplier, 0f);
    }

    private void ApplyAllocatedPoints(CharacterStats playerStats, int points, CharacterPermanentStats permStats)
    {
        List<StatType> availableStats = permStats.GetUnlockedStats();
        if (availableStats.Count == 0) return;

        for (int i = 0; i < points; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            float weight = GetWeightForStat(targetStat);

            switch (targetStat)
            {
                case StatType.Attack: playerStats.boosterDamageRatio += weight; break;
                case StatType.AttackSpeed: playerStats.boosterAttackSpeedRatio += weight; break;
                case StatType.MoveSpeed: playerStats.boosterMoveSpeedRatio += weight; break;
                case StatType.Health: playerStats.boosterHealthRatio += weight; break;
                case StatType.CritMultiplier: playerStats.boosterCritDamageRatio += weight; break;
            }
        }
    }

    private float GetWeightForStat(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health: return 0.02f;
            default: return 0.01f;
        }
    }
}