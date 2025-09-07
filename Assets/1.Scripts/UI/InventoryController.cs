using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Equipped Slots")]
    [SerializeField] private List<CardDisplay> equippedCardDisplays;
    [SerializeField] private List<GameObject> equippedEmptyVisuals;
    [SerializeField] private List<Button> equippedEmptySlotButtons;

    [Header("Owned Slots")]
    [SerializeField] private List<CardDisplay> ownedCardDisplays;
    [SerializeField] private List<GameObject> ownedEmptyVisuals;
    [SerializeField] private List<Button> ownedEmptySlotButtons;
    [SerializeField] private List<GameObject> ownedSlotLocks;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI attackValueText;
    [SerializeField] private TextMeshProUGUI healthValueText;
    [SerializeField] private TextMeshProUGUI attackSpeedValueText;
    [SerializeField] private TextMeshProUGUI critRateValueText;
    [SerializeField] private TextMeshProUGUI moveSpeedValueText;
    [SerializeField] private TextMeshProUGUI critDamageValueText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    // ���� ���� ������ ���� ����
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private bool isEditable;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Awake()
    {
        mainPanel.SetActive(false); // ���� �ÿ��� ��Ȱ��ȭ
    }

    private void OnEnable()
    {
        // �ڷΰ��� ��ư �̺�Ʈ ����
        backButton.onClick.AddListener(OnBackButtonClicked);

        // ��� ���� ��ư�� �̺�Ʈ ������ ����
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            int index = i; // Ŭ���� ���� ����
            equippedCardDisplays[i].selectButton.onClick.AddListener(() => OnSlotClicked(true, index));
            equippedEmptySlotButtons[i].onClick.AddListener(() => OnSlotClicked(true, index));
        }

        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            int index = i;
            ownedCardDisplays[i].selectButton.onClick.AddListener(() => OnSlotClicked(false, index));
            ownedEmptySlotButtons[i].onClick.AddListener(() => OnSlotClicked(false, index));
        }
    }

    private void OnDisable()
    {
        // �̺�Ʈ ������ ���� (�޸� ���� ����)
        backButton.onClick.RemoveAllListeners();
        foreach (var display in equippedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in equippedEmptySlotButtons) button.onClick.RemoveAllListeners();
        foreach (var display in ownedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in ownedEmptySlotButtons) button.onClick.RemoveAllListeners();
    }

    public void Show(bool editable)
    {
        this.isEditable = editable;
        cardManager = ServiceLocator.Get<CardManager>();

        // CardManager�� DontDestroyOnLoad ��ü�̹Ƿ� ���� �ٲ� �����˴ϴ�.
        if (cardManager != null)
        {
            // CardManager�� playerStats�� ������ �� �ִ� public ������Ƽ�� �ִٰ� �����մϴ�. (���ٸ� CardManager�� �߰��ؾ� ��)
            // ��: public CharacterStats PlayerStats => playerStats;
            playerStats = cardManager.PlayerStats;
        }

        // playerStats�� ������ ã�� ���ߴٸ� ��� ����� �ߴ��մϴ�.
        if (playerStats == null)
        {
            Debug.LogError("[InventoryController] CardManager�� ���ؼ��� CharacterStats�� ã�� �� �����ϴ�!");
            return;
        }

        mainPanel.SetActive(true);
        RefreshAllUI();
        StartCoroutine(SetupNavigationAndFocus());
    }
    public void Hide()
    {
        // ���� ���� �ʱ�ȭ �� ����
        CancelLockIn();
        mainPanel.SetActive(false);
    }
    private IEnumerator SetupNavigationAndFocus()
    {
        yield return null; // UI ��ҵ��� Ȱ��ȭ�ǰ� ��ġ�� ���� ������ �� ������ ���

        SetupNavigation(); // �׺���̼� ����

        // [����] �ʱ� ��Ŀ���� BackButton���� ����
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // ��Ŀ�� ���� �� �� ������ �� ����Ͽ� ������ Ȯ��

        if (backButton != null && backButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        }
    }
    private void SetupNavigation()
    {
        // 1. ���� ��ȣ�ۿ� ������ ��� ��ư ����� �����մϴ�.
        List<Button> allButtons = new List<Button>();
        allButtons.AddRange(equippedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : equippedEmptySlotButtons[i]));
        allButtons.AddRange(ownedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : ownedEmptySlotButtons[i]));
        allButtons.Add(backButton);

        List<Button> interactableButtons = allButtons.Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable).ToList();

        // 2. �� ��ư�� ���� �׺���̼��� �����մϴ�.
        foreach (var button in interactableButtons)
        {
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            nav.selectOnUp = FindNextSelectable(button, Vector2.up, interactableButtons);
            nav.selectOnDown = FindNextSelectable(button, Vector2.down, interactableButtons);
            nav.selectOnLeft = FindNextSelectable(button, Vector2.left, interactableButtons);
            nav.selectOnRight = FindNextSelectable(button, Vector2.right, interactableButtons);

            button.navigation = nav;
        }
    }

    private Button FindNextSelectable(Button current, Vector2 direction, List<Button> allButtons)
    {
        RectTransform currentRect = current.GetComponent<RectTransform>();
        Button bestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var potentialTarget in allButtons)
        {
            if (potentialTarget == current) continue;

            RectTransform targetRect = potentialTarget.GetComponent<RectTransform>();
            Vector2 toTargetVector = targetRect.position - currentRect.position;

            // 1. ���ϴ� ���⿡ �ִ��� Ȯ�� (Dot Product ���)
            // ���� ���Ϳ��� ������ ������� ���� �������� �����մϴ�.
            if (Vector2.Dot(direction, toTargetVector.normalized) < 0.2f) // 0.2f�� �ణ�� �밢���� ����ϱ� ����
            {
                continue;
            }

            // 2. �Ÿ� ��� (���� ����� ����� ã�� ����)
            float distance = toTargetVector.magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    private void OnBackButtonClicked()
    {
        // ���� ���� ���¿� ���� ���ư� UI�� �޶��� �� �ֽ��ϴ�.
        // CardReward ������ ��� CardRewardUIManager�� �ٽ� Ȱ��ȭ�մϴ�.
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Reward)
        {
            ServiceLocator.Get<CardRewardUIManager>()?.Show();
        }
        // TODO: ���� ���� ���°� Pause���, Pause UI�� �ٽ� Ȱ��ȭ�ϴ� �ڵ带 �߰��ؾ� �մϴ�.
        // else if (gameManager.CurrentState == GameManager.GameState.Pause) { ... }

        // �ڽ��� �κ��丮 �г��� �׻� ����ϴ�.
        Hide();
    }
    public void RefreshAllUI()
    {
        if (cardManager == null || playerStats == null) return;

        UpdateCardSlots();
        UpdateStatsUI();
        // TODO: �׺���̼� ������Ʈ ����
    }

    private void UpdateCardSlots()
    {
        // ���� ���� ������Ʈ
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            if (i < cardManager.equippedCards.Count)
            {
                equippedCardDisplays[i].gameObject.SetActive(true);
                equippedEmptyVisuals[i].SetActive(false);
                equippedCardDisplays[i].Setup(cardManager.equippedCards[i]);
                equippedCardDisplays[i].selectButton.interactable = isEditable;
            }
            else
            {
                equippedCardDisplays[i].gameObject.SetActive(false);
                equippedEmptyVisuals[i].SetActive(true);
                equippedEmptySlotButtons[i].interactable = isEditable;
            }
        }

        // ���� ���� ������Ʈ
        List<CardInstance> unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            bool isSlotUnlocked = i < (cardManager.maxOwnedSlots - cardManager.maxEquipSlots);
            ownedSlotLocks[i].SetActive(!isSlotUnlocked);

            if (isSlotUnlocked)
            {
                if (i < unequippedOwnedCards.Count)
                {
                    ownedCardDisplays[i].gameObject.SetActive(true);
                    ownedEmptyVisuals[i].SetActive(false);
                    ownedCardDisplays[i].Setup(unequippedOwnedCards[i]);
                    ownedCardDisplays[i].selectButton.interactable = isEditable;
                }
                else
                {
                    ownedCardDisplays[i].gameObject.SetActive(false);
                    ownedEmptyVisuals[i].SetActive(true);
                    ownedEmptySlotButtons[i].interactable = isEditable;
                }
            }
            else
            {
                ownedCardDisplays[i].gameObject.SetActive(false);
                ownedEmptyVisuals[i].SetActive(false);
            }
        }
    }

    private void UpdateStatsUI()
    {
        attackValueText.text = $"{playerStats.FinalDamageBonus:F1}%";
        healthValueText.text = $"{playerStats.FinalHealth:F0}";
        attackSpeedValueText.text = $"{playerStats.FinalAttackSpeed:F2}";
        critRateValueText.text = $"{playerStats.FinalCritRate:F1}%";
        moveSpeedValueText.text = $"{playerStats.FinalMoveSpeed:F2}";
        critDamageValueText.text = $"{playerStats.FinalCritDamage:F0}%";
    }

    private void OnSlotClicked(bool isEquippedSlot, int slotIndex)
    {
        if (!isEditable) return;

        // 1. Ŭ���� ������ ī�� �ν��Ͻ��� �����ɴϴ�. (������ null)
        CardInstance clickedCard = null;
        if (isEquippedSlot)
        {
            // ���� ���Կ��� Ŭ���� ī�� ã��
            if (slotIndex < cardManager.equippedCards.Count)
                clickedCard = cardManager.equippedCards[slotIndex];
        }
        else
        {
            // ���� ���Կ��� Ŭ���� ī�� ã�� (������ ī�� ����)
            var unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
            if (slotIndex < unequippedOwnedCards.Count)
                clickedCard = unequippedOwnedCards[slotIndex];
        }


        // 2. ���ε� ī�尡 ���� �� (ù ��° Ŭ��)
        if (lockedInCard == null)
        {
            if (clickedCard != null)
            {
                // Ŭ���� ���Կ� ī�尡 ������ ���� ���·� ��ȯ
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                // TODO: lockedInCard�� �ش��ϴ� CardDisplay�� ���̶���Ʈ �ð� ȿ�� ����
                Debug.Log($"[Inventory] ����: '{lockedInCard.CardData.basicInfo.cardName}'");
            }
            // �� ������ ó�� Ŭ���� ���� �ƹ��͵� ���� ����
        }
        // 3. ���ε� ī�尡 ���� �� (�� ��° Ŭ��)
        else
        {
            // 3-1. ���� ī�带 �ٽ� Ŭ���� ���: ���� ���
            if (lockedInCard == clickedCard)
            {
                CancelLockIn();
                return;
            }

            // 3-2. �ٸ� ī�带 Ŭ���� ���: ī�� ��ü(Swap)
            if (clickedCard != null)
            {
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            // 3-3. �� ���� ������ Ŭ���� ���: ī�� �̵�(Move)
            else if (isEquippedSlot) // clickedCard�� null�̰�, ���� ������ Ŭ���ߴٸ� �� ������
            {
                cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
            }
            // 3-4. �� ���� ������ Ŭ���� ���: ī�� ���� ����(Unequip)
            else // isEquippedSlot�� false�̰� clickedCard�� null
            {
                // ���ε� ī�尡 ������ ī���� ���� �ǹ̰� ����
                if (lockedInSlotInfo.isEquipped)
                {
                    cardManager.Unequip(lockedInCard);
                }
            }

            // ��� ��ȣ�ۿ� �� ���� �ʱ�ȭ �� UI ���ΰ�ħ
            CancelLockIn();
            RefreshAllUI();
        }
    }
    private void CancelLockIn()
    {
        lockedInCard = null;
        // TODO: ��� ī�� UI�� ���̶���Ʈ �ð� ȿ�� ����
        Debug.Log("[Inventory] ���� ���� ����.");
    }

    // Update �Լ��� �����Ͻ� ����� �����ϰ� �����մϴ�.
    void Update()
    {
        if (isEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null)
            {
                CancelLockIn();
            }
            else
            {
                // Pause �޴� Ȥ�� CardReward ���� ���� UI�� ���ư��� ����
                OnBackButtonClicked();
            }
        }
    }

}