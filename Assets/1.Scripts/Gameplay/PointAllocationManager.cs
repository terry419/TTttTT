using UnityEngine;
using TMPro; // TextMeshProUGUI 사용을 위해 추가
using UnityEngine.UI; // Button, Image 등 UI 요소 사용을 위해 추가

/// <summary>
/// 포인트 배분 씬의 전체적인 흐름과 UI를 관리하는 매니저입니다.
/// 캐릭터 선택 후 전달된 데이터를 기반으로 능력치를 계산하고,
/// 포인트 배분 UI와 결과 UI 간의 전환을 담당합니다.
/// </summary>
public class PointAllocationManager : MonoBehaviour
{
    [Header("UI 패널 참조")]
    [SerializeField] private GameObject pointInputPanel; // 포인트 입력 UI 패널
    [SerializeField] private PointAllocationResultUI resultUI; // 결과 UI 스크립트 참조

    [Header("포인트 입력 UI 요소")]
    [SerializeField] private TextMeshProUGUI currentPointsText; // 현재 남은 포인트 표시 텍스트

    [Header("능력치 입력 UI 요소")]
    [SerializeField] private TextMeshProUGUI healthText; // 체력 텍스트
    [SerializeField] private Button healthUpButton; // 체력 증가 버튼
    [SerializeField] private Button healthDownButton; // 체력 감소 버튼

    [SerializeField] private TextMeshProUGUI damageText; // 공격력 텍스트
    [SerializeField] private Button damageUpButton; // 공격력 증가 버튼
    [SerializeField] private Button damageDownButton; // 공격력 감소 버튼

    [SerializeField] private TextMeshProUGUI attackSpeedText; // 공격 속도 텍스트
    [SerializeField] private Button attackSpeedUpButton; // 공격 속도 증가 버튼
    [SerializeField] private Button attackSpeedDownButton; // 공격 속도 감소 버튼

    [SerializeField] private TextMeshProUGUI moveSpeedText; // 이동 속도 텍스트
    [SerializeField] private Button moveSpeedUpButton; // 이동 속도 증가 버튼
    [SerializeField] private Button moveSpeedDownButton; // 이동 속도 감소 버튼

    [SerializeField] private TextMeshProUGUI critRateText; // 치명타 확률 텍스트
    [SerializeField] private Button critRateUpButton; // 치명타 확률 증가 버튼
    [SerializeField] private Button critRateDownButton; // 치명타 확률 감소 버튼

    [SerializeField] private TextMeshProUGUI critDamageText; // 치명타 피해량 텍스트
    [SerializeField] private Button critDamageUpButton; // 치명타 피해량 증가 버튼
    [SerializeField] private Button critDamageDownButton; // 치명타 피해량 감소 버튼

    private int currentAllocatablePoints; // 현재 배분 가능한 포인트

    // 할당된 능력치 포인트
    private int allocatedHealthPoints;
    private int allocatedDamagePoints;
    private int allocatedAttackSpeedPoints;
    private int allocatedMoveSpeedPoints;
    private int allocatedCritRatePoints;
    private int allocatedCritDamagePoints;

