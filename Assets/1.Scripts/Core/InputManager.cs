using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 키보드 및 콘솔 입력을 이벤트화하여 모든 시스템이 구독할 수 있게 제공합니다.
/// </summary>
public class InputManager : MonoBehaviour
{
    // InputManager의 싱글톤 인스턴스입니다.
    public static InputManager Instance { get; private set; }

    // 이벤트 인스턴스화 (null 체크 불필요)
    public UnityEvent<Vector2> OnMove = new UnityEvent<Vector2>();
    public UnityEvent OnSubmit = new UnityEvent();  // 확인/선택 이벤트
    public UnityEvent OnCancel = new UnityEvent();  // 취소/뒤로가기 이벤트

    [Header("입력 지연 (초)")]
    public float inputBuffer = 0.1f;
    [Header("반복 입력 지연 (초)")]
    public float repeatRate = 0.12f;

    private float lastMoveTime;
    private float lastSubmitTime;
    private float lastCancelTime;

    void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        float now = Time.time;

        // 이동 입력 (대각선 입력은 무시하고 한 축만 반영하도록 처리)
        Vector2 move = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        if (move != Vector2.zero && now - lastMoveTime >= inputBuffer)
        {
            OnMove.Invoke(move.normalized);
            lastMoveTime = now;
        }

        // 확인/선택 입력: Enter 키 또는 설정된 "Submit" 버튼
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit"))
            && now - lastSubmitTime >= repeatRate)
        {
            OnSubmit.Invoke();
            lastSubmitTime = now;
        }

        // 취소/뒤로가기 입력: ESC 키 또는 설정된 "Cancel" 버튼
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel"))
            && now - lastCancelTime >= repeatRate)
        {
            OnCancel.Invoke();
            lastCancelTime = now;
        }
    }
}
