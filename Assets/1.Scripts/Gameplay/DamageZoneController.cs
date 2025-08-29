// 경로: Assets/1.Scripts/Gameplay/DamageZoneController.cs
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

    // [개선 방안 적용] new Vector3() GC 압박 감소를 위해 멤버 변수로 캐싱
    private Vector3 _scaleCache = Vector3.one;

    void Awake()
    {
        _monstersInZone = new HashSet<MonsterController>();
        _collider = GetComponent<CircleCollider2D>();
        _collider.isTrigger = true;
    }

    // [수정] radius와 tickInterval 파라미터를 추가로 받습니다.
    public void Initialize(float duration, float radius, float damagePerTick, float tickInterval)
    {
        _duration = duration;
        _damagePerTick = damagePerTick;
        // [수정] 0으로 설정 시 오류 방지를 위한 최소값 보정
        _tickInterval = Mathf.Max(0.1f, tickInterval);

        _lifeTimer = 0f;
        _damageTickTimer = 0f;
        _monstersInZone.Clear();

        // [수정] 전달받은 radius로 콜라이더와 시각적 크기를 설정합니다.
        if (radius > 0)
        {
            _collider.radius = radius;
            float scale = radius * 2f; // Scale은 지름 기준
            _scaleCache.Set(scale, scale, 1f);
            // 자식 오브젝트(Visuals)가 있다면 그 크기를 조절하는 것이 더 좋습니다.
            // 여기서는 일단 부모의 스케일을 조절합니다.
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
            // 리스트를 복사하지 않고 직접 순회
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