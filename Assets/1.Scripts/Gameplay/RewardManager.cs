// 경로: Assets/1.Scripts/Gameplay/RewardManager.cs
// [재수정됨] CardRewardTester와의 호환성을 위해 수동 보상 처리 함수(ProcessNextReward)를 복원했습니다.

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    private Queue<List<CardDataSO>> cardRewardQueue = new Queue<List<CardDataSO>>();

    public static event System.Action<List<CardDataSO>> OnCardRewardReady;

    public bool IsRewardSelectionComplete { get; private set; } = true;

    // [추가] 자동/수동 처리가 한 프레임에 중복 실행되는 것을 방지하기 위한 변수
    private int lastProcessedFrame = -1;

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

    // 일반적인 게임 흐름에서는 이 Update 함수가 자동으로 보상을 처리합니다.
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "CardReward" && cardRewardQueue.Count > 0)
        {
            // 이미 수동으로 처리했다면, Update에서 중복 처리하지 않도록 방지합니다.
            if (Time.frameCount == lastProcessedFrame) return;

            ProcessNextRewardInternal("자동");
        }
    }

    public void EnqueueReward(List<CardDataSO> cardChoices)
    {
        cardRewardQueue.Enqueue(cardChoices);
        IsRewardSelectionComplete = false;
        Debug.Log("[RewardManager] 보상이 추가되었습니다. IsRewardSelectionComplete = false");
    }

    // [복원] CardRewardTester에서 호출할 수 있도록 ProcessNextReward 함수를 다시 만듭니다.
    public void ProcessNextReward()
    {
        ProcessNextRewardInternal("수동");
    }

    // [추가] 실제 보상 처리 로직을 내부 함수로 분리하여 중복을 줄입니다.
    private void ProcessNextRewardInternal(string triggerType)
    {
        if (cardRewardQueue.Count > 0)
        {
            Debug.Log($"[RewardManager] 다음 보상을 처리합니다. (호출: {triggerType})");
            List<CardDataSO> choices = cardRewardQueue.Dequeue();
            OnCardRewardReady?.Invoke(choices);
            lastProcessedFrame = Time.frameCount; // 처리된 프레임을 기록
        }
        else
        {
            Debug.LogWarning($"[RewardManager] ProcessNextReward가 {triggerType}으로 호출되었지만 큐에 보상이 없습니다.");
        }
    }

    public void CompleteRewardSelection()
    {
        IsRewardSelectionComplete = true;
        Debug.Log("[RewardManager] 보상 선택이 완료되었습니다. IsRewardSelectionComplete = true");
    }
}
