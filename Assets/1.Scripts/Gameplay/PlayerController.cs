// PlayerController.cs: 플레이어의 움직임, 공격, 카드 트리거 등을 관리합니다.
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

    private Coroutine autoAttackRoutine; // 자동 공격 코루틴 참조
    private Coroutine cardTriggerRoutine; // 카드 트리거 코루틴 참조

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
        // 자동 공격 코루틴을 시작합니다. (첫 호출은 interval만큼 대기)
        StartAutoAttackLoop(stats.finalAttackSpeed);

        // 카드 트리거 코루틴을 시작합니다. (10초마다)
        StartCardTriggerLoop(10f);
    }

    void FixedUpdate()
    {
        // 현재 이동 속도를 사용하여 Rigidbody2D에 속도를 적용합니다.
        float speed = stats.finalMoveSpeed * baseMoveSpeed;
        rb.velocity = moveInput * speed;
    }

    // 이동 입력이 발생할 때 호출되는 메서드입니다.
    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    // 자동 공격 루프를 시작하거나 재시작합니다.
    private void StartAutoAttackLoop(float attackSpeed)
    {
        if (autoAttackRoutine != null)
            StopCoroutine(autoAttackRoutine); // 기존 코루틴이 있다면 중지합니다.
        autoAttackRoutine = StartCoroutine(AutoAttackLoop(attackSpeed)); // 새 코루틴을 시작합니다.
    }

    // 자동 공격을 수행하는 코루틴입니다.
    // 장착된 카드가 없을 경우 공격을 멈춥니다.
    private IEnumerator AutoAttackLoop(float attackSpeed)
    {
        // 공격 속도를 기반으로 공격 간격을 계산합니다. (0이거나 음수일 경우 기본 1초)
        float interval = attackSpeed > 0f ? 1f / attackSpeed : 1f;
        yield return new WaitForSeconds(interval); // 첫 공격 대기 시간을 기다립니다.

        while (true) // 게임이 실행되는 동안 지속적으로 공격을 시도합니다.
        {
            // 자동 공격 시스템은 장착된 카드가 있을 때만 작동합니다.
            if (cardManager.GetEquippedCards().Count > 0)
            {
                // 총알 프리팹과 발사 지점이 유효한지 확인 후 총알을 생성합니다.
                if (bulletPrefab != null && firePoint != null)
                {
                    GameObject bullet = PoolManager.Instance.Get(bulletPrefab);
                    bullet.transform.position = firePoint.position;
                    bullet.transform.rotation = firePoint.rotation;

                    BulletController bulletController = bullet.GetComponent<BulletController>();
                    if (bulletController != null)
                    {
                        // BulletController의 Initialize 메서드를 호출하여 총알 설정
                        bulletController.Initialize(firePoint.right, bulletPrefab.GetComponent<BulletController>().speed, stats.finalDamage);
                    }
                }
            }
            yield return new WaitForSeconds(interval); // 다음 공격 대기 시간을 기다립니다.
        }
    }

    // 카드 트리거 루프를 시작하거나 재시작합니다.
    private void StartCardTriggerLoop(float interval)
    {
        if (cardTriggerRoutine != null)
            StopCoroutine(cardTriggerRoutine); // 기존 코루틴이 있다면 중지합니다.
        cardTriggerRoutine = StartCoroutine(CardTriggerLoop(interval)); // 새 코루틴을 시작합니다.
    }

    // 카드 효과를 주기적으로 발동시키는 코루틴입니다.
    private IEnumerator CardTriggerLoop(float interval)
    {
        yield return new WaitForSeconds(interval); // 첫 트리거 대기 시간을 기다립니다.
        while (true) // 게임이 실행되는 동안 주기적으로 카드 효과를 발동합니다.
        {
            List<CardDataSO> equipped = cardManager.GetEquippedCards(); // 현재 장착된 카드 목록을 가져옵니다.
            // 장착된 카드가 있고, 목록이 비어있지 않은 경우에만 효과를 발동합니다.
            if (equipped != null && equipped.Count > 0)
            {
                // 장착된 카드 중 하나를 무작위로 선택합니다.
                int idx = Random.Range(0, equipped.Count);
                // 선택된 카드의 효과를 실행합니다. (EffectExecutor에 정의되어 있어야 합니다.)
                EffectExecutor.Instance.Execute(equipped[idx], 0f);
            }
            yield return new WaitForSeconds(interval); // 다음 트리거 대기 시간을 기다립니다.
        }
    }
}