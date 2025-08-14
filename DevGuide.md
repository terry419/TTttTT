
아래 순서대로 씬을 구현하는 것을 추천합니다.

퍼즈(Pause) UI

옵션(Options) 씬

카드 보상(Card Reward) 씬

도감(Codex) 씬

IV. 씬별 상세 구현 지시서
2. 퍼즈(Pause) UI
목표: 퍼즈.png 시안에 따라, Gameplay 씬에서 ESC 키로 여는 일시정지 메뉴를 구현합니다.

2.1. UI 구조 만들기 (Hierarchy)
Gameplay.unity 씬을 엽니다.

GameUICanvas 하위에 UI → Panel을 생성하고, 이름을 Panel_Pause 로 지정합니다.

Panel_Pause 하위에 시안과 같이 모든 버튼(UI → Button - TextMeshPro)을 생성하고 이름을 지정합니다.

Button_Continue (텍스트: "계속하기")

Button_Codex (텍스트: "도감")

Button_Forfeit (텍스트: "포기하기")

Button_Options (텍스트: "옵션")

Button_ExitGame (텍스트: "게임 종료")

Panel_Pause 오브젝트를 선택하고, Inspector 좌측 상단의 체크박스를 해제하여 비활성화합니다.

2.2. 스크립트 작성 및 추가
Assets/1.Scripts/UI 폴더에 PauseController.cs 스크립트를 새로 만들고, 아래 코드를 붙여넣습니다.

C#

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueButton; // 퍼즈 시 기본으로 선택될 버튼
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private Button codexButton; // 도감 버튼 추가

    private bool isPaused = false;

    void Start()
    {
        // InputManager의 OnCancel 이벤트(ESC 키)가 발생하면 TogglePause 함수를 실행하도록 구독
        InputManager.Instance.OnCancel.AddListener(TogglePause);

        // 각 버튼의 onClick 이벤트에 실행할 함수를 연결
        continueButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        exitGameButton.onClick.AddListener(ExitToMainMenu);
        codexButton.onClick.AddListener(OpenCodex);

        // 시작할 때는 퍼즈 패널을 비활성화
        pausePanel.SetActive(false);
    }

    void OnDestroy()
    {
        // 씬이 파괴될 때 구독을 해제하여 메모리 누수 방지
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancel.RemoveListener(TogglePause);
        }
    }

    public void TogglePause()
    {
        // 게임 플레이 중에만 퍼즈 가능하도록 조건 추가
        if (GameManager.Instance.CurrentState != GameManager.GameState.Gameplay) return;

        isPaused = !isPaused;
        if (isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // 게임 시간을 멈춤
        pausePanel.SetActive(true);

        // 퍼즈 메뉴가 열리면 '계속하기' 버튼을 기본으로 선택
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // 게임 시간을 다시 흐르게 함
        pausePanel.SetActive(false);
    }

    private void OpenOptions()
    {
        Time.timeScale = 1f;
        // GameManager에 Options 씬 상태를 추가하고 연결해야 합니다.
        GameManager.Instance.ChangeState(GameManager.GameState.Codex); // 임시로 Codex로 연결, 추후 Options 상태 추가 필요
        Debug.Log("옵션 씬으로 이동 (GameManager 연동 필요)");
    }

    private void OpenCodex()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.Codex);
    }

    private void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
    }
}
Hierarchy의 Panel_Pause 오브젝트에 PauseController.cs 스크립트를 추가합니다.

2.3. Inspector 참조 연결
Panel_Pause 오브젝트를 선택합니다.

Pause Controller 컴포넌트의 각 슬롯에 Hierarchy의 오브젝트들을 드래그하여 연결합니다.

Pause Panel ← Panel_Pause (자기 자신)

Continue Button ← Button_Continue

Options Button ← Button_Options

Exit Game Button ← Button_ExitGame

Codex Button ← Button_Codex

