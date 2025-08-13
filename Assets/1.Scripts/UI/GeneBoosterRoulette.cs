using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// '유전자 증폭제' 룰렛 UI의 모든 로직과 상호작용을 담당합니다.
/// ProgressionManager로부터 재화(GenePoints) 정보를 받아오고, 룰렛을 돌려 능력치 해금 권한을 얻으며,
/// 남은 포인트를 투자하여 캐릭터의 '영구적인' 기본 능력치를 강화합니다.
/// </summary>
public class GeneBoosterRoulette : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private Button spinRouletteButton; // 룰렛 돌리기 버튼
    [SerializeField] private TextMeshProUGUI genePointsText; // 현재 보유 유전자 포인트 텍스트
    [SerializeField] private TMP_InputField pointsToInvestInput; // 투자할 포인트 입력 필드
    [SerializeField] private Button investButton; // 투자 확정 버튼
    [SerializeField] private List<GameObject> statUIs; // 각 능력치 UI (해금 시 활성화)
    [SerializeField] private Image spinRouletteButtonImage; // 룰렛 버튼의 Image 컴포넌트 (시각 효과용)

    private const int ROULETTE_COST = 15; // 룰렛 1회 비용

    // 캐릭터별 영구 스탯 데이터를 저장하는 변수. 실제로는 파일로 저장/로드 되어야 함.
    private CharacterPermanentStats permanentStats;

    void Start()
    {
        // 현재 선택된 캐릭터의 ID를 가져와 영구 스탯 데이터를 불러옵니다.
        // GameManager.Instance.SelectedCharacter가 null일 경우를 대비한 안전장치 필요
        string characterId = "warrior"; // 임시: 실제로는 GameManager.Instance.SelectedCharacter.characterId 사용
        if (GameManager.Instance != null && GameManager.Instance.SelectedCharacter != null)
        {
            characterId = GameManager.Instance.SelectedCharacter.characterId;
        }
        permanentStats = ProgressionManager.Instance.GetPermanentStatsFor(characterId);
        
        // 리스너 연결
        spinRouletteButton.onClick.AddListener(SpinRoulette);
        investButton.onClick.AddListener(InvestPoints);

        UpdateUI();
    }

    /// <summary>
    /// UI의 모든 요소를 현재 데이터에 맞게 업데이트합니다.
    /// </summary>
    private void UpdateUI()
    {
        // 보유 포인트 표시
        genePointsText.text = $"보유 포인트: {ProgressionManager.Instance.GenePoints}";

        // 룰렛 버튼 상태 업데이트
        bool canSpin = ProgressionManager.Instance.GenePoints >= ROULETTE_COST && !permanentStats.AllStatsUnlocked();
        spinRouletteButton.interactable = canSpin;
        if (spinRouletteButtonImage != null)
        {
            spinRouletteButtonImage.color = canSpin ? Color.white : Color.gray; // 활성화/비활성화 시각 효과
        }

        // 투자 버튼 상태 업데이트 (1포인트 이상 있어야 투자 가능)
        investButton.interactable = ProgressionManager.Instance.GenePoints > 0;

        // TODO: 각 스탯 UI (statUIs)의 해금 상태를 permanentStats.unlockedStatus에 따라 업데이트
        // 예: for (int i = 0; i < statUIs.Count; i++) { statUIs[i].SetActive(permanentStats.unlockedStatus[(StatType)i]); }
    }

    /// <summary>
    /// 룰렛 돌리기 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    private void SpinRoulette()
    {
        if (!ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.GenePoints, ROULETTE_COST))
        {
            PopupController.Instance.ShowError("유전자 포인트가 부족합니다.", 1.5f);
            return;
        }

        List<StatType> availableStats = permanentStats.GetLockedStats();
        if (availableStats.Count == 0)
        {
            PopupController.Instance.ShowError("모든 스탯이 이미 해금되었습니다.", 1.5f);
            return;
        }

        StartCoroutine(SpinRouletteAnimation(availableStats));
    }

    private IEnumerator SpinRouletteAnimation(List<StatType> availableStats)
    {
        spinRouletteButton.interactable = false;
        // TODO: 룰렛이 돌아가는 화려한 애니메이션 연출 시작
        Debug.Log("룰렛 애니메이션 시작...");

        yield return new WaitForSeconds(2f);

        StatType unlockedStat = availableStats[Random.Range(0, availableStats.Count)];
        permanentStats.UnlockStat(unlockedStat);
        Debug.Log($"룰렛 결과: {unlockedStat} 능력치 해금!");

        // TODO: 룰렛 애니메이션 종료 및 결과 표시 연출

        spinRouletteButton.interactable = true;
        UpdateUI();
    }

    /// <summary>
    /// 포인트 투자 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    private void InvestPoints()
    {
        if (!int.TryParse(pointsToInvestInput.text, out int points) || points <= 0)
        {
            PopupController.Instance.ShowError("유효한 투자 포인트를 입력하세요.", 1.5f);
            return;
        }

        points = Mathf.Min(points, ProgressionManager.Instance.GenePoints);

        if (!ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.GenePoints, points))
        {
            PopupController.Instance.ShowError("투자에 필요한 유전자 포인트가 부족합니다.", 1.5f);
            return;
        }

        permanentStats.DistributePoints(points);
        Debug.Log($"{points} 포인트를 해금된 능력치에 랜덤하게 투자했습니다.");

        UpdateUI();
    }
}


