using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 캐릭터 선택 UI의 입출력과 상호작용을 담당합니다.
/// InputManager와 연동하여 키보드 입력을 처리하고, 선택 결과를 CharacterSelectController에 전달합니다.
/// </summary>
public class CharacterSelectUI : MonoBehaviour
{
    [Header("컨트롤러 참조")]
    [SerializeField] private CharacterSelectController controller;

    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI totalPointsText; // 보유 중인 총 포인트 (캐릭터별 연동)
    [SerializeField] private RectTransform cursorSprite; // 커서 RectTransform

    [Header("캐릭터별 일러스트 참조")]
    [SerializeField] private Image warriorIllustration; // 워리어 일러스트
    [SerializeField] private Image archerIllustration; // 아처 일러스트
    [SerializeField] private Image mageIllustration; // 메이지 일러스트

    [Header("메인 일러스트 스프라이트 참조")]
    [SerializeField] private Sprite warriorMainIllustrationSprite; // 워리어 메인 일러스트 스프라이트
    [SerializeField] private Sprite archerMainIllustrationSprite; // 아처 메인 일러스트 스프라이트
    [SerializeField] private Sprite mageMainIllustrationSprite; // 메이지 메인 일러스트 스프라이트

    [Header("버튼 이미지 참조")]
    [SerializeField] private Image warriorButtonImage; // 워리어 버튼의 Image 컴포넌트
    [SerializeField] private Image archerButtonImage; // 아처 버튼의 Image 컴포넌트
    [SerializeField] private Image mageButtonImage; // 메이지 버튼의 Image 컴포넌트

    [Header("버튼 목록")]
    [SerializeField] private Button warriorButton;
    [SerializeField] private Button archerButton;
    [SerializeField] private Button mageButton;
    [SerializeField] private Button StartButton; // Renamed from gameStartButton

    // 키보드 네비게이션을 위한 버튼 리스트
    private List<Button> selectableButtons;
    private int currentButtonIndex = 0;

    // --- 추가된 부분: 선택 상태 시각화 --- (기능 변경)
    [Header("선택 상태 시각화")]
    [SerializeField] private Color selectedButtonColor = Color.yellow; // 선택된 버튼 색상
    [SerializeField] private Color unselectedButtonColor = Color.white; // 선택되지 않은 버튼 색상
    private Button currentlySelectedCharacterButton; // 현재 선택된 캐릭터 버튼 추적
    // --- 추가된 부분 끝 ---

    void Awake()
    {
        // 버튼 리스트 초기화 (네비게이션 순서대로)
        selectableButtons = new List<Button> { warriorButton, archerButton, mageButton, StartButton }; // Use StartButton

        // 각 버튼의 OnClick 이벤트에 메서드 연결
        warriorButton.onClick.AddListener(() => SelectCharacter(warriorButton));
        archerButton.onClick.AddListener(() => SelectCharacter(archerButton));
        mageButton.onClick.AddListener(() => SelectCharacter(mageButton));
        StartButton.onClick.AddListener(OnGameStartClicked); // Use StartButton
    }

    // 캐릭터 버튼에 일러스트를 설정하는 헬퍼 함수
    private void SetCharacterButtonImages()
    {
        // DataManager에서 각 캐릭터의 데이터를 가져와 버튼 이미지에 할당합니다.
        // 이 함수는 Awake에서 한 번만 호출됩니다.
        CharacterDataSO warriorData = DataManager.Instance.GetCharacter("warrior");
        if (warriorData != null && warriorButtonImage != null) warriorButtonImage.sprite = warriorData.illustration;

        CharacterDataSO archerData = DataManager.Instance.GetCharacter("archer");
        if (archerData != null && archerButtonImage != null) archerButtonImage.sprite = archerData.illustration;

        CharacterDataSO mageData = DataManager.Instance.GetCharacter("mage");
        if (mageData != null && mageButtonImage != null) mageButtonImage.sprite = mageData.illustration;
    }

    void OnEnable()
    {
        // UI가 활성화될 때 InputManager 이벤트 구독
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.AddListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.AddListener(HandleSubmitInput);
        }

        // 시작 시 커서를 첫 번째 버튼(워리어)에 위치시킵니다.
        currentButtonIndex = 0;
        UpdateCursorPosition();

