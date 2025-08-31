// ���� ���: Assets/1/Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �ð� ���� �����Ǹ� ���� �� ������ ���������� ���ظ� �ִ� ���� ������Ʈ�� �����մϴ�.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    // ���� �ٽ� Transform ������ �����մϴ� ����
    [Header("�ð� ȿ�� ����")]
    [SerializeField] private Transform visualsTransform;

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

    public void Initialize(float duration, float radius, float damagePerTick, float tickInterval)
    {
        _duration = duration;
        _damagePerTick = damagePerTick;
        _tickInterval = Mathf.Max(0.1f, tickInterval);
        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        _monstersInZone.Clear();

        // 1. ���� �ݶ��̴��� �ݰ��� ������(radius)�� ���� �����մϴ�. (��Ȯ��)
        _collider.radius = radius;

        // 2. �� ������Ʈ(�θ�)�� �������� 1�� �����Ͽ� ���������� ����ϴ�.
        transform.localScale = Vector3.one;

        // ���� �ð� ȿ��(�ڽ�)�� �������� ��Ȯ�ϰ� ����Ͽ� �����մϴ� ����
        if (visualsTransform != null)
        {
            // 2-1. �ڽ��� SpriteRenderer ������Ʈ�� �����ɴϴ�.
            if (visualsTransform.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && spriteRenderer.sprite != null)
            {
                // 2-2. ��������Ʈ�� ���� �ؽ�ó ũ��(�ȼ�)�� Pixels Per Unit ���� �����ɴϴ�.
                float textureWidth = spriteRenderer.sprite.texture.width;
                float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;

                // 2-3. ��������Ʈ�� �⺻ ���� ���� ũ�⸦ ����մϴ�. (��: 128px / 100ppu = 1.28 ����)
                float baseSpriteDiameter = textureWidth / pixelsPerUnit;

                // 2-4. ��ǥ ����(radius * 2)�� �⺻ ũ��� ������ ��Ȯ�� ������ ������ ����մϴ�.
                float targetDiameter = radius * 2f;
                float requiredScale = targetDiameter / baseSpriteDiameter;

                // 2-5. ���� ������ ���� �ڽ� Transform�� �����մϴ�.
                visualsTransform.localScale = new Vector3(requiredScale, requiredScale, 1f);
            }
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