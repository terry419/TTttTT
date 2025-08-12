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

    // 유물 보상을 저장하는 FIFO 큐
    private readonly Queue<ArtifactDataSO> artifactRewardQueue = new Queue<ArtifactDataSO>();
    // 카드 보상을 저장하는 FIFO 큐
    private readonly Queue<CardDataSO> cardRewardQueue = new Queue<CardDataSO>();

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
    /// <param name="artifact">추가할 유물 데이터</param>
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
    /// 보상 큐에 새로운 카드를 추가합니다.
    /// </summary>
    /// <param name="card">추가할 카드 데이터</param>
    public void EnqueueReward(CardDataSO card)
    {
        if (card == null) 
        {
            Debug.LogError("[RewardManager] Null인 카드를 보상 큐에 추가할 수 없습니다.");
            return;
        }
        cardRewardQueue.Enqueue(card);
        Debug.Log($"[RewardManager] 카드 보상 추가: {card.cardName}");
    }

    /// <summary>
    /// 다음 보상 과정을 시작합니다. GameManager가 Reward 상태로 전환될 때 호출될 수 있습니다.
    /// </summary>
    public void ProcessNextReward()
    {
        // 기획서 규칙: 유물 보상을 항상 우선 처리합니다.
        if (artifactRewardQueue.Count > 0)
        {
            ArtifactDataSO nextArtifact = artifactRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 유물 보상 처리 시작: {nextArtifact.artifactName}");
            
            // TODO: 유물 획득 로직 실행
            // 이 부분은 플레이어의 유물 목록에 직접 추가하거나, 유물 획득 전용 UI를 띄우는 방식으로 구현될 수 있습니다.
            // Player.Instance.AddArtifact(nextArtifact);

            // 유물 획득 후, 다음 보상이 있는지 다시 확인합니다.
            ProcessNextReward();
        }
        else if (cardRewardQueue.Count > 0)
        {
            CardDataSO nextCard = cardRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 카드 보상 처리 시작: {nextCard.cardName}");

            // 카드 보상 UI를 활성화하고, 선택할 카드 목록을 전달합니다.
            // CardRewardController는 이 데이터를 받아 UI를 구성합니다.
            if (CardRewardController.Instance != null)
            {
                // TODO: CardRewardController에 카드 목록을 전달하는 메서드 호출
                // 현재는 단일 카드만 처리하지만, 향후 여러 카드 중 선택하는 로직으로 확장해야 합니다.
                // List<CardDataSO> cardChoices = GenerateCardChoices();
                // CardRewardController.Instance.Initialize(cardChoices);
            }
            else
            {
                Debug.LogError("[RewardManager] CardRewardController 인스턴스를 찾을 수 없습니다!");
            }
        }
        else
        {
            // 모든 보상 처리가 끝났을 때의 로직
            Debug.Log("[RewardManager] 모든 보상 처리가 완료되었습니다.");
            // TODO: 맵(루트 선택) 화면으로 전환하거나, 다음 라운드를 시작하는 로직 호출
            // GameManager.Instance.ChangeState(GameManager.GameState.MapSelect);
        }
    }

    /// <summary>
    /// 카드 보상 선택이 완료되었을 때 CardRewardController에 의해 호출됩니다.
    /// </summary>
    /// <param name="selectedCard">플레이어가 선택한 카드</param>
    public void OnCardRewardConfirmed(CardDataSO selectedCard)
    {
        Debug.Log($"[RewardManager] 플레이어가 카드 보상을 확정했습니다: {selectedCard.cardName}");

        // TODO: 슬롯 과부하 처리 로직 (기획서 내용)
        // CardManager.Instance.AddCard(selectedCard);

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
