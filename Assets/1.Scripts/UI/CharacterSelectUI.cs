using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("컨트롤러 참조")]
    [SerializeField] private CharacterSelectController controller;
    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI totalPointsText;
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
        warriorButton.onClick.AddListener(() => SelectCharacter("warrior"));
        archerButton.onClick.AddListener(() => SelectCharacter("archer"));
        mageButton.onClick.AddListener(() => SelectCharacter("mage"));
        startButton.onClick.AddListener(OnGameStartClicked);
    }

    void Start()
    {
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
        CharacterDataSO characterData = ServiceLocator.Get<DataManager>().GetCharacter(characterId);
        if (characterData == null) return;

        controller.OnCharacterSelected(characterData);
        UpdateCharacterInfo(characterData);
        DeactivateAllCharacterIllustrations();

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