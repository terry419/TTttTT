using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// 포인트 분배 결과 확인 및 게임 시작을 담당하는 UI 스크립트입니다.
/// CharacterSelectController로부터 계산된 최종 능력치를 받아와 표시하고,
/// 사용자의 최종 확인을 거쳐 게임 플레이 씬으로 전환합니다.
/// </summary>
public class PointAllocationResultUI : MonoBehaviour
{

    [Header("UI 요소 참조")]
    [SerializeField] private PointAllocationManager manager;
    [SerializeField] private TextMeshProUGUI finalStatsText; // 최종 능력치를 표시할 텍스트
    [SerializeField] private Button startGameButton; // 게임 시작 버튼
    [SerializeField] private Button backButton; // 뒤로가기 버튼

    void Awake()
    {
        startGameButton.onClick.AddListener(OnStartGameClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);
    }

    void OnEnable()
    {
        // InputManager 이벤트 구독 (ESC 키 처리)
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancel.AddListener(OnBackClicked);
        }
    }

    void Start()
    {
        // UpdateFinalStatsDisplay()는 PointAllocationUI에서 명시적으로 호출됩니다.
        // 기획서에 따라 1초간 입력을 받지 않는 로직 (선택적 구현)
        // StartCoroutine(DisableInputForDuration(1f));
    }

    void OnDisable()
    {
        // 패널이 비활성화될 때 InputManager 이벤트 구독 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnCancel.RemoveListener(OnBackClicked);
        }
    }

    /// <summary>
    /// 컨트롤러로부터 전달받은 최종 능력치를 UI에 표시합니다.
    /// </summary>
    /// <param name="finalStats">계산된 최종 능력치</param>
    public void UpdateFinalStatsDisplay(BaseStats finalStats)
    {
        if (finalStats == null)
        {
            finalStatsText.text = "오류: 최종 능력치 정보가 없습니다.";
            return;
        }

        string statsDisplay = "<능력치 분배 결과>\n\n";
        statsDisplay += $ "체력: {finalStats.baseHealth:F2}\n"; // :F2는 소수점 둘째 자리까지 표시
        statsDisplay += $ "공격력: {finalStats.baseDamage:F2}\n";
        statsDisplay += $ "공격 속도: {finalStats.baseAttackSpeed:F2}\n"; // 추가된 부분
        statsDisplay += $ "이동 속도: {finalStats.baseMoveSpeed:F2}\n";
        statsDisplay += $ "치명타 확률: {finalStats.baseCritRate:F2}\n";
        statsDisplay += $ "치명타 피해량: {finalStats.baseCritDamage:F2}\n";

        finalStatsText.text = statsDisplay;
        Debug.Log("최종 능력치 표시가 업데이트되었습니다.");
    }

    private void OnStartGameClicked()
    {
        Debug.Log("게임 시작 버튼 최종 클릭됨. 게임 플레이 씬으로 전환합니다.");
        StartCoroutine(LoadGameplaySceneAfterDelay(2f));
    }

    private IEnumerator LoadGameplaySceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
    }

    /// <summary>
    /// 뒤로가기 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnBackClicked()
    {
        Debug.Log("포인트 분배 결과 화면에서 뒤로가기 버튼 클릭됨. 포인트 분배 입력 화면으로 돌아갑니다.");
        if (manager != null)
        {
            manager.OnBackToAllocationInput();
        }
        else
        {
            Debug.LogError("PointAllocationManager 참조가 PointAllocationResultUI에 할당되지 않았습니다.");
        }
    }
}