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
        // 1. playerStats가 null이라는 이유로 함수가 조기 종료되지 않도록 조건을 수정합니다.
        if (ownedArtifacts.Contains(artifact)) return;
        
        ownedArtifacts.Add(artifact);
        
        // 2. 스탯 적용 로직은 playerStats 참조가 유효할 때만 실행되도록 if문으로 감싸줍니다.
        if (playerStats != null)
        {
            playerStats.AddModifier(StatType.Attack, new StatModifier(artifact.attackBoostRatio, artifact));
            playerStats.AddModifier(StatType.Health, new StatModifier(artifact.healthBoostRatio, artifact));
            playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(artifact.moveSpeedBoostRatio, artifact));
            playerStats.AddModifier(StatType.CritRate, new StatModifier(artifact.critChanceBoostRatio, artifact));
            playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(artifact.critDamageBoostRatio, artifact));
        }
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