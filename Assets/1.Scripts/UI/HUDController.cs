// --- 파일명: HUDController.cs (추적 디버깅 버전) ---
// 역할: HUD의 전체 생명주기를 추적하여 문제의 원인을 찾습니다.
// 수정 내용: Awake, OnEnable, Start, OnDisable, OnDestroy 등 모든 주요 함수에
//           실행 시점을 알리는 디버그 로그를 추가했습니다.
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;

    private int instanceId;

    // 1. 스크립트 인스턴스가 처음 로드될 때 호출됩니다. (게임 오브젝트가 비활성화 상태여도 호출됨)
    private void Awake()
    {
        // Awake에서 고유한 ID를 생성합니다. GetInstanceID()는 모든 유니티 오브젝트가 가진 고유 번호입니다.
        instanceId = gameObject.GetInstanceID();
        Debug.Log($"[HUD 추적 ID: {instanceId}] {gameObject.name} - Awake() 호출됨. (Frame: {Time.frameCount})");
    }

    private void OnEnable()
    {
        Debug.Log($"[HUD 추적 ID: {instanceId}] {gameObject.name} - OnEnable() 호출됨. 이벤트 구독 시작. (Frame: {Time.frameCount})");
        RoundManager.OnRoundStarted += HandleRoundStarted;
        RoundManager.OnKillCountChanged += UpdateKillCount;
        RoundManager.OnTimerChanged += UpdateTimer;
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    private void Start()
    {
        Debug.Log($"[HUD 추적 ID: {instanceId}] {gameObject.name} - Start() 호출됨. (Frame: {Time.frameCount})");
    }

    private void HandleRoundStarted(RoundDataSO roundData)
    {
        // [유령 추적 2] 어떤 HUD가 방송을 수신하는지 확인합니다.
        UpdateKillCount(0, roundData.killGoal);
        UpdateTimer(roundData.roundDuration);
    }

    private void OnDisable()
    {
        RoundManager.OnRoundStarted -= HandleRoundStarted;
        RoundManager.OnKillCountChanged -= UpdateKillCount;
        RoundManager.OnTimerChanged -= UpdateTimer;
        RoundManager.OnRoundEnded -= HandleRoundEnd;
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"[HUD 추적 ID: {instanceId}] {gameObject.name} - OnDestroy() 호출됨! (Frame: {Time.frameCount})");
    }

    // --- UI 업데이트 함수 (변경 없음) ---
    public void UpdateTimer(float time)
    {
        if (timerText == null) return;
        time = Mathf.Max(0, time);
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        timerText.text = $"Time : {minutes:00}:{seconds:00}";
    }

    public void UpdateKillCount(int currentKills, int goalKills)
    {
        if (killCountText != null)
        {
            killCountText.text = $"Kills: {currentKills} / {goalKills}";
        }
    }

    private void HandleRoundEnd(bool success)
    {
        Debug.Log($"[HUD 추적] {gameObject.name} - '라운드 종료' 방송 수신. (Frame: {Time.frameCount})");
    }
}