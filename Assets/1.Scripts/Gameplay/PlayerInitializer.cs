// --- 파일명: PlayerInitializer.cs (최종 수정본) ---

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class PlayerInitializer : MonoBehaviour
{
    void Start()
    {
        GameManager gameManager = GameManager.Instance;
        CharacterStats playerStats = GetComponent<CharacterStats>();
        ProgressionManager progressionManager = ProgressionManager.Instance;

        if (gameManager != null && gameManager.SelectedCharacter != null)
        {
            playerStats.stats = gameManager.SelectedCharacter.baseStats;

            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(gameManager.SelectedCharacter.characterId);
            ApplyPermanentStats(playerStats, permanentStats);

            int allocatedPoints = gameManager.AllocatedPoints;
            if (allocatedPoints > 0)
            {
                ApplyAllocatedPoints(playerStats, allocatedPoints, permanentStats);
            }

            playerStats.CalculateFinalStats();
            playerStats.currentHealth = playerStats.finalHealth;
        }
        else
        {
            playerStats.CalculateFinalStats();
            playerStats.currentHealth = playerStats.finalHealth;
        }
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
        switch (stat)
        {
            case StatType.Health: return 0.02f;
            default: return 0.01f;
        }
    }
}