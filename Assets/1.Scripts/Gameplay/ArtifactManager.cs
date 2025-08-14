// --- ���ϸ�: ArtifactManager.cs ---

using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    [Header("���� ���")]
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
            Debug.LogError("[ArtifactManager] PlayerStats�� ã�� �� ���� ������ ������ �� �����ϴ�.");
            return;
        }
        if (ownedArtifacts.Contains(artifact)) return;

        ownedArtifacts.Add(artifact);
        ApplyArtifactEffect(artifact);
    }

    private void ApplyArtifactEffect(ArtifactDataSO artifact)
    {
        // RewardManager�� �ִ� ������ �����ͼ� �������� ���
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
        Debug.Log($"[ArtifactManager] ���� ����: {artifact.artifactName}, ���� ���ݷ�: {playerStats.finalDamage}");
    }
}