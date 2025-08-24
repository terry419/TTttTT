using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    [Header("소유 유물")]
    public List<ArtifactDataSO> ownedArtifacts = new List<ArtifactDataSO>();

    private CharacterStats playerStats;

    private void Awake()
    {
        if (!ServiceLocator.IsRegistered<ArtifactManager>())
        {
            ServiceLocator.Register<ArtifactManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateArtifactStats();
    }

    public void EquipArtifact(ArtifactDataSO artifact)
    {
        if (playerStats == null || ownedArtifacts.Contains(artifact)) return;

        ownedArtifacts.Add(artifact);
        
        // [리팩토링] AddModifier 호출 (올바른 StatType 사용)
        playerStats.AddModifier(StatType.Attack, new StatModifier(artifact.attackBoostRatio, artifact));
        playerStats.AddModifier(StatType.Health, new StatModifier(artifact.healthBoostRatio, artifact));
        playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(artifact.moveSpeedBoostRatio, artifact));
        playerStats.AddModifier(StatType.CritRate, new StatModifier(artifact.critChanceBoostRatio, artifact));
        playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(artifact.critDamageBoostRatio, artifact));
    }

    private void RecalculateArtifactStats()
    {
        if (playerStats == null) return;

        var allOwnedArtifacts = new List<ArtifactDataSO>(ownedArtifacts);
        foreach (var artifact in allOwnedArtifacts)
        {
            playerStats.RemoveModifiersFromSource(artifact);
        }

        foreach (var artifact in allOwnedArtifacts)
        {
            EquipArtifact(artifact);
        }
    }
}