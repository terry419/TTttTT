// --- 파일명: PlayerController.cs (수정됨) ---
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- [유지] PlayerController는 몬스터 등 다른 객체가 직접 찾아야 하므로 예외적으로 Instance를 유지합니다. ---
    public static PlayerController Instance { get; private set; }

    // --- Private 멤버 변수 ---
    private Rigidbody2D rb;
    private CharacterStats stats;
    private Vector2 moveInput;

    // --- ServiceLocator를 통해 주입받을 매니저 ---
    private CardManager cardManager;
    private EffectExecutor effectExecutor;
    private InputManager inputManager; // OnDisable에서 사용하기 위해 멤버로 승격

    [Header("공격 시작 위치")]
    public Transform firePoint;

    void Awake()
    {
        // --- 1. 인스턴스 설정 ---
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[{GetType().Name}] 이미 다른 PlayerController 인스턴스가 있어 현재 오브젝트({name})를 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"[{GetType().Name}] 싱글톤 인스턴스를 설정했습니다.");

        // --- 2. 필수 컴포넌트 초기화 ---
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        Debug.Log($"[{GetType().Name}] Rigidbody2D, CharacterStats 컴포넌트를 초기화했습니다.");

        // --- 3. ServiceLocator를 통해 매니저 가져오기 ---
        // 기존의 .Instance 방식 대신, 중앙 허브인 ServiceLocator에 매니저를 요청합니다.
        Debug.Log($"[{GetType().Name}] ServiceLocator에서 매니저들을 가져옵니다...");
        cardManager = ServiceLocator.Get<CardManager>();
        effectExecutor = ServiceLocator.Get<EffectExecutor>();
        inputManager = ServiceLocator.Get<InputManager>(); // OnEnable/OnDisable에서 사용

        if (cardManager == null || effectExecutor == null || inputManager == null)
        {
            Debug.LogError($"[{GetType().Name}] Awake에서 필수 매니저 중 하나를 가져오지 못했습니다! ServiceLocator 등록 순서를 확인하세요.");
        }
        else
        {
            Debug.Log($"[{GetType().Name}] 모든 매니저를 성공적으로 가져왔습니다.");
        }
    }

    void OnEnable()
    {
        // --- 이벤트 구독 ---
        // InputManager의 이동 이벤트에 OnMove 함수를 등록합니다.
        if (inputManager != null)
        {
            inputManager.OnMove.AddListener(OnMove);
            Debug.Log($"[{GetType().Name}] InputManager.OnMove 이벤트에 구독했습니다.");
        }
        
        // 라운드 종료 이벤트에 HandleRoundEnd 함수를 등록합니다.
        RoundManager.OnRoundEnded += HandleRoundEnd;
        Debug.Log($"[{GetType().Name}] RoundManager.OnRoundEnded 이벤트에 구독했습니다.");
    }

    void OnDisable()
    {
        // --- 이벤트 구독 해제 ---
        // 오브젝트가 비활성화될 때, 등록했던 이벤트를 반드시 해제하여 메모리 누수를 방지합니다.
        if (inputManager != null)
        {
            inputManager.OnMove.RemoveListener(OnMove);
            Debug.Log($"[{GetType().Name}] InputManager.OnMove 이벤트 구독을 해제했습니다.");
        }
        RoundManager.OnRoundEnded -= HandleRoundEnd;
        Debug.Log($"[{GetType().Name}] RoundManager.OnRoundEnded 이벤트 구독을 해제했습니다.");
    }

    /// <summary>
    /// 라운드 종료 시 호출되어, 진행 중이던 자동 공격을 멈춥니다.
    /// </summary>
    private void HandleRoundEnd(bool success)
    {
        Debug.Log($"[{GetType().Name}] 라운드 종료(성공: {success}). 자동 공격을 중지합니다.");
        CancelInvoke(nameof(PerformAttack));
    }

    /// <summary>
    /// 자동으로 공격을 시작하는 루프를 설정합니다.
    /// </summary>
    public void StartAutoAttackLoop()
    {
        CancelInvoke(nameof(PerformAttack)); // 기존 루프가 있다면 중복 실행을 막기 위해 취소
        if (stats == null)
        {
            Debug.LogError($"[{GetType().Name}] CharacterStats가 없어 공격 루프를 시작할 수 없습니다!");
            return;
        }

        float interval = 1f / stats.finalAttackSpeed;
        Debug.Log($"[{GetType().Name}] 공격 루프 시작. (공격 속도: {stats.finalAttackSpeed}, 반복 주기: {interval}초)");

        if (float.IsInfinity(interval) || interval <= 0)
        {
            Debug.LogError($"[{GetType().Name}] 공격 주기가 비정상적({interval})이므로 공격을 시작할 수 없습니다!");
            return;
        }

        InvokeRepeating(nameof(PerformAttack), 0f, interval);
    }

    /// <summary>
    /// 실제 공격을 수행하는 메서드. InvokeRepeating에 의해 주기적으로 호출됩니다.
    /// </summary>
    private void PerformAttack()
    {
        // 공격 실행 전, 필요한 매니저와 활성 카드가 있는지 확인합니다.
        if (cardManager == null)
        {
            Debug.LogWarning($"[{GetType().Name}] PerformAttack: cardManager가 null이라 공격을 실행할 수 없습니다.");
            return;
        }
        if (cardManager.activeCard == null)
        {
            // 활성 카드가 없는 것은 정상적인 상황일 수 있으므로 Error 대신 Log로 남깁니다.
            // Debug.Log($"[{GetType().Name}] PerformAttack: 활성 카드가 없어 공격을 건너뜁니다.");
            return;
        }
        if (effectExecutor == null)
        {
            Debug.LogError($"[{GetType().Name}] PerformAttack: effectExecutor가 null이라 공격을 실행할 수 없습니다.");
            return;
        }

        // 모든 조건 통과 시, EffectExecutor에게 현재 활성 카드 효과 실행을 요청합니다.
        effectExecutor.Execute(cardManager.activeCard);
    }
    
    void FixedUpdate()
    {
        // 물리 업데이트는 FixedUpdate에서 처리하는 것이 정확합니다.
        if (stats == null || rb == null) return;
        Vector2 finalVelocity = moveInput * stats.finalMoveSpeed;
        rb.velocity = finalVelocity;
    }

    /// <summary>
    /// InputManager로부터 이동 입력을 받아 moveInput 변수에 저장합니다.
    /// </summary>
    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// 플레이어의 체력을 회복시킵니다.
    /// </summary>
    public void Heal(float amount)
    {
        if (stats != null)
        {
            Debug.Log($"[{GetType().Name}] {amount}만큼 체력을 회복합니다.");
            stats.Heal(amount);
        }
    }
}
