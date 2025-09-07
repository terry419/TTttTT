using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

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

    // 내부 상태 관리를 위한 변수
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private bool isEditable;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Awake()
    {
        mainPanel.SetActive(false); // 시작 시에는 비활성화
    }

    private void OnEnable()
    {
        // 뒤로가기 버튼 이벤트 연결
        backButton.onClick.AddListener(OnBackButtonClicked);

        // 모든 슬롯 버튼에 이벤트 리스너 연결
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            int index = i; // 클로저 문제 방지
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
        // 이벤트 리스너 해제 (메모리 누수 방지)
        backButton.onClick.RemoveAllListeners();
        foreach (var display in equippedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in equippedEmptySlotButtons) button.onClick.RemoveAllListeners();
        foreach (var display in ownedCardDisplays) display.selectButton.onClick.RemoveAllListeners();
        foreach (var button in ownedEmptySlotButtons) button.onClick.RemoveAllListeners();
    }

    public void Show(bool editable)
    {
        this.isEditable = editable;

        // 필요한 매니저 참조 가져오기
        cardManager = ServiceLocator.Get<CardManager>();
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerStats = playerController.GetComponent<CharacterStats>();
        }

        mainPanel.SetActive(true);
        RefreshAllUI();

        // 포커스 설정 (예: 첫 번째 장착 슬롯)
        if (equippedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(equippedCardDisplays[0].gameObject);
        }
    }

    public void Hide()
    {
        // 락인 상태 초기화 후 숨김
        CancelLockIn();
        mainPanel.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        // TODO: Pause 씬과 CardReward 씬에서 각각 다른 동작을 하도록 연결 필요
        // 예: UIManager.ShowPanel("PauseMenu"); 또는 cardRewardUIManager.HideInventory();
        Hide();
    }

    public void RefreshAllUI()
    {
        if (cardManager == null || playerStats == null) return;

        UpdateCardSlots();
        UpdateStatsUI();
        // TODO: 네비게이션 업데이트 로직
    }

    private void UpdateCardSlots()
    {
        // 장착 슬롯 업데이트
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

        // 소유 슬롯 업데이트
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

    private void OnSlotClicked(bool isEquipped, int index)
    {
        if (!isEditable) return;

        if (lockedInCard == null) // 첫 번째 카드 선택
        {
            CardInstance cardToLock = null;
            if (isEquipped)
            {
                if (index < cardManager.equippedCards.Count)
                    cardToLock = cardManager.equippedCards[index];
            }
            else
            {
                List<CardInstance> unequipped = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
                if (index < unequipped.Count)
                    cardToLock = unequipped[index];
            }

            if (cardToLock != null)
            {
                lockedInCard = cardToLock;
                lockedInSlotInfo = (isEquipped, index);
                // TODO: 시각적 하이라이트 효과 적용
                Debug.Log($"카드 락인: {lockedInCard.CardData.basicInfo.cardName}");
            }
        }
        else // 두 번째 카드(또는 빈 슬롯) 선택
        {
            // TODO: CardManager에 카드 교체 함수를 만들고 호출
            Debug.Log($"교체 시도: {lockedInCard.CardData.basicInfo.cardName} 와 (isEquipped: {isEquipped}, index: {index}) 슬롯");
            // cardManager.SwapCards(lockedInSlotInfo, (isEquipped, index));

            CancelLockIn();
            RefreshAllUI(); // 교체 후 UI 새로고침
        }
    }

    private void CancelLockIn()
    {
        if (lockedInCard != null)
        {
            // TODO: 시각적 하이라이트 효과 해제
            Debug.Log("락인 취소");
        }
        lockedInCard = null;
    }

    void Update()
    {
        // ESC 키로 락인 취소
        if (isEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null)
            {
                CancelLockIn();
            }
            else
            {
                OnBackButtonClicked();
            }
        }
    }
}