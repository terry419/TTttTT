using Cysharp.Threading.Tasks;
using System.Collections; 
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.UI;

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
        StartCoroutine(SetInitialFocus());
    }
    private IEnumerator SetInitialFocus()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);

        // 'DefalutFocus' 태그가 있었던 'inputActivationButton'에 포커스를 맞춥니다.
        EventSystem.current.SetSelectedGameObject(inputActivationButton.gameObject);
    }

    private async void OnConfirmAllocationClicked()
    {
        if (!int.TryParse(pointsToInvestInput_Actual.text, out int allocatedPoints))
        {
            allocatedPoints = 0;
        }

        // --- ServiceLocator를 통해 필요한 매니저들을 미리 가져옵니다. ---
        var gameManager = ServiceLocator.Get<GameManager>();
        var progressionManager = ServiceLocator.Get<ProgressionManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
         PrepareForNextScene(selectedCharacter);

        gameManager.AllocatedPoints = allocatedPoints;

        CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(selectedCharacter.characterId);
        Dictionary<StatType, int> distributedPoints = CalculateDistributedPoints(allocatedPoints, permanentStats);
        resultUI.UpdateDisplay(selectedCharacter.baseStats, distributedPoints);

        confirmButton.interactable = false;
        backButton.interactable = false;
        inputActivationButton.interactable = false;

        Debug.Log("[PointAllocationManager] 확인 버튼 클릭됨. 맵 생성을 시작합니다.");

        // 1. 맵 데이터를 먼저 생성하고 MapManager를 초기화합니다.
        if (mapGenerator != null && mapManager != null)
        {
            List<MapNode> mapData = mapGenerator.Generate();
            mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
            Debug.Log("[PointAllocationManager] MapManager 초기화 완료.");
        }
        else
        {
            Debug.LogError("[PointAllocationManager] MapGenerator 또는 MapManager 참조가 없어 맵을 생성할 수 없습니다!");
            confirmButton.interactable = true;
            backButton.interactable = true;
            inputActivationButton.interactable = true;
            return;
        }

        // 2. 캠페인 매니저를 통해 이번에 플레이할 캠페인을 미리 선택합니다.
        CampaignDataSO selectedCampaign = campaignManager.SelectRandomCampaign();
        if (selectedCampaign == null)
        {
            confirmButton.interactable = true;
            backButton.interactable = true;
            inputActivationButton.interactable = true;
            return;
        }

        // 3. 선택된 캠페인의 첫 번째 라운드 데이터를 가져옵니다.
        MapNode firstNode = mapManager.GetReachableNodes().FirstOrDefault();
        RoundDataSO firstRoundData = campaignManager.GetRoundDataForNode(firstNode);
        if (firstRoundData == null)
        { 
            Debug.LogError("첫 라운드 데이터를 찾을 수 없어 프리로딩을 시작할 수 없습니다!");
            confirmButton.interactable = true;
            backButton.interactable = true;
            inputActivationButton.interactable = true;
            return;
        }

        // 4. GameManager의 준비 코루틴에 첫 라운드 데이터를 전달하여 프리로딩을 시작합니다.
        await gameManager.PreloadAssetsForRound(firstRoundData);
        OnPreloadComplete();
    }

    private void PrepareForNextScene(CharacterDataSO characterData)
    {
        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (cardManager == null || artifactManager == null)
        {
            Debug.LogError("[PointAllocationManager] CardManager 또는 ArtifactManager를 찾을 수 없어 데이터 준비에 실패했습니다.");
            return;
        }

        // 카드 매니저와 유물 매니저를 초기 상태로 리셋합니다.
        cardManager.ClearAndResetDeck();
        // artifactManager.ClearAndResetArtifacts(); // 필요하다면 유물 매니저에도 리셋 함수를 만드세요.

        // 시작 카드와 유물을 장착'만' 해둡니다. (스탯 계산 X)
        // PlayerInitializer의 테스트 카드 로직을 참고하여 동일하게 구성합니다.
        if (characterData.startingCards != null)
        {
            foreach (var cardData in characterData.startingCards)
            {
                if (cardData == null) continue;

                CardInstance instanceToEquip = cardManager.AddCard(cardData);
                if (instanceToEquip != null)
                {
                    cardManager.Equip(instanceToEquip);
                }
            }
        }
        if (characterData.startingArtifacts != null)
        {
            foreach (var artifact in characterData.startingArtifacts)
            {
                artifactManager.EquipArtifact(artifact);
            }
        }
        Debug.Log("[PointAllocationManager] 다음 씬을 위한 시작 아이템 데이터 준비 완료.");
    }

    private void OnPreloadComplete()
    {
        Debug.Log("[PointAllocationManager] 프리로딩 완료 신호를 받았습니다. Gameplay 씬으로 전환합니다.");
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Gameplay);
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

    private void InitializeAllocation()
    {
                        selectedCharacter = ServiceLocator.Get<GameManager>().SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
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
        if (availableStats.Count == 0) return pointCounts;
        for (int i = 0; i < pointsToDistribute; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            pointCounts[targetStat]++;
        }
        return pointCounts;
    }
}