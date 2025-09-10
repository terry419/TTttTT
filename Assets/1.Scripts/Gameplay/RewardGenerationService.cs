using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 카드 보상 생성 로직을 전문적으로 처리하는 서비스입니다.
/// </summary>
public class RewardGenerationService : MonoBehaviour
{
    private DataManager dataManager;

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<RewardGenerationService>())
        {
            ServiceLocator.Register<RewardGenerationService>(this);
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        dataManager = ServiceLocator.Get<DataManager>();
    }

    /// <summary>
    /// 지정된 개수만큼 카드 보상 목록을 생성합니다.
    /// 중복 카드가 나올 경우 가중치가 감쇠하는 규칙을 따릅니다.
    /// </summary>
    /// <param name="count">생성할 보상 카드의 수</param>
    /// <returns>생성된 카드 보상 목록</returns>
    public List<NewCardDataSO> GenerateRewards(int count)
    {
        if (dataManager == null)
        {
            Debug.LogError("[RewardGen] DataManager is not available.");
            return new List<NewCardDataSO>();
        }

        List<NewCardDataSO> allCards = dataManager.GetAllNewCards();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("[RewardGen] No cards available for reward generation.");
            return new List<NewCardDataSO>();
        }

        var rewards = new List<NewCardDataSO>();
        var pickedCardCounts = new Dictionary<NewCardDataSO, int>();

        for (int i = 0; i < count; i++)
        {
            var tempWeights = new Dictionary<NewCardDataSO, float>();
            foreach (var card in allCards)
            {
                // 기본 가중치가 0 이하면 보상 후보에서 제외
                if (card.selectionWeight <= 0) continue;

                float currentWeight = card.selectionWeight;
                if (pickedCardCounts.TryGetValue(card, out int pickedCount))
                {
                    // 가중치 감쇠 적용: 기본 가중치 / 4^(뽑힌 횟수)
                    currentWeight /= Mathf.Pow(4, pickedCount);
                }
                tempWeights.Add(card, currentWeight);
            }

            NewCardDataSO selectedCard = SelectCardWithWeightedRandom(tempWeights);

            if (selectedCard != null)
            {
                rewards.Add(selectedCard);
                // 뽑힌 카드 횟수 업데이트
                if (pickedCardCounts.ContainsKey(selectedCard))
                {
                    pickedCardCounts[selectedCard]++;
                }
                else
                {
                    pickedCardCounts.Add(selectedCard, 1);
                }
            }
        }

        return rewards;
    }

    private NewCardDataSO SelectCardWithWeightedRandom(Dictionary<NewCardDataSO, float> weights)
    {
        if (weights == null || weights.Count == 0)
        {
            return null;
        }

        float totalWeight = weights.Values.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        foreach (var entry in weights)
        {
            currentWeightSum += entry.Value;
            if (randomValue <= currentWeightSum)
            { 
                return entry.Key;
            }
        }

        // 부동소수점 오류 등으로 인해 선택되지 않았을 경우, 마지막 카드를 안전하게 반환
        return weights.Keys.LastOrDefault();
    }
}
