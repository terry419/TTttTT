// 파일 경로: Assets/1.Scripts/Gameplay/RewardManager.cs

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 플레이 중 발생하는 모든 '보상' 관련 로직을 총괄하는 중앙 관리자입니다.
/// 라운드 승리/패배 여부를 기록하고, 카드 보상 큐를 관리하며, 보상 UI에 필요한 데이터를 제공합니다.
/// </summary>
public class RewardManager : MonoBehaviour
{
    // --- Public Properties --- //

    /// <summary>
    /// 마지막으로 플레이한 라운드에서 승리했는지 여부를 나타냅니다. (현재는 카드 보상에 직접적인 영향을 주지 않지만, 추후 다른 보상 시스템을 위해 유지됩니다.)
    /// </summary>
    public bool LastRoundWon { get; set; } = true;

    /// <summary>
    /// 현재 플레이어가 카드 보상 선택을 완료했는지 여부를 나타내는 중요한 상태 플래그입니다.
    /// true: 보상 선택이 끝났거나, 받을 보상이 없는 상태. (맵 노드 선택 가능)
    /// false: 보상 선택이 진행 중인 상태. (맵 노드 선택 불가, '보상 페이지로' 버튼 활성화)
    /// </summary>
    public bool IsRewardSelectionComplete { get; private set; } = true;


    // --- Private Fields --- //

    /// <summary>
    /// 제시할 카드 보상 목록을 순서대로 저장하는 큐(Queue)입니다.
    /// </summary>
    private Queue<List<NewCardDataSO>> cardRewardQueue = new Queue<List<NewCardDataSO>>();


    // --- Events --- //

    /// <summary>
    /// 처리할 새로운 카드 보상이 준비되었을 때 CardRewardUIManager에 알리는 static 이벤트입니다.
    /// </summary>
    public static event System.Action<List<NewCardDataSO>> OnCardRewardReady;

    /// <summary>
    /// 처리할 보상이 없어 즉시 맵 선택으로 건너뛰어야 할 때 UI에 알리는 static 이벤트입니다.
    /// </summary>
    public static event System.Action OnRewardSkipped;

    // --- Unity Lifecycle Methods --- //

    void Awake()
    {
        // ServiceLocator에 자기 자신을 등록하여 다른 시스템에서 접근할 수 있도록 합니다.
        if (!ServiceLocator.IsRegistered<RewardManager>())
        {
            ServiceLocator.Register<RewardManager>(this);
            // 씬이 전환되어도 파괴되지 않도록 설정합니다.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Public Methods --- //

    /// <summary>
    /// 새로운 카드 보상 목록을 큐의 맨 뒤에 추가합니다.
    /// </summary>
    /// <param name="cardChoices">플레이어에게 보여줄 카드 선택지 목록</param>
    public void EnqueueReward(List<NewCardDataSO> cardChoices)
    {
        cardRewardQueue.Enqueue(cardChoices);
        Debug.Log($"[RewardManager] 새로운 카드 보상이 큐에 추가되었습니다. 현재 대기 중인 보상 수: {cardRewardQueue.Count}");
    }

    /// <summary>
    /// 처리 대기 중인 보상이 있는지 확인합니다.
    /// </summary>
    /// <returns>큐에 보상이 하나 이상 있으면 true, 아니면 false를 반환합니다.</returns>
    public bool HasPendingRewards()
    {
        return cardRewardQueue.Count > 0;
    }

    /// <summary>
    /// 큐에서 다음 보상을 꺼내어 처리하도록 이벤트를 발생시킵니다.
    /// </summary>
    public void ProcessNextReward()
    {
        Debug.Log("[RewardManager] ProcessNextReward() 호출됨.");
        if (cardRewardQueue.Count > 0)
        {
            // [핵심 로직] 보상 처리를 시작하므로, '선택 완료' 상태를 '진행 중' (false)으로 변경합니다.
            // 이 상태는 RouteSelectionController가 '보상 페이지로' 버튼을 활성화하는 데 사용됩니다.
            IsRewardSelectionComplete = false;

            List<NewCardDataSO> nextReward = cardRewardQueue.Dequeue();
            Debug.Log($"[RewardManager] 다음 보상을 처리합니다. 남은 보상 수: {cardRewardQueue.Count}. 'IsRewardSelectionComplete' 상태를 [false]로 설정.");

            // UI가 이벤트를 구독하고 있다면, 카드 선택지를 전달하여 화면에 표시하도록 합니다.
            Debug.Log("[RewardManager] OnCardRewardReady 이벤트 발생 시도.");
            OnCardRewardReady?.Invoke(nextReward);
        }
        else
        {
            // 처리할 보상이 없는 경우, 바로 완료 상태로 설정하고 '스킵' 이벤트를 발생시킵니다.
            IsRewardSelectionComplete = true;
            Debug.LogWarning("[RewardManager] 처리할 보상이 없어 즉시 맵 선택으로 건너뜁니다. 'IsRewardSelectionComplete' 상태를 [true]로 설정.");
            OnRewardSkipped?.Invoke();
        }
    }

    /// <summary>
    /// 플레이어가 카드 선택(획득, 합성, 스킵)을 완료했을 때 호출됩니다.
    /// </summary>
    public void CompleteRewardSelection()
    {
        // [핵심 로직] 보상 선택이 모두 끝났으므로, 상태를 '완료' (true)로 변경합니다.
        // 이 상태는 RouteSelectionController가 '보상 페이지로' 버튼을 비활성화하는 데 사용됩니다.
        IsRewardSelectionComplete = true;
        Debug.Log("[RewardManager] 카드 보상 선택이 완료되었습니다. 'IsRewardSelectionComplete' 상태를 [true]로 설정.");
    }
}