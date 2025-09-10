// ./TTttTT/Assets/1.Scripts/UI/CharacterSelectUI.cs

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 
using UnityEngine.EventSystems; 

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
        warriorButton.onClick.AddListener(() => SelectCharacter(CharacterIDs.Warrior));
        archerButton.onClick.AddListener(() => SelectCharacter(CharacterIDs.Archer));
        mageButton.onClick.AddListener(() => SelectCharacter(CharacterIDs.Mage));
        startButton.onClick.AddListener(OnGameStartClicked);
    }

    void Start()
    {
        DeactivateAllCharacterIllustrations();
        if (totalPointsText != null) totalPointsText.text = "보유 포인트: --";
        ResetButtonColors();

        StartCoroutine(SetInitialFocus());
    }

    private IEnumerator SetInitialFocus()
    {
        // EventSystem이 UI 요소들을 인식할 때까지 한 프레임 기다립니다.
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        // 기본 선택 버튼인 '전사' 버튼에 포커스를 맞춥니다.
        EventSystem.current.SetSelectedGameObject(warriorButton.gameObject);
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
            case CharacterIDs.Warrior:
                if (warriorCharacterIllust != null) warriorCharacterIllust.SetActive(true);
                currentlySelectedCharacterButton = warriorButton;
                break;
            case CharacterIDs.Archer:
                if (archerCharacterIllust != null) archerCharacterIllust.SetActive(true);
                currentlySelectedCharacterButton = archerButton;
                break;
            case CharacterIDs.Mage:
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