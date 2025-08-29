// ���: Assets/1.Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

public class DamageZoneController : MonoBehaviour
{
    private float _duration;
    private float _damagePerSecond;
    private const float DAMAGE_TICK_INTERVAL = 0.5f; // 0.5�ʸ��� ����
    private float _lifeTimer;
    private float _damageTickTimer;
    // ���� ��� ����: GC �δ��� ���̱� ���� MonsterController�� ���� ����
    private HashSet<MonsterController> _monstersInZone;

    void Awake()
    {
        _monstersInZone = new HashSet<MonsterController>();
    }

    public void Initialize(float duration, float damagePerSecond)
    {
        _duration = duration;
        _damagePerSecond = damagePerSecond;
        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        // ���� ��� ����: Ǯ���� ������Ʈ ���� �� �ݵ�� �ʱ�ȭ
        _monstersInZone.Clear();
    }

    void Update()
    {
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _duration)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
            return;
        }
        // ���� ��� ����: ���ο� ���Ͱ� ������ ���ʿ��� ���� ��ŵ
        if (_monstersInZone.Count == 0) return;

        _damageTickTimer += Time.deltaTime;
        if (_damageTickTimer >= DAMAGE_TICK_INTERVAL)
        {
            float damageToDeal = _damagePerSecond * DAMAGE_TICK_INTERVAL;
            // ����Ʈ�� �������� �ʰ� ���� ��ȸ
            foreach (var monster in _monstersInZone)
            {
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    monster.TakeDamage(damageToDeal);
                }
            }
            _damageTickTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            if (other.TryGetComponent<MonsterController>(out var monster))
            {
                _monstersInZone.Add(monster);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            if (other.TryGetComponent<MonsterController>(out var monster))
            {
                _monstersInZone.Remove(monster);
            }
        }
    }
}
