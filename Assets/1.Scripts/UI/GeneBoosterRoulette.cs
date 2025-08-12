using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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

    private const int ROULETTE_COST = 15; // 룰렛 1회 비용

    // 캐릭터별 영구 스탯 데이터를 저장하는 변수. 실제로는 파일로 저장/로드 되어야 함.
    private CharacterPermanentStats permanentStats;

    void Start()
    {
        // TODO: 현재 선택된 캐릭터에 맞는 영구 스탯 데이터를 불러오는 로직 필요
        // permanentStats = ProgressionManager.Instance.GetPermanentStatsFor("Warrior");
        
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
        if (!canSpin) {
            // TODO: 룰렛 버튼을 비활성화된 모양(흑백 등)으로 표시
        }

        // 투자 버튼 상태 업데이트 (1포인트 이상 있어야 투자 가능)
        investButton.interactable = ProgressionManager.Instance.GenePoints > 0;
    }

    /// <summary>
    /// 룰렛 돌리기 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    private void SpinRoulette()
    {
        // 재화가 충분한지 다시 확인
        if (!ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.GenePoints, ROULETTE_COST))
        {
            Debug.LogWarning("룰렛을 돌리기에 유전자 포인트가 부족합니다.");
            // TODO: PopupController로 알림 표시
            return;
        }

        // 해금되지 않은 스탯 목록을 만듭니다.
        List<StatType> availableStats = permanentStats.GetLockedStats();
        if (availableStats.Count == 0)
        {
            Debug.LogWarning("모든 스탯이 이미 해금되었습니다.");
            return; // 이런 경우는 버튼이 비활성화되어야 하지만, 안전장치
        }

        // 랜덤하게 하나의 스탯을 선택하여 해금합니다.
        StatType unlockedStat = availableStats[Random.Range(0, availableStats.Count)];
        permanentStats.UnlockStat(unlockedStat);
        Debug.Log($"룰렛 결과: {unlockedStat} 능력치 해금!");

        // TODO: 룰렛이 돌아가는 화려한 애니메이션 연출 후 결과 표시

        // TODO: 영구 스탯 데이터 저장
        // ProgressionManager.Instance.SavePermanentStats(permanentStats);

        UpdateUI();
    }

    /// <summary>
    /// 포인트 투자 버튼을 클릭했을 때 호출됩니다.
    /// </summary>
    private void InvestPoints()
    {
        if (!int.TryParse(pointsToInvestInput.text, out int points) || points <= 0)
        {
            Debug.LogWarning("유효한 투자 포인트를 입력하세요.");
            return;
        }

        // 보유 재화 확인 및 사용
        if (!ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.GenePoints, points))
        {
            Debug.LogWarning("투자에 필요한 유전자 포인트가 부족합니다.");
            return;
        }

        // 기획서: 투자된 포인트를 해금된 능력치들에 랜덤하게 배분
        permanentStats.DistributePoints(points);
        Debug.Log($"{points} 포인트를 해금된 능력치에 랜덤하게 투자했습니다.");

        // TODO: 영구 스탯 데이터 저장
        // ProgressionManager.Instance.SavePermanentStats(permanentStats);

        UpdateUI();
    }
}


