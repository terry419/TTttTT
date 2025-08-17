using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{


    // ������� 1: [����] ���� ���ؿ� ���� ���ظ� ���� ���� �и�
    [Header("������ ����")]
    public float singleHitDamage = 0f; // �ĵ� ���� ���� �� �� ���� �� ������
    public float damagePerTick = 10f;  // ���� ���� ���� �� ƽ�� �� ������
    [Tooltip("���� ���� ���� �� �������� �ִ� ���� (��). �ĵ����� ����� ��� 100 �̻��� �Է��ϼ���.")]
    public float tickInterval = 1.0f;  // ���� ���� ���� �� �������� �ִ� ���� (��)
    public float duration = 5.0f;     // ����/�ĵ��� �� ���� �ð� (��)

    [Header("�ĵ�/���� Ȯ�� ����")]
    public float expansionSpeed = 1.0f;    // �ʴ� Ŀ���� �ӵ�
    public float expansionDuration = 2.0f; // �� ���� �ð� �� Ȯ���ϴ� �� �ɸ��� �ð�

    // ������� 2: [�߰�] �� ����/�ĵ� �ν��Ͻ��� ���� ID (���� ���� ��忡�� ���)
    public string shotInstanceID;

    // ������� 3: [�߰�] �� ������ ���� ���� �ĵ� ������� ���� ���� ���� ������� �����ϴ� �÷���
    public bool isSingleHitWaveMode = true;

    // ���� Ÿ�̸� �� ���� ����
    private List<MonsterController> targets = new List<MonsterController>(); // ���� ���� ��忡�� ���
    private float tickTimer;
    private float durationTimer;
    private float expansionTimer;
    private CircleCollider2D circleCollider;
    private Vector3 initialScale;
    
    private float initialColliderRadius;

    void Awake()
    {
        initialScale = transform.localScale;
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            initialColliderRadius = circleCollider.radius; // [�߰�] Awake���� �ʱ� ������ ����
        }
    }

    void OnEnable()
    {
        // ������� 4: [����] ������Ʈ�� Ȱ��ȭ�� ������ ��� Ÿ�̸ӿ� ���¸� �ʱ�ȭ�մϴ�.
        tickTimer = 0f;
        durationTimer = duration; // �� ���� �ð����� �ʱ�ȭ
        expansionTimer = 0f;
        transform.localScale = initialScale; // �������� �ʱ� ������ ����
        targets.Clear(); // ���� ���� ��带 ���� Ÿ�� ����Ʈ �ʱ�ȭ
        // shotInstanceID�� isSingleHitWaveMode�� Initialize���� �����ǹǷ� ���⼭ �ʱ�ȭ���� �ʽ��ϴ�.
    }

    // ������� 5: [����] �ܺο��� ��� �Ķ���͸� �����ϴ� Initialize �޼���
    public void Initialize(float singleHitDmg, float continuousDmgPerTick, float tickInt, float totalDur, float expSpeed, float expDur, bool isWave, string shotID)
    {
        this.singleHitDamage = singleHitDmg;
        this.damagePerTick = continuousDmgPerTick;
        this.tickInterval = tickInt;
        this.duration = totalDur; // �� ���� �ð� ����
        this.expansionSpeed = expSpeed;
        this.expansionDuration = expDur;
        this.isSingleHitWaveMode = isWave;
        this.shotInstanceID = shotID;

        // Initialize ȣ�� �� OnEnable ������ �ٽ� �����Ͽ� ���¸� �ʱ�ȭ
        OnEnable();
    }

    void Update()
    {
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }
        // ������� 6: [����] Ȯ�� ���� (expansionDuration������ Ȯ��)
        if (expansionTimer < expansionDuration)
        {
            expansionTimer += Time.deltaTime;
            transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;
        }

        // ������� 7: [����] �� ���� �ð� ����
        if (durationTimer <= 0)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }

        // ������� 8: [����] ���� ���� ����� ���� ƽ ������ ���� ����
        if (!isSingleHitWaveMode) // ���� ���� �ĵ� ��尡 �ƴ� �� (��, ���� ���� ���� ����� ��)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyDamageToTargets();
            }
        }
    }

    // ������� 9: [����] OnTriggerEnter2D ���� �б�
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (isSingleHitWaveMode) // ���� ���� �ĵ� ���
            {
                if (!monster.hitShotIDs.Contains(this.shotInstanceID))
                {
                    monster.hitShotIDs.Add(this.shotInstanceID);
                    monster.TakeDamage(this.singleHitDamage);
                }
            }
            else // ���� ���� ���� ���
            {
                if (!targets.Contains(monster))
                {
                    targets.Add(monster);
                }
            }
        }
    }

    // ������� 10: [����] OnTriggerExit2D ���� �б�
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (!isSingleHitWaveMode) // ���� ���� ���� ����� ���� Ÿ�� ����Ʈ���� ����
            {
                if (targets.Contains(monster))
                {
                    targets.Remove(monster);
                }
            }
        }
    }

    // ������� 11: [�߰�] ���� ���� ��忡�� ���� ApplyDamageToTargets �޼���
    private void ApplyDamageToTargets()
    {
        List<MonsterController> currentTargets = new List<MonsterController>(targets);
        foreach (var monster in currentTargets)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}
