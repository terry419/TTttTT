using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem; // Input System 네임스페이스 사용

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterStats stats;
    private Vector2 moveInput;
    private CardManager cardManager;
    private InputManager inputManager;
    private CancellationTokenSource _attackLoopCts;

    [Header("공격 시작 위치")]
    public Transform firePoint;

    void Awake()
    {
        ServiceLocator.Register<PlayerController>(this);
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cardManager = ServiceLocator.Get<CardManager>();
        inputManager = ServiceLocator.Get<InputManager>();
    }

    void OnEnable()
    {
        if (inputManager != null)
        {
            Debug.Log("[INPUT TRACE] PlayerController.OnEnable: 'Move' 액션 이벤트 구독 시도.");
            inputManager.InputActions.Gameplay.Move.performed += HandleMove;
            inputManager.InputActions.Gameplay.Move.canceled += HandleMove;
        }
        RoundManager.OnRoundStarted += HandleRoundStarted;
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.InputActions.Gameplay.Move.performed -= HandleMove;
            inputManager.InputActions.Gameplay.Move.canceled -= HandleMove;
        }
        RoundManager.OnRoundEnded -= HandleRoundEnd;
        _attackLoopCts?.Cancel();
        _attackLoopCts?.Dispose();
        _attackLoopCts = null;
    }
    private void HandleRoundStarted(RoundDataSO roundData)
    {
        // 라운드가 공식적으로 시작되었을 때만 자동 공격을 시작합니다.
        StartAutoAttackLoop();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerController>(this);
    }

    private void HandleRoundEnd(bool success)
    {
        _attackLoopCts?.Cancel();
        _attackLoopCts?.Dispose();
        _attackLoopCts = null;
    }

    public void StartAutoAttackLoop()
    {
        if (_attackLoopCts != null && !_attackLoopCts.IsCancellationRequested) return;

        _attackLoopCts = new CancellationTokenSource();
        AutoAttackLoop(_attackLoopCts.Token).Forget();
    }

    private async UniTask AutoAttackLoop(CancellationToken token)
    {
        PerformAttack();

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (stats == null) return;
                float interval = 1f / stats.FinalAttackSpeed;
                if (float.IsInfinity(interval) || interval <= 0) interval = 1f;

                await UniTask.Delay(System.TimeSpan.FromSeconds(interval), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
                PerformAttack();
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[PlayerController] AutoAttackLoop safely cancelled.");
        }
    }

    private async void PerformAttack()
    {
        try
        {
            if (cardManager?.activeCard == null) return;
            var cardInstance = cardManager.activeCard;

            // [FIX] 공격 실행 직전, 해당 카드 인스턴스가 여전히 장착 목록에 있는지 확인합니다.
            // 이를 통해 합성 등으로 제거된 카드를 사용하려는 시도를 막습니다.
            if (!cardManager.equippedCards.Contains(cardInstance))
            {
                return; // 카드가 더 이상 유효하지 않으므로 공격을 중단합니다.
            }

            ICardAction action = cardInstance.CardData.CreateAction();
            var context = new CardActionContext(cardInstance, stats, firePoint);
            await action.Execute(context);
        }
        catch (MissingReferenceException ex)
        {
            // [증거 확보] 만약 오류가 이 블록에서 발생했다면, 아래의 특수 로그가 출력됩니다.
            Debug.LogError($"[!!! 100% 증거 확보 !!!] MissingReferenceException이 PlayerController.PerformAttack() 내부에서 발생했습니다. 이는 공격 실행 과정(카드 모듈 등)의 문제입니다. 원본 오류: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void FixedUpdate()
    {
        if (stats != null) rb.velocity = moveInput * stats.FinalMoveSpeed;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

    }

    public void Heal(float amount)
    {
        if (stats != null) stats.Heal(amount);
    }
}