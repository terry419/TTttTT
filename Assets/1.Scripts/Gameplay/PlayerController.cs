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
    public Transform firePoint;

    private const string AutoAttackMethodName = "AutoAttack";
    private const string CardTriggerMethodName = "CardTrigger";

    // Awake에서는 오직 자기 자신의 Instance만 설정하고, 내부 컴포넌트만 찾습니다.
    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            // [추가됨] 씬 전환 시 파괴되지 않도록 설정합니다.
        }
        else 
        {
            // [수정됨] 이미 인스턴스가 존재하면 새로 생성된 오브젝트를 파괴합니다.
            Destroy(gameObject);
            return;
        }

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
        if (stats == null)
        {
            return;
        }
        if (rb == null)
        {
            return;
        }
        Vector2 finalVelocity = moveInput * stats.finalMoveSpeed;
        rb.velocity = finalVelocity;
    }

    private void OnMove(Vector2 input) 
    {
        moveInput = input; 
    }


    public void StartAutoAttackLoop()
    {
        CancelInvoke("PerformAttack");
        if (stats == null) return;
        float interval = 1f / stats.finalAttackSpeed;
        InvokeRepeating("PerformAttack", 0f, interval);
    }

    private void PerformAttack()
    {
        // CardManager 또는 활성 카드가 없으면 공격하지 않습니다.
        if (cardManager == null || cardManager.activeCard == null)
        {
            return;
        }

        // CardManager가 정해준 '활성 카드'를 가져와서 그 효과를 실행합니다.
        EffectExecutor.Instance.Execute(cardManager.activeCard);
    }

    public void Heal(float amount) 
    { 
        if (stats != null) stats.Heal(amount); 
    }
        
}
