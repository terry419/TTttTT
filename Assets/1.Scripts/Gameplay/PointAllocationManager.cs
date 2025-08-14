using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PointAllocationManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private PointAllocationResultUI resultUI;
    [SerializeField] private TMP_InputField pointsToInvestInput_Actual;
    [SerializeField] private Button inputActivationButton;
    [SerializeField] private TextMeshProUGUI inputActivationButtonText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI totalPointsText;

    private CharacterDataSO selectedCharacter;
    private int totalCharacterPoints;

    private void Awake()
    {
        inputActivationButton.onClick.AddListener(ActivateInputMode);
        confirmButton.onClick.AddListener(OnConfirmAllocationClicked);
        backButton.onClick.AddListener(OnBackClicked);
        pointsToInvestInput_Actual.onEndEdit.AddListener(DeactivateInputMode);
    }

    void Start()
    {
        InitializeAllocation();
        if (resultUI != null)
        {
            resultUI.gameObject.SetActive(true);
            resultUI.UpdateDisplay(selectedCharacter.baseStats, null);
        }
        pointsToInvestInput_Actual.gameObject.SetActive(false);
    }

    public void ActivateInputMode()
    {
        inputActivationButton.gameObject.SetActive(false);
        pointsToInvestInput_Actual.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(pointsToInvestInput_Actual.gameObject);
        pointsToInvestInput_Actual.ActivateInputField();
    }

    private void DeactivateInputMode(string text)
    {
        // 1. 먼저 입력값을 검증하고, 필요한 경우 InputField의 값을 수정합니다.
        ValidateInputValue();

        // 2. 그 다음에, '수정된' InputField의 텍스트를 읽어와 가짜 버튼에 표시합니다.
        string correctedText = pointsToInvestInput_Actual.text;
        if (string.IsNullOrEmpty(correctedText))
        {
            inputActivationButtonText.text = "포인트 입력...";
        }
        else
        {
            inputActivationButtonText.text = correctedText;
        }

        // 3. 원래 로직을 실행합니다.
        pointsToInvestInput_Actual.gameObject.SetActive(false);
        inputActivationButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(inputActivationButton.gameObject);
    }

    private void OnConfirmAllocationClicked()
    {
        // [수정] 분배할 포인트를 계산할 때도 '실제 입력 필드'의 값을 사용합니다.
        if (!int.TryParse(pointsToInvestInput_Actual.text, out int allocatedPoints))
        {
            allocatedPoints = 0;
        }
        GameManager.Instance.AllocatedPoints = allocatedPoints;

        CharacterPermanentStats permanentStats = ProgressionManager.Instance.GetPermanentStatsFor(selectedCharacter.characterId);
        Dictionary<StatType, int> distributedPoints = CalculateDistributedPoints(allocatedPoints, permanentStats);
        resultUI.UpdateDisplay(selectedCharacter.baseStats, distributedPoints);

        confirmButton.interactable = false;
        backButton.interactable = false;
        inputActivationButton.interactable = false;

        StartCoroutine(StartSceneTransitionAfterDelay(2f));
    }

    private void InitializeAllocation()
    {
        selectedCharacter = GameManager.Instance.SelectedCharacter ?? DataManager.Instance.GetCharacter("warrior");
        totalCharacterPoints = selectedCharacter.initialAllocationPoints;
        if (totalPointsText != null) totalPointsText.text = $"Total Points: {totalCharacterPoints}";
    }

    private void ValidateInputValue()
    {
        if (!string.IsNullOrEmpty(pointsToInvestInput_Actual.text) && int.TryParse(pointsToInvestInput_Actual.text, out int points))
        {
            if (points > totalCharacterPoints)
            {
                // 실제 InputField의 텍스트를 수정합니다.
                pointsToInvestInput_Actual.text = totalCharacterPoints.ToString();
            }
            else if (points < 0)
            {
                pointsToInvestInput_Actual.text = "0";
            }
        }
    }

    public void OnBackClicked()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.CharacterSelect);
    }

    private Dictionary<StatType, int> CalculateDistributedPoints(int pointsToDistribute, CharacterPermanentStats permStats)
    {
        var pointCounts = new Dictionary<StatType, int>();
        foreach (StatType type in System.Enum.GetValues(typeof(StatType))) pointCounts[type] = 0;
        List<StatType> availableStats = permStats.GetUnlockedStats();
        availableStats.Remove(StatType.CritRate);
        if (availableStats.Count == 0) return pointCounts;
        for (int i = 0; i < pointsToDistribute; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            pointCounts[targetStat]++;
        }
        return pointCounts;
    }

    private IEnumerator StartSceneTransitionAfterDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
    }
}