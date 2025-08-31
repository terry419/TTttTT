// 파일 경로: Assets/1/Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 일정 시간 동안 유지되며 범위 내 적에게 지속적으로 피해를 주는 장판 오브젝트를 제어합니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    // ▼▼▼ 다시 Transform 참조로 변경합니다 ▼▼▼
    [Header("시각 효과 참조")]
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

        // 1. 물리 콜라이더의 반경을 데이터(radius)에 따라 설정합니다. (정확함)
        _collider.radius = radius;

        // 2. 이 오브젝트(부모)의 스케일은 1로 고정하여 기준점으로 삼습니다.
        transform.localScale = Vector3.one;

        // ▼▼▼ 시각 효과(자식)의 스케일을 정확하게 계산하여 설정합니다 ▼▼▼
        if (visualsTransform != null)
        {
            // 2-1. 자식의 SpriteRenderer 컴포넌트를 가져옵니다.
            if (visualsTransform.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && spriteRenderer.sprite != null)
            {
                // 2-2. 스프라이트의 원본 텍스처 크기(픽셀)와 Pixels Per Unit 값을 가져옵니다.
                float textureWidth = spriteRenderer.sprite.texture.width;
                float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;

                // 2-3. 스프라이트의 기본 월드 유닛 크기를 계산합니다. (예: 128px / 100ppu = 1.28 유닛)
                float baseSpriteDiameter = textureWidth / pixelsPerUnit;

                // 2-4. 목표 지름(radius * 2)을 기본 크기로 나누어 정확한 스케일 배율을 계산합니다.
                float targetDiameter = radius * 2f;
                float requiredScale = targetDiameter / baseSpriteDiameter;

                // 2-5. 계산된 스케일 값을 자식 Transform에 적용합니다.
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