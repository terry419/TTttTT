using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 포인트 배분 씬의 UI 상호작용을 담당합니다.
/// 사용자의 숫자 입력을 처리하고, 캐릭터의 기본 능력치 및 최종 능력치를 표시하며,
/// 게임 플레이 씬으로의 전환을 관리합니다.
/// </summary>
public class PointAllocationUI : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI totalPointsDisplay; // 보유 중인 총 포인트 (캐릭터별 연동)
    [SerializeField] private TMP_InputField pointInputField; // 포인트 입력 필드
    [SerializeField] private Button confirmButton; // 결정 버튼
    [SerializeField] private Button backButton; // 뒤로가기 버튼

    [Header("기본 능력치 표시")]
    [SerializeField] private TextMeshProUGUI baseDamageText;
    [SerializeField] private TextMeshProUGUI baseAttackSpeedText;
    [SerializeField] private TextMeshProUGUI baseMoveSpeedText;
    [SerializeField] private TextMeshProUGUI baseHealthText;
    [SerializeField] private TextMeshProUGUI baseCritRateText;
    [SerializeField] private TextMeshProUGUI baseCritDamageText;

    [Header("최종 능력치 표시")]
    [SerializeField] private TextMeshProUGUI finalDamageText;
    [SerializeField] private TextMeshProUGUI finalAttackSpeedText;
    [SerializeField] private TextMeshProUGUI finalMoveSpeedText;
    [SerializeField] private TextMeshProUGUI finalHealthText;
    [SerializeField] private TextMeshProUGUI finalCritRateText;
    [SerializeField] private TextMeshProUGUI finalCritDamageText;

    [Header("패널 그룹")]
    [SerializeField] private GameObject baseStatsGroup; // 기본 능력치 UI 그룹
    [SerializeField] private GameObject finalStatsGroup; // 최종 능력치 UI 그룹

    private int maxPoints; // 플레이어의 현재 보유 포인트

    void Awake()
    {
        // 버튼 이벤트에 메서드 연결
        confirmButton.onClick.AddListener(OnConfirmClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        // 입력 필드의 값이 변경될 때마다 유효성 검사
        pointInputField.onValueChanged.AddListener(ValidateInput);
    }

    void OnEnable()
    {
        // InputManager 이벤트 구독 (ESC 키 처리)
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancel.AddListener(OnBackClicked);
        }
        InitializeUI(); // UI 초기화
    }

    void OnDisable()
    {
        // InputManager 이벤트 구독 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancel.RemoveListener(OnBackClicked);
        }
    }

    /// <summary>
    /// 씬 진입 시 UI를 초기화하고 데이터를 설정합니다.
    /// </summary>
    private void InitializeUI()
    {
        // GameManager에서 선택된 캐릭터의 초기 할당 포인트를 가져옵니다.
        if (GameManager.Instance != null && GameManager.Instance.SelectedCharacter != null)
        {
            maxPoints = GameManager.Instance.SelectedCharacter.initialAllocationPoints;
            totalPointsDisplay.text = $"보유 포인트: {maxPoints}";
            pointInputField.text = maxPoints.ToString(); // 입력 필드의 초기값을 최대 포인트로 설정
            
            // 기본 능력치 표시
            UpdateBaseStatsDisplay(GameManager.Instance.SelectedCharacter.baseStats);
            baseStatsGroup.SetActive(true);
            finalStatsGroup.SetActive(false); // 초기에는 최종 능력치 숨김
        }
        else
        {
            Debug.LogWarning("[PointAllocationUI] GameManager 또는 SelectedCharacter가 null입니다. 기본값 100으로 설정합니다.");
            maxPoints = 100; // 안전 장치
            totalPointsDisplay.text = $"보유 포인트: {maxPoints}";
            pointInputField.text = maxPoints.ToString();
            baseStatsGroup.SetActive(false);
            finalStatsGroup.SetActive(false);
        }

        pointInputField.Select(); // 입력 필드에 포커스
        pointInputField.ActivateInputField(); // 입력 필드 활성화
        if (backButton != null) backButton.interactable = true; // 뒤로가기 버튼 활성화
    }

    /// <summary>
    /// 입력된 값이 유효한지 (숫자인지, 최대값을 넘지 않는지) 검사합니다.
    /// </summary>
    private void ValidateInput(string input)
    {
        if (int.TryParse(input, out int value))
        {
            // 입력값이 최대 포인트를 초과하면 최대 포인트로 설정
            if (value > maxPoints)
            {
                pointInputField.text = maxPoints.ToString();
            }
            // 입력값이 음수이면 0으로 설정
            else if (value < 0)
            {
                pointInputField.text = "0";
            }
        }
        else if (!string.IsNullOrEmpty(input))
        {
            // 숫자가 아닌 값이 입력되면 0으로 초기화
            pointInputField.text = "0";
        }
    }

    /// <summary>
    /// 결정 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnConfirmClicked()
    {
        if (int.TryParse(pointInputField.text, out int points))
        {
            Debug.Log($"{points} 포인트로 분배를 확정합니다.");
            // GameManager에 할당된 포인트 저장
            GameManager.Instance.AllocatedPoints = points;

            // 최종 능력치 계산 및 표시
            BaseStats calculatedStats = CharacterStats.CalculatePreviewStats(
                GameManager.Instance.SelectedCharacter.baseStats,
                GameManager.Instance.AllocatedPoints
            );
            UpdateFinalStatsDisplay(calculatedStats);
            baseStatsGroup.SetActive(false); // 기본 능력치 숨김
            finalStatsGroup.SetActive(true); // 최종 능력치 표시

            if (backButton != null) backButton.interactable = false; // 결정 후 뒤로가기 버튼 비활성화

            // 2초 후 Gameplay 씬으로 전환
            StartCoroutine(TransitionToGameplayAfterDelay(2f));
        }
        else
        {
            Debug.LogError("유효하지 않은 포인트 값입니다.");
            // 사용자에게 알림을 띄울 수 있습니다.
        }
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnBackClicked()
    {
        Debug.Log("포인트 분배 화면에서 뒤로가기 버튼 클릭됨. CharacterSelect 씬으로 돌아갑니다.");
        GameManager.Instance.ChangeState(GameManager.GameState.MainMenu); // MainMenu로 가서 CharacterSelect 씬으로 로드되도록 변경
    }

    /// <summary>
    /// 캐릭터의 기본 능력치를 UI에 표시합니다.
    /// </summary>
    /// <param name="stats">표시할 기본 능력치</param>
    private void UpdateBaseStatsDisplay(BaseStats stats)
    {
        if (stats == null) return;
        baseDamageText.text = $"공격력: {stats.baseDamage:F2}";
        baseAttackSpeedText.text = $"공격 속도: {stats.baseAttackSpeed:F2}";
        baseMoveSpeedText.text = $"이동 속도: {stats.baseMoveSpeed:F2}";
        baseHealthText.text = $"체력: {stats.baseHealth:F2}";
        baseCritRateText.text = $"치명타 확률: {stats.baseCritRate:F2}";
        baseCritDamageText.text = $"치명타 피해량: {stats.baseCritDamage:F2}";
    }

    /// <summary>
    /// 계산된 최종 능력치를 UI에 표시합니다.
    /// </summary>
    /// <param name="stats">표시할 최종 능력치</param>
    private void UpdateFinalStatsDisplay(BaseStats stats)
    {
        if (stats == null) return;
        finalDamageText.text = $"공격력: {stats.baseDamage:F2}";
        finalAttackSpeedText.text = $"공격 속도: {stats.baseAttackSpeed:F2}";
        finalMoveSpeedText.text = $"이동 속도: {stats.baseMoveSpeed:F2}";
        finalHealthText.text = $"체력: {stats.baseHealth:F2}";
        finalCritRateText.text = $"치명타 확률: {stats.baseCritRate:F2}";
        finalCritDamageText.text = $"치명타 피해량: {stats.baseCritDamage:F2}";
    }

    private IEnumerator TransitionToGameplayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
    }
}
