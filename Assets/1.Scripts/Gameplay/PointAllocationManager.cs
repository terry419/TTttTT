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
        if (string.IsNullOrEmpty(text))
        {
            inputActivationButtonText.text = "포인트 입력...";
        }
        else
        {
            inputActivationButtonText.text = text;
        }
        ValidateInputValue();
        pointsToInvestInput_Actual.gameObject.SetActive(false);
        inputActivationButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(inputActivationButton.gameObject);
    }

    private void OnConfirmAllocationClicked()
    {
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
            if (points > totalCharacterPoints) pointsToInvestInput_Actual.text = totalCharacterPoints.ToString();
            else if (points < 0) pointsToInvestInput_Actual.text = "0";
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