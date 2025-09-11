// 경로: Assets/1.Scripts/Gameplay/RippleController.cs
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
    // 개선 방안 적용: HashSet<GameObject> 대신 InstanceID를 사용하는 int로 메모리 효율성 향상
    private HashSet<int> _hitMonsterIDs;
    // 개선 방안 적용: new Vector3() GC 압박 감소를 위해 멤버 변수로 캐싱
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

        // 개선 방안 적용: 풀링된 오브젝트 재사용 시 반드시 초기화
        _hitMonsterIDs.Clear();
        transform.localScale = Vector3.zero;
        _collider.radius = 0.2f;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _expandDuration);
        // 현재 진행도에 따른 크기와 충돌 범위 계산
        float currentRadius = Mathf.Lerp(0, _maxRadius, progress);
        float scale = currentRadius * 2f; // Scale은 지름 기준

        // 개선 방안 적용: 캐시된 Vector3 사용하여 GC 압박 최소화
        _scaleCache.Set(scale, scale, 1f);
        transform.localScale = _scaleCache;
        // 개선 방안 적용: 시각 효과와 콜라이더 크기 동기화
        //_collider.radius = currentRadius;

        if (_elapsedTime >= _expandDuration)
        {
            // 파동 효과가 끝나면 풀에 반환
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            int monsterID = other.gameObject.GetInstanceID();

            // 이미 피해를 입은 몬스터는 건너뛰어 중복 피해 방지
            if (_hitMonsterIDs.Contains(monsterID)) return;
            if (other.TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(_damage);
                _hitMonsterIDs.Add(monsterID);
            }
        }
    }
}
