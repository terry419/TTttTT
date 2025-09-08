// 파일 경로: Assets/1.Scripts/UI/InventorySceneController.cs

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySceneController : MonoBehaviour
{
    // 다른 씬에서 인벤토리를 열기 전, 이 값을 먼저 설정해야 합니다.
    public static bool IsEditable { get; set; } = false;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Equipped Slots (오망성 배치)")]
    [SerializeField] private List<CardDisplay> equippedCardDisplays;
    [SerializeField] private List<GameObject> equippedEmptyVisuals;
    [SerializeField] private List<Button> equippedEmptySlotButtons;

    [Header("Owned Slots (우측)")]
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

    // 내부 로직용 변수
    private CardInstance lockedInCard;
    private (bool isEquipped, int index) lockedInSlotInfo;
    private CardManager cardManager;
    private CharacterStats playerStats;

    void Start()
    {
        // [로그 추가]
        Debug.Log("[[ 4. InventorySceneController ]] Start() 메서드 시작.");

        cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            playerStats = cardManager.PlayerStats;
        }

        if (playerStats == null)
        {
            Debug.LogError("[InventorySceneController] CharacterStats를 찾을 수 없어 씬을 닫습니다.");
            OnBackButtonClicked();
            return;
        }

        SetupButtonListeners();
        RefreshAllUI();

        // [로그 추가]
        Debug.Log("[[ 5. InventorySceneController ]] SetupNavigationAndFocus 코루틴을 시작합니다.");
        StartCoroutine(SetupNavigationAndFocus());
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    void Update()
    {
        // 수정 가능 모드에서 ESC 키(게임패드의 B버튼 등)를 누를 때의 동작
        if (IsEditable && Input.GetKeyDown(KeyCode.Escape))
        {
            if (lockedInCard != null)
            {
                // 선택(Lock-in)된 카드가 있으면 선택을 취소합니다.
                CancelLockIn();
            }
            else
            {
                // 선택된 카드가 없으면 뒤로가기 버튼을 누른 것과 같이 동작합니다.
                OnBackButtonClicked();
            }
        }
    }

    private void SetupButtonListeners()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);

        // 수정 가능 모드일 때만 슬롯 클릭 리스너를 추가합니다.
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
        // [로그 추가]
        Debug.Log("[[ InventorySceneController ]] BackButton이 클릭되었습니다.");
        Time.timeScale = 1f;

        var cardRewardUI = FindObjectOfType<CardRewardUIManager>(true);
        if (cardRewardUI != null)
        {
            cardRewardUI.Show();
        }
        else
        {
            Debug.LogError("[InventorySceneController] CardRewardUIManager를 찾을 수 없습니다!");
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
        // 1. 장착 슬롯 업데이트
        for (int i = 0; i < equippedCardDisplays.Count; i++)
        {
            bool isSlotFilled = i < cardManager.equippedCards.Count;
            equippedCardDisplays[i].gameObject.SetActive(isSlotFilled);
            equippedEmptyVisuals[i].SetActive(!isSlotFilled);

            if (isSlotFilled)
            {
                equippedCardDisplays[i].Setup(cardManager.equippedCards[i]);
            }

            // 수정 가능 여부에 따라 버튼 상호작용을 설정합니다.
            equippedCardDisplays[i].selectButton.interactable = IsEditable;
            equippedEmptySlotButtons[i].interactable = IsEditable;
        }

        // 2. 소유 슬롯 업데이트
        List<CardInstance> unequippedOwnedCards = cardManager.ownedCards.Except(cardManager.equippedCards).ToList();
        for (int i = 0; i < ownedCardDisplays.Count; i++)
        {
            // 유물 등으로 인해 최대 소유 슬롯이 늘어났는지 확인
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
                // 잠긴 슬롯은 카드, 빈 슬롯 UI 모두 비활성화합니다.
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
        if (!IsEditable) return; // 수정 불가 모드일 경우 아무것도 하지 않음

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

        // --- 카드 교체 로직 ---
        if (lockedInCard == null) // 1. 첫 번째 카드 선택 (Lock-in)
        {
            if (clickedCard != null)
            {
                lockedInCard = clickedCard;
                lockedInSlotInfo = (isEquippedSlot, slotIndex);
                GetCardDisplay(isEquippedSlot, slotIndex)?.SetLockIn(true);
            }
        }
        else // 2. 두 번째 슬롯 선택 (교체 실행)
        {
            if (lockedInCard == clickedCard) // 같은 카드 다시 선택 시 Lock-in 취소
            {
                CancelLockIn();
                return;
            }

            if (clickedCard != null) // 다른 카드가 있는 슬롯 선택: Swap
            {
                cardManager.SwapCards(lockedInCard, clickedCard);
            }
            else // 빈 슬롯 선택: Move
            {
                if (isEquippedSlot) // 빈 장착 슬롯으로 이동
                {
                    cardManager.MoveCardToEmptyEquipSlot(lockedInCard, slotIndex);
                }
                else // 빈 소유 슬롯으로 이동 (장착 해제)
                {
                    if (lockedInSlotInfo.isEquipped) cardManager.Unequip(lockedInCard);
                }
            }

            CancelLockIn();
            RefreshAllUI(); // UI 전체 새로고침
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

    // isEquipped와 index를 기반으로 해당하는 CardDisplay 컴포넌트를 찾아 반환하는 헬퍼 함수
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

    // 키보드/패드 네비게이션 설정 및 초기 포커스 설정
    private IEnumerator SetupNavigationAndFocus()
    {
        // [로그 추가]
        Debug.Log("[[ 6. Coroutine ]] SetupNavigationAndFocus 코루틴 진입.");

        EventSystem.current.SetSelectedGameObject(null);
        // [로그 추가]
        Debug.Log("[[ 7. Coroutine ]] EventSystem 포커스 초기화 완료. 다음 프레임까지 대기합니다.");

        yield return null;

        // [로그 추가]
        Debug.Log("[[ 8. Coroutine ]] 대기 완료. BackButton 포커스 설정을 시도합니다.");

        if (backButton != null && backButton.interactable)
        {
            // [로그 추가]
            Debug.Log($"[[ 9. Coroutine ]] BackButton (이름: {backButton.gameObject.name})은 null이 아니고 활성화 상태입니다. 포커스를 설정합니다.");
            EventSystem.current.SetSelectedGameObject(backButton.gameObject);

            // [로그 추가]
            if (EventSystem.current.currentSelectedGameObject == backButton.gameObject)
            {
                Debug.Log("<color=green>[[ 10. Coroutine ]] 성공: EventSystem의 현재 선택된 오브젝트가 BackButton으로 확인되었습니다.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[[ 10. Coroutine ]] 실패: SetSelectedGameObject 호출 후, EventSystem이 여전히 BackButton을 선택하고 있지 않습니다!</color>");
            }
        }
        else
        {
            // [로그 추가]
            Debug.LogError($"<color=red>[[ 9. Coroutine ]] 실패: BackButton이 Null이거나 비활성화 상태여서 포커스를 설정할 수 없습니다.</color>");
        }

        // [로그 추가]
        Debug.Log("[[ 11. Coroutine ]] SetupNavigationAndFocus 코루틴 종료.");
    }

    // 모든 버튼의 상하좌우 연결을 설정합니다. (키보드/패드용)
    private void SetupNavigation()
    {
        // 이 부분은 각 버튼의 RectTransform 위치에 따라 매우 복잡해지므로,
        // 여기서는 기본 원리만 설명하고 Unity 에디터의 'Automatic' 네비게이션에 맡기는 것을 권장합니다.
        // 수동 설정이 꼭 필요하다면 각 버튼의 Navigation 속성을 코드나 인스펙터에서 직접 연결해야 합니다.
        // 예: 
        // var nav = backButton.navigation;
        // nav.mode = Navigation.Mode.Explicit;
        // nav.selectOnUp = ownedCardDisplays[1].selectButton;
        // backButton.navigation = nav;
    }
}