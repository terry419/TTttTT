using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem 사용을 위해 추가

/// <summary>
/// Pause 씬의 UI 요소에 대한 참조를 관리하고, 버튼 클릭 이벤트를 GameManager에 연결합니다.
/// </summary>
public class PauseUIController : MonoBehaviour
{
    [Header("UI 버튼 목록")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button codexButton;
    [SerializeField] private Button abandonButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button inventoryButton;

    [Header("연결될 패널")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject mainPausePanel;

    [Header("인벤토리 UI 요소")]
    [SerializeField] private Button inventoryBackButton;


    void Start()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            continueButton.onClick.AddListener(gameManager.ResumeGame);
            abandonButton.onClick.AddListener(gameManager.AbandonRun);
            optionButton.onClick.AddListener(() => gameManager.ChangeState(GameManager.GameState.Options));
        }
        else
        {
            Debug.LogError("[PauseUIController] GameManager를 찾을 수 없어 버튼의 이벤트를 연결하는데 실패했습니다!");
        }

        inventoryButton.onClick.AddListener(OpenInventoryPanel);
        quitButton.onClick.AddListener(QuitGame);
        
        if(inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        SetupNavigation();
    }

    /// <summary>
    /// 인벤토리 패널을 열고 메인 일시정지 메뉴를 숨깁니다.
    /// </summary>
    public void OpenInventoryPanel()
    {
        if (inventoryPanel == null) return;

        mainPausePanel.SetActive(false);

        // InventoryManager의 API를 사용하여 패널을 엽니다.
        var invManager = inventoryPanel.GetComponent<InventoryManager>();
        if (invManager != null)
        {
            invManager.Show(OnInventoryClosed); // 닫혔을 때 실행될 콜백을 전달합니다.
        }
        else
        {
            // Fallback: InventoryManager가 없을 경우 직접 활성화
            inventoryPanel.SetActive(true);
        }

        var invController = inventoryPanel.GetComponent<InventoryController>();
        if (invController != null)
        {
            invController.SetInteractive(false);
        }

        if (inventoryBackButton != null)
        {
            EventSystem.current.SetSelectedGameObject(inventoryBackButton.gameObject);

            Navigation nav = inventoryBackButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = inventoryBackButton;
            nav.selectOnDown = inventoryBackButton;
            nav.selectOnLeft = inventoryBackButton;
            nav.selectOnRight = inventoryBackButton;
            inventoryBackButton.navigation = nav;
        }
    }

    /// <summary>
    /// 인벤토리 패널이 닫힐 때 InventoryManager에 의해 호출될 콜백 메소드입니다.
    /// </summary>
    private void OnInventoryClosed()
    {
        mainPausePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(inventoryButton.gameObject);
    }

    /// <summary>
    /// 게임을 종료합니다.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public GameObject GetFirstFocusableElement()
    {
        return continueButton.gameObject;
    }

    private void SetupNavigation()
    {
        // --- 1번째 줄 ---
        Navigation navInventory = inventoryButton.navigation;
        navInventory.mode = Navigation.Mode.Explicit;
        navInventory.selectOnUp = quitButton;
        navInventory.selectOnDown = codexButton;
        navInventory.selectOnLeft = inventoryButton;
        navInventory.selectOnRight = inventoryButton;
        inventoryButton.navigation = navInventory;

        // --- 2번째 줄 ---
        Navigation navContinue = continueButton.navigation;
        navContinue.mode = Navigation.Mode.Explicit;
        navContinue.selectOnUp = inventoryButton;
        navContinue.selectOnDown = abandonButton;
        navContinue.selectOnLeft = codexButton;
        navContinue.selectOnRight = codexButton;
        continueButton.navigation = navContinue;

        Navigation navCodex = codexButton.navigation;
        navCodex.mode = Navigation.Mode.Explicit;
        navCodex.selectOnUp = inventoryButton;
        navCodex.selectOnDown = optionButton;
        navCodex.selectOnLeft = continueButton;
        navCodex.selectOnRight = continueButton;
        codexButton.navigation = navCodex;

        // --- 3번째 줄 ---
        Navigation navAbandon = abandonButton.navigation;
        navAbandon.mode = Navigation.Mode.Explicit;
        navAbandon.selectOnUp = continueButton;
        navAbandon.selectOnDown = inventoryButton;
        navAbandon.selectOnLeft = quitButton;
        navAbandon.selectOnRight = optionButton;
        abandonButton.navigation = navAbandon;

        Navigation navOption = optionButton.navigation;
        navOption.mode = Navigation.Mode.Explicit;
        navOption.selectOnUp = codexButton;
        navOption.selectOnDown = inventoryButton;
        navOption.selectOnLeft = abandonButton;
        navOption.selectOnRight = quitButton;
        optionButton.navigation = navOption;

        Navigation navQuit = quitButton.navigation;
        navQuit.mode = Navigation.Mode.Explicit;
        navQuit.selectOnUp = codexButton;
        navQuit.selectOnDown = inventoryButton;
        navQuit.selectOnLeft = optionButton;
        navQuit.selectOnRight = abandonButton;
        quitButton.navigation = navQuit;
    }
}