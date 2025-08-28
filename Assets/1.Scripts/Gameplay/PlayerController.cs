using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CharacterStats stats;
    private Vector2 moveInput;
    private CardManager cardManager;
    private InputManager inputManager;

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
        if (inputManager != null) inputManager.OnMove.AddListener(OnMove);
        RoundManager.OnRoundEnded += HandleRoundEnd;
    }

    void OnDisable()
    {
        if (inputManager != null) inputManager.OnMove.RemoveListener(OnMove);
        RoundManager.OnRoundEnded -= HandleRoundEnd;
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<PlayerController>(this);
    }

    private void HandleRoundEnd(bool success)
    {
        CancelInvoke(nameof(PerformAttack));
    }

    public void StartAutoAttackLoop()
    {
        CancelInvoke(nameof(PerformAttack));
        if (stats == null) return;
        float interval = 1f / stats.FinalAttackSpeed;
        if (float.IsInfinity(interval) || interval <= 0) interval = 1f;
        InvokeRepeating(nameof(PerformAttack), 0f, interval);
    }
    private async void PerformAttack()
    {
        if (cardManager?.activeCard == null) return;

        var cardInstance = cardManager.activeCard;
        // [수정] Context에 CardInstance 자체를 전달
        ICardAction action = cardInstance.CardData.CreateAction();
        var context = new CardActionContext(cardInstance, stats, firePoint);
        await action.Execute(context);
    }

    void FixedUpdate()
    {
        if (stats != null) rb.velocity = moveInput * stats.FinalMoveSpeed;
    }

    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    public void Heal(float amount)
    {
        if (stats != null) stats.Heal(amount);
    }
}