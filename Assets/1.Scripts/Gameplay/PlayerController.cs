// 파일 경로: Assets/1.Scripts/Gameplay/PlayerController.cs
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
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
        StopAllActions(); // OnDisable 시 모든 동작 중지
    }

    private void HandleRoundStarted(RoundDataSO roundData)
    {
        StartAutoAttackLoop();
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerController>(this);
    }

    private void HandleRoundEnd(bool success)
    {
        StopAllActions();
    }

    /// <summary>
    /// 플레이어의 모든 동작(이동, 공격)을 중지합니다.
    /// </summary>
    public void StopAllActions()
    {
        Debug.Log("[PlayerController] 모든 동작을 중지합니다.");

        // 이동 중지
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // 공격 루프 중지
        if (_attackLoopCts != null && !_attackLoopCts.IsCancellationRequested)
        {
            _attackLoopCts.Cancel();
            _attackLoopCts.Dispose();
            _attackLoopCts = null;
        }
    }

    public void StartAutoAttackLoop()
    {
        if (_attackLoopCts != null && !_attackLoopCts.IsCancellationRequested) return;
        _attackLoopCts = new CancellationTokenSource();
        AutoAttackLoop(_attackLoopCts.Token).Forget();
    }

    private async UniTask AutoAttackLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var cardToUse = cardManager.activeCard;
                if (cardToUse == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
                    continue;
                }

                await PerformAttack(cardToUse);

                if (stats == null) return;
                float interval = cardToUse.CardData.attackInterval / stats.FinalAttackSpeed;
                if (float.IsInfinity(interval) || interval <= 0) interval = 1f;

                await UniTask.Delay(System.TimeSpan.FromSeconds(interval), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[PlayerController] AutoAttackLoop가 안전하게 취소되었습니다.");
        }
    }

    private async UniTask PerformAttack(CardInstance cardInstance)
    {
        try
        {
            if (cardInstance == null) return;
            var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
            if (playerDataManager?.CurrentRunData == null) return;

            if (!playerDataManager.CurrentRunData.equippedCards.Contains(cardInstance))
            {
                return;
            }

            ICardAction action = cardInstance.CardData.CreateAction();
            var context = new CardActionContext(cardInstance, stats, firePoint);
            await action.Execute(context);
        }
        catch (MissingReferenceException ex)
        {
            Debug.LogError($"[!!! MissingReferenceException !!!] PlayerController.PerformAttack() 내부에서 오류 발생. 원본 오류: {ex.Message}");
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
