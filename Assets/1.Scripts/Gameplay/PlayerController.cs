using UnityEngine;
using Cysharp.Threading.Tasks; // UniTask 사용을 위해 추가

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
        if (float.IsInfinity(interval) || interval <= 0)
        {
            // 공격 속도가 0 이하일 경우 1초에 한 번으로 고정하여 오류 방지
            interval = 1f;
        }
        InvokeRepeating(nameof(PerformAttack), 0f, interval);
    }

    // [핵심 수정] async void로 변경하여 새로운 Action 시스템 사용
    private async void PerformAttack()
    {
        Debug.Log("[ATTACK-DEBUG 1/5] PerformAttack 호출됨.");

        if (cardManager == null || stats == null || firePoint == null)
        {
            Debug.LogError("[ATTACK-DEBUG] 필수 컴포넌트가 없어 공격을 중단합니다.");
            return;
        }

        CardDataSO currentCard = cardManager.activeCard;
        if (currentCard == null)
        {
            Debug.LogWarning("[ATTACK-DEBUG 2/5] 활성화된 카드(activeCard)가 없습니다.");
            return;
        }
        Debug.Log($"[ATTACK-DEBUG 2/5] 활성화된 카드: {currentCard.cardName}");

        ICardAction action = currentCard.CreateAction();
        if (action == null)
        {
            Debug.LogError($"[ATTACK-DEBUG 3/5] '{currentCard.name}'로부터 Action을 생성하는데 실패했습니다.");
            return;
        }
        Debug.Log($"[ATTACK-DEBUG 3/5] Action 생성 성공: {action.GetType().Name}");

        var context = new CardActionContext(currentCard, stats, firePoint);
        Debug.Log("[ATTACK-DEBUG 4/5] Action 실행(Execute) 직전입니다.");

        await action.Execute(context);

        Debug.Log("[ATTACK-DEBUG 5/5] Action 실행(Execute)이 완료되었습니다.");
    }
    void FixedUpdate()
    {
        if (stats == null || rb == null) return;
        rb.velocity = moveInput * stats.FinalMoveSpeed;
    }

    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    public void Heal(float amount)
    {
        if (stats != null)
        {
            stats.Heal(amount);
        }
    }
}