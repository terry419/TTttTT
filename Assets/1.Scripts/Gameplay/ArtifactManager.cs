// --- 파일명: ArtifactManager.cs ---

using System.Collections.Generic;
using UnityEngine;

public class ArtifactManager : MonoBehaviour
{
    [Header("소유 유물")]
    public List<ArtifactDataSO> ownedArtifacts = new List<ArtifactDataSO>();

    private CharacterStats playerStats;

    private void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        ServiceLocator.Register<ArtifactManager>(this);
        DontDestroyOnLoad(gameObject);
    }

    private void FindPlayerStats()
    {
        if (playerStats == null && PlayerController.Instance != null)
        {
            playerStats = PlayerController.Instance.GetComponent<CharacterStats>();
        }
    }

    /// <summary>
    /// [신규] 새로운 씬에서 플레이어가 다시 생성되었을 때, 이 ArtifactManager와 새로 생성된 플레이어를 연결합니다.
    /// </summary>
    public void LinkToNewPlayer(CharacterStats newPlayerStats)
    {
        Debug.Log($"[ArtifactManager] 새로운 플레이어({newPlayerStats.name})와 연결하고 스탯 재계산을 시작합니다.");
        playerStats = newPlayerStats;
        RecalculateArtifactStats();
    }

    public void EquipArtifact(ArtifactDataSO artifact)
    {
        if (ownedArtifacts.Contains(artifact)) return;
        
        Debug.Log($"[ArtifactManager] 유물 장착: {artifact.artifactName}");
        ownedArtifacts.Add(artifact);
        RecalculateArtifactStats();
    }

    /// <summary>
    /// [신규] 소유한 모든 유물을 기반으로 플레이어의 유물 보너스 스탯을 처음부터 다시 계산합니다.
    /// </summary>
    private void RecalculateArtifactStats()
    {
        FindPlayerStats();
        if (playerStats == null)
        {
            Debug.LogError("[ArtifactManager] PlayerStats를 찾을 수 없어 스탯을 재계산할 수 없습니다.");
            return;
        }

        // 1. 모든 유물 보너스 스탯을 0으로 초기화합니다.
        playerStats.artifactDamageRatio = 0f;
        playerStats.artifactHealthRatio = 0f;
        playerStats.artifactMoveSpeedRatio = 0f;
        playerStats.artifactCritRateRatio = 0f;
        playerStats.artifactCritDamageRatio = 0f;
        
        // 참고: 카드 슬롯 보너스도 원래는 초기화 후 재계산해야 더 안정적입니다.
        // if (CardManager.Instance != null) { CardManager.Instance.ResetBonusSlots(); }

        // 2. 현재 소유한 모든 유물을 순회하며 보너스를 다시 합산합니다.
        foreach (var artifact in ownedArtifacts)
        {
            playerStats.artifactDamageRatio += artifact.attackBoostRatio;
            playerStats.artifactHealthRatio += artifact.healthBoostRatio;
            playerStats.artifactMoveSpeedRatio += artifact.moveSpeedBoostRatio;
            playerStats.artifactCritRateRatio += artifact.critChanceBoostRatio;
            playerStats.artifactCritDamageRatio += artifact.critDamageBoostRatio;

            // if (CardManager.Instance != null)
            // {
            //     CardManager.Instance.maxOwnedSlots += artifact.ownedCardSlotBonus;
            // }
        }

        // 3. 모든 합산이 끝난 후, 최종 능력치 계산을 단 한 번만 호출합니다.
        playerStats.CalculateFinalStats();
        Debug.Log($"[ArtifactManager] 모든 유물 스탯을 재계산했습니다.");
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}
