using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{

    private CharacterStats playerStats;

    private PlayerDataManager _playerDataManager;
    private PlayerDataManager PlayerDataManager
    {
        get
        {
            if (_playerDataManager == null)
            {
                _playerDataManager = ServiceLocator.Get<PlayerDataManager>();
            }
            return _playerDataManager;
        }
    }

    private void Awake()
    {
        if (!ServiceLocator.IsRegistered<ArtifactManager>())
        {
            ServiceLocator.Register<ArtifactManager>(this);
            // DontDestroyOnLoad(gameObject); // 4단계에서 제거할 예정이지만, 지금 제거해도 무방합니다.
        }
        else
        {
            Destroy(gameObject);
        }
        if (PlayerDataManager == null)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: PlayerDataManager를 찾을 수 없습니다! 실행 순서를 확인하세요.");
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
        if (PlayerDataManager.OwnedArtifacts.Contains(artifact)) return;

        PlayerDataManager.OwnedArtifacts.Add(artifact);

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

        // [수정] PlayerDataManager의 데이터를 직접 사용합니다.
        var allOwnedArtifacts = new List<ArtifactDataSO>(PlayerDataManager.OwnedArtifacts);

        // 먼저 모든 유물 효과 제거
        foreach (var artifact in allOwnedArtifacts)
        {
            playerStats.RemoveModifiersFromSource(artifact);
        }

        // 현재 소유한 유물 효과 다시 적용
        foreach (var artifact in allOwnedArtifacts)
        {
            if (playerStats != null)
            {
                // EquipArtifact 내부의 스탯 적용 로직을 직접 호출
                playerStats.AddModifier(StatType.Attack, new StatModifier(artifact.attackBoostRatio, artifact));
                playerStats.AddModifier(StatType.Health, new StatModifier(artifact.healthBoostRatio, artifact));
                playerStats.AddModifier(StatType.MoveSpeed, new StatModifier(artifact.moveSpeedBoostRatio, artifact));
                playerStats.AddModifier(StatType.CritRate, new StatModifier(artifact.critChanceBoostRatio, artifact));
                playerStats.AddModifier(StatType.CritMultiplier, new StatModifier(artifact.critDamageBoostRatio, artifact));
            }
        }
    }
}