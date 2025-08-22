using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CardRewardTester : MonoBehaviour
{
    [Tooltip("�׽�Ʈ �� �������� ���� ī���� ������ �����մϴ�.")]
    [SerializeField] private int numberOfCardsToTest = 3;

    IEnumerator Start()
    {
        Debug.Log($"[ ���� ] CardRewardTester.Start() �ڷ�ƾ ���۵�. (Frame: {Time.frameCount})");

        yield return null; // �ٸ� Manager���� �ʱ�ȭ�� ������ �� ������ ���

        if (ServiceLocator.Get<DataManager>() == null || RewardManager.Instance == null)
        {
            Debug.LogError("[TESTER] DataManager �Ǵ� RewardManager�� ã�� �� �����ϴ�!");
            yield break;
        }

        // 1. ���� ����ó�� ��� ī�� ����� �����ɴϴ�.
        List<CardDataSO> allCards = ServiceLocator.Get<DataManager>().GetAllCards();
        if (allCards.Count < numberOfCardsToTest)
        {
            Debug.LogError("[TESTER] ī�� �����Ͱ� �����Ͽ� �׽�Ʈ�� ������ �� �����ϴ�.");
            yield break;
        }

        // 2. RoundManager�� ������ ����Ͽ� ����ġ ������� ���� ī�带 �̽��ϴ�.
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

            // ���� ����ġ ���� 0�̰ų� �ε��Ҽ��� ������ ������ �ȵ� ���, ������ ī�带 �����ϰ� ����
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

        // 3. �����ϰ� ���õ� ī�� ����� ���� ť�� �����ϰ�, ������ �����մϴ�.
        Debug.Log($"[TESTER] {randomChoices.Count}���� ���� ī�带 ���� ť�� �߰��մϴ�.");
        RewardManager.Instance.EnqueueReward(randomChoices);
        RewardManager.Instance.ProcessNextReward();
    }
}