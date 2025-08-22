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

    [Header("코어 로직 참조")]
    [SerializeField] private MapGenerator mapGenerator;

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

        if (mapGenerator == null)
        {
            Debug.LogError("[PointAllocationManager] MapGenerator 참조가 설정되지 않았습니다! Inspector에서 연결해주세요.");
        }
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
        ValidateInputValue();
        string correctedText = pointsToInvestInput_Actual.text;
        if (string.IsNullOrEmpty(correctedText))
        {
            inputActivationButtonText.text = "포인트 입력...";
        }
        else
        {
            inputActivationButtonText.text = correctedText;
        }
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
        ServiceLocator.Get<GameManager>().AllocatedPoints = allocatedPoints;

        CharacterPermanentStats permanentStats = ProgressionManager.Instance.GetPermanentStatsFor(selectedCharacter.characterId);
        Dictionary<StatType, int> distributedPoints = CalculateDistributedPoints(allocatedPoints, permanentStats);
        resultUI.UpdateDisplay(selectedCharacter.baseStats, distributedPoints);

        confirmButton.interactable = false;
        backButton.interactable = false;
        inputActivationButton.interactable = false;

        Debug.Log("[PointAllocationManager] 확인 버튼 클릭됨. 맵 생성을 시작합니다.");
        if (mapGenerator != null && MapManager.Instance != null)
        {
            List<MapNode> mapData = mapGenerator.Generate();

            // [수정됨] 생성된 맵 데이터와 '맵 크기'를 MapManager에 전달하여 초기화합니다.
            MapManager.Instance.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
            Debug.Log("[PointAllocationManager] MapManager 초기화 완료.");
        }
        else
        {
            Debug.LogError("[PointAllocationManager] MapGenerator 또는 MapManager 참조가 없습니다! 맵을 생성할 수 없습니다.");
            return; 
        }

        StartCoroutine(StartSceneTransitionAfterDelay(2f));
    }

    private void InitializeAllocation()
    {
        selectedCharacter = ServiceLocator.Get<GameManager>().SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter("warrior");
        totalCharacterPoints = selectedCharacter.initialAllocationPoints;
        if (totalPointsText != null) totalPointsText.text = $"Total Points: {totalCharacterPoints}";
    }

    private void ValidateInputValue()

    {
        if (!string.IsNullOrEmpty(pointsToInvestInput_Actual.text) && int.TryParse(pointsToInvestInput_Actual.text, out int points))
        {
            if (points > totalCharacterPoints)
            {
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
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.CharacterSelect);
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
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Gameplay);
    }
}
