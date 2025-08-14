using UnityEngine;
using System.Collections.Generic;

public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 기본 카드")]
    [SerializeField] private CardDataSO defaultCard;

    void Start()
    {
        CharacterStats playerStats = GetComponent<CharacterStats>();
        SpriteRenderer playerSpriteRenderer = GetComponent<SpriteRenderer>();

        GameManager gameManager = GameManager.Instance;
        DataManager dataManager = DataManager.Instance;
        ProgressionManager progressionManager = ProgressionManager.Instance;

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? dataManager.GetCharacter("warrior");

        if (characterToLoad != null)
        {
            playerStats.stats = characterToLoad.baseStats;
            playerSpriteRenderer.sprite = characterToLoad.illustration;

            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            ApplyPermanentStats(playerStats, permanentStats);

            int allocatedPoints = gameManager.AllocatedPoints;
            if (allocatedPoints > 0)
            {
                ApplyAllocatedPoints(playerStats, allocatedPoints, permanentStats);
            }
        }
        else
        {
            Debug.LogError("CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
        }

        /*if (defaultCard != null && CardManager.Instance != null)
        {
            CardManager.Instance.AddCard(defaultCard);
            CardManager.Instance.Equip(defaultCard);
        }*/

        if (characterToLoad.startingCard != null && CardManager.Instance != null)
        {
            CardManager.Instance.AddCard(characterToLoad.startingCard);
            CardManager.Instance.Equip(characterToLoad.startingCard);
            Debug.Log($"[PlayerInitializer] {characterToLoad.characterName}의 시작 카드 '{characterToLoad.startingCard.name}' 장착 완료.");
        }
        else
        {
            Debug.LogWarning($"[PlayerInitializer] {characterToLoad.characterName}에게 설정된 시작 카드가 없습니다.");
        }



        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.finalHealth;
    }

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
        return stat == StatType.Health ? 0.02f : 0.01f;
    }
}
