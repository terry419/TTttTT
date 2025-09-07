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
        cardManager = ServiceLocator.Get<CardManager>();

        // CardManager는 DontDestroyOnLoad 객체이므로 씬이 바뀌어도 유지됩니다.
        if (cardManager != null)
        {
            // CardManager에 playerStats를 참조할 수 있는 public 프로퍼티가 있다고 가정합니다. (없다면 CardManager에 추가해야 함)
            // 예: public CharacterStats PlayerStats => playerStats;
            playerStats = cardManager.PlayerStats;
        }

        // playerStats를 여전히 찾지 못했다면 경고를 남기고 중단합니다.
        if (playerStats == null)
        {
            Debug.LogError("[InventoryController] CardManager를 통해서도 CharacterStats를 찾을 수 없습니다!");
            return;
        }

        mainPanel.SetActive(true);
        RefreshAllUI();
        StartCoroutine(SetupNavigationAndFocus());
    }
    public void Hide()
    {
        // 락인 상태 초기화 후 숨김
        CancelLockIn();
        mainPanel.SetActive(false);
    }
    private IEnumerator SetupNavigationAndFocus()
    {
        yield return null; // UI 요소들이 활성화되고 위치가 계산될 때까지 한 프레임 대기

        SetupNavigation(); // 네비게이션 설정

        // [수정] 초기 포커스를 BackButton으로 고정
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // 포커스 해제 후 한 프레임 더 대기하여 안정성 확보

        if (backButton != null && backButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);
        }
    }
    private void SetupNavigation()
    {
        // 1. 현재 상호작용 가능한 모든 버튼 목록을 생성합니다.
        List<Button> allButtons = new List<Button>();
        allButtons.AddRange(equippedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : equippedEmptySlotButtons[i]));
        allButtons.AddRange(ownedCardDisplays.Select((display, i) => display.gameObject.activeSelf ? display.selectButton : ownedEmptySlotButtons[i]));
        allButtons.Add(backButton);

        List<Button> interactableButtons = allButtons.Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable).ToList();

        // 2. 각 버튼에 대해 네비게이션을 설정합니다.
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

            // 1. 원하는 방향에 있는지 확인 (Dot Product 사용)
            // 방향 벡터와의 내적이 양수여야 같은 방향으로 간주합니다.
            if (Vector2.Dot(direction, toTargetVector.normalized) < 0.2f) // 0.2f는 약간의 대각선도 허용하기 위함
            {
                continue;
            }

            // 2. 거리 계산 (가장 가까운 대상을 찾기 위함)
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
        // 현재 게임 상태에 따라 돌아갈 UI가 달라질 수 있습니다.
        // CardReward 상태일 경우 CardRewardUIManager를 다시 활성화합니다.
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Reward)
        {
            ServiceLocator.Get<CardRewardUIManager>()?.Show();
        }
        // TODO: 만약 게임 상태가 Pause라면, Pause UI를 다시 활성화하는 코드를 추가해야 합니다.
        // else if (gameManager.CurrentState == GameManager.GameState.Pause) { ... }

        // 자신의 인벤토리 패널은 항상 숨깁니다.
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

    private void OnSlotClicked(bool isEquippedSlot, int slotIndex)
    {
        if (!isEditable) return;

        // 1. 클릭된 슬롯의 카드 인스턴스를 가져옵니다. (없으면 null)
        CardInstance clickedCard = null;
        if (isEquippedSlot)
        {
            // 장착 슬롯에서 클릭된 카드 찾기
            if (slotIndex < cardManager.equippedCards.Count)
                clickedCard = cardManager.equippedCards[slotIndex];
        }
        else
        {
            // 소유 슬롯에서 클릭된 카드 찾기 (장착된 카드 제외)
            var unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
            if (slotIndex < unequippedOwnedCards.Count)
                clickedCard = unequippedOwnedCards[slotIndex];
        }


        // 2. 락인된 카드가 없을 때 (첫 번째 클릭)
        if (lockedInCard == null)
        {
            if (clickedCard != null)
            {
                // 클릭된 슬롯에 카드가 있으면 락인 상태로 전환
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                // TODO: lockedInCard에 해당하는 CardDisplay에 하이라이트 시각 효과 적용
                Debug.Log($"[Inventory] 락인: '{lockedInCard.CardData.basicInfo.cardName}'");
            }
            // 빈 슬롯을 처음 클릭한 경우는 아무것도 하지 않음
        }
        // 3. 락인된 카드가 있을 때 (두 번째 클릭)
        else
        {
            // 3-1. 같은 카드를 다시 클릭한 경우: 락인 취소
            if (lockedInCard == clickedCard)
            {
                CancelLockIn();
                return;
            }

            // 3-2. 다른 카드를 클릭한 경우: 카드 교체(Swap)
            if (clickedCard != null)
            {
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            // 3-3. 빈 장착 슬롯을 클릭한 경우: 카드 이동(Move)
            else if (isEquippedSlot) // clickedCard가 null이고, 장착 슬롯을 클릭했다면 빈 슬롯임
            {
                cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
            }
            // 3-4. 빈 소유 슬롯을 클릭한 경우: 카드 장착 해제(Unequip)
            else // isEquippedSlot이 false이고 clickedCard가 null
            {
                // 락인된 카드가 장착된 카드일 때만 의미가 있음
                if (lockedInSlotInfo.isEquipped)
                {
                    cardManager.Unequip(lockedInCard);
                }
            }

            // 모든 상호작용 후 상태 초기화 및 UI 새로고침
            CancelLockIn();
            RefreshAllUI();
        }
    }
    private void CancelLockIn()
    {
        lockedInCard = null;
        // TODO: 모든 카드 UI의 하이라이트 시각 효과 제거
        Debug.Log("[Inventory] 락인 상태 해제.");
    }

    // Update 함수는 제안하신 내용과 동일하게 유지합니다.
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
                // Pause 메뉴 혹은 CardReward 씬의 이전 UI로 돌아가는 로직
                OnBackButtonClicked();
            }
        }
    }

}