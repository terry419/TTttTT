using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class GeneBoosterRoulette : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private Button spinRouletteButton;
    [SerializeField] private TextMeshProUGUI genePointsText;
    [SerializeField] private TMP_InputField pointsToInvestInput;
    [SerializeField] private Button investButton;
    [SerializeField] private List<GameObject> statUIs;
    [SerializeField] private Image spinRouletteButtonImage;

    private const int ROULETTE_COST = 15;

    private CharacterPermanentStats permanentStats;

    // --- [추가] 서비스 로케이터를 통해 매니저 인스턴스를 저장할 변수 ---
    private ProgressionManager progressionManager;
    private PopupController popupController;

    void Start()
    {
        // --- [추가] 서비스 로케이터를 통해 필요한 매니저를 미리 찾아둡니다. ---
        progressionManager = ServiceLocator.Get<ProgressionManager>();
        popupController = ServiceLocator.Get<PopupController>();
        var gameManager = ServiceLocator.Get<GameManager>();

        string characterId = "warrior";
        if (gameManager != null && gameManager.SelectedCharacter != null)
        {
            characterId = gameManager.SelectedCharacter.characterId;
        }

        permanentStats = progressionManager.GetPermanentStatsFor(characterId);

        spinRouletteButton.onClick.AddListener(SpinRoulette);
        investButton.onClick.AddListener(InvestPoints);

        UpdateUI();
    }

    private void UpdateUI()
    {
        // --- [수정] 미리 찾아둔 progressionManager 변수를 사용합니다. ---
        genePointsText.text = $"보유 포인트: {progressionManager.GenePoints}";

        bool canSpin = progressionManager.GenePoints >= ROULETTE_COST && !permanentStats.AllStatsUnlocked();
        spinRouletteButton.interactable = canSpin;
        if (spinRouletteButtonImage != null)
        {
            spinRouletteButtonImage.color = canSpin ? Color.white : Color.gray;
        }

        investButton.interactable = progressionManager.GenePoints > 0;
    }

    private void SpinRoulette()
    {
        // --- [수정] 미리 찾아둔 변수들을 사용합니다. ---
        if (!progressionManager.SpendCurrency(MetaCurrencyType.GenePoints, ROULETTE_COST))
        {
            if (popupController != null) popupController.ShowError("유전자 포인트가 부족합니다.", 1.5f);
            return;
        }

        List<StatType> availableStats = permanentStats.GetLockedStats();
        if (availableStats.Count == 0)
        {
            if (popupController != null) popupController.ShowError("모든 스탯이 이미 해금되었습니다.", 1.5f);
            return;
        }

        StartCoroutine(SpinRouletteAnimation(availableStats));
    }

    private IEnumerator SpinRouletteAnimation(List<StatType> availableStats)
    {
        spinRouletteButton.interactable = false;
        Debug.Log("룰렛 애니메이션 시작...");

        yield return new WaitForSeconds(2f);

        StatType unlockedStat = availableStats[Random.Range(0, availableStats.Count)];
        permanentStats.UnlockStat(unlockedStat);
        Debug.Log($"룰렛 결과: {unlockedStat} 능력치 해금!");

        spinRouletteButton.interactable = true;
        UpdateUI();
    }

    private void InvestPoints()
    {
        if (!int.TryParse(pointsToInvestInput.text, out int points) || points <= 0)
        {
            // --- [수정] 미리 찾아둔 popupController 변수를 사용합니다. ---
            if (popupController != null) popupController.ShowError("유효한 투자 포인트를 입력하세요.", 1.5f);
            return;
        }

        // --- [수정] 미리 찾아둔 progressionManager 변수를 사용합니다. ---
        points = Mathf.Min(points, progressionManager.GenePoints);

        if (!progressionManager.SpendCurrency(MetaCurrencyType.GenePoints, points))
        {
            if (popupController != null) popupController.ShowError("투자에 필요한 유전자 포인트가 부족합니다.", 1.5f);
            return;
        }

        permanentStats.DistributePoints(points);
        Debug.Log($"{points} 포인트를 해금된 능력치에 랜덤하게 투자했습니다.");

        UpdateUI();
    }
}