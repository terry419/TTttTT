using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 인벤토리의 개별 카드 슬롯 하나하나를 제어하는 컴포넌트입니다.
/// [3단계 리팩토링] 이제 CardDisplay 프리팹을 생성하고 관리하는 역할만 담당합니다.
/// </summary>
public class CardSlot : MonoBehaviour
{
    public enum SlotState { Occupied, Empty, Locked }

    [Header("프리팹 부모")]
    [SerializeField] private Transform cardParent; // 생성된 CardDisplay 프리팹이 위치할 부모 Transform

    [Header("상태 UI")]
    [SerializeField] private GameObject emptyStateUI;
    [SerializeField] private GameObject lockedStateUI;
    [SerializeField] private Image highlightBorder;

    [Header("슬롯 정보")]
    public CardInstance currentCard { get; private set; }
    public SlotState currentState { get; private set; }
    public bool isEquipSlot;

    // 생성된 CardDisplay 프리팹의 인스턴스를 저장해둘 변수
    private CardDisplay currentCardDisplay;

    public event Action<CardSlot> OnSlotClicked;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnSlotClicked?.Invoke(this));
        }
        SetHighlight(false);
    }

    /// <summary>
    /// 카드 인스턴스 데이터로 슬롯을 설정합니다. CardDisplay 프리팹을 생성하거나 제거합니다.
    /// </summary>
    /// <param name="card">표시할 카드 데이터. 없으면 null</param>
    /// <param name="cardPrefab">생성할 CardDisplay 프리팹</param>
    public void Setup(CardInstance card, GameObject cardPrefab)
    {
        currentCard = card;

        // 1. 기존에 있던 카드 프리팹이 있다면 먼저 파괴합니다.
        if (currentCardDisplay != null)
        {
            Destroy(currentCardDisplay.gameObject);
            currentCardDisplay = null;
        }

        // 2. 표시할 카드가 있는지 확인합니다.
        if (card != null)
        {
            SetState(SlotState.Occupied);
            if (cardPrefab != null)
            {
                // CardDisplay 프리팹을 cardParent 아래에 생성합니다.
                GameObject cardGO = Instantiate(cardPrefab, cardParent);
                currentCardDisplay = cardGO.GetComponent<CardDisplay>();
                if (currentCardDisplay != null)
                {
                    // 생성된 프리팹에 카드 데이터를 넘겨 UI를 채우도록 합니다.
                    currentCardDisplay.Setup(card);
                }
            }
        }
        else
        {
            SetState(SlotState.Empty);
        }
    }

    public void SetState(SlotState newState)
    {
        currentState = newState;

        if (emptyStateUI != null) emptyStateUI.SetActive(newState == SlotState.Empty);
        if (lockedStateUI != null) lockedStateUI.SetActive(newState == SlotState.Locked);

        if (button != null)
        {
            button.interactable = (newState != SlotState.Locked);
        }
    }

    public void SetHighlight(bool show)
    {
        if (highlightBorder != null)
        {
            highlightBorder.gameObject.SetActive(show);
        }
    }
}