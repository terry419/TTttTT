using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// �κ��丮�� ���� ī�� ���� �ϳ��ϳ��� �����ϴ� ������Ʈ�Դϴ�.
/// [3�ܰ� �����丵] ���� CardDisplay �������� �����ϰ� �����ϴ� ���Ҹ� ����մϴ�.
/// </summary>
public class CardSlot : MonoBehaviour
{
    public enum SlotState { Occupied, Empty, Locked }

    [Header("������ �θ�")]
    [SerializeField] private Transform cardParent; // ������ CardDisplay �������� ��ġ�� �θ� Transform

    [Header("���� UI")]
    [SerializeField] private GameObject emptyStateUI;
    [SerializeField] private GameObject lockedStateUI;
    [SerializeField] private Image highlightBorder;

    [Header("���� ����")]
    public CardInstance currentCard { get; private set; }
    public SlotState currentState { get; private set; }
    public bool isEquipSlot;

    // ������ CardDisplay �������� �ν��Ͻ��� �����ص� ����
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
    /// ī�� �ν��Ͻ� �����ͷ� ������ �����մϴ�. CardDisplay �������� �����ϰų� �����մϴ�.
    /// </summary>
    /// <param name="card">ǥ���� ī�� ������. ������ null</param>
    /// <param name="cardPrefab">������ CardDisplay ������</param>
    public void Setup(CardInstance card, GameObject cardPrefab)
    {
        currentCard = card;

        // 1. ������ �ִ� ī�� �������� �ִٸ� ���� �ı��մϴ�.
        if (currentCardDisplay != null)
        {
            Destroy(currentCardDisplay.gameObject);
            currentCardDisplay = null;
        }

        // 2. ǥ���� ī�尡 �ִ��� Ȯ���մϴ�.
        if (card != null)
        {
            SetState(SlotState.Occupied);
            if (cardPrefab != null)
            {
                // CardDisplay �������� cardParent �Ʒ��� �����մϴ�.
                GameObject cardGO = Instantiate(cardPrefab, cardParent);
                currentCardDisplay = cardGO.GetComponent<CardDisplay>();
                if (currentCardDisplay != null)
                {
                    // ������ �����տ� ī�� �����͸� �Ѱ� UI�� ä�쵵�� �մϴ�.
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