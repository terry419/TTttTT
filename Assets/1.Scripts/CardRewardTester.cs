using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardRewardTester : MonoBehaviour
{
    [Tooltip("테스트 시 랜덤으로 뽑을 카드의 개수를 지정합니다.")]
    [SerializeField] private int numberOfCardsToTest = 3;

    IEnumerator Start()
    {
        Debug.Log($"[ 진단 ] CardRewardTester.Start() 코루틴 시작됨. (Frame: {Time.frameCount})");

        yield return null; // 다른 Manager들이 초기화될 때까지 한 프레임 대기

        if (DataManager.Instance == null || RewardManager.Instance == null)
        {
            Debug.LogError("[TESTER] DataManager 또는 RewardManager를 찾을 수 없습니다!");
            yield break;
        }

        // 1. 실제 게임처럼 모든 카드 목록을 가져옵니다.
        List<CardDataSO> allCards = DataManager.Instance.GetAllCards();
        if (allCards.Count < numberOfCardsToTest)
        {
            Debug.LogError("[TESTER] 카드 데이터가 부족하여 테스트를 진행할 수 없습니다.");
            yield break;
        }

        // 2. RoundManager의 로직을 사용하여 가중치 기반으로 랜덤 카드를 뽑습니다.
        List<CardDataSO> randomChoices = new List<CardDataSO>();
        List<CardDataSO> selectableCards = new List<CardDataSO>(allCards);

        for (int i = 0; i < numberOfCardsToTest; i++)
        {
            if (selectableCards.Count == 0) break;

            float totalWeight = selectableCards.Sum(card => card.rewardAppearanceWeight);
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeight = 0f;

            CardDataSO selectedCard = null;
            foreach (var card in selectableCards)
            {
                currentWeight += card.rewardAppearanceWeight;
                if (randomPoint <= currentWeight)
                {
                    selectedCard = card;
                    break;
                }
            }

            // 만약 가중치 합이 0이거나 부동소수점 오류로 선택이 안된 경우, 마지막 카드를 안전하게 선택
            if (selectedCard == null && selectableCards.Count > 0)
            {
                selectedCard = selectableCards[selectableCards.Count - 1];
            }

            if (selectedCard != null)
            {
                randomChoices.Add(selectedCard);
                selectableCards.Remove(selectedCard);
            }
        }

        // 3. 랜덤하게 선택된 카드 목록을 보상 큐에 주입하고, 절차를 시작합니다.
        Debug.Log($"[TESTER] {randomChoices.Count}개의 랜덤 카드를 보상 큐에 추가합니다.");
        RewardManager.Instance.EnqueueReward(randomChoices);
        RewardManager.Instance.ProcessNextReward();
    }
}