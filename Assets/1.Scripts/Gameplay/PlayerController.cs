// --- 파일명: PlayerController.cs (경로: /content/drive/MyDrive/Unity/9th/Assets/1.Scripts/Gameplay/PlayerController.cs) ---

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Rigidbody2D 컴포넌트가 필요함을 명시합니다.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PlayerController Instance { get; private set; }

    private Rigidbody2D rb; // 플레이어의 물리적 움직임을 제어하는 Rigidbody2D 컴포넌트
    private Vector2 moveInput; // 플레이어의 이동 입력을 저장합니다.
    private CharacterStats stats; // 플레이어의 능력치를 관리하는 CharacterStats 컴포넌트
    private CardManager cardManager; // 카드 관리자 인스턴스

    [Header("총알 발사")]
    public GameObject bulletPrefab; // 발사할 총알 프리팹
    public Transform firePoint; // 총알이 발사될 위치

    [Header("이동 속도 설정")]
    public float baseMoveSpeed = 5f; // 기본 이동 속도

    //private Coroutine autoAttackRoutine; // 자동 공격 코루틴 참조 (더 이상 사용하지 않음)
    //private Coroutine cardTriggerRoutine; // 카드 트리거 코루틴 참조 (더 이상 사용하지 않음)

    // InvokeRepeating을 위한 메서드 이름
    private const string AutoAttackMethodName = "AutoAttack";
    private const string CardTriggerMethodName = "CardTrigger";


    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 필요한 컴포넌트들을 가져오고 싱글톤 인스턴스를 할당합니다.
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cardManager = CardManager.Instance;

        // InputManager의 OnMove 이벤트에 OnMove 메서드를 리스너로 추가합니다.
        InputManager.Instance.OnMove.AddListener(OnMove);
    }

    void Start()
    {
        // CharacterStats의 능력치 계산 완료 이벤트에 StartAutoAttackLoop 메서드를 리스너로 추가합니다.
        stats.OnFinalStatsCalculated.AddListener(StartAutoAttackLoop);

        // 초기 능력치 계산이 완료된 후, 첫 자동 공격 루프를 시작합니다.
        StartAutoAttackLoop();

        // 카드 트리거 루프를 시작합니다.
        StartCardTriggerLoop();
    }

    void FixedUpdate()
    {
        // 현재 이동 속도를 사용하여 Rigidbody2D에 속도를 적용합니다.
        float speed = stats.finalMoveSpeed;
        rb.velocity = moveInput * speed;
    }

    // 이동 입력이 발생할 때 호출되는 메서드입니다.
    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// 자동 공격 루프를 시작하거나 재시작합니다.
    /// CharacterStats의 최종 공격 속도가 업데이트될 때마다 호출해야 합니다.
    /// </summary>
    public void StartAutoAttackLoop()
    {
        // 기존 Invoke를 취소하고 새롭게 시작합니다.
        CancelInvoke(AutoAttackMethodName);

        // 공격 속도를 기반으로 공격 간격을 계산합니다.
        float interval = stats.finalAttackSpeed > 0f ? 1f / stats.finalAttackSpeed : 1f;

        // 장착된 카드가 없을 경우 자동 공격을 멈춥니다.
        if (cardManager.GetEquippedCards().Count == 0)
        {
            return;
        }

        InvokeRepeating(AutoAttackMethodName, 0f, interval);
        Debug.Log($"자동 공격 시작. 공격 간격: {interval}초.");
    }

    /// <summary>
    /// 자동 공격을 수행하는 메서드입니다. InvokeRepeating에 의해 주기적으로 호출됩니다.
    /// </summary>
    private void AutoAttack()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = PoolManager.Instance.Get(bulletPrefab);
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;

            BulletController bulletController = bullet.GetComponent<BulletController>();
            if (bulletController != null)
            {
                bulletController.Initialize(firePoint.right, bulletPrefab.GetComponent<BulletController>().speed, stats.finalDamage);
            }
        }
    }


    /// <summary>
    /// 플레이어의 체력을 회복시킵니다.
    /// </summary>
    /// <param name="amount">회복량</param>
    public void Heal(float amount)
    {
        if (stats != null)
        {
            stats.Heal(amount);
        }
    }

    /// <summary>
    /// 카드 트리거 루프를 시작하거나 재시작합니다.
    /// </summary>
    public void StartCardTriggerLoop()
    {
        // 기존 Invoke를 취소하고 새롭게 시작합니다.
        CancelInvoke(CardTriggerMethodName);

        // 기획서: 장착 카드 중 1장 랜덤 선택 후 10초간 효과 발현 (InvokeRepeating 사용)
        // CardManager의 HandleTrigger 함수는 모든 카드를 순회하므로, 여기서는 랜덤 선택 로직을 구현합니다.

        InvokeRepeating(CardTriggerMethodName, 10f, 10f);
        Debug.Log("카드 트리거 루프 시작. 10초마다 랜덤 카드 발동.");
    }

    /// <summary>
    /// 카드 효과를 주기적으로 발동시키는 메서드입니다. InvokeRepeating에 의해 주기적으로 호출됩니다.
    /// </summary>
    private void CardTrigger()
    {
        List<CardDataSO> equipped = cardManager.GetEquippedCards(); // 현재 장착된 카드 목록을 가져옵니다.

        if (equipped != null && equipped.Count > 0)
        {
            // 장착된 카드 중 하나를 무작위로 선택합니다.
            int idx = Random.Range(0, equipped.Count);

            // 선택된 카드의 효과를 실행합니다.
            EffectExecutor.Instance.Execute(equipped[idx], 0f);
        }
    }
}