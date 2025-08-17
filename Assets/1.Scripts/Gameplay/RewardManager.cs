using UnityEngine;
using System.Collections.Generic;
using System; // Action을 사용하기 위해 필요합니다.

/// <summary>
/// 아티팩트 및 카드 보상 큐를 중앙에서 관리하고, 보상 과정을 제어합니다.
/// </summary>
public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    // 카드 보상이 준비되었을 때 UI 시스템에 알리기 위한 이벤트입니다.
    public static event Action<List<CardDataSO>> OnCardRewardReady;

    private readonly Queue<ArtifactDataSO> artifactRewardQueue = new Queue<ArtifactDataSO>();
    private readonly Queue<List<CardDataSO>> cardRewardQueue = new Queue<List<CardDataSO>>();
    private CharacterStats playerStats;

    public bool IsRewardQueueEmpty => artifactRewardQueue.Count == 0 && cardRewardQueue.Count == 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 씬 전용 매니저이므로 이 라인은 비활성화 또는 삭제합니다.
    }

    /// <summary>
    /// 보상 큐에 새로운 유물을 추가합니다.
    /// </summary>
    public void EnqueueReward(ArtifactDataSO artifact)
    {
        if (artifact == null) return;
        artifactRewardQueue.Enqueue(artifact);
        Debug.Log($"[RewardManager] 유물 보상 추가: {artifact.artifactName}");
    }

    /// <summary>
    /// 보상 큐에 새로운 카드 선택지를 추가합니다.
    /// </summary>
    public void EnqueueReward(List<CardDataSO> cardChoices)
    {
        if (cardChoices == null || cardChoices.Count == 0) return;
        cardRewardQueue.Enqueue(cardChoices);
        Debug.Log($"[RewardManager] 카드 보상 선택지 ({cardChoices.Count}개) 추가");
    }

    /// <summary>
    /// 다음 보상 과정을 시작합니다.
    /// </summary>
    public void ProcessNextReward()
    {
        Debug.LogWarning($"[ 진단 ] ProcessNextReward() 호출됨.");

        if (artifactRewardQueue.Count > 0)
        {
            // 유물 보상을 처리할 때만 플레이어의 CharacterStats를 찾습니다.
            if (playerStats == null)
            {
                playerStats = FindObjectOfType<CharacterStats>();
                if (playerStats == null)
                {
                    Debug.LogError("[RewardManager] 플레이어의 CharacterStats를 찾을 수 없어 유물 보상 처리를 중단합니다.");
                    return;
                }
            }

            ArtifactDataSO nextArtifact = artifactRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 유물 보상 처리 시작: {nextArtifact.artifactName}");
            ApplyArtifactEffect(nextArtifact);
            ProcessNextReward(); // 다음 보상이 있는지 재귀적으로 확인
        }
        else if (cardRewardQueue.Count > 0)
        {
            List<CardDataSO> nextCardChoices = cardRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 카드 보상 UI에 선택지를 전달합니다: {nextCardChoices.Count}개");

            // UI 매니저를 직접 호출하는 대신, 이벤트(방송)를 보냅니다.
            OnCardRewardReady?.Invoke(nextCardChoices);
        }
        else
        {
            Debug.Log("[RewardManager] 모든 보상 처리가 완료되었습니다. 루트 선택 화면으로 전환합니다.");

            // CardReward UI를 닫고, RouteSelectionController를 호출하여 맵을 보여줍니다.
            if (CardRewardUIManager.Instance != null)
            {
                CardRewardUIManager.Instance.gameObject.SetActive(false);
            }

            if (RouteSelectionController.Instance != null)
            {
                RouteSelectionController.Instance.Show();
            }
            else
            {
                Debug.LogError("[RewardManager] RouteSelectionController.Instance를 찾을 수 없습니다!");
            }
        }
    }

    /// <summary>
    /// 카드 보상 선택이 완료되었을 때 CardRewardUIManager에 의해 호출됩니다.
    /// </summary>
    public void OnCardRewardConfirmed(CardDataSO selectedCard)
    {
        Debug.Log($"[RewardManager] 플레이어가 카드 보상을 확정했습니다: {selectedCard.cardName}");
        if (CardManager.Instance != null)
        {
            CardManager.Instance.AddCard(selectedCard);
        }
        ProcessNextReward(); // 다음 보상 처리
    }

    /// <summary>
    /// 카드 보상을 포기했을 때 CardRewardUIManager에 의해 호출됩니다.
    /// </summary>
    public void OnCardRewardSkipped()
    {
        Debug.Log("[RewardManager] 플레이어가 카드 보상을 포기했습니다.");
        ProcessNextReward(); // 다음 보상 처리
    }

    private void ApplyArtifactEffect(ArtifactDataSO artifact)
    {
        if (playerStats == null) return;
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
        Debug.Log($"{artifact.artifactName} 효과 적용 완료. 최종 공격력: {playerStats.finalDamage}");
    }
}