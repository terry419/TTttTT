using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 카드 인벤토리 UI의 표시, 숨김, 데이터 채우기 등 전반적인 동작을 제어하는 클래스입니다.
/// 에디터에 미리 배치된 슬롯들을 사용하며, '선택-선택-교체' 로직과 자체 테스트 모드를 지원합니다.
/// </summary>
public class CardInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("에디터에서 직접 배치한 장착 카드 슬롯들을 순서대로 연결하세요.")]
    [SerializeField] private List<CardDisplay> equippedCardSlots = new List<CardDisplay>();
    [Tooltip("에디터에서 직접 배치한 소유 카드 슬롯들을 순서대로 연결하세요.")]
    [SerializeField] private List<CardDisplay> ownedCardSlots = new List<CardDisplay>();

    [Header("Debug / Test Mode")]
    [Tooltip("체크하면 CardManager 없이 가짜 데이터로 UI를 테스트합니다.")]
    [SerializeField] private bool testMode = false;
    [SerializeField] private List<NewCardDataSO> testCards; // 테스트에 사용할 카드 데이터 에셋들

    private CardManager cardManager;
    private bool isCurrentlyEditable;
    private CardDisplay firstSelectedSlot = null;

    void Start()
    {
        // 테스트 모드는 다른 시스템 없이 이 UI만 독립적으로 실행하고 검증하기 위해 사용됩니다.
        if (testMode)
        {
            RunTestMode();
        }
        else
        {
            mainPanel?.SetActive(false);
        }
    }

    public void Show(CardManager manager, bool isEditable)
    {
        this.cardManager = manager;
        this.isCurrentlyEditable = isEditable;
        firstSelectedSlot = null; // 패널을 열 때마다 선택 상태를 초기화합니다.

        if (mainPanel == null) return;

        RefreshAllSlots();
        mainPanel.SetActive(true);
    }

    public void Hide()
    {
        mainPanel?.SetActive(false);
    }

    /// <summary>
    /// 모든 슬롯의 UI를 현재 데이터에 맞게 새로고침합니다.
    /// </summary>
    private void RefreshAllSlots()
    {
        if (testMode)
        {
            SetupSlots(equippedCardSlots, _testEquippedCards, false, _testMaxEquippedSlots);
            SetupSlots(ownedCardSlots, _testOwnedCards, true, _testMaxOwnedSlots);
        }
        else
        {
            if (cardManager == null) return;
            SetupSlots(equippedCardSlots, cardManager.equippedCards, false, cardManager.maxEquipSlots);
            SetupSlots(ownedCardSlots, cardManager.ownedCards, true, cardManager.maxOwnedSlots);
        }
    }

    private void SetupSlots(List<CardDisplay> uiSlots, List<CardInstance> cards, bool canBeLocked, int maxSlots)
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            CardDisplay uiSlot = uiSlots[i];
            CardInstance cardInstance = (i < cards.Count) ? cards[i] : null;
            bool isLocked = canBeLocked && (i >= maxSlots);

            uiSlot.Setup(cardInstance, isLocked, this.isCurrentlyEditable);
            uiSlot.OnCardSelected.RemoveAllListeners(); // 이전 리스너를 제거하여 중복 방지
            uiSlot.OnCardSelected.AddListener(OnSlotSelected);
        }
    }

    /// <summary>
    /// 카드 슬롯이 선택되었을 때 호출되는 메인 핸들러 함수입니다.
    /// </summary>
    public void OnSlotSelected(CardDisplay selectedSlot)
    {
        if (!isCurrentlyEditable) return;

        if (firstSelectedSlot == null)
        {
            // 첫 번째 선택. 비어있는 슬롯은 교체할 카드가 없으므로 선택할 수 없습니다.
            if (selectedSlot.IsEmpty) return;
            
            firstSelectedSlot = selectedSlot;
            firstSelectedSlot.SetHighlight(true);
        }
        else if (firstSelectedSlot == selectedSlot)
        {
            // 같은 카드를 다시 선택하면 선택 취소
            firstSelectedSlot.SetHighlight(false);
            firstSelectedSlot = null;
        }
        else
        {
            // 두 번째 카드 선택 (교체 실행)
            PerformSwap(firstSelectedSlot, selectedSlot);

            // 교체 후, 선택 상태 초기화 및 UI 새로고침
            firstSelectedSlot.SetHighlight(false);
            firstSelectedSlot = null;
            RefreshAllSlots();
        }
    }

    private void PerformSwap(CardDisplay fromSlot, CardDisplay toSlot)
    {
        Debug.Log($"'{fromSlot.CurrentCardInstance.CardData.basicInfo.cardName}'와(과) '{toSlot.name}' 슬롯 교체를 시도합니다.");

        if (testMode)
        {
            PerformSwap_Test(fromSlot, toSlot);
            return;
        }

        // TODO: CardManager에 카드 교환을 위한 전용 함수를 만들고 호출해야 합니다.
        // 현재 CardManager의 Equip/Unequip 함수만으로는 복잡한 교환 로직을 안정적으로 구현하기 어렵습니다.
        // 다음 단계에서 CardManager에 SwapCards(cardA, cardB)와 같은 함수를 추가하는 것을 제안합니다.
        Debug.LogWarning("실제 카드 교환 로직은 CardManager에 Swap 함수 구현 후 연동해야 합니다.");
    }

    #region Test Mode
    private List<CardInstance> _testEquippedCards = new List<CardInstance>();
    private List<CardInstance> _testOwnedCards = new List<CardInstance>();
    private int _testMaxEquippedSlots = 5;
    private int _testMaxOwnedSlots = 2;

    private void RunTestMode()
    {
        Debug.LogWarning("--- 인벤토리 UI 테스트 모드 실행 ---");
        
        // 가짜 카드 데이터 생성
        for (int i = 0; i < 3; i++)
        {
            if(testCards.Count > i) _testEquippedCards.Add(new CardInstance(testCards[i]));
        }
        for (int i = 3; i < 5; i++)
        {
            if(testCards.Count > i) _testOwnedCards.Add(new CardInstance(testCards[i]));
        }

        Show(null, true); // 가짜 데이터로 UI 표시
    }

    private void PerformSwap_Test(CardDisplay fromSlot, CardDisplay toSlot)
    {
        CardInstance fromCard = fromSlot.CurrentCardInstance;
        bool isFromEquipped = equippedCardSlots.Contains(fromSlot);
        var fromList = isFromEquipped ? _testEquippedCards : _testOwnedCards;
        int fromIndex = equippedCardSlots.Contains(fromSlot) ? equippedCardSlots.IndexOf(fromSlot) : ownedCardSlots.IndexOf(fromSlot);

        CardInstance toCard = toSlot.CurrentCardInstance;
        bool isToEquipped = equippedCardSlots.Contains(toSlot);
        var toList = isToEquipped ? _testEquippedCards : _testOwnedCards;
        int toIndex = equippedCardSlots.Contains(toSlot) ? equippedCardSlots.IndexOf(toSlot) : ownedCardSlots.IndexOf(toSlot);

        // Swap logic for test lists
        if (isFromEquipped == isToEquipped)
        {
            // 같은 목록 내에서 교환
            var temp = fromList[fromIndex];
            fromList[fromIndex] = toList[toIndex];
            toList[toIndex] = temp;
        }
        else
        {
            // 장착/소유 목록 간 교환
            fromList.Remove(fromCard);
            if(toCard != null) toList.Remove(toCard);

            if(fromCard != null) toList.Insert(toIndex, fromCard);
            if(toCard != null) fromList.Insert(fromIndex, toCard);
        }
    }
    #endregion
}
