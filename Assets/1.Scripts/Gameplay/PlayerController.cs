// PlayerController.cs: �÷��̾��� ������, ����, ī�� �ߵ� ���� �����մϴ�.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Rigidbody2D ������Ʈ�� �ʿ����� �����մϴ�.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb; // �÷��̾��� ������ �������� �����ϴ� Rigidbody2D ������Ʈ
    private Vector2 moveInput; // �÷��̾��� �̵� �Է��� �����մϴ�.
    private CharacterStats stats; // �÷��̾��� �ɷ�ġ�� �����ϴ� CharacterStats ������Ʈ
    private CardManager cardManager; // ī�� ������ �ν��Ͻ�

    [Header("�Ѿ� �߻�")]
    public GameObject bulletPrefab; // �߻��� �Ѿ� ������
    public Transform firePoint; // �Ѿ��� �߻�� ��ġ

    [Header("�̵� �ӵ� ����")]
    public float baseMoveSpeed = 5f; // �⺻ �̵� �ӵ�

    private Coroutine autoAttackRoutine; // �ڵ� ���� �ڷ�ƾ ����
    private Coroutine cardTriggerRoutine; // ī�� �ߵ� �ڷ�ƾ ����

    void Awake()
    {
        // �ʿ��� ������Ʈ���� �������� �̱��� �ν��Ͻ��� �Ҵ��մϴ�.
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<CharacterStats>();
        cardManager = CardManager.Instance;

        // InputManager�� OnMove �̺�Ʈ�� OnMove �޼��带 �����ʷ� �߰��մϴ�.
        InputManager.Instance.OnMove.AddListener(OnMove);
    }

    void Start()
    {
        // �ڵ� ���� �ڷ�ƾ�� �����մϴ�. (ù ȣ���� interval��ŭ ����)
        StartAutoAttackLoop(stats.finalAttackSpeed);

        // ī�� �ߵ� �ڷ�ƾ�� �����մϴ�. (10�ʸ���)
        StartCardTriggerLoop(10f);
    }

    void FixedUpdate()
    {
        // ���� �̵� �ӵ��� ����Ͽ� Rigidbody2D�� �����մϴ�.
        float speed = stats.finalMoveSpeed * baseMoveSpeed;
        rb.velocity = moveInput * speed;
    }

    // �̵� �Է��� ���� �� ȣ��Ǵ� �޼����Դϴ�.
    private void OnMove(Vector2 input)
    {
        moveInput = input;
    }

    // �ڵ� ���� ������ �����ϰų� ������մϴ�.
    private void StartAutoAttackLoop(float attackSpeed)
    {
        if (autoAttackRoutine != null)
            StopCoroutine(autoAttackRoutine); // ���� �ڷ�ƾ�� �ִٸ� �����մϴ�.
        autoAttackRoutine = StartCoroutine(AutoAttackLoop(attackSpeed)); // �� �ڷ�ƾ�� �����մϴ�.
    }

    // �ڵ� ������ �����ϴ� �ڷ�ƾ�Դϴ�.
    // ������ ī�尡 ���� ��� ������ ����ϴ�.
    private IEnumerator AutoAttackLoop(float attackSpeed)
    {
        // ���� �ӵ��� ���� ���� ������ ����մϴ�. (0���� �۰ų� ������ �⺻ 1��)
        float interval = attackSpeed > 0f ? 1f / attackSpeed : 1f;
        yield return new WaitForSeconds(interval); // ù ���ݱ��� ����մϴ�.

        while (true) // ���� ������ ���� ���������� ������ �õ��մϴ�.
        {
            // �ڵ� ���� �ý����� ������ ī�尡 ���� ���� �۵��մϴ�.
            if (cardManager.GetEquippedCards().Count > 0)
            {
                // �Ѿ� �����հ� �߻� ������ ��ȿ���� Ȯ�� �� �Ѿ��� �����մϴ�.
                if (bulletPrefab != null && firePoint != null)
                {
                    GameObject bullet = PoolManager.Instance.Get(bulletPrefab);
                    bullet.transform.position = firePoint.position;
                    bullet.transform.rotation = firePoint.rotation;

                    BulletController bulletController = bullet.GetComponent<BulletController>();
                    if (bulletController != null)
                    {
                        bulletController.damage = stats.finalDamage;
                    }
                }
            }
            yield return new WaitForSeconds(interval); // ���� ���ݱ��� ����մϴ�.
        }
    }

    // ī�� �ߵ� ������ �����ϰų� ������մϴ�.
    private void StartCardTriggerLoop(float interval)
    {
        if (cardTriggerRoutine != null)
            StopCoroutine(cardTriggerRoutine); // ���� �ڷ�ƾ�� �ִٸ� �����մϴ�.
        cardTriggerRoutine = StartCoroutine(CardTriggerLoop(interval)); // �� �ڷ�ƾ�� �����մϴ�.
    }

    // ī�� ȿ���� �ֱ������� �ߵ���Ű�� �ڷ�ƾ�Դϴ�.
    private IEnumerator CardTriggerLoop(float interval)
    {
        yield return new WaitForSeconds(interval); // ù �ߵ����� ����մϴ�.
        while (true) // ���� ������ ���� �ֱ������� ī�� ȿ���� �ߵ��մϴ�.
        {
            List<CardDataSO> equipped = cardManager.GetEquippedCards(); // ���� ������ ī�� ����� �����ɴϴ�.
            // ������ ī�尡 �ְ�, ����� ������� ���� ��쿡�� ȿ���� �ߵ��մϴ�.
            if (equipped != null && equipped.Count > 0)
            {
                // ������ ī�� �� �ϳ��� �������� �����մϴ�.
                int idx = Random.Range(0, equipped.Count);
                // ���õ� ī���� ȿ���� �����մϴ�. (EffectExecutor�� ������ �����Ǿ�� �մϴ�.)
                EffectExecutor.Instance.Execute(equipped[idx]);
            }
            yield return new WaitForSeconds(interval); // ���� �ߵ����� ����մϴ�.
        }
    }
}