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
        if (cardManager?.activeCard == null) return;
        var cardInstance = cardManager.activeCard;
        ICardAction action = cardInstance.CardData.CreateAction();
        var context = new CardActionContext(cardInstance, stats, firePoint);
        await action.Execute(context);
    }

    void FixedUpdate()
    {
        if (stats != null) rb.velocity = moveInput * stats.FinalMoveSpeed;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"[INPUT TRACE] PlayerController.HandleMove: 'Move' 액션 이벤트 수신! 입력값: {moveInput}");

    }

    public void Heal(float amount)
    {
        if (stats != null) stats.Heal(amount);
    }
}