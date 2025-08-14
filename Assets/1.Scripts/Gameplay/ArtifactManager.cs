// --- 파일명: ArtifactManager.cs ---

using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    [Header("유물 목록")]
    public List<ArtifactDataSO> ownedArtifacts = new List<ArtifactDataSO>();

    private CharacterStats playerStats;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void FindPlayerStats()
    {
        if (playerStats == null && PlayerController.Instance != null)
        {
            playerStats = PlayerController.Instance.GetComponent<CharacterStats>();
        }
    }

    public void EquipArtifact(ArtifactDataSO artifact)
    {
        FindPlayerStats();
        if (playerStats == null)
        {
            Debug.LogError("[ArtifactManager] PlayerStats를 찾을 수 없어 유물을 장착할 수 없습니다.");
            return;
        }
        if (ownedArtifacts.Contains(artifact)) return;

        ownedArtifacts.Add(artifact);
        ApplyArtifactEffect(artifact);
    }

    private void ApplyArtifactEffect(ArtifactDataSO artifact)
    {
        // RewardManager에 있던 로직을 가져와서 공용으로 사용
        playerStats.artifactDamageRatio += artifact.attackBoostRatio;
        playerStats.artifactHealthRatio += artifact.healthBoostRatio;
        playerStats.artifactMoveSpeedRatio += artifact.moveSpeedBoostRatio;
        playerStats.artifactCritRateRatio += artifact.critChanceBoostRatio;
        playerStats.artifactCritDamageRatio += artifact.critDamageBoostRatio;

        if (CardManager.Instance != null)
        {
            CardManager.Instance.maxOwnedSlots += artifact.ownedCardSlotBonus;
        }

        playerStats.CalculateFinalStats();
        Debug.Log($"[ArtifactManager] 유물 장착: {artifact.artifactName}, 최종 공격력: {playerStats.finalDamage}");
    }
}