        // --- 추가된 부분: 초기화 시 모든 캐릭터 버튼 색상 리셋 --- (기능 변경)
        warriorButton.image.color = unselectedButtonColor;
        archerButton.image.color = unselectedButtonColor;
        mageButton.image.color = unselectedButtonColor;
        currentlySelectedCharacterButton = null; // 초기에는 선택된 캐릭터 없음
        // --- 추가된 부분 끝 ---
    }

    void Start()
    {
        // 초기에는 어떤 캐릭터 UI도 활성화하지 않습니다.
        // project_plan.md의 "씬 돌입 시 커서는 Warrior에게 되어 있어야 하나, 선택된 상황은 아니기 때문에 화면 중앙과 좌측에는 아무 일러스트도 나오지 않아야 하며"를 따릅니다.
        DeactivateAllCharacterUI();

        // 초기 포인트 텍스트 설정 (캐릭터 선택 전)
        if (totalPointsText != null)
        {
            totalPointsText.text = "보유 포인트: --"; // project_plan.md에 명시된 초기값
        }

        // DataManager의 데이터 로드가 완료된 후 버튼 이미지 설정
        SetCharacterButtonImages();
    }

    // 모든 캐릭터별 UI 요소를 비활성화하는 헬퍼 함수
    private void DeactivateAllCharacterUI()
    {
        warriorIllustration.gameObject.SetActive(false);
        archerIllustration.gameObject.SetActive(false);
        mageIllustration.gameObject.SetActive(false);
        // totalPointsText는 항상 활성화되어 있어야 하므로 비활성화하지 않습니다.
    }

    void OnDisable()
    {
        // UI가 비활성화될 때 InputManager 이벤트 구독 해제
        if (InputManager.Instance != null) // InputManager가 먼저 파괴될 경우를 대비
        {
            InputManager.Instance.OnMove.RemoveListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.RemoveListener(HandleSubmitInput);
        }
    }

    private void HandleMoveInput(Vector2 input)
    {
        // 입력 벡터의 크기가 0에 가까우면 입력을 무시합니다. (Deadzone)
        if (input.magnitude < 0.5f) return;

        int previousButtonIndex = currentButtonIndex;

        // 수직 입력(W, S, 위/아래 화살표)과 수평 입력(A, D, 좌/우 화살표) 중 어느 쪽의 입력이 더 강한지 확인합니다.
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            if (input.y > 0) // 위
            {
                currentButtonIndex = (currentButtonIndex - 1 + selectableButtons.Count) % selectableButtons.Count;
            }
            else if (input.y < 0) // 아래
            {
                currentButtonIndex = (currentButtonIndex + 1) % selectableButtons.Count;
            }
        }
        else
        {
            if (input.x < 0) // 왼쪽 (위와 동일하게 처리)
            {
                currentButtonIndex = (currentButtonIndex - 1 + selectableButtons.Count) % selectableButtons.Count;
            }
            else if (input.x > 0) // 오른쪽 (아래와 동일하게 처리)
            {
                currentButtonIndex = (currentButtonIndex + 1) % selectableButtons.Count;
            }
        }

        if (previousButtonIndex != currentButtonIndex)
        {
            UpdateCursorPosition();
        }
    }

    private void HandleSubmitInput()
    {
        Debug.Log("[HandleSubmitInput] 함수 호출됨."); // 이 줄을 추가합니다.
        // 현재 커서가 위치한 버튼의 onClick 이벤트 실행
        Debug.Log($"[HandleSubmitInput] Invoking onClick for button index: {currentButtonIndex}, name: {selectableButtons[currentButtonIndex].name}");
        selectableButtons[currentButtonIndex].onClick.Invoke();
    }

    private void UpdateCursorPosition()
    {
        Button selectedButton = selectableButtons[currentButtonIndex];
        cursorSprite.position = selectedButton.transform.position;
    }

    private void SelectCharacter(Button selectedButton)
    {
        Debug.Log($"[SelectCharacter] Selected button: {selectedButton.name}");
        string characterId = "";
        if (selectedButton == warriorButton) characterId = "warrior"; // 실제 사용할 ID
        else if (selectedButton == archerButton) characterId = "archer";
        else if (selectedButton == mageButton) characterId = "mage";

        Debug.Log($"[SelectCharacter] Character ID determined: {characterId}");

        CharacterDataSO characterData = DataManager.Instance.GetCharacter(characterId);
        if (characterData != null)
        {
            controller.OnCharacterSelected(characterData);
            UpdateCharacterInfo(characterData);

            // --- 추가된 부분: 시각적 선택 상태 업데이트 --- (기능 변경)
            if (currentlySelectedCharacterButton != null)
            {
                currentlySelectedCharacterButton.image.color = unselectedButtonColor; // 이전 선택 해제
            }
            currentlySelectedCharacterButton = selectedButton;
            currentlySelectedCharacterButton.image.color = selectedButtonColor; // 현재 선택 표시
            // --- 추가된 부분 끝 ---
        }
        else
        {
            Debug.LogWarning($"{characterId}에 해당하는 캐릭터 데이터를 찾을 수 없습니다.");
        }
    }

    private void UpdateCharacterInfo(CharacterDataSO characterData)
    {
        Debug.Log($"[UpdateCharacterInfo] Updating UI for character: {characterData.characterName} ({characterData.characterId})");
        if (characterData == null) return;

        // 모든 캐릭터별 일러스트를 비활성화합니다.
        // project_plan.md의 "커서 이동을 통해 캐릭터를 선택하면 해당 캐릭터에 할당된 특수한 일러스트가 나와야 한다"를 따릅니다.
        // 여기서 "선택"은 Enter 키를 누르는 것을 의미합니다.
        warriorIllustration.gameObject.SetActive(false);
        archerIllustration.gameObject.SetActive(false);
        mageIllustration.gameObject.SetActive(false);

        // 선택된 캐릭터에 따라 해당 일러스트를 활성화하고 정보를 업데이트합니다.
        Image currentIllustration = null;

        switch (characterData.characterId)
        {
            case "warrior":
                currentIllustration = warriorIllustration;
                break;
            case "archer":
                currentIllustration = archerIllustration;
                break;
            case "mage":
                currentIllustration = mageIllustration;
                break;
        }

        if (currentIllustration != null) currentIllustration.gameObject.SetActive(true);

        // DataManager에서 받아온 실제 데이터로 UI를 채웁니다.
        if (currentIllustration != null)
        {
            switch (characterData.characterId)
            {
                case "warrior":
                    currentIllustration.sprite = warriorMainIllustrationSprite;
                    break;
                case "archer":
                    currentIllustration.sprite = archerMainIllustrationSprite;
                    break;
                case "mage":
                    currentIllustration.sprite = mageMainIllustrationSprite;
                    break;
            }
        }

        // 캐릭터별 초기 할당 포인트 표시
        if (totalPointsText != null)
        {
            totalPointsText.text = $"보유 포인트: {characterData.initialAllocationPoints}";
        }

        Debug.Log($"{characterData.characterName} 정보 표시.");
    }

    private void OnGameStartClicked()
    {
        Debug.Log("게임 시작 버튼 클릭됨. 컨트롤러에 포인트 분배 진행 요청.");
        controller.ProceedToPointAllocation();
    }
}