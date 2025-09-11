// ���: Assets/1.Scripts/Gameplay/RippleController.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class RippleController : MonoBehaviour
{
    private float _maxRadius;
    private float _expandDuration;
    private float _damage;
    private EntityStats _caster;
    private CircleCollider2D _collider;
    private float _elapsedTime;
    // ���� ��� ����: HashSet<GameObject> ��� InstanceID�� ����ϴ� int�� �޸� ȿ���� ���
    private HashSet<int> _hitMonsterIDs;
    // ���� ��� ����: new Vector3() GC �й� ���Ҹ� ���� ��� ������ ĳ��
    private Vector3 _scaleCache = Vector3.one;

    void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
        _hitMonsterIDs = new HashSet<int>();
    }

    public void Initialize(EntityStats caster, float maxRadius, float duration, float damage) // CharacterStats -> EntityStats
    {
        _caster = caster;
        _maxRadius = maxRadius;
        _expandDuration = duration;
        _damage = damage;
        _elapsedTime = 0f;

        // ���� ��� ����: Ǯ���� ������Ʈ ���� �� �ݵ�� �ʱ�ȭ
        _hitMonsterIDs.Clear();
        transform.localScale = Vector3.zero;
        _collider.radius = 0.2f;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _expandDuration);
        // ���� ���൵�� ���� ũ��� �浹 ���� ���
        float currentRadius = Mathf.Lerp(0, _maxRadius, progress);
        float scale = currentRadius * 2f; // Scale�� ���� ����

        // ���� ��� ����: ĳ�õ� Vector3 ����Ͽ� GC �й� �ּ�ȭ
        _scaleCache.Set(scale, scale, 1f);
        transform.localScale = _scaleCache;
        // ���� ��� ����: �ð� ȿ���� �ݶ��̴� ũ�� ����ȭ
        //_collider.radius = currentRadius;

        if (_elapsedTime >= _expandDuration)
        {
            // �ĵ� ȿ���� ������ Ǯ�� ��ȯ
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            int monsterID = other.gameObject.GetInstanceID();

            // �̹� ���ظ� ���� ���ʹ� �ǳʶپ� �ߺ� ���� ����
            if (_hitMonsterIDs.Contains(monsterID)) return;
            if (other.TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(_damage);
                _hitMonsterIDs.Add(monsterID);
            }
        }
    }
}
