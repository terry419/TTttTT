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
    [SerializeField] private TMP_InputField pointsToInvestInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    [SerializeField] private RectTransform cursorSprite;

    private CharacterDataSO selectedCharacter;
    private int totalCharacterPoints;

    void OnEnable()
    {
        // 씬이 활성화될 때 UICursorManager에 이 씬의 커서를 등록합니다.
        if (UICursorManager.Instance != null)
        {
            UICursorManager.Instance.RegisterCursor(cursorSprite);
        }
    }

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmAllocationClicked);
        backButton.onClick.AddListener(OnBackClicked);
        pointsToInvestInput.onEndEdit.AddListener(OnInputEndEdit);
        pointsToInvestInput.contentType = TMP_InputField.ContentType.IntegerNumber;
    }

    void Start()
    {
        InitializeAllocation();
        if (resultUI != null)
        {
            resultUI.gameObject.SetActive(true);
            resultUI.UpdateDisplay(selectedCharacter.baseStats, null);
        }
        pointsToInvestInput.Select();
    }

    private void OnInputEndEdit(string text)
    {
        ValidateInputValue();
        if (Input.GetButtonDown("Submit"))
        {
            EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
        }
    }

    private void OnConfirmAllocationClicked()
    {
        if (!int.TryParse(pointsToInvestInput.text, out int allocatedPoints))
        {
            allocatedPoints = 0;
        }
        GameManager.Instance.AllocatedPoints = allocatedPoints;

        CharacterPermanentStats permanentStats = ProgressionManager.Instance.GetPermanentStatsFor(selectedCharacter.characterId);
        Dictionary<StatType, int> distributedPoints = CalculateDistributedPoints(allocatedPoints, permanentStats);
        resultUI.UpdateDisplay(selectedCharacter.baseStats, distributedPoints);

        confirmButton.interactable = false;
        backButton.interactable = false;
        pointsToInvestInput.interactable = false;

        StartCoroutine(StartSceneTransitionAfterDelay(2f));
    }

    private void InitializeAllocation()
    {
        selectedCharacter = (GameManager.Instance?.SelectedCharacter != null) ? GameManager.Instance.SelectedCharacter : DataManager.Instance.GetCharacter("warrior");
        totalCharacterPoints = selectedCharacter.initialAllocationPoints;
        if (totalPointsText != null) totalPointsText.text = $"Total Points: {totalCharacterPoints}";
    }

    private void ValidateInputValue()
    {
        if (!string.IsNullOrEmpty(pointsToInvestInput.text) && int.TryParse(pointsToInvestInput.text, out int points))
        {
            if (points > totalCharacterPoints) pointsToInvestInput.text = totalCharacterPoints.ToString();
            else if (points < 0) pointsToInvestInput.text = "0";
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