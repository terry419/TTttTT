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
        _attackLoopCts?.Cancel();
        _attackLoopCts?.Dispose();
        _attackLoopCts = null;
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

    // ▼▼▼ [핵심 수정] 공격 간격 계산 방식을 변경합니다. ▼▼▼
    private async UniTask AutoAttackLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // 1. 공격에 사용할 카드를 가져옵니다.
                var cardToUse = cardManager.activeCard;
                if (cardToUse == null)
                {
                    await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token); // 사용할 카드가 없으면 한 프레임 대기
                    continue;
                }

                // 2. 해당 카드로 공격을 실행합니다.
                await PerformAttack(cardToUse);

                // 3. 방금 사용한 카드의 고유 쿨타임을 기반으로 대기 시간을 계산합니다.
                if (stats == null) return;
                float interval = cardToUse.CardData.attackInterval / stats.FinalAttackSpeed;
                if (float.IsInfinity(interval) || interval <= 0) interval = 1f;

                // 4. 계산된 시간만큼 대기합니다.
                await UniTask.Delay(System.TimeSpan.FromSeconds(interval), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: token);
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("[PlayerController] AutoAttackLoop safely cancelled.");
        }
    }

    private async UniTask PerformAttack(CardInstance cardInstance)
    {
        try
        {
            if (cardInstance == null) return;

            // 카드가 여전히 장착 목록에 있는지 확인
            if (!cardManager.equippedCards.Contains(cardInstance))
            {
                return;
            }

            ICardAction action = cardInstance.CardData.CreateAction();
            var context = new CardActionContext(cardInstance, stats, firePoint);
            await action.Execute(context);
        }
        catch (MissingReferenceException ex)
        {
            Debug.LogError($"[!!! MissingReferenceException !!!] PlayerController.PerformAttack() 내부에서 오류 발생. 원본 오류: {ex.Message}\n{ex.StackTrace}");
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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