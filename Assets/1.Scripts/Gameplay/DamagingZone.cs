// --- ���ϸ�: DamagingZone.cs ---
using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{
    [Header("������ ����")]
    public float damagePerTick = 10f; // 1�ʸ��� �� ������
    public float tickInterval = 1.0f; // �������� �ִ� ���� (��)
    public float duration = 5.0f; // ������ ���ӵǴ� �ð� (��)

    // ���� �ȿ� ���� �ִ� ���� ���
    private List<MonsterController> targets = new List<MonsterController>();
    private float tickTimer;
    private float durationTimer;

    void OnEnable()
    {
        // ������Ʈ�� Ȱ��ȭ�� ������ Ÿ�̸� �ʱ�ȭ
        tickTimer = 0f;
        durationTimer = duration;
        targets.Clear(); // ���� ��� �ʱ�ȭ
    }

    void Update()
    {
        // ���� ���ӽð� ����
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            // PoolManager�� ����Ѵٸ� Release, �ƴ϶�� Destroy
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }

        // ������ ƽ Ÿ�̸� ����
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyDamageToTargets();
        }
    }

    // ���� ���� ������ ���Ͱ� ������ ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster")) // ���� �±׸� ����Ѵٰ� ����
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null && !targets.Contains(monster))
            {
                targets.Add(monster);
            }
        }
    }

    // ���� ���� ������ ���Ͱ� ������ ��
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null && targets.Contains(monster))
            {
                targets.Remove(monster);
            }
        }
    }

    // ��Ͽ� �ִ� ��� Ÿ�ٿ��� �������� �ִ� �Լ�
    private void ApplyDamageToTargets()
    {
        // ����Ʈ�� �����ؼ� ��ȸ (�������� �԰� �׾ ����Ʈ���� ���ŵ� ��츦 ���)
        List<MonsterController> currentTargets = new List<MonsterController>(targets);
        foreach (var monster in currentTargets)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                // �ٷ� �� �κ�! ������ TakeDamage�� ȣ���ϸ� ��!
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}