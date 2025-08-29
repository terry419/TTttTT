// ���: Assets/1.Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    private float _duration;
    private float _damagePerTick;
    private float _tickInterval;

    private float _lifeTimer;
    private float _damageTickTimer;
    private CircleCollider2D _collider;

    private HashSet<MonsterController> _monstersInZone;

    // [���� ��� ����] new Vector3() GC �й� ���Ҹ� ���� ��� ������ ĳ��
    private Vector3 _scaleCache = Vector3.one;

    void Awake()
    {
        _monstersInZone = new HashSet<MonsterController>();
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
    }

    // [����] radius�� tickInterval �Ķ���͸� �߰��� �޽��ϴ�.
    public void Initialize(float duration, float radius, float damagePerTick, float tickInterval)
    {
        _duration = duration;
        _damagePerTick = damagePerTick;
        // [����] 0���� ���� �� ���� ������ ���� �ּҰ� ����
        _tickInterval = Mathf.Max(0.1f, tickInterval);

        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        _monstersInZone.Clear();

        // [����] ���޹��� radius�� �ݶ��̴��� �ð��� ũ�⸦ �����մϴ�.
        if (radius > 0)
        {
            _collider.radius = radius;
            float scale = radius * 2f; // Scale�� ���� ����
            _scaleCache.Set(scale, scale, 1f);
            // �ڽ� ������Ʈ(Visuals)�� �ִٸ� �� ũ�⸦ �����ϴ� ���� �� �����ϴ�.
            // ���⼭�� �ϴ� �θ��� �������� �����մϴ�.
            transform.localScale = _scaleCache;
        }
    }

    void Update()
    {
        _lifeTimer += Time.deltaTime;
        if (_lifeTimer >= _duration)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
            return;
        }

        if (_monstersInZone.Count == 0) return;

        _damageTickTimer += Time.deltaTime;
        if (_damageTickTimer >= _tickInterval)
        {
            // ����Ʈ�� �������� �ʰ� ���� ��ȸ
            foreach (var monster in _monstersInZone)
            {
                if (monster != null && monster.gameObject.activeInHierarchy)
                {
                    monster.TakeDamage(_damagePerTick);
                }
            }
            _damageTickTimer = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster) && other.TryGetComponent<MonsterController>(out var monster))
        {
            _monstersInZone.Add(monster);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster) && other.TryGetComponent<MonsterController>(out var monster))
        {
            _monstersInZone.Remove(monster);
        }
    }
}