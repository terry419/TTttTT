// 파일명: PlayerController.cs (리팩토링 완료)
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterStats stats;
    private Vector2 moveInput;

    private CardManager cardManager;
    private EffectExecutor effectExecutor;
    private InputManager inputManager;

    [Header("공격 시작 위치")]
    public Transform firePoint;

    void Awake()
    {
        // 씬이 시작될 때 ServiceLocator에 자신을 등록합니다.
        ServiceLocator.Register<PlayerController>(this);
        Debug.Log($"[{GetType().Name}] ServiceLocator에 PlayerController를 등록했습니다.");

        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();

        cardManager = ServiceLocator.Get<CardManager>();
        effectExecutor = ServiceLocator.Get<EffectExecutor>();
        inputManager = ServiceLocator.Get<InputManager>();

        if (cardManager == null || effectExecutor == null || inputManager == null)
        {
            Debug.LogError($"[{GetType().Name}] Awake에서 필수 매니저 중 하나를 가져오지 못했습니다! ServiceLocator 등록 순서를 확인하세요.");
        }
    }

    void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnMove.AddListener(OnMove);
        }
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnMove.RemoveListener(OnMove);
        }
        RoundManager.OnRoundEnded -= HandleRoundEnd;
    }

    // 오브젝트가 파괴될 때 ServiceLocator에서 등록을 해제합니다.
    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerController>(this);
        Debug.Log($"[{GetType().Name}] ServiceLocator에서 PlayerController를 등록 해제했습니다.");
    }

    private void HandleRoundEnd(bool success)
    {
        Debug.Log($"[{GetType().Name}] 라운드 종료(성공: {success}). 자동 공격을 중지합니다.");
        CancelInvoke(nameof(PerformAttack));
    }

    public void StartAutoAttackLoop()
    {
        CancelInvoke(nameof(PerformAttack));
        if (stats == null)
        {
            Debug.LogError($"[{GetType().Name}] CharacterStats가 없어 공격 루프를 시작할 수 없습니다!");
            return;
        }

        float interval = 1f / stats.FinalAttackSpeed;
        Debug.Log($"[{GetType().Name}] 공격 루프 시작. (공격 속도: {stats.FinalAttackSpeed}, 반복 주기: {interval}초)");

        if (float.IsInfinity(interval) || interval <= 0)
        {
            Debug.LogError($"[{GetType().Name}] 공격 주기가 비정상적({interval})이므로 공격을 시작할 수 없습니다!");
            return;
        }

        InvokeRepeating(nameof(PerformAttack), 0f, interval);
    }

    private void PerformAttack()
    {
        if (cardManager == null) return;
        if (cardManager.activeCard == null) return;
        if (effectExecutor == null) return;

        // 자신의 정보(stats, firePoint)를 인자로 넘겨줌
        effectExecutor.Execute(cardManager.activeCard, stats, firePoint);
    }

    void FixedUpdate()
    {
        if (stats == null || rb == null) return;
        Vector2 finalVelocity = moveInput * stats.FinalMoveSpeed;
        rb.velocity = finalVelocity;
    }

    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    public void Heal(float amount)
    {
        if (stats != null)
        {
            Debug.Log($"[{GetType().Name}] {amount}만큼 체력을 회복합니다.");
            stats.Heal(amount);
        }
    }
}