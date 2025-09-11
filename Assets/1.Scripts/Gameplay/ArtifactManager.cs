using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유물의 '동작'(장착, 효과 적용)을 관리합니다.
/// [2단계 리팩토링] 이제 유물 '데이터'(목록)는 PlayerDataManager가 소유합니다.
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    private CharacterStats playerStats;

    // PlayerDataManager의 데이터를 사용하기 위한 참조 속성
    private PlayerDataManager _playerDataManager;
    private PlayerDataManager PlayerDataManager
    {
        get
        {
            if (_playerDataManager == null) _playerDataManager = ServiceLocator.Get<PlayerDataManager>();
            return _playerDataManager;
        }
    }

    private void Awake()
    {
        if (!ServiceLocator.IsRegistered<ArtifactManager>()) ServiceLocator.Register<ArtifactManager>(this);
        else Destroy(gameObject);
    }

    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        playerStats = newPlayerStats;
        RecalculateArtifactStats();
    }

    public void EquipArtifact(ArtifactDataSO artifact)
    {
        if (PlayerDataManager.CurrentRunData.ownedArtifacts.Contains(artifact)) return;
        PlayerDataManager.CurrentRunData.ownedArtifacts.Add(artifact);

        if (playerStats != null)
        {
            ApplyArtifactStats(artifact);
        }
    }

    private void RecalculateArtifactStats()
    {
        if (playerStats == null) return;

        var allOwnedArtifacts = new List<ArtifactDataSO>(PlayerDataManager.CurrentRunData.ownedArtifacts);

        foreach (var artifact in allOwnedArtifacts)
        {
            playerStats.RemoveModifiersFromSource(artifact);
        }

        foreach (var artifact in allOwnedArtifacts)
        {
            ApplyArtifactStats(artifact);
        }
    }

    private void ApplyArtifactStats(ArtifactDataSO artifact)
    {
        if (playerStats == null || artifact == null) return;
        playerStats.AddModifier(StatType.Attack, new StatModifier(artifact.attackBoostRatio, artifact));
        playerStats.AddModifier(StatType.Health, new StatModifier(artifact.healthBoostRatio, artifact));
        playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(artifact.moveSpeedBoostRatio, artifact));
        playerStats.AddModifier(StatType.CritRate, new StatModifier(artifact.critChanceBoostRatio, artifact));
        playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(artifact.critDamageBoostRatio, artifact));
    }

    private void OnDestroy()
    {
        // ServiceLocator에 내가 등록되어 있을 경우에만 등록 해제를 시도합니다.
        if (ServiceLocator.IsRegistered<ArtifactManager>() && ServiceLocator.Get<ArtifactManager>() == this)
        {
            ServiceLocator.Unregister<ArtifactManager>(this);
        }
    }
}
