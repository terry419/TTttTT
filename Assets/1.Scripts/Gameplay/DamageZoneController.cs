// 파일 경로: Assets/1/Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 일정 시간 동안 유지되며 범위 내 적에게 지속적으로 피해를 주는 장판 오브젝트를 제어합니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    // ▼▼▼ 제어 대상을 ParticleSystem에서 Transform으로 변경 ▼▼▼
    [Header("시각 효과 참조")]
    [SerializeField] private Transform visualsTransform; // 파티클 대신 시각 효과 오브젝트의 Transform을 연결

    // 내부 상태 변수들
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
    /// 외부에서 호출되어 이 장판의 모든 속성을 설정합니다.
    /// </summary>
    public void Initialize(float duration, float radius, float damagePerTick, float tickInterval)
    {
        // 전달받은 데이터들을 내부 변수에 저장
        _duration = duration;
        _damagePerTick = damagePerTick;
        _tickInterval = Mathf.Max(0.1f, tickInterval);
        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        _monstersInZone.Clear();

        // 물리 콜라이더의 크기를 전달받은 radius로 설정
        _collider.radius = radius;

        // 이 오브젝트 자체의 스케일은 1로 고정
        transform.localScale = Vector3.one;

        // ▼▼▼ 파티클 제어 로직을 Transform 스케일 제어로 변경 ▼▼▼
        if (visualsTransform != null)
        {
            // 전달받은 radius를 기준으로 시각 효과의 지름(scale)을 설정합니다.
            // (스프라이트의 기본 지름이 1 유닛일 때 기준)
            float diameter = radius * 2f;
            visualsTransform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    // ... Update() 및 OnTrigger 관련 함수는 이전과 동일하게 유지 ...
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