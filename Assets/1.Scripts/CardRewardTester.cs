using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 카드 보상 시스템을 테스트하기 위한 클래스입니다.
/// 게임 실행 중 F5 키를 누르면 Inspector에 설정된 개수만큼 랜덤 카드를 생성하여 RewardManager에 전달합니다.
/// 이 스크립트는 정식 게임 로직이 아니므로, 테스트 시에만 활성화하는 것을 권장합니다.
/// </summary>
public class CardRewardTester : MonoBehaviour
{
    [Tooltip("테스트 시 보상으로 주어질 카드의 개수를 정합니다.")]
    [SerializeField] private int numberOfCardsToTest = 3;

    /// <summary>
    /// 매 프레임 키 입력을 감지합니다.
    /// </summary>
    void Update()
    {
        // F5 키를 누르면 테스트 보상 생성 함수를 호출합니다.
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[TESTER] F5 키 입력 감지. 테스트 보상을 생성합니다.");
            StartCoroutine(GenerateTestRewardCoroutine());
        }
    }

    /// <summary>
    /// RoundManager의 로직과 유사한 방식으로 테스트용 카드 보상을 생성하고 RewardManager에 전달합니다.
    /// </summary>
    private IEnumerator GenerateTestRewardCoroutine()
    {
        // 다른 Manager들이 준비될 시간을 줍니다.
        yield return null;

        var rewardManager = ServiceLocator.Get<RewardManager>();

        // 필수 매니저가 있는지 확인
        if (ServiceLocator.Get<DataManager>() == null || rewardManager == null)
        {
            Debug.LogError("[TESTER] DataManager 또는 RewardManager를 찾을 수 없어 테스트를 진행할 수 없습니다!");
            yield break;
        }

        // 1. DataManager에서 모든 카드 목록을 가져옵니다.
        List<CardDataSO> allCards = ServiceLocator.Get<DataManager>().GetAllCards();
        if (allCards.Count < numberOfCardsToTest)
        {
            Debug.LogError("[TESTER] 카드 데이터가 부족하여 테스트를 진행할 수 없습니다.");
            yield break;
        }

        // 2. 가중치에 따라 랜덤 카드를 선택합니다. (중복 없이 N개의 '선택지'를 만드는 로직)
        List<CardDataSO> randomChoices = new List<CardDataSO>();
        List<CardDataSO> selectableCards = new List<CardDataSO>(allCards);

        for (int i = 0; i < numberOfCardsToTest; i++)
        {
            if (selectableCards.Count == 0) break;

            float totalWeight = selectableCards.Sum(card => card.rewardAppearanceWeight);

            // 가중치가 모두 0일 경우, 일반 랜덤으로 선택
            if (totalWeight <= 0)
            {
                int randomIndex = Random.Range(0, selectableCards.Count);
                randomChoices.Add(selectableCards[randomIndex]);
                selectableCards.RemoveAt(randomIndex);
                continue;
            }

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

            // 부동소수점 오류 등으로 카드가 선택되지 않았을 경우, 마지막 카드를 안전하게 선택
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

        // 3. 생성된 카드 목록을 RewardManager의 큐에 추가하고, 보상 처리를 즉시 시작합니다.
        if (randomChoices.Count > 0)
        {
            Debug.Log($"[TESTER] {randomChoices.Count}개의 테스트 카드 보상을 큐에 추가합니다.");
            rewardManager.EnqueueReward(randomChoices);
            rewardManager.ProcessNextReward();
        }
    }
}