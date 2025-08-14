using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Rigidbody2D rb;
    private CharacterStats stats;
    private CardManager cardManager;
    private Vector2 moveInput;

    [Header("총알 발사")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private const string AutoAttackMethodName = "AutoAttack";
    private const string CardTriggerMethodName = "CardTrigger";

    // Awake에서는 오직 자기 자신의 Instance만 설정하고, 내부 컴포넌트만 찾습니다.
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
    }

    // 모든 다른 오브젝트들의 Awake가 끝난 후, Start에서 안전하게 다른 매니저들을 참조합니다.
    void Start()
    {
        cardManager = CardManager.Instance;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.AddListener(OnMove);
        }

        if (stats != null)
        {
            stats.OnFinalStatsCalculated.AddListener(StartAutoAttackLoop);
        }

        StartAutoAttackLoop();
        StartCardTriggerLoop();
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.RemoveListener(OnMove);
        }
    }

    void FixedUpdate()
    {
        if (stats == null) return;
        rb.velocity = moveInput * stats.finalMoveSpeed;
    }

    private void OnMove(Vector2 input) { moveInput = input; }

    public void StartAutoAttackLoop()
    {
        CancelInvoke(AutoAttackMethodName);
        if (stats == null) return;
        float interval = stats.finalAttackSpeed > 0f ? 1f / stats.finalAttackSpeed : 1f;
        if (cardManager != null && cardManager.GetEquippedCards().Count == 0) return;
        InvokeRepeating(AutoAttackMethodName, 0f, interval);
    }

    private void AutoAttack()
    {
        if (cardManager == null || EffectExecutor.Instance == null) return;
        List<CardDataSO> equipped = cardManager.GetEquippedCards();
        if (equipped.Count > 0)
        {
            EffectExecutor.Instance.Execute(equipped[0]);
        }
    }

    public void Heal(float amount) { if (stats != null) stats.Heal(amount); }

    public void StartCardTriggerLoop()
    {
        CancelInvoke(CardTriggerMethodName);
        InvokeRepeating(CardTriggerMethodName, 10f, 10f);
    }

    private void CardTrigger()
    {
        if (cardManager == null || EffectExecutor.Instance == null) return;
        List<CardDataSO> equipped = cardManager.GetEquippedCards();
        if (equipped != null && equipped.Count > 0)
        {
            int idx = Random.Range(0, equipped.Count);
            // [디버그 로그 추가] 어떤 카드가 발동되었는지 콘솔에 출력
            Debug.Log($"[CardTrigger] 발동된 카드: {equipped[idx].cardName}");
            EffectExecutor.Instance.Execute(equipped[idx]);
        }
    }
}