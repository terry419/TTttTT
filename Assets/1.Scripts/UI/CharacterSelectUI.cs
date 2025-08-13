using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("컨트롤러 참조")]
    [SerializeField] private CharacterSelectController controller;

    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private RectTransform cursorSprite;

    [Header("캐릭터별 일러스트 오브젝트 참조")]
    [SerializeField] private GameObject warriorCharacterIllust;
    [SerializeField] private GameObject archerCharacterIllust;
    [SerializeField] private GameObject mageCharacterIllust;

    [Header("버튼 목록")]
    [SerializeField] private Button warriorButton;
    [SerializeField] private Button archerButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button startButton;

    [Header("선택 상태 시각화")]
    [SerializeField] private Color selectedButtonColor = Color.yellow;
    [SerializeField] private Color unselectedButtonColor = Color.white;
    private Button currentlySelectedCharacterButton;

    void Awake()
    {
        // 각 버튼이 클릭되었을 때 어떤 함수를 실행할지 연결합니다.
        warriorButton.onClick.AddListener(() => SelectCharacter("warrior"));
        archerButton.onClick.AddListener(() => SelectCharacter("archer"));
        mageButton.onClick.AddListener(() => SelectCharacter("mage"));
        startButton.onClick.AddListener(OnGameStartClicked);
    }

    void OnEnable()
    {
        // 씬이 활성화될 때 UICursorManager에 이 씬의 커서를 등록합니다.
        if (UICursorManager.Instance != null)
        {
            UICursorManager.Instance.RegisterCursor(cursorSprite);
        }
    }

    void Start()
    {
        // 씬 시작 시 기본 상태를 설정합니다.
        DeactivateAllCharacterIllustrations();
        if (totalPointsText != null) totalPointsText.text = "보유 포인트: --";
        ResetButtonColors();
    }

    private void ResetButtonColors()
    {
        warriorButton.image.color = unselectedButtonColor;
        archerButton.image.color = unselectedButtonColor;
        mageButton.image.color = unselectedButtonColor;
        currentlySelectedCharacterButton = null;
    }

    private void DeactivateAllCharacterIllustrations()
    {
        if (warriorCharacterIllust != null) warriorCharacterIllust.SetActive(false);
        if (archerCharacterIllust != null) archerCharacterIllust.SetActive(false);
        if (mageCharacterIllust != null) mageCharacterIllust.SetActive(false);
    }

    private void SelectCharacter(string characterId)
    {
        CharacterDataSO characterData = DataManager.Instance.GetCharacter(characterId);
        if (characterData == null) return;

        controller.OnCharacterSelected(characterData);
        UpdateCharacterInfo(characterData);
        DeactivateAllCharacterIllustrations();

        // 선택 시각 효과 처리
        if (currentlySelectedCharacterButton != null)
        {
            currentlySelectedCharacterButton.image.color = unselectedButtonColor;
        }

        switch (characterId)
        {
            case "warrior":
                if (warriorCharacterIllust != null) warriorCharacterIllust.SetActive(true);
                currentlySelectedCharacterButton = warriorButton;
                break;
            case "archer":
                if (archerCharacterIllust != null) archerCharacterIllust.SetActive(true);
                currentlySelectedCharacterButton = archerButton;
                break;
            case "mage":
                if (mageCharacterIllust != null) mageCharacterIllust.SetActive(true);
                currentlySelectedCharacterButton = mageButton;
                break;
        }

        if (currentlySelectedCharacterButton != null)
        {
            currentlySelectedCharacterButton.image.color = selectedButtonColor;
        }
    }

    private void UpdateCharacterInfo(CharacterDataSO characterData)
    {
        if (totalPointsText != null)
        {
            totalPointsText.text = $"보유 포인트: {characterData.initialAllocationPoints}";
        }
    }

    private void OnGameStartClicked()
    {
        controller.ProceedToPointAllocation();
    }
}