using UnityEngine;
using UnityEngine.UI;
using System;

public class CardSlot : MonoBehaviour
{
    public enum SlotState { Occupied, Empty, Locked }

    [Header("���º� ���� ������Ʈ")]
    [SerializeField] private GameObject cardDisplayObject;
    [SerializeField] private GameObject emptyStateObject;
    [SerializeField] private GameObject lockedStateObject;

    // [����] HighlightBorder ���� �ʵ�� �Լ��� ��� �����߽��ϴ�.
    // [SerializeField] private Image highlightBorder; 

    public CardInstance currentCard { get; private set; }
    public SlotState currentState { get; private set; }
    private CardDisplay cardDisplay;
    private Button button;
    public event Action<CardSlot> OnSlotClicked;

    void Awake()
    {
        button = GetComponent<Button>();
        // [�α� �߰�] ��ư ������Ʈ�� ã�Ҵ���, �����ʸ� �߰��ϴ��� Ȯ��
        if (button != null)
        {
            Debug.Log($"[{gameObject.name}] CardSlot.Awake(): Button ������Ʈ�� ã������, Ŭ�� �����ʸ� �߰��մϴ�.");
            button.onClick.AddListener(() =>
            {
                // [�α� �߰�] ��ư�� ������ Ŭ���Ǿ��� �� �̺�Ʈ�� �߻��ϴ��� Ȯ��
                Debug.Log($"[{gameObject.name}] Button clicked! OnSlotClicked �̺�Ʈ�� �߻���ŵ�ϴ�.");
                OnSlotClicked?.Invoke(this);
            });
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] CardSlot.Awake(): Button ������Ʈ�� ã�� ���߽��ϴ�! Inspector�� Ȯ�����ּ���.");
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