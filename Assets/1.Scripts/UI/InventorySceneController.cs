// InventorySceneController.cs - 새로운 씬 기반 컨트롤러

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySceneController : MonoBehaviour
{
    // 이 씬을 로드하기 전에 외부에서 설정해주어야 하는 값입니다.
    public static bool IsEditable { get; set; } = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel; // 씬의 최상위 패널

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

    // 상태 저장용 변수
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Start()
    {
        // 서비스 및 데이터 로드
        cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            playerStats = cardManager.PlayerStats;
        }

        if (playerStats == null)
        {
            Debug.LogError("[InventorySceneController] CharacterStats를 찾을 수 없습니다!");
            // 이 경우, 씬을 바로 닫아버리는 것도 좋은 방법입니다.
            OnBackButtonClicked();
            return;
        }

        // 이벤트 리스너 연결
        SetupButtonListeners();

        // UI 새로고침 및 내비게이션 설정
        RefreshAllUI();
        StartCoroutine(SetupNavigationAndFocus());
    }

    private void OnDestroy()
    {
        // 씬이 언로드될 때 리스너 해제
        RemoveButtonListeners();
    }

    private void SetupButtonListeners()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);

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
        // 수정된 SceneTransitionManager의 뒤로가기 함수를 호출합니다.
        ServiceLocator.Get<SceneTransitionManager>()?.UnloadTopScene();
    }

    // --- 기존 InventoryController의 로직 대부분 재사용 ---

    private IEnumerator SetupNavigationAndFocus()
    {
        yield return null;
        SetupNavigation();
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        if (backButton != null && backButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        }
    }

    private void SetupNavigation()
    {
        List<Button> allButtons = new List<Button>();
        allButtons.AddRange(equippedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : equippedEmptySlotButtons[i]));
        allButtons.AddRange(ownedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : ownedEmptySlotButtons[i]));
        allButtons.Add(backButton);
        List<Button> interactableButtons = allButtons.Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable).ToList();

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
            if (Vector2.Dot(direction, toTargetVector.normalized) < 0.2f) { continue; }
            float distance = toTargetVector.magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }

    public void RefreshAllUI()
    {
        if (cardManager == null || playerStats == null) return;
        UpdateCardSlots();
        UpdateStatsUI();
    }

    private void UpdateCardSlots()
    {
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            bool isSlotFilled = i < cardManager.equippedCards.Count;
            equippedCardDisplays[i].gameObject.SetActive(isSlotFilled);
            equippedEmptyVisuals[i].SetActive(!isSlotFilled);
            if (isSlotFilled)
            {
                equippedCardDisplays[i].Setup(cardManager.equippedCards[i]);
            }
            equippedCardDisplays[i].selectButton.interactable = IsEditable;
            equippedEmptySlotButtons[i].interactable = IsEditable;
        }

        List<CardInstance> unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
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
        if (!IsEditable) return;

        CardInstance clickedCard = null;
        if (isEquippedSlot)
        {
            if (slotIndex < cardManager.equippedCards.Count) clickedCard = cardManager.equippedCards[slotIndex];
        }
        else
        {
            var unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
            if (slotIndex < unequippedOwnedCards.Count) clickedCard = unequippedOwnedCards[slotIndex];
        }

        if (lockedInCard == null)
        {
            if (clickedCard != null)
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                GetCardDisplay(isEquippedSlot, slotIndex)?.SetLockIn(true);
            }
        }
        else
        {
            if (lockedInCard == clickedCard)
            {
                CancelLockIn();
                return;
            }
            if (clickedCard != null) { cardManager.SwapCards(lockedInCard, clickedCard); }
            else if (isEquippedSlot) { cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex); }
            else { if (lockedInSlotInfo.isEquipped) { cardManager.Unequip(lockedInCard); } }
            CancelLockIn();
            RefreshAllUI();
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

    private CardDisplay GetCardDisplay(bool isEquipped, int index)
    {
        if (isEquipped) { if (index < equippedCardDisplays.Count) return equippedCardDisplays[index]; }
        else { if (index < ownedCardDisplays.Count) return ownedCardDisplays[index]; }
        return null;
    }

    void Update()
    {
        if (IsEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null) { CancelLockIn(); }
            else { OnBackButtonClicked(); }
        }
    }
}
