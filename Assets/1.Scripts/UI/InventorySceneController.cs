// ���� ���: Assets/1.Scripts/UI/InventorySceneController.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySceneController : MonoBehaviour
{
    // �ٸ� ������ �κ��丮�� ���� ��, �� ���� ���� �����ؾ� �մϴ�.
    public static bool IsEditable { get; set; } = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Equipped Slots (������ ��ġ)")]
    [SerializeField] private List<CardDisplay> equippedCardDisplays;
    [SerializeField] private List<GameObject> equippedEmptyVisuals;
    [SerializeField] private List<Button> equippedEmptySlotButtons;

    [Header("Owned Slots (����)")]
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

    // ���� ������ ����
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Start()
    {
        // [�α� �߰�]
        Debug.Log("[[ 4. InventorySceneController ]] Start() �޼��� ����.");

        cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            playerStats = cardManager.PlayerStats;
        }

        if (playerStats == null)
        {
            Debug.LogError("[InventorySceneController] CharacterStats�� ã�� �� ���� ���� �ݽ��ϴ�.");
            OnBackButtonClicked();
            return;
        }

        SetupButtonListeners();
        RefreshAllUI();

        // [�α� �߰�]
        Debug.Log("[[ 5. InventorySceneController ]] SetupNavigationAndFocus �ڷ�ƾ�� �����մϴ�.");
        StartCoroutine(SetupNavigationAndFocus());
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    void Update()
    {
        // ���� ���� ��忡�� ESC Ű(�����е��� B��ư ��)�� ���� ���� ����
        if (IsEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null)
            {
                // ����(Lock-in)�� ī�尡 ������ ������ ����մϴ�.
                CancelLockIn();
            }
            else
            {
                // ���õ� ī�尡 ������ �ڷΰ��� ��ư�� ���� �Ͱ� ���� �����մϴ�.
                OnBackButtonClicked();
            }
        }
    }

    private void SetupButtonListeners()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);

        // ���� ���� ����� ���� ���� Ŭ�� �����ʸ� �߰��մϴ�.
        if (IsEditable)
        {
            for (int i = 0; i < equippedCardDisplays.Count; i++)
            {
                int index = i;
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
    }

    private void RemoveButtonListeners()
    {
        backButton.onClick.RemoveAllListeners();
        foreach (var display in equippedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in equippedEmptySlotButtons) button.onClick.RemoveAllListeners();
        foreach (var display in ownedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in ownedEmptySlotButtons) button.onClick.RemoveAllListeners();
    }

    private void OnBackButtonClicked()
    {
        // [�α� �߰�]
        Debug.Log("[[ InventorySceneController ]] BackButton�� Ŭ���Ǿ����ϴ�.");
        Time.timeScale = 1f;

        var cardRewardUI = FindObjectOfType<CardRewardUIManager>(true);
        if (cardRewardUI != null)
        {
            cardRewardUI.Show();
        }
        else
        {
            Debug.LogError("[InventorySceneController] CardRewardUIManager�� ã�� �� �����ϴ�!");
        }

        ServiceLocator.Get<SceneTransitionManager>()?.UnloadTopScene();
    }

    private void RefreshAllUI()
    {
        if (cardManager == null || playerStats == null) return;
        UpdateCardSlots();
        UpdateStatsUI();
    }

    private void UpdateCardSlots()
    {
        // 1. ���� ���� ������Ʈ
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            bool isSlotFilled = i < cardManager.equippedCards.Count;
            equippedCardDisplays[i].gameObject.SetActive(isSlotFilled);
            equippedEmptyVisuals[i].SetActive(!isSlotFilled);

            if (isSlotFilled)
            {
                equippedCardDisplays[i].Setup(cardManager.equippedCards[i]);
            }

            // ���� ���� ���ο� ���� ��ư ��ȣ�ۿ��� �����մϴ�.
            equippedCardDisplays[i].selectButton.interactable = IsEditable;
            equippedEmptySlotButtons[i].interactable = IsEditable;
        }

        // 2. ���� ���� ������Ʈ
        List<CardInstance> unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            // ���� ������ ���� �ִ� ���� ������ �þ���� Ȯ��
            bool isSlotUnlocked = i < (cardManager.maxOwnedSlots - cardManager.maxEquipSlots);
            ownedSlotLocks[i].SetActive(!isSlotUnlocked);

            if (isSlotUnlocked)
            {
                bool isSlotFilled = i < unequippedOwnedCards.Count;
                ownedCardDisplays[i].gameObject.SetActive(isSlotFilled);
                ownedEmptyVisuals[i].SetActive(!isSlotFilled);
                if (isSlotFilled)
                {
                    ownedCardDisplays[i].Setup(unequippedOwnedCards[i]);
                }
                ownedCardDisplays[i].selectButton.interactable = IsEditable;
                ownedEmptySlotButtons[i].interactable = IsEditable;
            }
            else
            {
                // ��� ������ ī��, �� ���� UI ��� ��Ȱ��ȭ�մϴ�.
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
        if (!IsEditable) return; // ���� �Ұ� ����� ��� �ƹ��͵� ���� ����

        CardInstance clickedCard = null;
        if (isEquippedSlot)
        {
            if (slotIndex < cardManager.equippedCards.Count)
                clickedCard = cardManager.equippedCards[slotIndex];
        }
        else
        {
            var unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
            if (slotIndex < unequippedOwnedCards.Count)
                clickedCard = unequippedOwnedCards[slotIndex];
        }

        // --- ī�� ��ü ���� ---
        if (lockedInCard == null) // 1. ù ��° ī�� ���� (Lock-in)
        {
            if (clickedCard != null)
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                GetCardDisplay(isEquippedSlot, slotIndex)?.SetLockIn(true);
            }
        }
        else // 2. �� ��° ���� ���� (��ü ����)
        {
            if (lockedInCard == clickedCard) // ���� ī�� �ٽ� ���� �� Lock-in ���
            {
                CancelLockIn();
                return;
            }

            if (clickedCard != null) // �ٸ� ī�尡 �ִ� ���� ����: Swap
            {
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            else // �� ���� ����: Move
            {
                if (isEquippedSlot) // �� ���� �������� �̵�
                {
                    cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
                }
                else // �� ���� �������� �̵� (���� ����)
                {
                    if (lockedInSlotInfo.isEquipped) cardManager.Unequip(lockedInCard);
                }
            }

            CancelLockIn();
            RefreshAllUI(); // UI ��ü ���ΰ�ħ
        }
    }

    private void CancelLockIn()
    {
        if (lockedInCard != null)
        {
            GetCardDisplay(lockedInSlotInfo.isEquipped, lockedInSlotInfo.index)?.SetLockIn(false);
        }
        lockedInCard = null;
    }

    // isEquipped�� index�� ������� �ش��ϴ� CardDisplay ������Ʈ�� ã�� ��ȯ�ϴ� ���� �Լ�
    private CardDisplay GetCardDisplay(bool isEquipped, int index)
    {
        if (isEquipped)
        {
            if (index < equippedCardDisplays.Count) return equippedCardDisplays[index];
        }
        else
        {
            if (index < ownedCardDisplays.Count) return ownedCardDisplays[index];
        }
        return null;
    }

    // Ű����/�е� �׺���̼� ���� �� �ʱ� ��Ŀ�� ����
    private IEnumerator SetupNavigationAndFocus()
    {
        // [�α� �߰�]
        Debug.Log("[[ 6. Coroutine ]] SetupNavigationAndFocus �ڷ�ƾ ����.");

        EventSystem.current.SetSelectedGameObject(null);
        // [�α� �߰�]
        Debug.Log("[[ 7. Coroutine ]] EventSystem ��Ŀ�� �ʱ�ȭ �Ϸ�. ���� �����ӱ��� ����մϴ�.");

        yield return null;

        // [�α� �߰�]
        Debug.Log("[[ 8. Coroutine ]] ��� �Ϸ�. BackButton ��Ŀ�� ������ �õ��մϴ�.");

        if (backButton != null && backButton.interactable)
        {
            // [�α� �߰�]
            Debug.Log($"[[ 9. Coroutine ]] BackButton (�̸�: {backButton.gameObject.name})�� null�� �ƴϰ� Ȱ��ȭ �����Դϴ�. ��Ŀ���� �����մϴ�.");
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);

            // [�α� �߰�]
            if (EventSystem.current.currentSelectedGameObject == backButton.gameObject)
            {
                Debug.Log("<color=green>[[ 10. Coroutine ]] ����: EventSystem�� ���� ���õ� ������Ʈ�� BackButton���� Ȯ�εǾ����ϴ�.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[[ 10. Coroutine ]] ����: SetSelectedGameObject ȣ�� ��, EventSystem�� ������ BackButton�� �����ϰ� ���� �ʽ��ϴ�!</color>");
            }
        }
        else
        {
            // [�α� �߰�]
            Debug.LogError($"<color=red>[[ 9. Coroutine ]] ����: BackButton�� Null�̰ų� ��Ȱ��ȭ ���¿��� ��Ŀ���� ������ �� �����ϴ�.</color>");
        }

        // [�α� �߰�]
        Debug.Log("[[ 11. Coroutine ]] SetupNavigationAndFocus �ڷ�ƾ ����.");
    }

    // ��� ��ư�� �����¿� ������ �����մϴ�. (Ű����/�е��)
    private void SetupNavigation()
    {
        // �� �κ��� �� ��ư�� RectTransform ��ġ�� ���� �ſ� ���������Ƿ�,
        // ���⼭�� �⺻ ������ �����ϰ� Unity �������� 'Automatic' �׺���̼ǿ� �ñ�� ���� �����մϴ�.
        // ���� ������ �� �ʿ��ϴٸ� �� ��ư�� Navigation �Ӽ��� �ڵ峪 �ν����Ϳ��� ���� �����ؾ� �մϴ�.
        // ��: 
        // var nav = backButton.navigation;
        // nav.mode = Navigation.Mode.Explicit;
        // nav.selectOnUp = ownedCardDisplays[1].selectButton;
        // backButton.navigation = nav;
    }
}