    void Awake()
    {
        // 버튼 이벤트 리스너 연결
        healthUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedHealthPoints, healthText));
        healthDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedHealthPoints, healthText));

        damageUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedDamagePoints, damageText));
        damageDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedDamagePoints, damageText));

        attackSpeedUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedAttackSpeedPoints, attackSpeedText));
        attackSpeedDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedAttackSpeedPoints, attackSpeedText));

        moveSpeedUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedMoveSpeedPoints, moveSpeedText));
        moveSpeedDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedMoveSpeedPoints, moveSpeedText));

        critRateUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedCritRatePoints, critRateText));
        critRateDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedCritRatePoints, critRateText));

        critDamageUpButton.onClick.AddListener(() => OnStatUpClicked(ref allocatedCritDamagePoints, critDamageText));
        critDamageDownButton.onClick.AddListener(() => OnStatDownClicked(ref allocatedCritDamagePoints, critDamageText));
    }

    void Start()
    {
        InitializeAllocation();
    }

    /// <summary>
    /// 포인트 배분 씬 초기화 및 데이터 로드
    /// </summary>
    private void InitializeAllocation()
    {
        // GameManager에서 선택된 캐릭터와 초기 할당 포인트를 가져옵니다.
        if (GameManager.Instance == null || GameManager.Instance.SelectedCharacter == null)
        {
            Debug.LogError("GameManager 또는 SelectedCharacter 데이터가 없습니다. 메인 메뉴로 돌아갑니다.");
            GameManager.Instance?.ChangeState(GameManager.GameState.MainMenu);
            return;
        }

        currentAllocatablePoints = GameManager.Instance.SelectedCharacter.initialAllocationPoints;

        // 할당된 능력치 포인트 초기화
        allocatedHealthPoints = 0;
        allocatedDamagePoints = 0;
        allocatedAttackSpeedPoints = 0;
        allocatedMoveSpeedPoints = 0;
        allocatedCritRatePoints = 0;
        allocatedCritDamagePoints = 0;

        UpdateCurrentPointsUI();
        UpdateAllStatDisplays();

        // 초기에는 포인트 입력 패널을 활성화하고 결과 패널은 비활성화합니다.
        pointInputPanel.SetActive(true);
        resultUI.gameObject.SetActive(false); // PointAllocationResultUI가 붙어있는 GameObject를 비활성화
    }

    /// <summary>
    /// 현재 남은 포인트를 UI에 업데이트합니다.
    /// </summary>
    private void UpdateCurrentPointsUI()
    {
        if (currentPointsText != null)
        {
            currentPointsText.text = $"남은 포인트: {currentAllocatablePoints}";
        }
    }

    /// <summary>
    /// 모든 능력치 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateAllStatDisplays()
    {
        healthText.text = allocatedHealthPoints.ToString();
        damageText.text = allocatedDamagePoints.ToString();
        attackSpeedText.text = allocatedAttackSpeedPoints.ToString();
        moveSpeedText.text = allocatedMoveSpeedPoints.ToString();
        critRateText.text = allocatedCritRatePoints.ToString();
        critDamageText.text = allocatedCritDamagePoints.ToString();
    }

    /// <summary>
    /// 능력치 증가 버튼 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="allocatedStat">증가시킬 능력치 포인트 변수</param>
    /// <param name="statText">해당 능력치 텍스트 UI</param>
    private void OnStatUpClicked(ref int allocatedStat, TextMeshProUGUI statText)
    {
        if (currentAllocatablePoints > 0)
        {
            allocatedStat++;
            currentAllocatablePoints--;
            UpdateCurrentPointsUI();
            statText.text = allocatedStat.ToString();
        }
        else
        {
            Debug.Log("남은 포인트가 없습니다.");
        }
    }

    /// <summary>
    /// 능력치 감소 버튼 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="allocatedStat">감소시킬 능력치 포인트 변수</param>
    /// <param name="statText">해당 능력치 텍스트 UI</param>
    private void OnStatDownClicked(ref int allocatedStat, TextMeshProUGUI statText)
    {
        if (allocatedStat > 0)
        {
            allocatedStat--;
            currentAllocatablePoints++;
            UpdateCurrentPointsUI();
            statText.text = allocatedStat.ToString();
        }
        else
        {
            Debug.Log("할당된 포인트가 없습니다.");
        }
    }

    /// <summary>
    /// '결과 확인' 버튼 클릭 시 호출됩니다.
    /// 최종 능력치를 계산하고 결과 UI를 표시합니다.
    /// </summary>
    public void OnConfirmAllocationClicked()
    {
        BaseStats finalStats = CalculateFinalStats();

        // 포인트 입력 패널을 비활성화하고 결과 패널을 활성화합니다.
        pointInputPanel.SetActive(false);
        resultUI.gameObject.SetActive(true);

        // 결과 UI에 최종 능력치를 전달하여 표시합니다.
        resultUI.UpdateFinalStatsDisplay(finalStats);
    }

    /// <summary>
    /// PointAllocationResultUI에서 '뒤로가기' 버튼 클릭 시 호출됩니다.
    /// 포인트 입력 화면으로 돌아갑니다.
    /// </summary>
    public void OnBackToAllocationInput()
    {
        resultUI.gameObject.SetActive(false);
        pointInputPanel.SetActive(true);
        // 필요하다면 여기서 포인트 입력 상태를 초기화하거나 이전 상태로 복원합니다.
        // 현재는 InitializeAllocation()이 Start()에서만 호출되므로, 뒤로가기 시 상태가 유지됩니다.
        // 만약 뒤로가기 시 포인트 재분배를 원한다면 InitializeAllocation()을 다시 호출해야 합니다.
    }

    /// <summary>
    /// GameManager.Instance.SelectedCharacter와 현재 배분된 포인트를 기반으로 최종 BaseStats를 계산합니다.
    /// </summary>
    private BaseStats CalculateFinalStats()
    {
        CharacterDataSO selectedChar = GameManager.Instance.SelectedCharacter;

        // 각 능력치에 대한 포인트당 증가량 (예시 값, 실제 게임 디자인에 따라 조절 필요)
        // 이 값들은 ScriptableObject 등으로 관리하는 것이 더 좋습니다.
        float healthPerPoint = 10f; // 체력 1포인트당 10 증가
        float damagePerPoint = 1f; // 공격력 1포인트당 1 증가
        float attackSpeedPerPoint = 0.01f; // 공격 속도 1포인트당 0.01 증가 (공격 속도는 낮을수록 좋으므로 감소)
        float moveSpeedPerPoint = 0.1f; // 이동 속도 1포인트당 0.1 증가
        float critRatePerPoint = 0.005f; // 치명타 확률 1포인트당 0.5% 증가 (0.005)
        float critDamagePerPoint = 0.01f; // 치명타 피해량 1포인트당 1% 증가 (0.01)

        BaseStats finalStats = new BaseStats
        {
            baseHealth = selectedChar.baseHealth + (allocatedHealthPoints * healthPerPoint),
            baseDamage = selectedChar.baseDamage + (allocatedDamagePoints * damagePerPoint),
            baseAttackSpeed = selectedChar.baseAttackSpeed - (allocatedAttackSpeedPoints * attackSpeedPerPoint), // 공격 속도는 감소
            baseMoveSpeed = selectedChar.baseMoveSpeed + (allocatedMoveSpeedPoints * moveSpeedPerPoint),
            baseCritRate = selectedChar.baseCritRate + (allocatedCritRatePoints * critRatePerPoint),
            baseCritDamage = selectedChar.baseCritDamage + (allocatedCritDamagePoints * critDamagePerPoint)
        };

        return finalStats;
    }
}