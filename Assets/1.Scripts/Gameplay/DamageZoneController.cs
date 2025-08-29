// 경로: Assets/1.Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;

public class DamageZoneController : MonoBehaviour
{
    private float _duration;
    private float _damagePerSecond;
    private const float DAMAGE_TICK_INTERVAL = 0.5f; // 0.5초마다 피해
    private float _lifeTimer;
    private float _damageTickTimer;
    // 개선 방안 적용: GC 부담을 줄이기 위해 MonsterController를 직접 저장
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
        // 개선 방안 적용: 풀링된 오브젝트 재사용 시 반드시 초기화
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
        // 개선 방안 적용: 내부에 몬스터가 없으면 불필요한 연산 스킵
        if (_monstersInZone.Count == 0) return;

        _damageTickTimer += Time.deltaTime;
        if (_damageTickTimer >= DAMAGE_TICK_INTERVAL)
        {
            float damageToDeal = _damagePerSecond * DAMAGE_TICK_INTERVAL;
            // 리스트를 복사하지 않고 직접 순회
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