3. 옵션 씬
목표: 옵션.png 시안에 따라 게임 설정을 변경하는 씬을 구현합니다.

3.1. UI 구조 만들기
Options.unity 씬을 새로 만듭니다.

Canvas와 EventSystem을 생성합니다.

시안에 맞게 UI 요소들(Slider, Toggle, Dropdown, Button)을 배치합니다.

3.2. 스크립트 작성 및 추가
OptionsController.cs 스크립트를 새로 만들고 아래 코드를 붙여넣습니다.

C#

using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button backButton;

    void Awake()
    {
        backButton.onClick.AddListener(BackToPreviousScene);
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    void Start()
    {
        // 저장된 설정값 불러오기 (PlayerPrefs 사용 예시)
        bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void SetBgmVolume(float volume) {
        // AudioManager.Instance.SetBgmVolume(volume); // AudioManager가 준비되면 이 부분을 활성화
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSfxVolume(float volume) {
        // AudioManager.Instance.SetSfxVolume(volume); // AudioManager가 준비되면 이 부분을 활성화
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void BackToPreviousScene()
    {
        PlayerPrefs.Save(); // 설정을 저장하고 메인 메뉴로 돌아감
        // 이전 씬으로 돌아가는 로직이 필요하지만, 여기서는 메인메뉴로 고정합니다.
        GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
    }
}
Options 씬의 Canvas에 OptionsController.cs 스크립트를 추가하고, Inspector에서 각 UI 요소들을 연결합니다.

EventSystem의 First Selected 에 처음 선택될 UI(예: backButton)를 연결합니다.

4. 카드 보상 씬
목표: 카드 보상.png 시안에 따라 라운드 클리어 후 카드 보상을 선택하는 씬을 구현합니다.

4.1. 프리팹 및 UI 구조 만들기
카드 한 장의 UI(이미지, 이름, 설명, 전체를 감싸는 투명 Button)를 패널로 구성하고, 이것을 CardDisplay.prefab 으로 만듭니다. 이 프리팹에는 CardDisplay.cs 스크립트가 붙어있어야 합니다.

CardReward.unity 씬을 새로 만들고, Canvas와 EventSystem을 배치합니다.

카드가 생성될 빈 GameObject들(Slot_1, Slot_2, Slot_3, Slot_4)과 버튼들(Button_Acquire, Button_Synthesize, Button_Skip, Button_Map)을 배치합니다.

4.2. 스크립트 작성 및 추가
CardRewardController.cs 스크립트를 새로 만들고 아래 코드를 붙여넣습니다.

C#

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardRewardController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private List<Transform> cardSlots;
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;

    [Header("프리팹")]
    [SerializeField] private GameObject cardDisplayPrefab;

    private List<GameObject> spawnedCardUIs = new List<GameObject>();
    private CardDataSO selectedCard = null;

    void Awake()
    {
        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
    }

    public void Initialize(List<CardDataSO> cardChoices)
    {
        foreach (var ui in spawnedCardUIs) Destroy(ui);
        spawnedCardUIs.Clear();
        selectedCard = null;

        for (int i = 0; i < cardChoices.Count && i < cardSlots.Count; i++)
        {
            cardSlots[i].gameObject.SetActive(true);
            GameObject cardUI = Instantiate(cardDisplayPrefab, cardSlots[i]);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            cardDisplay.Setup(cardChoices[i]);
            cardDisplay.selectButton.onClick.AddListener(() => OnCardSelected(cardDisplay));
            spawnedCardUIs.Add(cardUI);
        }

        // 사용되지 않은 슬롯은 비활성화
        for (int i = cardChoices.Count; i < cardSlots.Count; i++)
        {
            cardSlots[i].gameObject.SetActive(false);
        }

        UpdateButtonsState();
        if (spawnedCardUIs.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardUIs[0].GetComponent<CardDisplay>().selectButton.gameObject);
        }
    }

    public void OnCardSelected(CardDisplay cardDisplay)
    {
        selectedCard = cardDisplay.GetCurrentCard();
        foreach(var ui in spawnedCardUIs) ui.GetComponent<CardDisplay>().SetHighlight(false);
        cardDisplay.SetHighlight(true);
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        acquireButton.interactable = (selectedCard != null);
        synthesizeButton.interactable = (selectedCard != null && CardManager.Instance.HasSynthesizablePair(selectedCard));
    }

    private void OnAcquireClicked() { if (selectedCard != null) RewardManager.Instance.OnCardRewardConfirmed(selectedCard); }
    private void OnSynthesizeClicked() { /* 합성 로직 구현 */ }
    private void OnSkipClicked() { RewardManager.Instance.OnCardRewardSkipped(); }
}
CardReward 씬의 Canvas에 CardRewardController.cs를 추가하고, Inspector에서 모든 UI 요소와 프리팹을 연결합니다.

5. 도감 씬
목표: 카드 도감.png 시안에 따라 게임의 모든 카드와 유물 정보를 보여주는 씬을 구현합니다.

5.1. 프리팹 및 UI 구조 만들기
도감 목록의 아이템 하나(이름, 배경 등)를 버튼 형태로 CodexListItem.prefab 으로 만듭니다.

Codex.unity 씬을 만들고, Canvas, EventSystem, 탭 버튼, Scroll View, 상세 정보 패널(Panel_Details) 등을 배치합니다.

5.2. 스크립트 작성 및 추가
CodexController.cs를 새로 만들고 아래 코드를 붙여넣습니다.

C#

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CodexController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button cardTabButton;
    [SerializeField] private Button artifactTabButton;
    [SerializeField] private Transform contentRoot; // Scroll View의 Content 오브젝트
    [SerializeField] private TextMeshProUGUI detailNameText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    // (기타 상세 정보 UI 요소들 추가)
    [SerializeField] private Button backButton;

    [Header("프리팹")]
    [SerializeField] private GameObject codexListItemPrefab;

    private List<GameObject> spawnedListItems = new List<GameObject>();

    void Awake()
    {
        cardTabButton.onClick.AddListener(PopulateCardList);
        artifactTabButton.onClick.AddListener(PopulateArtifactList);
        backButton.onClick.AddListener(BackToMainMenu);
    }

    void Start()
    {
        PopulateCardList();
    }

    void ClearList()
    {
        foreach (var item in spawnedListItems) Destroy(item);
        spawnedListItems.Clear();
    }

    void PopulateCardList()
    {
        ClearList();
        List<CardDataSO> allCards = DataManager.Instance.GetAllCards();
        foreach (var cardData in allCards)
        {
            GameObject itemGO = Instantiate(codexListItemPrefab, contentRoot);
            itemGO.GetComponentInChildren<TextMeshProUGUI>().text = cardData.cardName;
            itemGO.GetComponent<Button>().onClick.AddListener(() => OnCardListItemClicked(cardData));
            spawnedListItems.Add(itemGO);
        }
        if(spawnedListItems.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedListItems[0]);
        }
    }

    void PopulateArtifactList() 
    {
        ClearList();
        List<ArtifactDataSO> allArtifacts = DataManager.Instance.GetAllArtifacts();
        // 카드 목록과 동일한 방식으로 유물 목록 구현
    }

    void OnCardListItemClicked(CardDataSO cardData)
    {
        detailNameText.text = cardData.cardName;
        detailDescriptionText.text = cardData.effectDescription;
    }

    void OnArtifactListItemClicked(ArtifactDataSO artifactData)
    {
        // 유물 상세 정보 표시
    }

    void BackToMainMenu() { GameManager.Instance.ChangeState(GameManager.GameState.MainMenu); }
}
Codex 씬의 Canvas에 CodexController.cs를 추가하고, Inspector에서 모든 UI 요소와 프리팹을 연결합니다.

EventSystem의 First Selected 에 cardTabButton을 연결합니다.