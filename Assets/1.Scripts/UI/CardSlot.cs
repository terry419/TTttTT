using UnityEngine;
using UnityEngine.UI;
using System;

public class CardSlot : MonoBehaviour
{
    public enum SlotState { Occupied, Empty, Locked }

    [Header("상태별 게임 오브젝트")]
    [SerializeField] private GameObject cardDisplayObject;
    [SerializeField] private GameObject emptyStateObject;
    [SerializeField] private GameObject lockedStateObject;

    // [수정] HighlightBorder 관련 필드와 함수를 모두 제거했습니다.
    // [SerializeField] private Image highlightBorder; 

    public CardInstance currentCard { get; private set; }
    public SlotState currentState { get; private set; }
    private CardDisplay cardDisplay;
    private Button button;
    public event Action<CardSlot> OnSlotClicked;

    void Awake()
    {
        button = GetComponent<Button>();
        // [로그 추가] 버튼 컴포넌트를 찾았는지, 리스너를 추가하는지 확인
        if (button != null)
        {
            Debug.Log($"[{gameObject.name}] CardSlot.Awake(): Button 컴포넌트를 찾았으며, 클릭 리스너를 추가합니다.");
            button.onClick.AddListener(() =>
            {
                // [로그 추가] 버튼이 실제로 클릭되었을 때 이벤트가 발생하는지 확인
                Debug.Log($"[{gameObject.name}] Button clicked! OnSlotClicked 이벤트를 발생시킵니다.");
                OnSlotClicked?.Invoke(this);
            });
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] CardSlot.Awake(): Button 컴포넌트를 찾지 못했습니다! Inspector를 확인해주세요.");
        }

        if (cardDisplayObject != null) cardDisplay = cardDisplayObject.GetComponent<CardDisplay>();
    }
    public void Setup(CardInstance card)
    {
        currentCard = card;
        if (card != null)
        {
            SetState(SlotState.Occupied);
            if (cardDisplay != null) cardDisplay.Setup(card);
        }
        else
        {
            SetState(SlotState.Empty);
        }
    }

    public void SetState(SlotState newState)
    {
        currentState = newState;
        if (cardDisplayObject != null) cardDisplayObject.SetActive(newState == SlotState.Occupied);
        if (emptyStateObject != null) emptyStateObject.SetActive(newState == SlotState.Empty);
        if (lockedStateObject != null) lockedStateObject.SetActive(newState == SlotState.Locked);
        if (button != null) button.interactable = (newState != SlotState.Locked);
    }
}