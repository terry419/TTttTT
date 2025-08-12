using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 아티팩트 및 카드 보상 큐를 중앙에서 관리하고, 보상 과정을 제어합니다.
/// 기획서에 명시된 FIFO(선입선출) 규칙을 철저히 따르며, 유물 보상을 항상 카드 보상보다 우선 처리합니다.
/// 이 클래스는 싱글톤으로 구현되어 게임의 어느 곳에서든 보상을 추가하고 처리할 수 있습니다.
/// </summary>
public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    private readonly Queue<ArtifactDataSO> artifactRewardQueue = new Queue<ArtifactDataSO>();
    private readonly Queue<List<CardDataSO>> cardRewardQueue = new Queue<List<CardDataSO>>();

    private CharacterStats playerStats; // 플레이어 능력치 참조

    public bool IsRewardQueueEmpty => artifactRewardQueue.Count == 0 && cardRewardQueue.Count == 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 보상 큐에 새로운 유물을 추가합니다.
    /// </summary>
    public void EnqueueReward(ArtifactDataSO artifact)
    {
        if (artifact == null) 
        {
            Debug.LogError("[RewardManager] Null인 유물을 보상 큐에 추가할 수 없습니다.");
            return;
        }
        artifactRewardQueue.Enqueue(artifact);
        Debug.Log($"[RewardManager] 유물 보상 추가: {artifact.artifactName}");
    }

    /// <summary>
    /// 보상 큐에 새로운 카드 선택지를 추가합니다. (3장 중 1택)
    /// </summary>
    public void EnqueueReward(List<CardDataSO> cardChoices)
    {
        if (cardChoices == null || cardChoices.Count == 0)
        {
            Debug.LogError("[RewardManager] Null이거나 비어있는 카드 목록을 보상 큐에 추가할 수 없습니다.");
            return;
        }
        cardRewardQueue.Enqueue(cardChoices);
        Debug.Log($"[RewardManager] 카드 보상 선택지 ({cardChoices.Count}개) 추가");
    }

    /// <summary>
    /// 다음 보상 과정을 시작합니다. GameManager가 Reward 상태로 전환될 때 호출될 수 있습니다.
    /// </summary>
    public void ProcessNextReward()
    {
        // 플레이어 능력치 참조가 없으면 찾아옵니다.
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<CharacterStats>(); // Player 태그를 가진 오브젝트에서 찾기
            if (playerStats == null)
            {
                Debug.LogError("[RewardManager] 플레이어의 CharacterStats를 찾을 수 없어 보상 처리를 중단합니다.");
                return;
            }
        }

        // 기획서 규칙: 유물 보상을 항상 우선 처리합니다.
        if (artifactRewardQueue.Count > 0)
        {
            ArtifactDataSO nextArtifact = artifactRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 유물 보상 처리 시작: {nextArtifact.artifactName}");
            
            // 유물 획득 로직 실행: 플레이어 능력치에 직접 적용
            ApplyArtifactEffect(nextArtifact);

            // 유물 획득 후, 다음 보상이 있는지 재귀적으로 다시 확인합니다.
            ProcessNextReward();
        }
        else if (cardRewardQueue.Count > 0)
        {
            List<CardDataSO> nextCardChoices = cardRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 카드 보상 처리 시작: {nextCardChoices.Count}개의 선택지 제공");

            if (CardRewardController.Instance != null)
            {
                // CardRewardController에 카드 목록을 전달하여 UI 표시 요청
                CardRewardController.Instance.Initialize(nextCardChoices);
            }
            else
            { 
                Debug.LogError("[RewardManager] CardRewardController 인스턴스를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.Log("[RewardManager] 모든 보상 처리가 완료되었습니다.");
            // TODO: 맵(루트 선택) 화면으로 전환하거나, 다음 라운드를 시작하는 로직 호출
            // GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        }
    }

    private void ApplyArtifactEffect(ArtifactDataSO artifact)
    {
        if (playerStats == null) return;

        // 유물 효과를 플레이어의 artifact...Ratio 변수에 더합니다.
        playerStats.artifactDamageRatio += artifact.attackBoostRatio;
        playerStats.artifactHealthRatio += artifact.healthBoostRatio;
        playerStats.artifactMoveSpeedRatio += artifact.moveSpeedBoostRatio;
        playerStats.artifactCritRateRatio += artifact.critChanceBoostRatio;
        playerStats.artifactCritDamageRatio += artifact.critDamageBoostRatio;
        // TODO: lifesteal 등 다른 효과들도 추가

        // CardManager의 슬롯 수도 변경
        if (CardManager.Instance != null)
        {
            CardManager.Instance.maxOwnedSlots += artifact.ownedCardSlotBonus;
        }

        // 능력치 변경 후에는 반드시 최종 능력치 재계산을 호출해야 합니다.
        playerStats.CalculateFinalStats();
        Debug.Log($"{artifact.artifactName} 효과 적용 완료. 최종 공격력: {playerStats.finalDamage}");
    }

    /// <summary>
    /// 카드 보상 선택이 완료되었을 때 CardRewardController에 의해 호출됩니다.
    /// </summary>
    public void OnCardRewardConfirmed(CardDataSO selectedCard)
    {
        Debug.Log($"[RewardManager] 플레이어가 카드 보상을 확정했습니다: {selectedCard.cardName}");

        // CardManager에 카드 추가 요청
        CardManager.Instance.AddCard(selectedCard);

        // 현재 카드 보상 절차가 끝났으므로, 다음 보상을 처리합니다.
        ProcessNextReward();
    }

    /// <summary>
    /// 카드 보상을 포기했을 때 CardRewardController에 의해 호출됩니다.
    /// </summary>
    public void OnCardRewardSkipped()
    {
        Debug.Log("[RewardManager] 플레이어가 카드 보상을 포기했습니다.");
        // 현재 카드 보상 절차가 끝났으므로, 다음 보상을 처리합니다.
        ProcessNextReward();
    }
}
