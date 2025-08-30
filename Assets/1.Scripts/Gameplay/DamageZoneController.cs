// ���� ���: Assets/1/Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �ð� ���� �����Ǹ� ���� �� ������ ���������� ���ظ� �ִ� ���� ������Ʈ�� �����մϴ�.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    // ���� ���� ����� ParticleSystem���� Transform���� ���� ����
    [Header("�ð� ȿ�� ����")]
    [SerializeField] private Transform visualsTransform; // ��ƼŬ ��� �ð� ȿ�� ������Ʈ�� Transform�� ����

    // ���� ���� ������
    private float _duration;
    private float _damagePerTick;
    private float _tickInterval;
    private float _lifeTimer;
    private float _damageTickTimer;
    private CircleCollider2D _collider;
    private HashSet<MonsterController> _monstersInZone;

    void Awake()
    {
        _monstersInZone = new HashSet<MonsterController>();
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
    }

    /// <summary>
    /// �ܺο��� ȣ��Ǿ� �� ������ ��� �Ӽ��� �����մϴ�.
    /// </summary>
    public void Initialize(float duration, float radius, float damagePerTick, float tickInterval)
    {
        // ���޹��� �����͵��� ���� ������ ����
        _duration = duration;
        _damagePerTick = damagePerTick;
        _tickInterval = Mathf.Max(0.1f, tickInterval);
        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        _monstersInZone.Clear();

        // ���� �ݶ��̴��� ũ�⸦ ���޹��� radius�� ����
        _collider.radius = radius;

        // �� ������Ʈ ��ü�� �������� 1�� ����
        transform.localScale = Vector3.one;

        // ���� ��ƼŬ ���� ������ Transform ������ ����� ���� ����
        if (visualsTransform != null)
        {
            // ���޹��� radius�� �������� �ð� ȿ���� ����(scale)�� �����մϴ�.
            // (��������Ʈ�� �⺻ ������ 1 ������ �� ����)
            float diameter = radius * 2f;
            visualsTransform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    // ... Update() �� OnTrigger ���� �Լ��� ������ �����ϰ� ���� ...
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
        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            _monstersInZone.Add(monster);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            _monstersInZone.Remove(monster);
        }
    }
}